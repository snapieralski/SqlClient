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
        private readonly BackgroundWorker _queryWorker;

        public MainWindow() {
            InitializeComponent();

            _queryWorker = new BackgroundWorker();
            _queryWorker.DoWork += DoWorkForQuery;
            _queryWorker.RunWorkerCompleted += HandleQueryWorkerCompleted;
            _queryWorker.WorkerSupportsCancellation = true;
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
                }
                else if (dialog.ServerType == "SQL Server") {
                    SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder();
                    connStrBuilder.DataSource = dialog.ServerUrl;
                    connStrBuilder.InitialCatalog = dialog.InitialCatalog;
                    connStrBuilder.UserID = dialog.Username;
                    connStrBuilder.Password = dialog.Password;
                    connStrBuilder.IntegratedSecurity = false;
                }
                else {
                    throw new ApplicationException("Unable to connect to DB Type: '" + dialog.ServerType + "'");
                }
            }
        }

        private void HandleRun(object sender, RoutedEventArgs e) {
            RunQuery();
        }

        private void HandleQueryBoxKey(object sender, KeyEventArgs e) {
            if (e.Key == Key.F5) {
                RunQuery();
            }
        }

        private void RunQuery() {
            if (VerifyConnection()) {
                _queryTextBox.Focus(); // we have to focus the query box to make SelectedText work

                string queryText = _queryTextBox.SelectedText;
                if (queryText == string.Empty) {
                    queryText = _queryTextBox.Text;
                }
                
                _statusText.Text = "Running Query";
                _cancelButton.IsEnabled = true;

                DoubleAnimation anim = new DoubleAnimation(100.0, new Duration(TimeSpan.FromSeconds(5)));
                anim.RepeatBehavior = RepeatBehavior.Forever;
                _statusProgress.BeginAnimation(RangeBase.ValueProperty, anim);

                _queryWorker.RunWorkerAsync(queryText);
            }
        }

        void DoWorkForQuery(object sender, DoWorkEventArgs e) {
            string queryToRun = (string)e.Argument;
            DataSet result = _db.ExecuteQuery(queryToRun);

            e.Result = result;
        }

        void HandleQueryWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                // TODO: display error info
            } else if (!e.Cancelled) {
                DataSet result = (DataSet)e.Result;
                _resultsGrid.DataContext = result.Tables[0].DefaultView;
            }
            _statusText.Text = "";
            _statusProgress.BeginAnimation(RangeBase.ValueProperty, null); // stop
            _cancelButton.IsEnabled = false;
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

        private void HandleCancel(object sender, RoutedEventArgs e) {
            Button button = (Button) sender;
            if(button.IsEnabled) {
                _queryWorker.CancelAsync();
            }
        }
    }
}