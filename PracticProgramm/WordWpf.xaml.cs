using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;

namespace PracticProgramm
{
    /// <summary>
    /// Логика взаимодействия для WordWpf.xaml
    /// </summary>
    public partial class WordWpf : Window
    {
        //объект для подключения к бд
        static ConnectionData connectiont = new ConnectionData();
        // строка подключения
        public static string connectionString;

        public WordWpf()
        {
            InitializeComponent();
            // взятие информации для подклчения
            connectiont = connectiont.LoadFromFile();
            //передача пути
            connectionString = connectiont.StringsPath();
        }
        /// <summary>
        /// кнопка назад
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
        /// <summary>
        /// кнопка по созданию отчета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddWord_Click(object sender, RoutedEventArgs e)
        {
            string selectedItem = TextName.Text;
            if (string.IsNullOrEmpty(selectedItem))//условие на существование текста
            {
                MessageBox.Show("Пожалуйста, введите название компонента.");
                return;
            }

            List<Tuple<string, int, List<Tuple<string, int>>>> components = await GetComponentDataAsync(selectedItem);

            if (components.Count == 0)
            {
                List<Tuple<string, int>> componentsAuto = GetEngineComponents(selectedItem);
                if (componentsAuto.Count>0)
                {

                    CreateWordDocumentAuto(selectedItem, componentsAuto);
                }
                else
                {
                    MessageBox.Show("Для выбранного имени не найдено компонентов а также двигателей.");
                    return;
                }

            }
            else
            {
                CreateWordDocument(selectedItem, components); 
            }
        }
        /// <summary>
        /// поиск двигателя
        /// </summary>
        /// <param name="carName"></param>
        /// <returns></returns>
        private List<Tuple<string, int>> GetEngineComponents(string carName)
        {
            List<Tuple<string, int>> components = new List<Tuple<string, int>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query =DatabaseQueries.SELECTqueryComponents;//sql скрипт

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CarName", carName);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string componentName = reader.GetString(0);
                            int componentCount = reader.GetInt32(1);
                            components.Add(new Tuple<string, int>(componentName, componentCount));
                        }
                    }
                }
            }

            return components;
        }
        /// <summary>
        /// Создание отчета для Двигателя
        /// </summary>
        /// <param name="carName"></param>
        /// <param name="components"></param>
        private void CreateWordDocumentAuto(string carName, List<Tuple<string, int>> components)
        {
            Word.Application wordApp = new Word.Application();
            Word.Document wordDoc = wordApp.Documents.Add();

            wordDoc.Content.Text += $"Отчет о сводном составе компонентов двигателя для машины \"{carName}\"\n\n";

            Word.Table table = wordDoc.Tables.Add(wordDoc.Content, components.Count + 1, 2, Type.Missing, Type.Missing);
            table.Borders.Enable = 1; // Включаем границы таблицы
            table.Columns[1].SetWidth(400, Word.WdRulerStyle.wdAdjustNone); // Устанавливаем ширину первого столбца
            table.Columns[2].SetWidth(400, Word.WdRulerStyle.wdAdjustNone); // Устанавливаем ширину второго столбца

            // Заполняем заголовки таблицы
            table.Cell(1, 1).Range.Text = "Название компонента";
            table.Cell(1, 2).Range.Text = "Общее количество";

            // Заполняем данные таблицы
            int rowIndex = 2; // Индекс строки для вставки данных
            foreach (var component in components)
            {
                table.Cell(rowIndex, 1).Range.Text = component.Item1;
                table.Cell(rowIndex, 2).Range.Text = component.Item2.ToString();
                rowIndex++;
            }

            // Отображаем документ в Word
            wordApp.Visible = true;
            wordDoc.Activate();
        }
        /// <summary>
        /// поиск компонентов
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        private async Task<List<Tuple<string, int, List<Tuple<string, int>>>>> GetComponentDataAsync(string componentName)
        {
    List<Tuple<string, int, List<Tuple<string, int>>>> components = new List<Tuple<string, int, List<Tuple<string, int>>>>();

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();

        string query =DatabaseQueries.SELECTqueryEngine;// sql скрипт

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@ComponentName", componentName);
            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                Dictionary<string, Tuple<int, List<Tuple<string, int>>>> nestedElements = new Dictionary<string, Tuple<int, List<Tuple<string, int>>>>();

                while (await reader.ReadAsync())
                {
                    string componentKey = reader.GetString(0);
                    int componentCount = reader.GetInt32(1);
                    string elementName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    int elementCount = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                    if (!nestedElements.ContainsKey(componentKey))
                    {
                        nestedElements[componentKey] = new Tuple<int, List<Tuple<string, int>>>(componentCount, new List<Tuple<string, int>>());
                    }

                    if (!string.IsNullOrEmpty(elementName))
                    {
                        nestedElements[componentKey].Item2.Add(new Tuple<string, int>(elementName, elementCount));
                    }
                }

                foreach (var component in nestedElements)
                {
                    components.Add(new Tuple<string, int, List<Tuple<string, int>>>(component.Key, component.Value.Item1, component.Value.Item2));
                }
            }
        }
    }

    return components;
}
        /// <summary>
        /// создание отчета для компонентов
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="components"></param>
        private void CreateWordDocument(string componentName, List<Tuple<string, int, List<Tuple<string, int>>>> components)
        {
            Word.Application wordApp = new Word.Application();
            Word.Document wordDoc = wordApp.Documents.Add();
           
            Word.Table table = wordDoc.Tables.Add(wordDoc.Content, components.Count + 1, 2, Type.Missing, Type.Missing);
            table.Range.ParagraphFormat.SpaceBefore = 12;

            table.Borders.Enable = 1; // Включаем границы таблицы
            table.Columns[1].SetWidth(400, Word.WdRulerStyle.wdAdjustNone); // Устанавливаем ширину первого столбца
            table.Columns[2].SetWidth(400, Word.WdRulerStyle.wdAdjustNone); // Устанавливаем ширину второго столбца
            // Заполняем заголовки таблицы
            table.Cell(1, 1).Range.Text = "Название элемента";
            table.Cell(1, 2).Range.Text = "Количество";

            // Заполняем данные таблицы
            int rowIndex = 2;
            // Индекс строки для вставки данных
            foreach (var component in components)
            {
                if (component.Item3.Count > 0)
                {
                    foreach (var element in component.Item3)
                    {
                        table.Cell(rowIndex, 1).Range.Text = element.Item1;
                        table.Cell(rowIndex, 2).Range.Text = element.Item2.ToString();
                        // Добавляем новую строку в таблицу
                        table.Rows.Add(Type.Missing);
                        int lastRowIndex = table.Rows.Count;
                        rowIndex = lastRowIndex;
                    }
                }
                else
                {
                    table.Cell(rowIndex, 1).Range.Text = component.Item1;
                    table.Cell(rowIndex, 2).Range.Text = component.Item2.ToString();
                }
            }
            // Отображаем документ Word
            wordApp.Visible = true;
            wordDoc.Activate();
        }
    }
}
