using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AzureWebSitesPublisher
{
    public class MainWindowViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private string _publishSettingsPath;
        private string _sourcePath;

        public string PublishSettingsPath
        {
            get { return this._publishSettingsPath; }
            set
            {
                this.SetProperty(ref this._publishSettingsPath, value);
                if (string.IsNullOrEmpty(value))
                {
                    this.errors["PublishSettingsPath"] = new[] { "PublishSettings ファイルを入力してください" };
                }
                else
                {
                    this.errors["PublishSettingsPath"] = null;
                }
                this.OnErrorsChanged();
            }
        }

        public string SourcePath
        {
            get { return this._sourcePath; }
            set
            {
                this.SetProperty(ref this._sourcePath, value);
                if (string.IsNullOrEmpty(value))
                {
                    this.errors["SourcePath"] = new[] { "WebDeploy Package ファイルを入力してください" };
                }
                else
                {
                    this.errors["SourcePath"] = null;
                }
                this.OnErrorsChanged();
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            var h = this.PropertyChanged;
            if (h != null) { h(this, new PropertyChangedEventArgs(propertyName)); }
        }

        // INotifyDataErrorInfo
        private Dictionary<string, IEnumerable> errors = new Dictionary<string, IEnumerable>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private void OnErrorsChanged([CallerMemberName] string propertyName = null)
        {
            var h = this.ErrorsChanged;
            if (h != null) { h(this, new DataErrorsChangedEventArgs(propertyName)); }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            IEnumerable error = null;
            this.errors.TryGetValue(propertyName, out error);
            return error;
        }

        public bool HasErrors
        {
            get { return this.errors.Values.Any(e => e != null); }
        }
    }
}
