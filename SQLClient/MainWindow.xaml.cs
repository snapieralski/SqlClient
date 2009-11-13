using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SQLClient.DBInteraction;

namespace SQLClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private IDatabase _db;

        public MainWindow() {
            InitializeComponent();
        }

        private void HandleRun(object sender, RoutedEventArgs e) {
        }

        private void HandleConnect(object sender, RoutedEventArgs e) {
            ConnectDialog dialog = new ConnectDialog();

            if (dialog.ShowDialog().GetValueOrDefault(false)) {
                if (dialog.ServerType == "Oracle") {
                    OracleConnectionStringBuilder connStrBuilder = new OracleConnectionStringBuilder();
                    connStrBuilder.DataSource = dialog.ServerUrl;
                    connStrBuilder.UserID = dialog.Username;
                    connStrBuilder.Password = dialog.Password;
                    connStrBuilder.IntegratedSecurity = false;
                    _db = new OracleDatabase(connStrBuilder.ConnectionString);
                } else if (dialog.ServerType == "SQL Server") {
                    SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder();
                    connStrBuilder.DataSource = dialog.ServerUrl;
                    connStrBuilder.InitialCatalog = dialog.InitialCatalog;
                    connStrBuilder.UserID = dialog.Username;
                    connStrBuilder.Password = dialog.Password;
                    connStrBuilder.IntegratedSecurity = false;
                    // TODO: populate the treeview
                } else {
                    throw new ApplicationException("Unable to connect to DB Type: '" + dialog.ServerType + "'");
                }
            }
        }

        private void HandleQueryBoxKey(object sender, KeyEventArgs e) {
            if (e.Key == Key.F5) {
                DataSet result = _db.ExecuteQuery(_queryTextBox.Text);
                _resultsGrid.DataContext = result.Tables[0].DefaultView;
            }
        }

        private void HandleNavigationExpanded(object sender, RoutedEventArgs e) {
            TreeViewItem expandedItem = (TreeViewItem) sender;
            if( !expandedItem.HasItems ) {
                List<string> objectsToAdd = null;
                string tag = null;
                if (expandedItem.Name == "_tablesTreeItem") {
                    objectsToAdd = _db.GetTables();
                    tag = "hasColumns";
                }
                else if (expandedItem.Name == "_viewsTreeItem") {
                    objectsToAdd = _db.GetViews();
                    tag = "hasColumns";
                }
                else if (expandedItem.Name == "_procsTreeItem") {
                    objectsToAdd = _db.GetProcedures();
                } else if( expandedItem.Tag.Equals("hasColumns")) {
                    
                } else {
                    return; // do nothing here
                }

                foreach(string objectName in objectsToAdd) {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Header = objectName;
                    
                    expandedItem.Items.Add(newItem);
                }
            }
        }
    }
}
