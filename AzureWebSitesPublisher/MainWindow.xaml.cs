using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private void buttonBrowseProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "PublishSettings File (*.PublishSettings)|*.PublishSettings";
            if (dialog.ShowDialog() == true)
            {
                this.textBoxProfile.Text = dialog.FileName;
            }
        }

        private void buttonBrowsePackage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Zip File (*.zip) | *.zip";
            if (dialog.ShowDialog() == true)
            {
                this.textBoxPackage.Text = dialog.FileName;
            }
        }

        private void buttonBrowseParameters_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "XML File (*.xml) | *.xml";
            if (dialog.ShowDialog() == true)
            {
                this.textBoxParameters.Text = dialog.FileName;
            }
        }

        private async void buttonDeploy_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindowViewModel.UpdateItemSource(this) == false)
            {
                return;
            }
                        
            try
            {
                this.IsEnabledControl(false);
                this.progressBar.Visibility = Visibility.Visible;

                var publishSettingsPath = this.ViewModel.PublishSettingsPath;
                var sourcePath = this.ViewModel.SourcePath;
                var parametersPath = this.ViewModel.ParametersPath;

                var result = await Task.Run(() =>
                {
                    return WebSitePublisherHelpler.Publish(publishSettingsPath, sourcePath, parametersPath);
                });
                this.progressBar.Visibility = Visibility.Hidden;

                var resultString = new StringBuilder();
                if (result != null)
                {
                    resultString.Append("Deployment finsihed." + Environment.NewLine);
                    resultString.Append(Environment.NewLine);
                    resultString.Append("Added: " + result.ObjectsAdded + Environment.NewLine);
                    resultString.Append("Updated: " + result.ObjectsUpdated + Environment.NewLine);
                    resultString.Append("Deleted: " + result.ObjectsDeleted + Environment.NewLine);
                    resultString.Append("Total errors: " + result.Errors + Environment.NewLine);
                    resultString.Append("Total changes: " + result.TotalChanges + Environment.NewLine);
                }
                else
                {
                    resultString.Append("Deployment failed." + Environment.NewLine);
                }
                MessageBox.Show(resultString.ToString(), this.Title);
            }
            catch (Exception ex)
            {
                this.progressBar.Visibility = Visibility.Hidden;
                MessageBox.Show(ex.Message, this.Title);
            }
            finally
            {
                this.IsEnabledControl(true);
            }
        }

        private void IsEnabledControl(bool isEnabled)
        {
            this.textBoxProfile.IsEnabled = isEnabled;
            this.buttonBrowseProfile.IsEnabled = isEnabled;

            this.textBoxPackage.IsEnabled = isEnabled;
            this.buttonBrowsePackage.IsEnabled = isEnabled;

            this.textBoxParameters.IsEnabled = isEnabled;
            this.buttonBrowseParameters.IsEnabled = isEnabled;

            this.buttonDeploy.IsEnabled = isEnabled;
        }
    }
}
