using System.IO;
using System.Threading.Tasks;

namespace NSmartProxy.Infrastructure
{
    public static class StreamExtension
    {
        public static async Task WriteAndFlushAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            await stream.WriteAsync(buffer, offset, count);
            await stream.FlushAsync();
        }
    }
}