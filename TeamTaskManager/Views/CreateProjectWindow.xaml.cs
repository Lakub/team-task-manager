using System;
using System.Collections.Generic;
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

namespace TeamTaskManager.Views
{
    /// <summary>
    /// Logika interakcji dla klasy CreateProjectWindow.xaml
    /// </summary>
    public partial class CreateProjectWindow : Window
    {
        public CreateProjectWindow()
        {
            InitializeComponent();
        }
    private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Nazwa zadania jest wymagana!", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }



            MessageBox.Show($"Utworzono zadanie: {TitleTextBox.Text}", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }
    }
}
