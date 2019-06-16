using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SeaBattle.Lib.Messaging
{
    public static class MessageFactory
    {
        private static BinaryFormatter Formatter { get; }
        
        static MessageFactory()
        {
            Formatter = new BinaryFormatter();
        }

        public static byte[] Serialize(this object obj)
        {
            using (var memory = new MemoryStream())
            {
                Formatter.Serialize(memory, obj);
                memory.Flush();
                return memory.ToArray();
            }
        }

        public static TE Deserialize<TE>(byte[] data)
        {
            using (var memory = new MemoryStream(data))
            {
                return (TE)Formatter.Deserialize(memory);
            }
        }
    }
}