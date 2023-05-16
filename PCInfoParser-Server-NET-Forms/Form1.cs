using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCInfoParser_Server_NET_Forms
{
    public partial class Form1 : Form
    {
        // Глобальные переменные
        Dictionary<int, bool> checkedItems = new Dictionary<int, bool>();
        AsyncTcpServer server = new(12345);
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
            await Task.Delay(500);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            server.StopServer();
            listView1.Items.Clear();
            server_status = "Выключен";
            UpdateStatus();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            listView1.Columns[0].Width = 30;
            listView1.Columns[1].Width = 60;
            listView1.Columns[2].Width = 200;
            listView1.Columns[3].Width = 60;
            SizeLastColumn(listView1);
        }

        private void listView1_Resize(object sender, System.EventArgs e)
        {
            SizeLastColumn((ListView)sender);
        }

        private void SizeLastColumn(ListView lv)
        {
            lv.Columns[lv.Columns.Count - 1].Width = -2;
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

        private void button3_Click(object sender, EventArgs e)
        {
            List<string[]> selectedClients = new();
            // Переберите все элементы в ListView и найдите выбранные элементы
            foreach (ListViewItem listViewItem in listView1.Items)
            {
                string[] infoClients = new string[4];

                if (listViewItem.Checked)
                {
                    for(int i=1; i<5; i++) infoClients[i-1] = listViewItem.SubItems[i].Text;

                    // Теперь вы можете использовать выбранные значения
                    selectedClients.Add(infoClients);
                }
            }
        }
    }
}
