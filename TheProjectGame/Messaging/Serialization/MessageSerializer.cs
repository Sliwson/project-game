using Messaging.Contracts;
using Messaging.Serialization.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Messaging.Serialization
{
    public static class MessageSerializer
    {
        #region Serialization

        public static byte[] SerializeAndWrapMessage(BaseMessage message)
        {
            return WrapSerializedMessage(SerializeMessage(message));
        }

        public static byte[] WrapSerializedMessage(string serializedMessage)
        {
            var messageData = Encoding.UTF8.GetBytes(serializedMessage);
            var messageLength = (short)messageData.Length;

            var data = BitConverter.GetBytes(messageLength);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data);

            Array.Resize(ref data, messageLength + 2);
            Array.Copy(messageData, 0, data, 2, messageLength);

            return data;
        }

        public static string SerializeMessage(BaseMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }

        #endregion

        #region Deserialization

        public static BaseMessage UnwrapAndDeserializeMessage(byte[] wrappedMessage)
        {
            return DeserializeMessage(UnwrapMessage(wrappedMessage));
        }

        public static BaseMessage DeserializeMessage(string serializedMessage)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = { new PayloadTypeConverter() },
            };

            return JsonConvert.DeserializeObject<BaseMessage>(serializedMessage, settings);
        }

        public static string UnwrapMessage(byte[] wrappedMessage)
        {
            string serializedMessage;

            var littleEndianBytes = new byte[2];
            Array.Copy(wrappedMessage, littleEndianBytes, 2);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(littleEndianBytes);

            var messageLength = BitConverter.ToInt16(littleEndianBytes, 0);

            if (messageLength <= (1 << 13) - 2)
            {
                serializedMessage = Encoding.UTF8.GetString(wrappedMessage, 2, messageLength);

                return serializedMessage;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Received message was too long (expected maximum { 1 >> 13 }, got {messageLength + 2})");
            }
        }

        #endregion


    }
}
