using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PracticProgramm
{
    /// <summary>
    /// Логика взаимодействия для ReditCompanent.xaml
    /// </summary>
    public partial class ReditCompanent : Window
    {
        //объект для подключения к бд
        static ConnectionData connectiont = new ConnectionData();
        // строка подключения
        public static string connectionString;
        public ReditCompanent()
        {
            InitializeComponent();
            // взятие информации для подклчения
            connectiont = connectiont.LoadFromFile();
            //передача пути
            connectionString = connectiont.StringsPath();
        }
        /// <summary>
        /// Кнопка по изминению названия
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateName_Click(object sender, RoutedEventArgs e)
        {
            string currentName = NameElements.Text;
            string newName = PromptForNewName(currentName);
            if(newName=="CLOSE")
            {
                MessageBox.Show("Отмена");
                NameElements.Text = "";
                return;
            }
            if(newName == currentName)
            {
                MessageBox.Show("Вы написали одно и тоже слово, измените его");
                NameElements.Text = "";
                return;
            }
            else
             {
                if(IsUniqueComponentName(newName))
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Обновляем название компонента в базе данных в таблице EngineComponents
                        string updateEngineComponentsQuery = DatabaseQueries.UPDATEEngineComponents;
                        using (SqlCommand command = new SqlCommand(updateEngineComponentsQuery, connection))
                        {
                            command.Parameters.AddWithValue("@NewName", newName);
                            command.Parameters.AddWithValue("@CurrentName", currentName);
                            command.ExecuteNonQuery();
                        }

                        // Обновляем название компонента в базе данных в таблице NestedElements
                        string updateNestedElementsQuery = DatabaseQueries.UPDATENestedElements;
                        using (SqlCommand command = new SqlCommand(updateNestedElementsQuery, connection))
                        {
                            command.Parameters.AddWithValue("@NewName", newName);
                            command.Parameters.AddWithValue("@CurrentName", currentName);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Изменения успешно прошло");
                        // Обновляем название компонента в окне программы
                        UpdateComponentNameInUI(currentName, newName);

                    }
                }
                else
                {
                    MessageBox.Show("Название уже занято. Введите другое название.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Проверяем наличие компонента 
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        private bool IsUniqueComponentName(string newName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Проверяем наличие компонента с таким же названием в базе данных в таблице EngineComponents
                string checkEngineComponentsQuery = DatabaseQueries.checkEngineComponentsQuery;//sql скрипт
                using (SqlCommand command = new SqlCommand(checkEngineComponentsQuery, connection))
                {
                    command.Parameters.AddWithValue("@NewName", newName);
                    int engineComponentsCount = (int)command.ExecuteScalar();

                    // Проверяем наличие компонента с таким же названием в базе данных в таблице NestedElements
                    string checkNestedElementsQuery = DatabaseQueries.checkNestedElementsQuery;//sql скрипт
                    using (SqlCommand nestedElementsCommand = new SqlCommand(checkNestedElementsQuery, connection))
                    {
                        nestedElementsCommand.Parameters.AddWithValue("@NewName", newName);
                        int nestedElementsCount = (int)nestedElementsCommand.ExecuteScalar();

                        // Если название не найдено ни в одной из таблиц, возвращаем true
                        return engineComponentsCount == 0 && nestedElementsCount == 0;
                    }
                }
            }
        }
        /// <summary>
        /// Метод по созданию сообщения с Переименованием
        /// </summary>
        /// <param name="currentName"></param>
        /// <returns></returns>
        private string PromptForNewName(string currentName)
        {
            string newName = Interaction.InputBox("Введите новое название компонента", "Переименование компонента", currentName);
            if (!string.IsNullOrEmpty(newName))
            {
                return newName;
            }
            else
            {
                return "CLOSE";
            }
           
        }
        /// <summary>
        /// метод для обновления строки
        /// </summary>
        /// <param name="currentName"></param>
        /// <param name="newName"></param>
        private void UpdateComponentNameInUI(string currentName, string newName)
        {
            NameElements.Text = "";
           
        }
        /// <summary>
        ///  кнопка назад
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();// сборщик мусора
            GC.WaitForPendingFinalizers();
            MainWindow mainWindow = new MainWindow();// открытие главной вкладки
            mainWindow.Show();
            this.Close();
        }
    }
}
