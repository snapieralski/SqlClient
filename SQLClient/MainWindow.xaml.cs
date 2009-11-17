using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SQLClient.DBInteraction;

namespace SQLClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private IDatabase _db;

        public MainWindow() {
            InitializeComponent();
        }

        private bool VerifyConnection() {
            if (_db == null) {
                MessageBox.Show("How do you expect to run a query when you aren't connected?", "Hey Dummy!",
                                MessageBoxButton.OK, MessageBoxImage.Hand);
                return false;
            }
            return true;
        }


        private void HandleNavigationExpanded(object sender, RoutedEventArgs e) {
            TreeViewItem expandedItem = (TreeViewItem) sender;
            
            if (VerifyConnection() && !expandedItem.HasItems) {
                ForceCursor = true;
                Cursor = Cursors.Wait;
                List<string> objectsToAdd = null;
                string tag = null;
                if (expandedItem.Name == "_tablesTreeItem") {
                    objectsToAdd = _db.GetTables();
                    tag = "hasColumns";
                } else if (expandedItem.Name == "_viewsTreeItem") {
                    objectsToAdd = _db.GetViews();
                    tag = "hasColumns";
                } else if (expandedItem.Name == "_procsTreeItem") {
                    objectsToAdd = _db.GetProcedures();
                } else if (expandedItem.Tag != null && expandedItem.Tag.Equals("hasColumns")) {
                    objectsToAdd = _db.GetColumns(tag);
                    tag = expandedItem.Header.ToString();
                } else {
                    Cursor = null;
                    return;
                }

                foreach (string objectName in objectsToAdd) {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Header = objectName;
                    newItem.Expanded += HandleNavigationExpanded;
                    newItem.Tag = tag;

                    expandedItem.Items.Add(newItem);
                }
            }
            Cursor = null;
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
                    _db = new SqlDatabase(connStrBuilder.ConnectionString);
                } else {
                    throw new ApplicationException("Unable to connect to DB Type: '" + dialog.ServerType + "'");
                }

                TabItem tab = new TabItem();
                TextBlock headerText = new TextBlock();
                headerText.Text = dialog.Name;
                tab.Header = headerText;

                QueryControl queryCtrl = new QueryControl();
                queryCtrl.Database = _db;
                tab.Content = queryCtrl;

                _tabs.Items.Add(tab);
                _tabs.SelectedItem = tab;
            }
        }

        private void HandleTabClosing(object sender, RoutedEventArgs e) {
            DependencyObject clickedButton = (DependencyObject)sender;

            TabItem itemToRemove = null;
            foreach(TabItem item in _tabs.Items) {
                if( item.IsAncestorOf(clickedButton)) {
                    itemToRemove = item;
                    break;
                }
            }
            if( itemToRemove != null ) {
                _tabs.Items.Remove(itemToRemove);
            }
            
        }
    }
}