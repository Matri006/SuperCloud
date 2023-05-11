using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Newtonsoft.Json;

namespace General
{
    public static class TcpClientExtensions
    {
        public static byte[] Receive(this TcpClient client, int bufferSize = 1024 * 1024 * 10)
        {
            var stream = client.GetStream();
            var result = new List<byte>();
            var buffer = new byte[bufferSize];
            var received = 0;

            while (stream.DataAvailable) {
                received = stream.Read(buffer, 0, bufferSize);
                result.AddRange(buffer.Take(received));
            }

            return result.ToArray();
        }

        public static byte[] ReceiveUntil(this TcpClient client)
        {
            var result = Array.Empty<byte>();

            while (!result.Any()) {
                result = client.Receive();
            }

            return result;
        }

        public static int ReceiveCodeUntil(this TcpClient client)
        {
            var stream = client.GetStream();

            var result = 0;

            while (client.Connected) {
                try {
                    result = stream.ReadByte();
                }
                catch {
                    return -1;
                }

                if (result == -1) {
                    continue;
                }

                return (byte)result;
            }

            return -1;
        }

        public static T ReceiveJson<T>(this TcpClient client) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(client.ReceiveUntil()));

        public static void Send(this TcpClient client, byte[] message)
        {
            if (!client.Connected)
                return;

            client.GetStream().Write(message, 0, message.Length);
        }

        public static void SendCode(this TcpClient client, byte code) => client.Send(new[] {code});
        
        public static void SendJson(this TcpClient client, object obj) => client.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));
    }
}