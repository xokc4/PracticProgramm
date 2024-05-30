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
    /// Логика взаимодействия для DeleteWpf.xaml
    /// </summary>
    public partial class DeleteWpf : Window
    {
        //объект для подключения к бд
        static ConnectionData connectiont = new ConnectionData();
        // строка подключения
        public static string connectionString;
        public DeleteWpf()
        {
            InitializeComponent();
            // взятие информации для подклчения
            connectiont = connectiont.LoadFromFile();
            //передача пути
            connectionString = connectiont.StringsPath();
        }
        /// <summary>
        /// кнопка для удаления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteName_Click(object sender, RoutedEventArgs e)
        {
            string componentNameToDelete = NameElements.Text;

            try
            {
                bool componentExists = await ComponentExistsAsync(componentNameToDelete);
                if (!componentExists)
                {
                    MessageBox.Show("Компонент не существует в базе данных.");
                    return;
                }
                bool canDelete = await CanDeleteComponentAsync(componentNameToDelete);
                if (canDelete)
                {
                    await DeleteComponentAndNestedElementsAsync(componentNameToDelete);
                    MessageBox.Show("Компонент и связанные с ним вложенные элементы успешно удалены.");
                }
                else
                {
                    MessageBox.Show("Невозможно удалить компонент, так как его вложенные элементы используются в других компонентах.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении компонента: {ex.Message}");
            }
        }
        /// <summary>
        /// проверка на существование компонента перед удалением
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        private async Task<bool> ComponentExistsAsync(string componentName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string checkQuery = DatabaseQueries.EngineComponentsEngineComponents;//sql скрипт

                using (SqlCommand command = new SqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@ComponentName", componentName);
                    int count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
        /// <summary>
        /// проверка на удаление
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        private async Task<bool> CanDeleteComponentAsync(string componentName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string checkQuery = DatabaseQueries.checkQuerySELECTNestedElements;//sql скрипт

                using (SqlCommand command = new SqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@ComponentName", componentName);
                    int count = (int)await command.ExecuteScalarAsync();
                    return count == 0;
                }
            }
        }
        /// <summary>
        /// метод по удалению компонента и вложенных элементов
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        private async Task DeleteComponentAndNestedElementsAsync(string componentName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Удаление NestedElements связанных с EngineComponents
                string deleteNestedElementsQuery = DatabaseQueries.deleteNestedElementsQuery;//sql скрипт

                using (SqlCommand command = new SqlCommand(deleteNestedElementsQuery, connection))
                {
                    command.Parameters.AddWithValue("@ComponentName", componentName);
                    await command.ExecuteNonQueryAsync();
                }

                // Удаление самого EngineComponents
                string deleteEngineComponentsQuery = DatabaseQueries.deleteEngineComponentsQuery;

                using (SqlCommand command = new SqlCommand(deleteEngineComponentsQuery, connection))
                {
                    command.Parameters.AddWithValue("@ComponentName", componentName);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        /// <summary>
        /// Кнопка назад
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
