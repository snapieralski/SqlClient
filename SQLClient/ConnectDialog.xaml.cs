using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SQLClient
{
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog : Window
    {
        private ObservableCollection<ConnectionInfo> _savedConnections;

        public string ServerType {
            get {
                return ((ComboBoxItem) _serverTypeComboBox.SelectedItem).Content.ToString();
            }
        }

        public string ServerUrl {
            get {
                return _serverTextBox.Text;
            }
        }

        public string Username {
            get {
                return _usernameTextBox.Text;
            }
        }

        public string Password {
            get {
                return _passwordTextBox.Text;
            }
        }

        public string InitialCatalog {
            get {
                return _initCatalogTextBox.Text;
            }
        }

        public string Name {
            get {
                return _nameTextBox.Text;
            }
        }

        public ConnectDialog()
        {
            InitializeComponent();
        }

        private void HandleConnectionSelected(object sender, SelectionChangedEventArgs e) {
            ConnectionInfo info = (ConnectionInfo)_savedConnectionsListBox.SelectedValue;
            if (info != null) {
                _passwordTextBox.Text = info.Password;
                _serverTextBox.Text = info.Server;
                foreach (ComboBoxItem item in _serverTypeComboBox.Items) {
                    item.IsSelected = item.Content.ToString() == info.Type;
                }

                _usernameTextBox.Text = info.Username;
                _nameTextBox.Text = info.Name;
                _initCatalogTextBox.Text = info.InitialCatalog;
            }
        }

        private void HandleCancel(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void HandleOK(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void HandleDelete(object sender, RoutedEventArgs e)
        {
            int indexToDelete = _savedConnectionsListBox.SelectedIndex;
            if (indexToDelete < 0)
            {
                MessageBox.Show("You have to select something to delete.", "Hey Dummy!", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                _savedConnections.RemoveAt(indexToDelete);
                SaveConnectionInfo();
            }
        }

        private void HandleSave(object sender, RoutedEventArgs e)
        {
            ConnectionInfo info = new ConnectionInfo();
            info.InitialCatalog = InitialCatalog;
            info.Name = _nameTextBox.Text; 
            info.Password = Password;
            info.Server = ServerUrl;
            info.Type = ServerType;
            info.Username = Username;

            if (!_savedConnections.Contains(info))
            {
                _savedConnections.Add(info);
                SaveConnectionInfo();
            }
            else
            {
                MessageBox.Show(string.Format("There is already a connection named '{0}'", _nameTextBox.Text), "Cannot Add Connection", MessageBoxButton.OK, MessageBoxImage.Stop);  
            }
            // TODO: implement edit -- can't search by name -- what happens when we change name?
        }

        private void SaveConnectionInfo()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<ConnectionInfo>));

            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SqlClientSavedConnections.xml");
            TextWriter writer = File.CreateText(path);

            serializer.Serialize(writer, _savedConnections);
        }

        private void HandleLoad(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SqlClientSavedConnections.xml");
            if (File.Exists(path))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<ConnectionInfo>));
                    _savedConnections = (ObservableCollection<ConnectionInfo>)serializer.Deserialize(File.OpenRead(path));
                }
                catch (Exception ex)
                {
                    MessageBoxResult result = MessageBox.Show("Found an error in the saved connections file:\n" + ex.Message + "\nShould we delete it?",
                        "Error loading connections", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (MessageBoxResult.Yes == result)
                    {
                        File.Delete(path);
                        _savedConnections = new ObservableCollection<ConnectionInfo>();
                    }
                }
            }
            else
            {
                _savedConnections = new ObservableCollection<ConnectionInfo>();
            }
            _savedConnectionsListBox.DataContext = _savedConnections;
        }
    }
}
