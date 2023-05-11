using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using General;

namespace Server
{
    public sealed class Server : IDisposable
    {
        private readonly TcpListener _server;
        private readonly CancellationTokenSource _token = new CancellationTokenSource();
        
        public Server(IPAddress address, int port)
        {
            _server = new TcpListener(address, port);
        }

        ~Server()
        {
            Stop();
        }

        public Dictionary<byte, Action<TcpClient>> Actions { get; } = new Dictionary<byte, Action<TcpClient>>();

        public void Start()
        {
            _server.Start();

            Task.Run(async () => {
                         while (true) {
                             var client = await _server.AcceptTcpClientAsync();

                             if (client == null) {
                                 continue;
                             }

                             await Task.Run(() => {
                                 var ip = client.Client.RemoteEndPoint;
                                 //Console.WriteLine($"New connect: {ip}");
                                 HandleClient(client);
                                 //Console.WriteLine($"Disconnect: {ip}");
                             }, _token.Token);
                         }
                     },
                     _token.Token);
            
            //Console.WriteLine("Server started");
            //Console.WriteLine($"IP: {_server.LocalEndpoint}");
        }

        public void Stop()
        {
            _token.Cancel();
            _server.Stop();
            
            //Console.WriteLine("Server stopped");
        }

        public void Dispose()
        {
            Stop();
            _token.Dispose();
            GC.SuppressFinalize(this);
        }
        
        private void HandleClient(TcpClient client)
        {
            int code;
            
            while (client.Connected) {
                code = client.ReceiveCodeUntil();

                if (code == -1) {
                    break;
                }
                
                //Console.WriteLine($"Code: {code}");
            
                if (Actions.TryGetValue((byte)code, out var action)) {
                    action?.Invoke(client);
                }
                else {
                    //Console.WriteLine("Invalid");
                }
            }
        }
    }
}