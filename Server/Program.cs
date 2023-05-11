using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using General;

using SQLite;

namespace Server
{
    public enum Commands
    {
        CreateTable = 1,
        CheckData = 2,
        AddUser = 3,
        DeleteUser = 4,
        CheckDbExists = 5,
        GetUsers = 6,
        GetFiles = 7,
        DeleteFile = 8,
        CheckFolderExists = 9,
        SendFile = 10,
        CheckFileExists = 11,
        LoadFile = 12,
        CloseConnect = 13,
    }

    internal class Program
    {
        private const string DbPath = "data.sqlite";

        private static readonly SQLiteConnection _connection;
        private static readonly Server _server;

        static Program()
        {
            if (!File.Exists(DbPath)) {
                File.WriteAllBytes(DbPath, Array.Empty<byte>());
            }

            _connection = new SQLiteConnection(DbPath);
            _connection.CreateTable<TableWorker>();

            _server = new Server(IPAddress.Any, 5000);
        }

        private static void Main(string[] args)
        {
            _server.Actions.Add((int) Commands.CreateTable, CreateTablePasswords);
            _server.Actions.Add((int) Commands.CheckData, CheckLoginData);
            _server.Actions.Add((int) Commands.AddUser, AddUser);
            _server.Actions.Add((int) Commands.DeleteUser, DeleteUser);
            _server.Actions.Add((int) Commands.CheckDbExists, CheckDbEmpty);
            _server.Actions.Add((int) Commands.GetUsers, GetUsers);
            _server.Actions.Add((int) Commands.GetFiles, GetFiles);
            _server.Actions.Add((int) Commands.DeleteFile, DeleteFile);
            _server.Actions.Add((int) Commands.CheckFolderExists, CheckFolderExists);
            _server.Actions.Add((int) Commands.SendFile, SendFile);
            _server.Actions.Add((int) Commands.CheckFileExists, CheckFileExists);
            _server.Actions.Add((int) Commands.LoadFile, LoadFile);
            _server.Actions.Add((int)Commands.CloseConnect, CloseConnect);

            _server.Start();
            Console.ReadKey();
            _server.Stop();
        }

        private static void CreateTablePasswords(TcpClient client)
        {
            var data = client.ReceiveJson<Worker>();
            
            if (_connection.Table<TableWorker>().Any()) {
                return;
            }

            data.Type = "admin";
            
            _connection.Insert(data, typeof(TableWorker));
        }
        
        private static void CheckLoginData(TcpClient client)
        {
            var data = client.ReceiveJson<Worker>();
        
            var worker = _connection.Table<TableWorker>().ToArray().FirstOrDefault(x => Equals(x, data));
        
            client.SendCode((byte)(worker == null ? 0 : 1));
        }
        
        private static void AddUser(TcpClient client)
        {
            var data = client.ReceiveJson<Worker>();
        
            var worker = _connection.Table<TableWorker>().ToArray().FirstOrDefault(x => Equals(x, data));
        
            if (worker != null) {
                client.SendCode(0);
                return;
            }
        
            Task.Run(() => _connection.Insert(data, typeof(TableWorker)));
            client.SendCode(1);
        }
        
        private static void DeleteUser(TcpClient client)
        {
            var data = client.ReceiveJson<Worker>();
        
            var worker = _connection.Table<TableWorker>().ToArray().FirstOrDefault(x => Equals(x, data));
        
            if (worker == null) {
                client.SendCode(0);
                return;
            }

            Task.Run(() => _connection.Delete<TableWorker>(worker.Id));
            client.SendCode(1);
        }
        
        private static void CheckDbEmpty(TcpClient client)
        {
            client.SendCode((byte)(_connection.Table<TableWorker>().Any() ? 0 : 1));
        }
        
        private static void GetUsers(TcpClient client)
        {
            client.SendJson(_connection.Table<TableWorker>().Cast<Worker>().ToArray());
        }

        private static void GetFiles(TcpClient client)
        {
            var s = new List<Share>();

            if (!Directory.Exists("publicfiles")) {
                Directory.CreateDirectory("publicfiles");
            }
            
            var dir = new DirectoryInfo("publicfiles");
            var dirs = dir.GetDirectories();
            var files = dir.GetFiles();

            foreach (var i in files) {
                var temp = new Share();
                temp.Name = i.Name;
                temp.Folder = "publicfiles";
                temp.Type = "file";
                s.Add(temp);
            }

            foreach (var i in dirs) {
                var temp = new Share();
                temp.Name = i.Name;
                temp.Folder = "publicfiles";
                temp.Type = "folder";
                s.Add(temp);
            }

            foreach (var i in dirs) {

                var f = i.GetFiles();

                foreach (var j in f) {
                    s.Add(new Share
                    {
                        Name = j.Name,
                        Folder = i.Name,
                        Type = "file"
                    });
                }
            }

            client.SendJson(s);
        }

        private static void DeleteFile(TcpClient client)
        {
            var data = client.ReceiveJson<Share>();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, data.Path);

            switch (data.Type) {
                case "file":
                    File.Delete(path);
                    break;

                case "folder":
                    Directory.Delete(path, true);
                    break;
            }
        }

        private static void CheckFolderExists(TcpClient client)
        {
            var res = "no";
            var name = Encoding.UTF8.GetString(client.Receive());
            var dir = new DirectoryInfo("publicfiles");
            var dirs = dir.GetDirectories();

            foreach (var i in dirs) {
                if (i.Name == name) {
                    res = "yes";
                    break;
                }
            }

            if (res == "no") {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "publicfiles", name);
                Directory.CreateDirectory(path);
            }
            
            client.Send(Encoding.UTF8.GetBytes(res));
        }

        private static void SendFile(TcpClient client)
        {
            var share = client.ReceiveJson<Share>();

            client.Send(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, share.Path)));
        }

        private static void CheckFileExists(TcpClient client)
        {
            var data = client.ReceiveJson<Share>();

            client.SendCode((byte)(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, data.Path)) ? 1 : 0));
        }
        
        private static void LoadFile(TcpClient client)
        {
            var data = client.ReceiveJson<Share>();

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, data.Path);

            if (File.Exists(filePath)) {
                client.SendCode(0);
                return;
            }
            
            client.SendCode(1);

            new FileInfo(filePath).Directory.Create();

            var fileData = client.ReceiveUntil();
            File.WriteAllBytes(filePath, fileData);
        }

        private static void CloseConnect(TcpClient client)
        {
            client.Close();
        }
    }
}