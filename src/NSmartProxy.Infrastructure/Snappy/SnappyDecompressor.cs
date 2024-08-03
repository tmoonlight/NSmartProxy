using System;
using System.Diagnostics;

namespace Snappy.Sharp
{
    public class SnappyDecompressor
    {
        private const int MAX_INCREMENT_COPY_OVERFLOW = 10;
        private const int bitMask = 0x80;

        public int[] ReadUncompressedLength(byte[] data, int offset)
        {
            int sum = 0, currentShift = 0;
            while ((data[offset] & bitMask) != 0)
            {
                sum = UpdateSum(data, offset, currentShift, sum);
                offset++;
                currentShift += 7;
            }
            sum = UpdateSum(data, offset, currentShift, sum);
            offset++;
            return new[] { sum, offset };
        }

        private static int UpdateSum(byte[] data, int offset, int currentShift, int sum)
        {
            int nextValue = data[offset] & (bitMask - 1);
            nextValue <<= currentShift;
            sum += nextValue;
            return sum;
        }

        public byte[] Decompress(byte[] compressed, int compressedOffset, int compressedSize)
        {
            var sizeHeader = ReadUncompressedLength(compressed, compressedOffset);
            var data = new byte[sizeHeader[0]];

            Decompress(compressed, sizeHeader[1], compressedSize + compressedOffset - sizeHeader[1], data, 0, data.Length);

            return data;
        }
        public int Decompress(byte[] input, int inputOffset, int inputSize, byte[] output, int outputOffset, int outputLimit)
        {
            int ipLimit = inputOffset + inputSize;
            int opIndex = outputOffset;
            int ipIndex = inputOffset;

            while (ipIndex < ipLimit - 5)
            {
                byte opCode = input[ipIndex++];
                ushort entry = opLookupTable[opCode];
                byte trailerBytes = (byte) (entry >> 11);

                uint trailer = (BitConverter.ToUInt32(input, ipIndex) & wordmask[trailerBytes]);

                // advance the ipIndex past the op codes
                ipIndex += entry >> 11;
                int length = entry & 0xff;

                if ((opCode & 0x3) == Snappy.LITERAL)
                {
                    int literalLength = (int) (length + trailer);
                    CopyLiteral(input, ipIndex, output, opIndex, literalLength);
                    ipIndex += literalLength;
                    opIndex += literalLength;
                }
                else
                {
                    // copyOffset/256 is encoded in bits 8..10.  By just fetching
                    // those bits, we get copyOffset (since the bit-field starts at
                    // bit 8).
                    int copyOffset = entry & 0x700;
                    copyOffset += (int)trailer;

                    // inline to force hot-spot to keep inline
                    //
                    // Equivalent to incrementalCopy (below) except that it can write up to ten extra
                    // bytes after the end of the copy, and that it is faster.
                    //
                    // The main part of this loop is a simple copy of eight bytes at a time until
                    // we've copied (at least) the requested amount of bytes.  However, if op and
                    // src are less than eight bytes apart (indicating a repeating pattern of
                    // length < 8), we first need to expand the pattern in order to get the correct
                    // results. For instance, if the buffer looks like this, with the eight-byte
                    // <src> and <op> patterns marked as intervals:
                    //
                    //    abxxxxxxxxxxxx
                    //    [------]           src
                    //      [------]         op
                    //
                    // a single eight-byte copy from <src> to <op> will repeat the pattern once,
                    // after which we can move <op> two bytes without moving <src>:
                    //
                    //    ababxxxxxxxxxx
                    //    [------]           src
                    //        [------]       op
                    //
                    // and repeat the exercise until the two no longer overlap.
                    //
                    // This allows us to do very well in the special case of one single byte
                    // repeated many times, without taking a big hit for more general cases.
                    //
                    // The worst case of extra writing past the end of the match occurs when
                    // op - src == 1 and len == 1; the last copy will read from byte positions
                    // [0..7] and write to [4..11], whereas it was only supposed to write to
                    // position 1. Thus, ten excess bytes.
                    // TODO: Java inlined this function manually. Investigate.
                    CopyCopy(output, length, opIndex, outputLimit, copyOffset);
                    opIndex += length;
                }
            }

            for (; ipIndex < ipLimit; ) {
                int[] result = DecompressTagSlow(input, ipIndex, output, outputLimit, outputOffset, opIndex);
                ipIndex = result[0];
                opIndex = result[1];
            }

            return opIndex;
        }

        /**
         * NOTE: from Java version, need to determine if true in .Net as well.
         * This is a second copy of the inner loop of decompressTags used when near the end
         * of the input. The key difference is the reading of the trailer bytes.  The fast
         * code does a blind read of the next 4 bytes as an int, and this code assembles
         * the int byte-by-byte to assure that the array is not over run.  The reason this
         * code path is separate is the if condition to choose between these two seemingly
         * small differences costs like 10-20% of the throughput.  I'm hoping in future
         * versions of hot-spot this code can be integrated into the main loop but for now
         * it is worth the extra maintenance pain to get the extra 10-20%.
         */
        private static int[] DecompressTagSlow(byte[] input, int ipIndex, byte[] output, int outputLimit, int outputOffset, int opIndex)
        {
            // read the op code
            byte opCode = input[ipIndex++];
            ushort entry = opLookupTable[opCode];
            byte trailerBytes = (byte) (entry >> 11);
            //
            // Key difference here
            //
            uint trailer = 0;
            if (trailerBytes >= 4)
                trailer = (uint) ((input[ipIndex + 3] & 0xff) << 24);
            if (trailerBytes >= 3)
                trailer |= (uint) ((input[ipIndex + 2] & 0xff) << 16);
            if (trailerBytes >= 2)
                trailer |= (uint) ((input[ipIndex + 1] & 0xff) << 8);
            if (trailerBytes >= 1)
                trailer |= (uint) (input[ipIndex] & 0xff);

            // advance the ipIndex past the op codes
            ipIndex += trailerBytes;
            int length = entry & 0xff;

            if ((opCode & 0x3) == Snappy.LITERAL) {
                int literalLength = (int) (length + trailer);
                CopyLiteral(input, ipIndex, output, opIndex, literalLength);
                ipIndex += literalLength;
                opIndex += literalLength;
            }
            else {
                // copyOffset/256 is encoded in bits 8..10.  By just fetching
                // those bits, we get copyOffset (since the bit-field starts at
                // bit 8).
                int copyOffset = entry & 0x700;
                copyOffset += (int)trailer;

                // TODO: Java inlined this function manually. Investigate.
                CopyCopy(output, length, opIndex, outputLimit, copyOffset);
                opIndex += length;
            }
            return new int[] {ipIndex, opIndex};
        }

        private static void CopyLiteral(byte[] input, int ipIndex, byte[] output, int opIndex, int length)
        {
            Debug.Assert(length > 0);
            Debug.Assert(ipIndex >= 0);
            Debug.Assert(opIndex >= 0);

            int spaceLeft = output.Length - opIndex;
            int readableBytes = input.Length - ipIndex; 

            if (readableBytes < length || spaceLeft < length)
            {
                throw new InvalidOperationException("Corrupt literal length");
            }
            if (length <= 16 && spaceLeft >= 16 && readableBytes >= 16)
            {
                Utilities.UnalignedCopy64(input, ipIndex, output, opIndex);
                Utilities.UnalignedCopy64(input, ipIndex + 8, output, opIndex + 8);
            }
            else
            {
                int fastLength = (int)(length & 0xFFFFFFF8);
                if (fastLength <= 64)
                {
                    // copy long-by-long
                    for (int i = 0; i < fastLength; i += 8)
                    {
                        Utilities.UnalignedCopy64(input, ipIndex + i, output, opIndex + i);
                    }

                    // copy byte-by-byte
                    int slowLength = length & 0x7;
                    // NOTE: This is not a manual array copy.  We are copying an overlapping region
                    // and we want input data to repeat as it is recopied. see incrementalCopy below.
                    for (int i = 0; i < slowLength; i += 1)
                    {
                        output[opIndex + fastLength + i] = input[ipIndex + fastLength + i];
                    }
                }
                else
                {
                    Buffer.BlockCopy(input, ipIndex, output, opIndex, length);
                }
            }
        }

        private static void CopyCopy(byte[] output, int length, int opIndex, int outputLimit, int copyOffset)
        {
            int spaceLeft = outputLimit - opIndex;
            int srcIndex = opIndex - copyOffset;

            if (length <= 16 && copyOffset >= 8 && spaceLeft >= 16)
            {
                // Fast path, used for the majority (70-80%) of dynamic invocations.
                Utilities.UnalignedCopy64(output, srcIndex, output, opIndex);
                Utilities.UnalignedCopy64(output, srcIndex + 8, output, opIndex + 8);
            }
            else if (spaceLeft >= length + MAX_INCREMENT_COPY_OVERFLOW)
            {
                IncrementalCopyFastPath(output, srcIndex, opIndex, length);
            }
            else
            {
                IncrementalCopy(output, srcIndex, output, opIndex, length);
            }
        }

        /**
         * Copy "len" bytes from "src" to "op", one byte at a time.  Used for
         * handling COPY operations where the input and output regions may
         * overlap.  For example, suppose:
         * src    == "ab"
         * op     == src + 2
         * len    == 20
         *
         * After incrementalCopy, the result will have
         * eleven copies of "ab"
         * ababababababababababab
         * Note that this does not match the semantics of either memcpy()
         * or memmove().
         */
        private static void IncrementalCopy(byte[] source, int srcIndex, byte[] output, int opIndex, int length)
        {
            Debug.Assert(source != null);
            Debug.Assert(output != null);
            // do <= because we do a postfix increment index but a prefix decrement of length
            Debug.Assert(srcIndex + length <= source.Length);
            Debug.Assert(opIndex + length <= output.Length);

            do
            {
                output[opIndex++] = source[srcIndex++];
            } while (--length > 0);
        }

        private static void IncrementalCopyFastPath(byte[] output, int srcIndex, int opIndex, int length)
        {
            int copiedLength = 0;
            while ((opIndex + copiedLength) - srcIndex < 8)
            {
                Utilities.UnalignedCopy64(output, srcIndex, output, opIndex + copiedLength);
                copiedLength += (opIndex + copiedLength) - srcIndex;
            }

            for (int i = 0; i < length - copiedLength; i += 8)
            {
                Utilities.UnalignedCopy64(output, srcIndex + i, output, opIndex + copiedLength + i);
            }
        }

        // Mapping from i in range [0,4] to a mask to extract the bottom 8*i bits
        private static readonly uint[] wordmask = new uint[]{
                0, 0xff, 0xffff, 0xffffff, 0xffffffff
        };

        // Data stored per entry in lookup table:
        //      Range   Bits-used       Description
        //      ------------------------------------
        //      1..64   0..7            Literal/copy length encoded in opcode byte
        //      0..7    8..10           Copy offset encoded in opcode byte / 256
        //      0..4    11..13          Extra bytes after opcode
        //
        // We use eight bits for the length even though 7 would have sufficed
        // because of efficiency reasons:
        //      (1) Extracting a byte is faster than a bit-field
        //      (2) It properly aligns copy offset so we do not need a <<8
        private static readonly ushort[] opLookupTable = new ushort[]{
                0x0001, 0x0804, 0x1001, 0x2001, 0x0002, 0x0805, 0x1002, 0x2002,
                0x0003, 0x0806, 0x1003, 0x2003, 0x0004, 0x0807, 0x1004, 0x2004,
                0x0005, 0x0808, 0x1005, 0x2005, 0x0006, 0x0809, 0x1006, 0x2006,
                0x0007, 0x080a, 0x1007, 0x2007, 0x0008, 0x080b, 0x1008, 0x2008,
                0x0009, 0x0904, 0x1009, 0x2009, 0x000a, 0x0905, 0x100a, 0x200a,
                0x000b, 0x0906, 0x100b, 0x200b, 0x000c, 0x0907, 0x100c, 0x200c,
                0x000d, 0x0908, 0x100d, 0x200d, 0x000e, 0x0909, 0x100e, 0x200e,
                0x000f, 0x090a, 0x100f, 0x200f, 0x0010, 0x090b, 0x1010, 0x2010,
                0x0011, 0x0a04, 0x1011, 0x2011, 0x0012, 0x0a05, 0x1012, 0x2012,
                0x0013, 0x0a06, 0x1013, 0x2013, 0x0014, 0x0a07, 0x1014, 0x2014,
                0x0015, 0x0a08, 0x1015, 0x2015, 0x0016, 0x0a09, 0x1016, 0x2016,
                0x0017, 0x0a0a, 0x1017, 0x2017, 0x0018, 0x0a0b, 0x1018, 0x2018,
                0x0019, 0x0b04, 0x1019, 0x2019, 0x001a, 0x0b05, 0x101a, 0x201a,
                0x001b, 0x0b06, 0x101b, 0x201b, 0x001c, 0x0b07, 0x101c, 0x201c,
                0x001d, 0x0b08, 0x101d, 0x201d, 0x001e, 0x0b09, 0x101e, 0x201e,
                0x001f, 0x0b0a, 0x101f, 0x201f, 0x0020, 0x0b0b, 0x1020, 0x2020,
                0x0021, 0x0c04, 0x1021, 0x2021, 0x0022, 0x0c05, 0x1022, 0x2022,
                0x0023, 0x0c06, 0x1023, 0x2023, 0x0024, 0x0c07, 0x1024, 0x2024,
                0x0025, 0x0c08, 0x1025, 0x2025, 0x0026, 0x0c09, 0x1026, 0x2026,
                0x0027, 0x0c0a, 0x1027, 0x2027, 0x0028, 0x0c0b, 0x1028, 0x2028,
                0x0029, 0x0d04, 0x1029, 0x2029, 0x002a, 0x0d05, 0x102a, 0x202a,
                0x002b, 0x0d06, 0x102b, 0x202b, 0x002c, 0x0d07, 0x102c, 0x202c,
                0x002d, 0x0d08, 0x102d, 0x202d, 0x002e, 0x0d09, 0x102e, 0x202e,
                0x002f, 0x0d0a, 0x102f, 0x202f, 0x0030, 0x0d0b, 0x1030, 0x2030,
                0x0031, 0x0e04, 0x1031, 0x2031, 0x0032, 0x0e05, 0x1032, 0x2032,
                0x0033, 0x0e06, 0x1033, 0x2033, 0x0034, 0x0e07, 0x1034, 0x2034,
                0x0035, 0x0e08, 0x1035, 0x2035, 0x0036, 0x0e09, 0x1036, 0x2036,
                0x0037, 0x0e0a, 0x1037, 0x2037, 0x0038, 0x0e0b, 0x1038, 0x2038,
                0x0039, 0x0f04, 0x1039, 0x2039, 0x003a, 0x0f05, 0x103a, 0x203a,
                0x003b, 0x0f06, 0x103b, 0x203b, 0x003c, 0x0f07, 0x103c, 0x203c,
                0x0801, 0x0f08, 0x103d, 0x203d, 0x1001, 0x0f09, 0x103e, 0x203e,
                0x1801, 0x0f0a, 0x103f, 0x203f, 0x2001, 0x0f0b, 0x1040, 0x2040
        };
    }
}
