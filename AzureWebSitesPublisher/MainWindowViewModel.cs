using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AzureWebSitesPublisher
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _publishSettingsPath;
        private string _sourcePath;

        public string PublishSettingsPath
        {
            get { return this._publishSettingsPath; }
            set { this.SetProperty(ref this._publishSettingsPath, value); }
        }

        public string SourcePath
        {
            get { return this._sourcePath; }
            set { this.SetProperty(ref this._sourcePath, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            var h = this.PropertyChanged;
            if (h != null) { h(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
