using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PCInfoParser_Server_NET_Forms
{
    public partial class Form1 : Form
    {
        AsyncTcpServer server = new(12345);
        bool start = false;
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            listView1.FullRowSelect = true;
            listView1.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            // Добавление столбцов
            listView1.Columns.Add("ID");
            listView1.Columns.Add("ФИО");
            listView1.Columns.Add("Кабинет");
            listView1.Columns.Add("Организация");
            server.StartAsync();
            start = true;
            UpdateClientListAsync();
                //    while (!server.isNewConnected())
                //    {
                //        await Task.Delay(2000);
                //    }
                //foreach (string client in server.clientsinfo) if (client != null) listView1.Nodes.Add(client);
                //}
            }


        // Асинхронное обновление списка клиентов
        async void UpdateClientListAsync()
        {
            while(start)
            {
                // Получение списка клиентов от сервера
                Dictionary<int, string[]> clientList = GetClientListFromServerAsync();

                // Очистка списка клиентов в ListView
                listView1.Items.Clear();

                // Добавление клиентов в ListView
                foreach (KeyValuePair<int, string[]> client in clientList)
                {
                    // Создание элемента списка для клиента
                    ListViewItem item = new ListViewItem(client.Key.ToString());
                    // Добавление информации о клиенте в подэлемент списка
                    foreach(string i in client.Value) if(i != "VALIDATION") item.SubItems.Add(i);
                    // Добавление элемента в ListView
                    listView1.Items.Add(item);
                }
                await Task.Delay(2000);
            }
        }

        // Асинхронный метод получения списка клиентов от сервера
        Dictionary<int, string[]> GetClientListFromServerAsync()
        {
            // Код получения списка клиентов от сервера
            // ...

            // Возвращение списка клиентов
            return server.GetClients();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}
