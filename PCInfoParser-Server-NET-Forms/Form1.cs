using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace PCInfoParser_Server_NET_Forms
{
    public partial class Form1 : Form
    {
        // Глобальные переменные
        bool close = false;
        IniFile ini = new("PCInfoParser-Server.ini");
        UserSettings userSettings = new();
        StringWriter consoleOutput = new StringWriter(new StringBuilder());
        AsyncTcpServer server = new();
        MySQLConnector connector = new();
        string server_status = "Выключен";
        string mysql_status = "Отключено";

        public Form1()
        {
            Console.SetOut(consoleOutput);
            Console.SetError(consoleOutput);
            InitializeComponent();
        }

        public async void LayersAutoUpdate()
        {
            while (true)
            {
                this.label1.Text = $"Состояние сервера:\r\n\r\n{server_status}";
                this.label2.Text = $"Состояние MySQL:\r\n\r\n{mysql_status}";
                // Получаем текущее содержимое StringWriter
                string output = consoleOutput.ToString();
                // Если содержимое изменилось, обновляем TextBox
                if (textBox1.Text != output)
                {
                    textBox1.Text = output;

                    // Если курсор находится вверху, прокручиваем TextBox вниз
                    textBox1.SelectionStart = textBox1.Text.Length;
                    textBox1.ScrollToCaret();
                }
                await Task.Delay(1000);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(ini.GetValue("App", "ConnectMySQL"))) ini.SetValue("App", "ServerStart", "false");

            userSettings.ini = ini;
            connector.ini = ini;
            server.ini = ini;

            server.connector = connector;

            if (Convert.ToBoolean(this.ini.GetValue("App", "Minimaze"))) this.Close();
            
            Change(2);
            if (ini.GetValue("MySQL", "IP") != "" && ini.GetValue("MySQL", "Port") != "" && ini.GetValue("MySQL", "Database") != "" && ini.GetValue("MySQL", "User") != "" && ini.GetValue("MySQL", "Password") != "")
            {
                Change(1);
                if (Convert.ToBoolean(this.ini.GetValue("App", "ConnectMySQL"))) button3_Click(sender, e);
                if (ini.GetValue("Server", "Port") != "" && ini.GetValue("Server", "Password")!= "")
                {
                    Change(0);
                    if (Convert.ToBoolean(this.ini.GetValue("App", "ServerStart")))
                    {
                        button1_Click(sender, e);
                        Change(3);
                    }
                }
            }

            LayersAutoUpdate();
        }

        public void Change(int status)
        {
            menuStrip1.Enabled = true;
            switch (status)
            {
                case 0:
                    splitContainer1.Panel1.Enabled = true;
                    splitContainer1.Panel2.Enabled = true;
                    break;
                case 1:
                    splitContainer1.Panel1.Enabled = true;
                    splitContainer1.Panel2.Enabled = false;
                    break;
                case 2:
                    splitContainer1.Panel1.Enabled = false;
                    splitContainer1.Panel2.Enabled = false;
                    break;
                case 3:
                    splitContainer1.Panel1.Enabled = false;
                    splitContainer1.Panel2.Enabled = true;
                    break;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            server_status = "Включение...";
            server.StartAsync();
            server_status = "Включен";
            await Task.Delay(500);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            server.StopServer();
            server_status = "Выключен";
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userSettings.ShowDialog();
            Change(2);
            if (ini.GetValue("MySQL", "IP") != "" && ini.GetValue("MySQL", "Port") != "" && ini.GetValue("MySQL", "Database") != "" && ini.GetValue("MySQL", "User") != "" && ini.GetValue("MySQL", "Password") != "")
            {
                Change(1);

                if (ini.GetValue("Server", "Port") != "" && ini.GetValue("Server", "Password") != "")
                {
                    Change(0);
                    if(server.start) Change(3);
                }
            }
        }

        private void NotifyIcon1_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                notifyIcon1.Visible = false;
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }

        private void ContextMenuStrip1_Click(object sender, EventArgs e)
        {
            if (sender.ToString() == "Закрыть")
            {
                close = true;
                this.Close();
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!close)
            {
                this.Hide();
                notifyIcon1.Visible = true;
                e.Cancel = true;
            }
        }
    }
}
