using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCInfoParser_Server_NET_Forms
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static int Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            return 0;
        }
    }

    public class AsyncTcpServer
    {
        TcpClient client = new();
        bool start = false;
        private readonly int port;
        private readonly TcpListener listener;
        static string password = "12345678";
        private readonly ConcurrentDictionary<int, TcpClient> clients = new ConcurrentDictionary<int, TcpClient>();
        private int nextClientId = 0;
        Dictionary<int, string[]> clientsinfo = new();
        string[,] clientsinfocheck = new string[400000, 3];

        public AsyncTcpServer(int port)
        {
            this.port = port;
            listener = new TcpListener(IPAddress.Any, port);
        }

        public async void StartAsync()
        {
            start = true;
            listener.Start();
            Console.WriteLine($"Server started listening on port {port}.");
            while (start)
            {
                try { 
                    client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");

                    int clientId = nextClientId++;
                    clients.TryAdd(clientId, client);

                    _ = Task.Run(() => HandleClientAsync(clientId));
                }
                catch{ Console.WriteLine(); }
            }

        }

        public Dictionary<int, string[]> GetClients()
        {
            return this.clientsinfo;
        }

        private async Task HandleClientAsync(int clientId)
        {
            TcpClient client = clients[clientId];
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (client.Connected)
            {
                int bytesRead = await ReadDataAsync(stream, buffer);
                if (bytesRead == 0) break;

                // Process the data received from the client
                string cipherText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string data = Cryptography.Decrypt(cipherText, password);
                Console.WriteLine($"Received data from client {clientId}: {data}");
                if (data.StartsWith("VALIDATION"))
                {
                    clientsinfo.Add(clientId, data.Split(';'));
                    string responseMessage = "VALID;0";
                    string encryptedMessage = Cryptography.Encrypt(responseMessage, password);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(encryptedMessage);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    await stream.FlushAsync();
                    //StartSendingMessages(client, clientId);
                }
                // Echo the data back to the client
                else if (data.StartsWith("CHECKCON")) { }
                else
                {
                    Console.WriteLine(data);
                    //CloseClientConnection(clientId);
                }
                await Task.Delay(1000);
            }
            if (!client.Connected)
                Console.WriteLine($"Client {clientId} disconnected from {client.Client.RemoteEndPoint}");
        }

        private async Task<int> ReadDataAsync(NetworkStream stream, byte[] buffer)
        {
            return await stream.ReadAsync(buffer, 0, buffer.Length);
        }

        private async Task WriteDataAsync(NetworkStream stream, byte[] buffer, int length)
        {
            await stream.WriteAsync(buffer, 0, length);
            await stream.FlushAsync();
        }

        public void CloseClientConnection(int clientId)
        {
            if (clients.TryGetValue(clientId, out TcpClient client))
            {
                Console.WriteLine($"Closing connection with client {clientId}");
                client.Close();
                clients.TryRemove(clientId, out _);
                clientsinfo.Remove(clientId);
            }
        }
        public async void StartSendingMessages(TcpClient client, int clientId)
        {
            await Task.Delay(1000);
            // Запуск таймера, который будет запускать метод отправки сообщения каждые 5 секунд
            while (true)
            {
                try
                {
                    await SendMessageToClient("CHECKCON", client, clientId);
                    await Task.Delay(10000);
                }
                catch (Exception) {
                    CloseClientConnection(clientId);
                    break;
                }
            }
        }

        public async Task<bool> SendMessageToClient(string message, TcpClient client, int clientId)
        {
            // Отправка сообщения на всех клиентов
            try
            {
                NetworkStream stream = client.GetStream();
                string encryptedMessage = Cryptography.Encrypt(message, password);
                byte[] responseBytes = Encoding.UTF8.GetBytes(encryptedMessage);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine($"Client disconnected from {client.Client.RemoteEndPoint}");
                client.Close();
                CloseClientConnection(clientId);
                return false;
            }
        }
        public void StopServer()
        {
            Console.WriteLine("Stopping server...");
            start = false;
            listener.Stop();
            Task.Delay(2000);
            foreach (int clientId in clients.Keys)
            {
                CloseClientConnection(clientId);
            }
            

        }
    }


    public class Cryptography
    {
        private static readonly byte[] Salt = Encoding.ASCII.GetBytes("SaltySalty");

        public static string Encrypt(string plainText, string password)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = new Rfc2898DeriveBytes(password, Salt).GetBytes(32);
            byte[] ivBytes = new Rfc2898DeriveBytes(password, Salt).GetBytes(16);

            using Aes aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            using var encryptor = aes.CreateEncryptor();
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string cipherText, string password)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] keyBytes = new Rfc2898DeriveBytes(password, Salt).GetBytes(32);
            byte[] ivBytes = new Rfc2898DeriveBytes(password, Salt).GetBytes(16);

            using Aes aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static string[,] Encrypt(string[,] plainTextArray, string password)
        {
            int rowCount = plainTextArray.GetLength(0);
            int columnCount = plainTextArray.GetLength(1);

            string[,] cipherTextArray = new string[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    cipherTextArray[i, j] = Encrypt(plainTextArray[i, j], password);
                }
            }

            return cipherTextArray;
        }

        public static string[,] Decrypt(string[,] cipherTextArray, string password)
        {
            int rowCount = cipherTextArray.GetLength(0);
            int columnCount = cipherTextArray.GetLength(1);

            string[,] plainTextArray = new string[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    plainTextArray[i, j] = Decrypt(cipherTextArray[i, j], password);
                }
            }

            return plainTextArray;
        }

        public static string[,,] Encrypt(string[,,] plainTextArray, string password)
        {
            int rowCount = plainTextArray.GetLength(0);
            int columnCount = plainTextArray.GetLength(1);
            int depthCount = plainTextArray.GetLength(2);

            string[,,] cipherTextArray = new string[rowCount, columnCount, depthCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    for (int k = 0; k < depthCount; k++)
                    {
                        cipherTextArray[i, j, k] = Encrypt(plainTextArray[i, j, k], password);
                    }
                }
            }
            return cipherTextArray;
        }

        public static string[,,] Decrypt(string[,,] cipherTextArray, string password)
        {
            int rowCount = cipherTextArray.GetLength(0);
            int columnCount = cipherTextArray.GetLength(1);
            int depthCount = cipherTextArray.GetLength(2);

            string[,,] plainTextArray = new string[rowCount, columnCount, depthCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    for (int k = 0; k < depthCount; k++)
                    {
                        plainTextArray[i, j, k] = Decrypt(cipherTextArray[i, j, k], password);
                    }
                }
            }

            return plainTextArray;
        }
    }
}
