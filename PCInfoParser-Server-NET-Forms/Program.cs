﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCInfoParser_Server_NET_Forms
{

    class ArrayStringConverter
    {
        private const string ArraySeparator = "@@";
        private const string ElementSeparator = "##";

        public static string ToString2D(string[,] arr)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);
            List<string> arrStrings = new List<string>(rows);
            for (int i = 0; i < rows; i++)
            {
                List<string> rowStrings = new List<string>(cols);
                for (int j = 0; j < cols; j++)
                {
                    string value = arr[i, j];
                    rowStrings.Add(EncodeValue(value));
                }
                arrStrings.Add(string.Join(ElementSeparator, rowStrings));
            }
            return string.Join(ArraySeparator, arrStrings);
        }

        public static string[,] FromString2D(string s)
        {
            string[] arrStrings = s.Split(new[] { ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            int rows = arrStrings.Length;
            int cols = arrStrings[0].Split(new[] { ElementSeparator }, StringSplitOptions.None).Length;
            string[,] arr = new string[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                string[] rowStrings = arrStrings[i].Split(new[] { ElementSeparator }, StringSplitOptions.None);
                for (int j = 0; j < cols; j++)
                {
                    arr[i, j] = DecodeValue(rowStrings[j]);
                }
            }
            return arr;
        }

        public static string ToString3D(string[,,] arr)
        {
            int depth = arr.GetLength(0);
            int rows = arr.GetLength(1);
            int cols = arr.GetLength(2);
            List<string> matrixStrings = new List<string>(depth);
            for (int k = 0; k < depth; k++)
            {
                List<string> arrStrings = new List<string>(rows);
                for (int i = 0; i < rows; i++)
                {
                    List<string> rowStrings = new List<string>(cols);
                    for (int j = 0; j < cols; j++)
                    {
                        string value = arr[k, i, j];
                        rowStrings.Add(EncodeValue(value));
                    }
                    arrStrings.Add(string.Join(ElementSeparator, rowStrings));
                }
                matrixStrings.Add(string.Join(ArraySeparator, arrStrings));
            }
            return string.Join(Environment.NewLine + Environment.NewLine, matrixStrings);
        }

        public static string[,,] FromString3D(string s)
        {
            string[] matrixStrings = s.Split(new[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            int depth = matrixStrings.Length;
            int rows = matrixStrings[0].Split(new[] { ArraySeparator }, StringSplitOptions.RemoveEmptyEntries).Length;
            int cols = matrixStrings[0].Split(new[] { ArraySeparator }, StringSplitOptions.RemoveEmptyEntries)[0].Split(new[] { ElementSeparator }, StringSplitOptions.None).Length;
            string[,,] arr = new string[depth, rows, cols];
            for (int k = 0; k < depth; k++)
            {
                string[] arrStrings = matrixStrings[k].Split(new[] { ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < rows; i++)
                {
                    string[] rowStrings = arrStrings[i].Split(new[] { ElementSeparator }, StringSplitOptions.None);
                    for (int j = 0; j < cols; j++)
                    {
                        arr[k, i, j] = DecodeValue(rowStrings[j]);
                    }
                }
            }
            return arr;
        }

        private static string EncodeValue(string value)
        {
            // Заменяем специальные символы на их эскейп-последовательности
            value = value.Replace("@", "@@");
            value = value.Replace("#", "##");
            return value;
        }

        private static string DecodeValue(string value)
        {
            // Восстанавливаем специальные символы из эскейп-последовательностей
            value = value.Replace("##", "#");
            value = value.Replace("@@", "@");
            return value;
        }
    }


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

        private async Task HandleClientAsync(int clientId)
        {
            TcpClient client = clients[clientId];
            NetworkStream stream = client.GetStream();
            string[] user;
            string[,] general;
            string[,,] disk;
            while (client.Connected)
            {
                string data = await ReadDataAsync(stream);

                //Console.WriteLine($"Received data from client {clientId}: {data}");
                if (data.StartsWith("VALIDATION"))
                {
                    user = data.Split(';');
                    if (user[4] == "NeedToGet") user[4] = GetID();
                    await WriteDataAsync(stream, $"VALID;{user[4]}", 10);
                }
                else if(data.StartsWith("General: "))
                {
                    data = data.Replace("General: ", "");
                    general = ArrayStringConverter.FromString2D(data);
                }
                else if (data.StartsWith("Disk: "))
                {
                    data = data.Replace("Disk: ", "");
                    disk = ArrayStringConverter.FromString3D(data);
                }
                else if(data == "END")
                {

                }
            }
            if (!client.Connected)
                Console.WriteLine($"Client {clientId} disconnected from {client.Client.RemoteEndPoint}");
        }

        private async Task WriteDataAsync(Stream stream, string message, int bytes)
        {
            string encryptedMessage = Cryptography.Encrypt(message, password);
            foreach (string chunk in Chunks(encryptedMessage, bytes))
            {
                byte[] data = Encoding.UTF8.GetBytes(chunk);
                await stream.WriteAsync(data, 0, data.Length);
                byte[] buffer = new byte[4096];
                await stream.ReadAsync(buffer, 0, buffer.Length);
            }

            byte[] endSignal = Encoding.UTF8.GetBytes("end");
            await stream.WriteAsync(endSignal, 0, endSignal.Length);
            byte[] endBuffer = new byte[4096];
            await stream.ReadAsync(endBuffer, 0, endBuffer.Length);
        }

        private async Task<string> ReadDataAsync(Stream stream)
        {
            byte[] message = new byte[0];
            byte[] buffer = new byte[4096];
            int bytesTotal = 0;
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                byte[] messageRaw = new byte[bytesRead];
                Array.Copy(buffer, messageRaw, bytesRead);

                if (Encoding.UTF8.GetString(messageRaw) != "end")
                {
                    message = message.Concat(messageRaw).ToArray();
                    await stream.WriteAsync(Encoding.UTF8.GetBytes("1"), 0, 1);
                    bytesTotal += bytesRead;
                }
                else
                {
                    await stream.WriteAsync(Encoding.UTF8.GetBytes("1"), 0, 1);
                    string dataReceived = Encoding.UTF8.GetString(message, 0, bytesTotal);
                    return Cryptography.Decrypt(dataReceived, password);
                }
            }
        }

        private static IEnumerable<string> Chunks(string lst, int n)
        {
            for (int i = 0; i < lst.Length; i += n)
            {
                yield return lst.Substring(i, Math.Min(n, lst.Length - i));
            }
        }



        public void CloseClientConnection(int clientId)
        {
            if (clients.TryGetValue(clientId, out TcpClient client))
            {
                Console.WriteLine($"Closing connection with client {clientId}");
                client.Close();
                clients.TryRemove(clientId, out _);
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
