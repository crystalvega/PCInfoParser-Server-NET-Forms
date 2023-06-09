using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PCInfoParser_Server_NET_Forms
{
    public partial class UserSettings : Form
    {
        public IniFile ini;
        public string[] mysql = new string[5];
        public string[] server = new string[2];
        public string[] app = new string[3];
        public UserSettings()
        {
            InitializeComponent();
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Обработчик события для кнопки сохранения настроек
            // В этом методе можно выполнить сохранение настроек в файл или базу данных
            // и закрыть форму

            ini.SetValue("MySQL", "IP", textBox1.Text);
            ini.SetValue("MySQL", "Port", textBox2.Text);
            ini.SetValue("MySQL", "Database", textBox3.Text);
            ini.SetValue("MySQL", "User", textBox4.Text);
            ini.SetValue("MySQL", "Password", textBox5.Text);
            ini.SetValue("Server", "Port", textBox6.Text);
            ini.SetValue("Server", "Password", textBox7.Text);
            ini.SetValue("App", "Minimaze", checkBox1.Checked.ToString());
            ini.SetValue("App", "ConnectMySQL", checkBox2.Checked.ToString());
            ini.SetValue("App", "ServerStart", checkBox3.Checked.ToString());
            ini.Save();
            this.Close();
        }

        public void Change(int status)
        {
            saveButton.Enabled = true;
            switch(status)
            {
                case 0:
                    groupBox1.Enabled = true;
                    groupBox2.Enabled = true;
                    break;
                case 1:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = true;
                    break;
                case 2:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                    break;
                case 3:
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                    saveButton.Enabled = false;
                    break;
            }
        }

        private void UserSetting_Load(object sender, EventArgs e)
        {
            textBox1.Text = ini.GetValue("MySQL", "IP");
            textBox2.Text = ini.GetValue("MySQL", "Port");
            textBox3.Text = ini.GetValue("MySQL", "Database");
            textBox4.Text = ini.GetValue("MySQL", "User");
            textBox5.Text = ini.GetValue("MySQL", "Password");
            textBox6.Text = ini.GetValue("Server", "Port");
            textBox7.Text = ini.GetValue("Server", "Password");
            checkBox1.Checked = Convert.ToBoolean(ini.GetValue("App", "Minimaze"));
            checkBox2.Checked = Convert.ToBoolean(ini.GetValue("App", "ConnectMySQL"));
            checkBox3.Checked = Convert.ToBoolean(ini.GetValue("App", "ServerStart"));
            if (checkBox2.Checked) checkBox3.Enabled = true;
            else checkBox3.Enabled = false;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, нажата ли клавиша Ctrl+V (вставка из буфера обмена)
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Получаем текст из буфера обмена
                string clipboardText = Clipboard.GetText();

                // Проверяем, содержит ли вставляемый текст символы, отличные от цифр
                if (!Regex.IsMatch(clipboardText, @"^\d+$"))
                {
                    // Если вставляемый текст содержит символы, отличные от цифр,
                    // то отменяем вставку
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void textBox_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                                          (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox2.Checked)
            {
                checkBox3.Enabled = true;
                checkBox3.Checked = true;
            }
            else
            {
                checkBox3.Enabled = false;
                checkBox3.Checked = false;
            }
        }
    }
}
