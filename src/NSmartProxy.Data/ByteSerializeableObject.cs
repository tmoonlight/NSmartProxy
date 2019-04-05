namespace NSmartProxy.Data
{
    public abstract class ByteSerializeableObject
    {
        public abstract byte[] ToBytes();
        /// <summary>
        /// 整型转双字节
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public byte[] IntTo2Bytes(int number)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(number / 256);
            bytes[1] = (byte)(number % 256);
            return bytes;
        }

        /// <summary>
        /// 双字节转整型
        /// </summary>
        /// <param name="hByte"></param>
        /// <param name="lByte"></param>
        /// <returns></returns>
        public static int DoubleBytesToInt(byte hByte, byte lByte)
        {
            return (hByte << 8) + lByte;
        }
    }
}