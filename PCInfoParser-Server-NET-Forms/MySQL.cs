using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace PCInfoParser_Server_NET_Forms
{

    public class MySQLConnector
    {
        private string connectionString;

        public MySQLConnector(string server, string database, string username, string password)
        {
            // Формируем строку подключения
            connectionString = $"Server={server};Database={database};Uid={username};Pwd={password};";
        }

        public bool ExecuteCommand(string commandText)
        {
            bool success = false;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = new MySqlCommand(commandText, connection);
                    int rowsAffected = command.ExecuteNonQuery();

                    // Если хотя бы одна строка была затронута, считаем операцию успешной
                    success = rowsAffected > 0;
                }
                catch (MySqlException ex)
                {
                    // Обработка ошибок подключения к базе данных
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            return success;
        }
    }

    internal static class MySQLCommand
    {
        private static string[] LoadTableParametras(string filename)
        {
            string[] returnvalue = new string[2];
            returnvalue[0] = $@"
        CREATE TABLE `{filename}_General`
        (
            `ID`	VARCHAR(512),
            `Кабинет`	VARCHAR(512),
            `LAN`	VARCHAR(512),
            `ФИО`	VARCHAR(512),
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
            `Кабинет`	VARCHAR(512),
            `LAN`	VARCHAR(512),
            `ФИО`	VARCHAR(512),
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

            //createAllConfTable = createAllConfTable.Replace("{filename}", filename);
            //createDiskConfTable = createDiskConfTable.Replace("{filename}", filename);

            return returnvalue;
        }
        private static string[] GenExecuteDisk(string lan, string[] user, string[,,] list)
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
                executeCharters.Add($"INSERT INTO `{user[2]}_Disk`(");

                executeCharters[i] += $"ID`, `";
                executeCharters[i] += $"Кабинет`, `";
                executeCharters[i] += $"LAN`, `";
                executeCharters[i] += $"ФИО`, `";
                valuesChartersSet += $"'{user[3]}', ";
                valuesChartersSet += $"'{user[1]}', ";
                valuesChartersSet += $"'{lan}', ";
                valuesChartersSet += $"'{user[0]}', ";

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

        private static string GenExecuteParams(string lan, string[] user, string[,] list)
        {
            string executeCharters = $"INSERT INTO `{user[2]}_General`(";
            string valuesCharters = "";

            executeCharters += $"ID`, `";
            executeCharters += $"Кабинет`, `";
            executeCharters += $"LAN`, `";
            executeCharters += $"ФИО`, `";
            valuesCharters += $"'{user[3]}', ";
            valuesCharters += $"'{user[1]}', ";
            valuesCharters += $"'{lan}', ";
            valuesCharters += $"'{user[0]}', ";

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

        public static string[] LoadExecuteParametras(string[,] charters, string[,,] disk, string[] user, string lan)
        {
            List<string> returnvalue = new List<string>();

            string[] createTables = LoadTableParametras(user[2]);
            foreach (string table in createTables) returnvalue.Add(table);

            returnvalue.Add(GenExecuteParams(lan, user, charters));

            string[] executeDisk = GenExecuteDisk(lan, user, disk);
            foreach (string value in executeDisk) returnvalue.Add(value);

            return returnvalue.ToArray();
        }
    }
}
