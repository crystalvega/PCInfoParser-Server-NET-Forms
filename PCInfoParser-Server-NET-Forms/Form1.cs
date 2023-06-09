using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                if (ini.GetValue("Server", "Port") != "" && ini.GetValue("Server", "Password")!= "" && connector.connection_status)
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

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("[Server] Запуск сервера...");
            server_status = "Запуск...";
            server.StartAsync();
            if(server.GetStatus())
            { 
            server_status = "Запущен";
            Console.WriteLine($"[Server] Сервер запущен на порте {ini.GetValue("Server", "Port")}!");
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = false;
            userSettings.Change(2);
			}
            else
            {
				server_status = "Выключен";
				Console.WriteLine("[Server] Не удалось запустить сервер!");
				button1.Enabled = true;
				button2.Enabled = false;
				button3.Enabled = false;
				button4.Enabled = true;
				Change(0);
				userSettings.Change(1);
			}
		}

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("[Server] Выключение сервера...");
            server_status = "Выключение...";
            server.StopServer();
            server_status = "Выключен";
            Console.WriteLine("[Server] Сервер выключен!");
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = true;
            Change(0);
            userSettings.Change(1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("[MySQL] Подключение к MySQL...");
            mysql_status = "Подключение...";
            button3.Enabled = false;
            button4.Enabled = false;
            if (connector.Connect())
            {
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = true;
                Console.WriteLine("[MySQL] Подключено к MySQL успешно!");
                mysql_status = "Подключено";
                userSettings.Change(1);
                if (ini.GetValue("Server", "Port") != "" && ini.GetValue("Server", "Password") != "") Change(3);
                else Change(1);
            }
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = true;
                button4.Enabled = false;
                Console.WriteLine("[MySQL] Не удалось подключиться к MySQL!");
                mysql_status = "Отключено";
                Change(1);
                userSettings.Change(0);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Console.WriteLine("[MySQL] Отключение от MySQL...");
            mysql_status = "Отключение";
            connector.Disconnect();
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = true;
            button4.Enabled = false;
            Console.WriteLine("[MySQL] Отключено от MySQL успешно!");
            mysql_status = "Отключено";
            userSettings.Change(0);
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
                e.Cancel = true;
            }
        }
        private async void просмотрЭкспортToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string processName = "PCInfoParser-DB-Viewer-NET"; // Замените на название вашего приложения
                try
                {
                    Process.Start(processName + ".exe");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при открытии средства просмотра базы данных: " + ex.Message);
                }
			await Task.Delay(1);
		}

		private void mySQLToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (File.Exists("PCInfoParser-DB-Viewer-NET.exe")) просмотрЭкспортToolStripMenuItem.Enabled = true;
            else просмотрЭкспортToolStripMenuItem.Enabled = false;
		}
	}
}
