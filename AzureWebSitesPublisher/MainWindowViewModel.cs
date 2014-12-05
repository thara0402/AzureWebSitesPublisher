using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AzureWebSitesPublisher
{
    public class MainWindowViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private string _publishSettingsPath;
        private string _sourcePath;
        private Dictionary<string, IEnumerable> _errors = new Dictionary<string, IEnumerable>();

        public string PublishSettingsPath
        {
            get { return this._publishSettingsPath; }
            set
            {
                this.SetProperty(ref this._publishSettingsPath, value);
                if (string.IsNullOrEmpty(value))
                {
                    this._errors["PublishSettingsPath"] = new[] { "PublishSettings ファイルを入力してください" };
                }
                else
                {
                    this._errors["PublishSettingsPath"] = null;
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
                    this._errors["SourcePath"] = new[] { "WebDeploy Package ファイルを入力してください" };
                }
                else
                {
                    this._errors["SourcePath"] = null;
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
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private void OnErrorsChanged([CallerMemberName] string propertyName = null)
        {
            var h = this.ErrorsChanged;
            if (h != null) { h(this, new DataErrorsChangedEventArgs(propertyName)); }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            IEnumerable error = null;
            this._errors.TryGetValue(propertyName, out error);
            return error;
        }

        public bool HasErrors
        {
            get { return this._errors.Values.Any(e => e != null); }
        }

        public static bool UpdateItemSource(FrameworkElement item)
        {
            var result = true;
            var infos = item.GetType().GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in infos)
            {
                if (field.FieldType == typeof(DependencyProperty))
                {
                    var dp = (DependencyProperty)field.GetValue(null);
                    var be = item.GetBindingExpression(dp);
                    if (be != null)
                    {
                        if (be.ParentBinding.UpdateSourceTrigger == UpdateSourceTrigger.Explicit)
                        {
                            be.UpdateSource();
                            if (be.ValidationErrors != null)
                            {
                                result = false;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
            {
                var child = VisualTreeHelper.GetChild(item, i) as FrameworkElement;
                if (child != null)
                {
                    if (UpdateItemSource(child) == false)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }
    }
}
