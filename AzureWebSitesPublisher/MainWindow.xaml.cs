using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AzureWebSitesPublisher
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Elysium.Controls.Window
    {
        public MainWindowViewModel ViewModel
        {
            get { return this.DataContext as MainWindowViewModel; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "PublishSettings File (*.PublishSettings)|*.PublishSettings";
            if (dialog.ShowDialog() == true)
            {
                this.textBox1.Text = dialog.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Zip File (*.zip) | *.zip";
            if (dialog.ShowDialog() == true)
            {
                this.textBox2.Text = dialog.FileName;
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var publishSettingsPath = this.ViewModel.PublishSettingsPath;
            var sourcePath = this.ViewModel.SourcePath;

            try
            {
                this.buttonDeploy.IsEnabled = false;
                this.progressBar.Visibility = Visibility.Visible;
                var result = await Task.Run(() =>
                {
                    return WebSitePublisherHelpler.Publish(publishSettingsPath, sourcePath);
                });
                this.buttonDeploy.IsEnabled = true;
                this.progressBar.Visibility = Visibility.Hidden;
                MessageBox.Show("Deployment finsihed.", this.Title);
            }
            catch (Exception ex)
            {
                this.buttonDeploy.IsEnabled = true;
                this.progressBar.Visibility = Visibility.Hidden;
                MessageBox.Show(ex.Message, this.Title);
            }
        }
    }
}
