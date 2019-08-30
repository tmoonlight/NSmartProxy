using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NSmartProxy.Infrastructure
{
    public static class TcpPipelinesExt
    {
        private static async Task ProcessLinesAsync(this Socket socket, Action<ReadOnlyMemory<byte>> proc)
        {
            Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");
            var pipe = new Pipe();
            Task writing = FillPipeAsync(socket, pipe.Writer);
            Task reading = ReadPipeAsync(socket, pipe.Reader, proc);

            await Task.WhenAll(reading, writing);
            Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
        }

        private static async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                try
                {
                    // 从pipeWriter请求512字节长度的内存
                    Memory<byte> memory = writer.GetMemory(minimumBufferSize);

                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    // 移动游标到bytesRead的位置
                    writer.Advance(bytesRead);
                }
                catch
                {
                    break;
                }

                // 清空writer缓冲区
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // 标记writer完成
            writer.Complete();
        }

        private static async Task ReadPipeAsync(Socket socket, PipeReader reader, Action<ReadOnlyMemory<byte>> proc)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync();

                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    // Find the EOL
                    position = buffer.PositionOf((byte)'\n');

                    if (position != null)
                    {
                        var line = buffer.Slice(0, position.Value);
                        ProcessLine(socket, line, proc);

                        // 获取下一个内存位置
                        var next = buffer.GetPosition(1, position.Value);

                        // 游标前进到下一个字节的位置 \n
                        buffer = buffer.Slice(next);
                    }
                }
                while (position != null);

                // 移动游标到buffer.start，并且读取到buffer.end
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            //标记reader完成
            reader.Complete();
        }

        private static void ProcessLine(Socket socket, in ReadOnlySequence<byte> buffer, Action<ReadOnlyMemory<byte>> proc)
        {
            if (proc != null)
            {
                foreach (var segment in buffer)
                {
                    proc(segment);
                }
            }
        }

        public static Task<int> ReceiveAsync(this Socket socket, Memory<byte> memory, SocketFlags socketFlags)
        {
            var arraySegment = GetArray(memory);
            return SocketTaskExtensions.ReceiveAsync(socket, arraySegment, socketFlags);
        }

        private static ArraySegment<byte> GetArray(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }
            
            return result;
        }

#if NET461
    internal static class Extensions
    {
        public static Task<int> ReceiveAsync(this Socket socket, Memory<byte> memory, SocketFlags socketFlags)
        {
            var arraySegment = GetArray(memory);
            return SocketTaskExtensions.ReceiveAsync(socket, arraySegment, socketFlags);
        }

        public static string GetString(this Encoding encoding, ReadOnlyMemory<byte> memory)
        {
            var arraySegment = GetArray(memory);
            return encoding.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
        }

        private static ArraySegment<byte> GetArray(Memory<byte> memory)
        {
            return GetArray((ReadOnlyMemory<byte>)memory);
        }

        private static ArraySegment<byte> GetArray(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }
    }
#endif
    }
}
