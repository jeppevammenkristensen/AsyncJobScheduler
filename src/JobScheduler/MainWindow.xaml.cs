using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using JobScheduler.Model;
using JobScheduler.ViewModels;

namespace JobScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel
        {
            get { return this.Model<MainWindowViewModel>(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await ViewModel.StartJobAsync();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ViewModel.Cancel();
        }
    }
}

