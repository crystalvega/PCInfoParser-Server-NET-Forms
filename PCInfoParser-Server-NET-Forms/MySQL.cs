using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PCInfoParser_Server_NET_Forms
{

    public class MySQLConnector
    {
        private string connectionString;
        bool connection_status;
        MySqlConnection connection;
        public IniFile ini;

        public bool Connect()
        {
            connectionString = $"Server={ini.GetValue("MySQL", "IP")};Port={ini.GetValue("MySQL", "Port")};Database={ini.GetValue("MySQL", "Database")};Uid={ini.GetValue("MySQL", "User")};Pwd={ini.GetValue("MySQL", "Password")};";
            connection_status = false;
            try
            {
                connection = new(connectionString);
                connection.Open();
                connection_status = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return connection_status;
        }

        public bool CheckID(string[] user)
        {
            string query = $"SELECT ID FROM {user[3]}_Users"; // Замените на ваш запрос SELECT

            try
            {
                MySqlCommand command = new(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                List<int> columnValues = new();


                while (reader.Read())
                {
                    string columnValue = reader.GetString(0); // Получение значения столбца по индексу (0)
                    if (columnValue == user[4])
                    {
                        reader.Close();
                        return true;
                    }
                }
                reader.Close();
                return false;
            }
            catch
            {
                return false;
            }
        }

                public string LastID(string table)
        {
            string query = $"SELECT ID FROM {table}_Users"; // Замените на ваш запрос SELECT

            try
            {
                MySqlCommand command = new(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                List<int> columnValues = new();


                while (reader.Read())
                {
                    string columnValue = reader.GetString(0); // Получение значения столбца по индексу (0)
                    columnValues.Add(Convert.ToInt32(columnValue));
                }

                columnValues = columnValues.Distinct().ToList();
                columnValues.Sort();
                
                int lastvalue = 0;

                foreach(int value in columnValues)
                {
                    lastvalue++;
                    if (value != lastvalue - 1)
                    {
                        lastvalue--;
                        break;
                    }
                }

                reader.Close();
                return lastvalue.ToString();
            }
            catch(Exception) 
            {
                return "0";
            }
        }
        public void Disconnect()
        {
            if(!connection_status) connection.Close();
        }
        public bool ExecuteCommand(string commandText)
        {
            bool success = false;
            try
            {
                MySqlCommand command = new(commandText, connection);
                int rowsAffected = command.ExecuteNonQuery();

                // Если хотя бы одна строка была затронута, считаем операцию успешной
                success = rowsAffected > 0;
            }
            catch (MySqlException ex)
            {
                // Обработка ошибок подключения к базе данных
                if (ex.ErrorCode != -2147467259) Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return success;
        }
    }

    internal static class MySQLCommand
    {
        public static string[] LoadTableParametras(string filename)
        {
            string[] returnvalue = new string[3];
            returnvalue[0] = $@"
        CREATE TABLE `{filename}_General`
        (
            `ID`	VARCHAR(512),
            `Монитор`	VARCHAR(512),
            `Диагональ`	VARCHAR(512),
            `Тип принтера`	VARCHAR(512),
            `Модель принтера`	VARCHAR(512),
            `ПК`	VARCHAR(512),
            `Материнская плата`	VARCHAR(512),
            `Процессор`	VARCHAR(512),
            `Частота процессора`	VARCHAR(512),
            `Баллы Passmark`	VARCHAR(512),
            `Дата выпуска`	VARCHAR(512),
            `Тип ОЗУ`	VARCHAR(512),
            `ОЗУ, 1 Планка`	VARCHAR(512),
            `ОЗУ, 2 Планка`	VARCHAR(512),
            `ОЗУ, 3 Планка`	VARCHAR(512),
            `ОЗУ, 4 Планка`	VARCHAR(512),
            `Сокет`	VARCHAR(512),
            `Диск 1`	VARCHAR(512),
            `Состояние диска 1`	VARCHAR(512),
            `Диск 2`	VARCHAR(512),
            `Состояние диска 2`	VARCHAR(512),
            `Диск 3`	VARCHAR(512),
            `Состояние диска 3`	VARCHAR(512),
            `Диск 4`	VARCHAR(512),
            `Состояние диска 4`	VARCHAR(512),
            `Операционная система`	VARCHAR(512),
            `Антивирус`	VARCHAR(512),
            `CPU Под замену`	VARCHAR(512),
            `Все CPU под сокет`	LONGTEXT,
            `Дата создания` DATETIME  NOT NULL
        );
        ";

            returnvalue[1] = $@"
        CREATE TABLE `{filename}_Disk` 
        (
            `ID`	VARCHAR(512),
            `Диск`	VARCHAR(512),
            `Наименование`	VARCHAR(512),
            `Прошивка`	VARCHAR(512),
            `Размер`	VARCHAR(512),
            `Время работы`	VARCHAR(512),
            `Включён`	VARCHAR(512),
            `Состояние`	VARCHAR(512),
            `Температура`	VARCHAR(512),
            `Дата создания` DATETIME  NOT NULL
        );
        ";

            returnvalue[2] = $@"
        CREATE TABLE `{filename}_Users`
        (
            `ID`	VARCHAR(512),
            `Кабинет`	VARCHAR(512),
            `LAN`	VARCHAR(512),
            `ФИО`	VARCHAR(512)
        );
        ";
            return returnvalue;
        }
        private static string[] GenExecuteDisk(string organization, string id, string[,,] list)
        {
            List<string> executeCharters = new List<string>();

            int diskNumbs = 0;

            for (int i = 0; i < 4; i++)
            {
                if (list[i, 0, 1] != "") diskNumbs += 1;
                else break;
            }

            for (int i = 0; i < diskNumbs; i++)
            {
                string valuesChartersSet = "";
                executeCharters.Add($"INSERT INTO `{organization}_Disk`(");

                executeCharters[i] += $"`ID`, `";
                
                executeCharters[i] += $"`Диск`, `";
                valuesChartersSet += $"'{id}', ";
                valuesChartersSet += $"'{i+1}', ";

                for (int i3 = 0; i3 < list.GetLength(1); i3++)
                {
                    executeCharters[i] += $"{list[i, i3, 0]}`, `";
                    valuesChartersSet += $"'{list[i, i3, 1]}', ";
                }
                executeCharters[i] += "Дата создания`";
                valuesChartersSet += "NOW());";
                executeCharters[i] += ") VALUES (" + valuesChartersSet;
            }

            return executeCharters.ToArray();
        }

        private static string GenExecuteParams(string organization, string id, string[,] list)
        {
            string executeCharters = $"INSERT INTO `{organization}_General`(";
            string valuesCharters = "";

            executeCharters += $"`ID`, `";
            valuesCharters += $"'{id}', ";

            for (int i = 0; i < list.GetLength(0); i++)
            {
                executeCharters += $"{list[i, 0]}`, `";
                valuesCharters += $"'{list[i, 1]}', ";
            }

            executeCharters += "Дата создания`";
            valuesCharters += "NOW());";
            executeCharters += ") VALUES (" + valuesCharters;

            return executeCharters;
        }

        public static string GenExecuteUser(string lan, string[] user)
        {
            string executeCharters = $"INSERT INTO `{user[3]}_Users`(";
            string valuesCharters = "";

            executeCharters += $"`ID`, `";
            executeCharters += $"Кабинет`, `";
            executeCharters += $"LAN`, `";
            executeCharters += $"ФИО`";
            valuesCharters += $"'{user[4]}', ";
            valuesCharters += $"'{user[2]}', ";
            valuesCharters += $"'{lan}', ";
            valuesCharters += $"'{user[1]}');";

            executeCharters += ") VALUES (" + valuesCharters;

            return executeCharters;
        }

        public static string[] LoadExecuteParametras(string[,] charters, string[,,] disk, string[] user)
        {
            List<string> returnvalue = new()
            {
                GenExecuteParams(user[3], user[4], charters)
            };

            string[] executeDisk = GenExecuteDisk(user[3], user[4], disk);
            foreach (string value in executeDisk) returnvalue.Add(value);

            return returnvalue.ToArray();
        }
    }
}
