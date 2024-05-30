using Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PracticProgramm
{
    /// <summary>
    /// объект для подключения
    /// </summary>
    internal class ConnectionData
    {
        // конструктор
        public ConnectionData(string serverName, string databaseName, string userName, string password, int port)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
            UserName = userName;
            Password = password;
            Port = port;
        }

        public ConnectionData()
        {
        }

        public string path = "connection.txt";// название файла где находится информация для подключения к бд
        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        public string UserName { get; set; }
        
        public string Password { get; set; }
        public int Port { get; set; }
        /// <summary>
        /// метод по октрытию файла
        /// </summary>
        /// <returns></returns>
        public  ConnectionData LoadFromFile()
        {
            try
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length < 5)
                {
                    throw new InvalidDataException("Недостаточно данных в файле конфигурации.");
                }

                string server = lines[0].Split('=')[1];
                string database = lines[1].Split('=')[1];
                string userId = lines[2].Split('=')[1];
                string password = lines[3].Split('=')[1];
                int port = int.Parse(lines[4].Split('=')[1]);

                return new ConnectionData(server, database, userId, password, port);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных подключения: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// метод по сохранению файла
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="Database"></param>
        /// <param name="UserId"></param>
        /// <param name="Password"></param>
        public void SaveToFile(string Server, string Database, string UserId, string Password)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    int Port = 322;
                    writer.WriteLine($"Server={Server}");
                    writer.WriteLine($"Database={Database}");
                    writer.WriteLine($"UserId={UserId}");
                    writer.WriteLine($"Password={Password}");
                    writer.WriteLine($"Port={Port}");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// солздание строки подключения
        /// </summary>
        /// <returns></returns>
        public string StringsPath()
        {
            return $"Server={ServerName};Database={DatabaseName};User ID={UserName};password={Password};Trusted_Connection=True;TrustServerCertificate=True;";
        }
    }
}
