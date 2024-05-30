using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PracticProgramm
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static ConnectionData connection = new ConnectionData();
        public MainWindow()
        {
            InitializeComponent();
            OpenCreateConnect();
        }
        /// <summary>
        /// Метод для проверки существовании бд, таблиц
        /// </summary>
        public void OpenCreateConnect()
        {
            
            connection = connection.LoadFromFile();
            //connection.SetConnect();
            if (connection == null)
            {
                MessageBox.Show("Нужно написать данные для подключения бд в окне, и нажать на подключение");
            }
            else
            {
                helpText.Visibility = Visibility.Hidden;
                helServer.Visibility = Visibility.Hidden;
                helDatabase.Visibility = Visibility.Hidden;
                helpassword.Visibility = Visibility.Hidden;
                helUser.Visibility = Visibility.Hidden;

                TextServer.Visibility = Visibility.Hidden;
                TextPassword.Visibility = Visibility.Hidden;
                TextDatabase.Visibility = Visibility.Hidden;
                TextUser.Visibility = Visibility.Hidden;

                AddConnect.Visibility = Visibility.Hidden;

                List<string> NameDb = new List<string> { "AutoTable", "EngineComponents", "EngineComponents" };
                int CountTable = 0;
                foreach (string name in NameDb)
                {
                    bool tableExists = DatabaseQueries.CheckTableExists(name, connection.StringsPath());
                    if (tableExists == true)
                    {
                        CountTable += 1;
                    }
                }
                if (CountTable == 3)
                {
                    return;
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Каких-то таблиц нет, но мы их можем создать и добавить данные. Продолжить?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DatabaseQueries.CreateTables(connection.StringsPath());
                    }
                    else
                    {
                    }
                }
            }
        }
        /// <summary>
        /// открытие вкладки для изменения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditName_Click(object sender, RoutedEventArgs e)
        {
            ReditCompanent reditCompanent = new ReditCompanent();
            reditCompanent.Show();
            this.Close();
        }
        /// <summary>
        ///  открытие вкладки для удаления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void DeleteWpf_Click(object sender, RoutedEventArgs e)
        {
            DeleteWpf deleteWpf = new DeleteWpf();
            deleteWpf.Show();
            this.Close();
        }
        /// <summary>
        ///  открытие вкладки для добавления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void AddCompanents_Click(object sender, RoutedEventArgs e)
        {
            AddCompanents addCompanents = new AddCompanents();
            addCompanents.Show();
            this.Close();
        }
        /// <summary>
        ///  открытие вкладки для создания отчета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void MSWord_Click(object sender, RoutedEventArgs e)
        {
            WordWpf wordWpf = new WordWpf();
            wordWpf.Show();
            this.Close();
        }
        /// <summary>
        ///  Метод для подключения к бд 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void AddConnect_Click(object sender, RoutedEventArgs e)
        {

            string server = TextServer.Text;
            string database= TextDatabase.Text;
            string user = TextUser.Text;
            string password = TextPassword.Text;
            try
            {
                ConnectionData CON = new ConnectionData();

                CON.SaveToFile(server, database, user, password);
                MessageBox.Show("подключение успешно произошло");
                // Закрываем текущий экземпляр приложения
                Application.Current.Shutdown();

                // Запускаем новый экземпляр приложения
                Process.Start(Application.ResourceAssembly.Location);
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
