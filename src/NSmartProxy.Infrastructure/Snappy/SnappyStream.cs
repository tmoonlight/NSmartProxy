using System;
using System.IO;
using System.IO.Compression;

namespace Snappy.Sharp
{
    // Modeled after System.IO.Compression.DeflateStream in the framework
    public class SnappyStream : Stream
    {
        private const int BLOCK_LOG = 15;
        private const int BLOCK_SIZE = 1 << BLOCK_LOG;

        private Stream stream;
        private readonly CompressionMode compressionMode;
        private readonly bool leaveStreamOpen;
        private readonly bool writeChecksums;
        private static readonly byte[] StreamHeader = new byte[] { (byte)'s', (byte)'N', (byte)'a', (byte)'P', (byte)'p', (byte)'Y'};
        private const byte StreamIdentifier = 0xff;
        private const byte CompressedType = 0x00;
        private const byte UncompressedType = 0x01;

        // allocate a 64kB buffer for the (de)compressor to use
        private readonly byte[] internalBuffer = new byte[1<<(BLOCK_LOG + 1)];
        private int internalBufferIndex = 0;
        private int internalBufferLength = 0;

        private readonly SnappyCompressor compressor;
        private readonly SnappyDecompressor decompressor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnappyStream"/> class.
        /// </summary>
        /// <param name="s">The stream.</param>
        /// <param name="mode">The compression mode.</param>
        public SnappyStream(Stream s, CompressionMode mode) : this(s, mode, false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnappyStream"/> class.
        /// </summary>
        /// <param name="s">The stream.</param>
        /// <param name="mode">The compression mode.</param>
        /// <param name="leaveOpen">If set to <c>true</c> leaves the stream open when complete.</param>
        /// <param name="checksum"><c>true</c> if checksums should be written to the stream </param>
        public SnappyStream(Stream s, CompressionMode mode, bool leaveOpen, bool checksum)
        {
            stream = s;
            compressionMode = mode;
            leaveStreamOpen = leaveOpen;
            writeChecksums = checksum;

            if (compressionMode == CompressionMode.Decompress)
            {
                if (!stream.CanRead)
                    throw new InvalidOperationException("Trying to decompress and underlying stream not readable.");

                decompressor =  new SnappyDecompressor();

                CheckStreamIdentifier();
                CheckStreamHeader();
            }
            if (compressionMode == CompressionMode.Compress)
            {
                if (!stream.CanWrite)
                    throw new InvalidOperationException("Trying to compress and underlying stream is not writable.");

                compressor = new SnappyCompressor();

                stream.WriteByte(StreamIdentifier);
                stream.Write(StreamHeader, 0, StreamHeader.Length);
            }
        }

        /// <summary>
        /// Provides access to the underlying (compressed) <see cref="T:System.IO.Stream"/>.
        /// </summary>
        public Stream BaseStream { get { return stream; } }

        public override bool CanRead
        {
            get { return stream != null && compressionMode == CompressionMode.Decompress && stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return stream != null && compressionMode == CompressionMode.Compress && stream.CanWrite; }
        }

        public override void Flush()
        {
            if (stream != null) 
                stream.Flush();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }
        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (compressionMode != CompressionMode.Decompress || decompressor == null)
                throw new InvalidOperationException("Cannot read if not set to decompression mode.");

            int readCount = 0;
            int firstByte = stream.ReadByte();
            // first byte can indicate stream header, we just read it and move on.
            if (firstByte == StreamIdentifier)
            {
                CheckStreamHeader();
            }
            else if (firstByte == UncompressedType)
            {
                var length = GetChunkUncompressedLength();
                readCount = ProcessRemainingInternalBuffer(buffer, offset, count);
                if (readCount != count)
                {
                    stream.Read(internalBuffer, 0, length);
                    Array.Copy(internalBuffer, 0, buffer, offset, count - readCount);
                    internalBufferIndex = count - readCount;
                    internalBufferLength = length;
                }
            }
            else if (firstByte == CompressedType)
            {
                var length = GetChunkUncompressedLength();
                count = ProcessRemainingInternalBuffer(buffer, offset, count);

                // we at most have 64kb in the buffer to read
                byte[] tempBuffer = new byte[1 << (BLOCK_LOG + 1)];
                stream.Read(tempBuffer, 0, tempBuffer.Length);

                decompressor.Decompress(tempBuffer, 0, tempBuffer.Length, internalBuffer, 0, length);

                Array.Copy(internalBuffer, 0, buffer, offset, count);
                internalBufferIndex = count;
                internalBufferLength = length;
            }
            else if (firstByte > 0x2 && firstByte < 0x7f)
            {
                throw new InvalidOperationException("Found unskippable chunk type that cannot be undertood.");
            }
            else
            {
                // getting the length and skipping the data.
                var length = GetChunkUncompressedLength();
                stream.Seek(length, SeekOrigin.Current);
                readCount += length;
            }
            return readCount;
        }

        private int ProcessRemainingInternalBuffer(byte[] buffer, int offset, int count)
        {
            if (internalBufferLength - internalBufferIndex > count)
            {
                Array.Copy(internalBuffer, internalBufferIndex, buffer, offset, count);
                internalBufferIndex += count;
            }
            else if (internalBufferLength > 0)
            {
                Array.Copy(internalBuffer, internalBufferIndex, buffer, offset, internalBufferLength - internalBufferIndex);
                count -= (internalBufferLength - internalBufferIndex);
            }
            return count;
        }

        private int GetChunkUncompressedLength()
        {
            int len1 = stream.ReadByte();
            int len2 = stream.ReadByte();
            int length = (len1 << 8) | len2;
            if (length > BLOCK_SIZE)
                throw new InvalidOperationException("Chunk length is too big.");
            return length;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }
        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (compressionMode != CompressionMode.Compress || compressor == null)
                throw new InvalidOperationException("Cannot write if not set to compression mode.");

            if (buffer.Length < count)
                throw new InvalidOperationException();

            for (int i = 0; i < count; i += BLOCK_SIZE)
            {
                stream.WriteByte(CompressedType);
                compressor.WriteUncomressedLength(buffer, 1, count);
                int compressedLength = compressor.CompressInternal(buffer, offset, count, internalBuffer, 2);
                stream.Write(internalBuffer, 0, compressedLength + 3);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && stream != null)
                {
                    Flush();
                    if (compressionMode == CompressionMode.Compress && stream != null)
                    {
                        // Make sure all data written
                    }
                }
            }
            finally
            {
                try
                {
                    if (disposing && !leaveStreamOpen && stream != null)
                    {
                        stream.Close();
                    }
                }
                finally
                {
                    stream = null;
                    base.Dispose(disposing);
                }
            }            
        }

        /// <summary>
        /// This operation is not supported and always throws a <see cref="T:System.NotSupportedException" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This operation is not supported on this stream.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This operation is not supported and always throws a <see cref="T:System.NotSupportedException" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This operation is not supported on this stream.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This property is not supported and always throws a <see cref="T:System.NotSupportedException" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// This property is not supported and always throws a <see cref="T:System.NotSupportedException" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        private void CheckStreamHeader()
        {
            byte[] heading = new byte[StreamHeader.Length];
            stream.Read(heading, 0, heading.Length);
            for (int i = 1; i < heading.Length; i++)
            {
                if (heading[i] != StreamHeader[i])
                    throw new InvalidDataException("Stream does not start with required header");
            }
        }

        private void CheckStreamIdentifier()
        {
            int firstByte = stream.ReadByte();
            if (firstByte == -1)
                throw new InvalidOperationException("Found EOF when trying to read header.");
            if (firstByte != StreamIdentifier)
                throw new InvalidOperationException("Invalid stream identifier found.");
        }

    }
}
