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
    /// Логика взаимодействия для AddCompanents.xaml
    /// </summary>
    public partial class AddCompanents : Window
    {
        static ConnectionData connectiont = new ConnectionData();
        public static string connectionString;
        public AddCompanents()
        {
            InitializeComponent();
            connectiont = connectiont.LoadFromFile();
            connectionString = connectiont.StringsPath();
        }
        /// <summary>
        /// кнопка для добавления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            string componentName = NameCompanents.Text;
            string engineName = engineNameTextBox.Text;
            int? componentCount = null;
            bool isNested = isNestedCheckBox.IsChecked ?? false;

            int ComponentsId = 0;
            if (NameCompanentsElements.LineCount>-1)
            {
                ComponentsId = 1;
               
            }
            else
            {
                ComponentsId = Convert.ToInt32(GetComponentIdByNameAsync(NameCompanentsElements.Text));
            }

            if (isNested)
            {
                if (!int.TryParse(ComponentCount.Text, out int count))
                {
                    MessageBox.Show("Неверный формат количества компонентов.");
                    return;
                }
                componentCount = count;
            }

            try
            {
                int engineId = await GetOrCreateEngineIdAsync(engineName);
               


                if (isNested && componentCount.HasValue)
                {
                    await AddNestedElementsAsync(ComponentsId, NameCompanentsElements.Text, Convert.ToInt32(ComponentCount.Text));
                    MessageBox.Show("Один элемент добавлен.");
                }
                else
                {
                    int componentId = await GetOrCreateComponentIdAsync(componentName, engineId, Convert.ToInt32(ComponentCount.Text), isNested);
                    MessageBox.Show("Компонент успешно добавлен.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении компонента: {ex.Message}");
            }
        }
        /// <summary>
        /// метод для поиска айди компонента
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        private async Task<int?> GetComponentIdByNameAsync(string componentName)
        {
            int Id = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query =DatabaseQueries.SELECTComponentNameFrom;
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ComponentName", componentName);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if(reader.GetString(0)==null)
                            {
                                Id = 0;
                            }
                            else
                            {
                                Id = Convert.ToInt32(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            if(Id!=0)
            {
                return Id;
            }
            else
            {
                return 1; // Если значение не найдено
            }
        }
        /// <summary>
        /// метод для проверки айди двигателя по названию машины
        /// </summary>
        /// <param name="carName"></param>
        /// <returns></returns>
        private async Task<int> GetOrCreateEngineIdAsync(string carName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Проверяем, существует ли автомобиль с таким именем
                    string checkCarSql = DatabaseQueries.checkCarSql;
                    using (SqlCommand command = new SqlCommand(checkCarSql, connection))
                    {
                        command.Parameters.AddWithValue("@CarName", carName);
                        object engineId = await command.ExecuteScalarAsync();

                        if (engineId != null)
                        {
                            // Если автомобиль найден, возвращаем его идентификатор двигателя
                            return (int)engineId;
                        }
                        else
                        {
                            return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
        /// <summary>
        /// создание компонента
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="engineId"></param>
        /// <param name="componentCount"></param>
        /// <param name="isNested"></param>
        /// <returns></returns>
        private async Task<int> GetOrCreateComponentIdAsync(string componentName, int engineId, int? componentCount, bool isNested)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Проверка, существует ли компонент
                string checkQuery =DatabaseQueries.checkQuerySELECTComponentID;
                using (SqlCommand command = new SqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@ComponentName", componentName);
                    command.Parameters.AddWithValue("@EngineID", engineId);
                    int? componentId = (int?)await command.ExecuteScalarAsync();

                    if (componentId.HasValue)
                    {
                        return componentId.Value;
                    }
                }

                // Добавление нового компонента
                string addComponentQuery = DatabaseQueries.addComponentQuery;

                using (SqlCommand command = new SqlCommand(addComponentQuery, connection))
                {
                    command.Parameters.AddWithValue("@ComponentName", componentName);
                    command.Parameters.AddWithValue("@EngineID", engineId);
                    command.Parameters.AddWithValue("@ComponentCount", componentCount ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsNested", isNested);
                    int componentId = (int)await command.ExecuteScalarAsync();
                    return componentId;
                }
            }
        }
       /// <summary>
       /// создание элемента
       /// </summary>
       /// <param name="componentId"></param>
       /// <param name="elementName"></param>
       /// <param name="elementCount"></param>
       /// <returns></returns>
        private async Task AddNestedElementsAsync(int componentId, string elementName, int elementCount)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Добавление вложенных элементов
                string addNestedElementsQuery =DatabaseQueries.addNestedElementsQuery;

                using (SqlCommand command = new SqlCommand(addNestedElementsQuery, connection))
                {
                    command.Parameters.AddWithValue("@ComponentID", componentId);
                    command.Parameters.AddWithValue("@ElementName", elementName);
                    command.Parameters.AddWithValue("@ElementCount", elementCount);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        /// <summary>
        /// кнопка назад
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();//cборщик мусора
            GC.WaitForPendingFinalizers();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();

        }
    }
}
