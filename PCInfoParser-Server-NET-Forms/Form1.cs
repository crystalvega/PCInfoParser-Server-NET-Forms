using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCInfoParser_Server_NET_Forms
{
    public partial class Form1 : Form
    {
        // Глобальные переменные
        int selectedItemIndex = -1;
        Dictionary<int, bool> checkedItems = new Dictionary<int, bool>();
        AsyncTcpServer server = new(12345);
        bool start = false;
        string server_status = "Выключен";

        public Form1()
        {
            InitializeComponent();
            UpdateStatus();
            listView1.FullRowSelect = true;
            listView1.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            ColumnHeader header = new();
            header.Text = "";
            header.TextAlign = HorizontalAlignment.Left;
            header.Width = 30;
            listView1.Columns.Add(header);
            listView1.Columns.Add("ID");
            listView1.Columns.Add("ФИО");
            listView1.Columns.Add("Кабинет");
            listView1.Columns.Add("Организация");
        }

        void UpdateStatus()
        {
            this.label1.Text = $"Состояние сервера:\r\n\r\n{server_status}";
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            server_status = "Включение...";
            UpdateStatus();
            server.StartAsync();
            server_status = "Включен";
            UpdateStatus();
            start = true;
            UpdateClientListAsync();
            await Task.Delay(500);
        }

        async void UpdateClientListAsync()
        {
            while (start)
            {
                // Сохранение состояния элементов ListView
                List<bool> checkedItems = new List<bool>();
                int selectedItemIndex = -1;
                if (listView1.SelectedItems.Count > 0)
                {
                    selectedItemIndex = listView1.SelectedItems[0].Index;
                }
                foreach (ListViewItem item in listView1.Items)
                {
                    checkedItems.Add(item.Checked);
                }

                // Получение списка клиентов от сервера
                Dictionary<int, string[]> clientList = GetClientListFromServerAsync();

                // Очистка списка клиентов в ListView
                listView1.BeginUpdate();
                listView1.Items.Clear();

                // Добавление клиентов в ListView
                foreach (KeyValuePair<int, string[]> client in clientList)
                {
                    // Создание элемента списка для клиента
                    ListViewItem item = new ListViewItem("");
                    // Добавление информации о клиенте в подэлемент списка
                    item.SubItems.Add(client.Key.ToString());
                    foreach (string i in client.Value) if (i != "VALIDATION") item.SubItems.Add(i);
                    // Добавление элемента в ListView
                    listView1.Items.Add(item);
                }

                // Восстановление состояния элементов ListView
                if (selectedItemIndex >= 0 && selectedItemIndex < listView1.Items.Count)
                {
                    listView1.Items[selectedItemIndex].Selected = true;
                }
                for (int i = 0; i < listView1.Items.Count && i < checkedItems.Count; i++)
                {
                    listView1.Items[i].Checked = checkedItems[i];
                }

                listView1.EndUpdate();

                await Task.Delay(2000);
            }
        }

        Dictionary<int, string[]> GetClientListFromServerAsync()
        {
            return server.GetClients();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            start = false;
            server.StopServer();
            listView1.Items.Clear();
            server_status = "Выключен";
            UpdateStatus();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // если все элементы списка выделены, выделяем CheckBox
            checkBox1.Checked = listView1.CheckedItems.Count == listView1.Items.Count;
            checkedItems[e.Item.Index] = e.Item.Checked;

        }

        void listView1_ItemCheck(object sender, EventArgs e)
        {
            // если пользователь выбрал элемент в заголовке
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = checkBox1.Checked;
            }
        }

        // Обработчик события выделения элемента ListView
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                selectedItemIndex = listView1.SelectedItems[0].Index;
            }
            else
            {
                selectedItemIndex = -1;
            }
        }
    }
}
