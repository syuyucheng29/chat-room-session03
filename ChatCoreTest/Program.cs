using System;
using System.Text;

namespace ChatCoreTest
{
    enum DataType
    {
        isInt,
        isFloat,
        isString
    }
    internal class Program
    {
        private static byte[] m_PacketData;
        private static uint m_Pos;
        private static int r_Pos;

        public static void Main(string[] args)
        {
            m_PacketData = new byte[1024];
            m_Pos = 0;
            r_Pos = 0;

            Write(109);
            Write(109.99f);
            Write("Hello!");

            Console.Write($"Output Byte array(length:{m_Pos}): ");
            for (var i = 0; i < m_Pos; i++)
            {
                Console.Write(m_PacketData[i] + ", ");
            }
            Console.Write("\n\n");
            Console.WriteLine($"Read: ");
            while (Read(m_PacketData));
            Console.Write("\n\n");
        }

        private static bool Read(byte[] message)
        {
            if (r_Pos >= m_Pos) return false;
            int byte4 = 4;
            byte type = message[r_Pos];
            r_Pos++;
            if (type == (byte)DataType.isInt)
            {
                Console.WriteLine($"{"(int)",8:g} message: {BitConverter.ToInt32(GetSection(message, r_Pos, byte4), 0)}");
                r_Pos += byte4;
            }
            else if (type == (byte)DataType.isFloat)
            {
                Console.WriteLine($"{"(float)",8:g} message: {BitConverter.ToSingle(GetSection(message, r_Pos, byte4), 0)}");
                r_Pos += byte4;
            }
            else if (type == (byte)DataType.isString)
            {
                r_Pos ++; //skip int tag
                int messageLength = BitConverter.ToInt32(GetSection(message, r_Pos, byte4), 0);
                r_Pos += byte4;
                string get = Encoding.Unicode.GetString(GetSection(message, r_Pos, messageLength), 0, messageLength);
                Console.WriteLine($"{"(string)",8:g} message: {get}");
                r_Pos += messageLength;
            }
            return true;
        }
        private static byte[] GetSection(byte[] data, int start, int count)
        {
            byte[] result = new byte[count];

            for (int i = 0; i < count; i++)
                result[i] = data[start + i];
            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }
        // write an integer into a byte array
        private static bool Write(int i)
        {
            // convert int to byte array
            var bytes = BitConverter.GetBytes(i);
            _Write(new byte[] { (byte)DataType.isInt });
            _Write(bytes);
            return true;
        }

        // write a float into a byte array
        private static bool Write(float f)
        {
            // convert int to byte array
            var bytes = BitConverter.GetBytes(f);
            _Write(new byte[] { (byte)DataType.isFloat });
            _Write(bytes);
            return true;
        }

        // write a string into a byte array
        private static bool Write(string s)
        {
            // convert string to byte array
            var bytes = Encoding.Unicode.GetBytes(s);
            _Write(new byte[] { (byte)DataType.isString });
            // write byte array length to packet's byte array
            if (Write(bytes.Length) == false)
                return false;
            _Write(bytes);
            return true;
        }

        // write a byte array into packet's byte array
        private static void _Write(byte[] byteData)
        {
            // converter little-endian to network's big-endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(byteData);
            byteData.CopyTo(m_PacketData, m_Pos);
            m_Pos += (uint)byteData.Length;
        }
    }
}
