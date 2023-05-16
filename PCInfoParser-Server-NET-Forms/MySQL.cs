using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCInfoParser_Server_NET_Forms
{
    internal class MySQL
    {
        public static Tuple<string, string> LoadTableParametras(string filename)
        {
            string createAllConfTable = $@"
        CREATE TABLE `{filename}`.`all configuration`
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

            string createDiskConfTable = $@"
        CREATE TABLE `{filename}`.`disk configuration` 
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

            createAllConfTable = createAllConfTable.Replace("{filename}", filename);
            createDiskConfTable = createDiskConfTable.Replace("{filename}", filename);

            return new Tuple<string, string>(createAllConfTable, createDiskConfTable);
        }
        public static List<string> GenExecuteDisk(List<object> list, string tablename, string filename)
        {
            List<string> executeCharters = new List<string>();
            string[] defpar1 = new string[3];
            string[] defpar2 = new string[3];
            int diskNumbs = list.Count - 3;

            for (int i = 0; i < 3; i++)
            {
                defpar1[i] = (string)((object[])list[i])[0];
                defpar2[i] = (string)((object[])list[i])[1];
            }

            list = list.GetRange(3, diskNumbs);

            for (int i = 0; i < diskNumbs; i++)
            {
                executeCharters.Add($"INSERT INTO `{filename}`.`{tablename}`(");
                string valuesChartersSet = "";

                foreach (string i2 in defpar1)
                {
                    executeCharters[i] += $"{i2}`, `";
                }

                foreach (string i2 in defpar2)
                {
                    valuesChartersSet += $"'{i2}', ";
                }
            }

            for (int i = 0; i < diskNumbs; i++)
            {
                string valuesCharters = valuesChartersSet;

                foreach (object par in (object[])list[i])
                {
                    executeCharters[i] += $"{par[0]}`, `";
                    valuesCharters += $"'{par[1]}', ";
                }

                executeCharters[i] += "Дата создания`";
                valuesCharters += "NOW());";
                executeCharters[i] += ") VALUES (" + valuesCharters;
            }

            return executeCharters;
        }

        public static string GenExecuteParams(string[,] list, string tablename, string filename)
        {
            string executeCharters = $"INSERT INTO `{filename}`.`{tablename}`(";
            string valuesCharters = "";

            foreach (string[] parameters in list)
            {
                executeCharters += $"{parameters[0]}`, `";
                valuesCharters += $"'{parameters[1]}', ";
            }

            executeCharters += "Дата создания`";
            valuesCharters += "NOW());";
            executeCharters += ") VALUES (" + valuesCharters;

            return executeCharters;
        }

        public static Tuple<string, string> LoadExecuteParametras(List<object> charters, List<object> disk, string filename)
        {
            string executeCharters = GenExecuteParams(charters, "all configuration", filename);
            List<string> executeDisk = GenExecuteDisk(disk, "disk configuration", filename);

            return new Tuple<string, string>(executeCharters, string.Join(Environment.NewLine, executeDisk));
        }
    }
}
