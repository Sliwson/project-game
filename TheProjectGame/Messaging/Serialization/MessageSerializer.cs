using Messaging.Contracts;
using Messaging.Serialization.JsonConverters;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Serialization
{
    public static class MessageSerializer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

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

        public static IEnumerable<BaseMessage> UnwrapAndDeserializeMessages(byte[] wrappedMessage, int bytesRead, bool throwIfNoAgentId = false)
        {
            foreach(var serializedMessage in UnwrapMessages(wrappedMessage, bytesRead))
            {
                logger.Debug("[ClientNetworkComponent] Received: {message}", serializedMessage);
                yield return DeserializeMessage(serializedMessage, throwIfNoAgentId);
            }
        }

        public static BaseMessage DeserializeMessage(string serializedMessage, bool throwIfNoAgentId = false)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = { new PayloadTypeConverter(throwIfNoAgentId) },
            };

            return JsonConvert.DeserializeObject<BaseMessage>(serializedMessage, settings);
        }

        public static IEnumerable<string> UnwrapMessages(byte[] wrappedMessage, int bytesRead)
        {
            string serializedMessage;
            int offset = 0;
            var littleEndianBytes = new byte[2];

            while (offset < bytesRead)
            {
                Array.Copy(wrappedMessage, offset, littleEndianBytes, 0, 2);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(littleEndianBytes);

                var messageLength = BitConverter.ToInt16(littleEndianBytes, 0);

                if (messageLength <= (1 << 13) - 2)
                {
                    serializedMessage = Encoding.UTF8.GetString(wrappedMessage, offset + 2, messageLength);
                    offset += (messageLength + 2);
                    yield return serializedMessage;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Received message was too long (expected maximum { 1 << 13 }, got {messageLength + 2})");
                }
            }
        }

        #endregion


    }
}
