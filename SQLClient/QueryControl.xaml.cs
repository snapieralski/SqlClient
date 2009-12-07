using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using SQLClient.DBInteraction;

namespace SQLClient {
    /// <summary>
    /// Interaction logic for QueryControl.xaml
    /// </summary>
    public partial class QueryControl : UserControl {
        private IDatabase _db;
        private readonly BackgroundWorker _queryWorker;
        
        public QueryControl() {
            InitializeComponent();

            _queryWorker = new BackgroundWorker();
            _queryWorker.DoWork += DoWorkForQuery;
            _queryWorker.RunWorkerCompleted += HandleQueryWorkerCompleted;
            _queryWorker.WorkerSupportsCancellation = true;
        }

        public IDatabase Database {
            get {
                return _db;
            }
            set {
                _db = value;
            }
        }
        
        private void HandleQueryBoxKey(object sender, KeyEventArgs e) {
            if (e.Key == Key.F5) {
                RunQuery();
            } else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control) {
                string text = _queryTextBox.Text;
                string[] lines = text.Split('\n');
                int pos = 0;
                string lineToDuplicate = string.Empty;
                foreach(string line in lines) {
                    pos += line.Length + 1;
                    if( _queryTextBox.CaretIndex <= pos ) {
                        lineToDuplicate = line;
                    }
                }

                _queryTextBox.AppendText(Environment.NewLine + lineToDuplicate);
               
                // TODO: put dup on next line instead of at the end
            }
        }

        private void HandleRun(object sender, RoutedEventArgs e) {
            RunQuery();
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

        private void DoWorkForQuery(object sender, DoWorkEventArgs e) {
            string queryToRun = (string)e.Argument;
            DataSet result = _db.ExecuteQuery(queryToRun);

            e.Result = result;
        }

        private void HandleQueryWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                // TODO: display error info
            } else if (!e.Cancelled) {
                DataSet result = (DataSet)e.Result;
                if (result.Tables.Count > 0) {
                    _resultsGrid.DataContext = result.Tables[0].DefaultView;
                } else {
                    MessageBox.Show("Query completed but no data was returned.", "Something weird happened.",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            _statusText.Text = "";
            _statusProgress.BeginAnimation(RangeBase.ValueProperty, null); // stop
            _cancelButton.IsEnabled = false;
        }

        private void HandleCancel(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            
            _queryWorker.CancelAsync();
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
            TreeViewItem expandedItem = (TreeViewItem)sender;

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

        private void HandleSave(object sender, RoutedEventArgs e) {
            SaveBuffer();
        }

        private void SaveBuffer() {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = "Query";
            saveDialog.DefaultExt = ".sql";
            saveDialog.Filter = "SQL documents (.sql)|*.sql|All Files (*.*)|*";
            bool? result = saveDialog.ShowDialog();
            if (result == true) {
                string filename = saveDialog.FileName;

                File.WriteAllText(filename, _queryTextBox.Text);
            }
        }

        private void HandleOpen(object sender, RoutedEventArgs e) {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "SQL documents (.sql)|*.sql|All Files (*.*)|*";

            bool? result = openDialog.ShowDialog();
            if( result == true ) {
                string filename = openDialog.FileName;

                _queryTextBox.Text = File.ReadAllText(filename);
            } 
        }

        /// <summary>
        /// Puts line numbers in the row header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleLoadingRow(object sender, Microsoft.Windows.Controls.DataGridRowEventArgs e) {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void HandleKey(object sender, KeyEventArgs e) {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) {
                SaveBuffer();
            }
        }
    }
}
