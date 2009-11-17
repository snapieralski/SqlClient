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

using SQLClient.ConnectUI;

namespace SQLClient
{
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog : Window
    {
        private ObservableCollection<ConnectionInfo> _savedConnections;
        private ConnectUI.IConnectionControl _connCtrl;

        public string ServerType {
            get {
                return ((ComboBoxItem) _serverTypeComboBox.SelectedItem).Content.ToString();
            }
        }

        public string ConnectionName {
            get {
                return _nameTextBox.Text;
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connCtrl.ConnectionInfo.ConnectionString;
            }
        }

        public ConnectDialog()
        {
            InitializeComponent();
        }

        private void HandleTypeSelected(object sender, SelectionChangedEventArgs e)
        {
            _connCtrlPanel.Children.Clear();
            if (ServerType == "Oracle")
            {
                _connCtrl = new OracleConnectionControl();
                
            }
            else if (ServerType == "SQL Server")
            {
                _connCtrl = new SqlServerConnectionControl();
            }
            else 
            {
                throw new ArgumentException("Unrecognized type: " + ServerType);
            }
            _connCtrlPanel.Children.Add((UIElement)_connCtrl);
            ConnectionInfo info = (ConnectionInfo)_savedConnectionsListBox.SelectedValue;
            if (info != null)
            {
                _connCtrl.ConnectionInfo = info;
            }
        }

        private void HandleConnectionSelected(object sender, SelectionChangedEventArgs e) {
            ConnectionInfo info = (ConnectionInfo)_savedConnectionsListBox.SelectedValue;
            if (info != null) {
                foreach (ComboBoxItem item in _serverTypeComboBox.Items) {
                    item.IsSelected = item.Content.ToString() == info.Type;
                }
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
            ConnectionInfo info = _connCtrl.ConnectionInfo;
            info.Name = ConnectionName; 
            
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

        private void HandleDblClick(object sender, MouseButtonEventArgs e) {
            if( _savedConnectionsListBox.SelectedIndex >= 0) {
                DialogResult = true;
            }
        }
    }
}
