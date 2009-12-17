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
                    if( _queryTextBox.CaretOffset <= pos ) {
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

            ExpandTreeItem(expandedItem);
        }

        private string GetHeaderText(object header) {
            StackPanel panel = (StackPanel)header;

            foreach (object child in panel.Children) {
                if (child is TextBlock) {
                    return ((TextBlock)child).Text;
                }
            }
            return string.Empty;
        }

        private void ExpandTreeItem(TreeViewItem expandedItem) {
            if (VerifyConnection() && !expandedItem.HasItems) {
                ForceCursor = true;
                Cursor = Cursors.Wait;
                List<string> objectsToAdd = null;
                string tag = null;
                string icon = null;
                if (expandedItem.Name == "_tablesTreeItem") {
                    objectsToAdd = _db.GetTables();
                    tag = "hasColumns";
                    icon = "table.png";
                } else if (expandedItem.Name == "_viewsTreeItem") {
                    objectsToAdd = _db.GetViews();
                    tag = "hasColumns";
                    icon = "view.png";
                } else if (expandedItem.Name == "_procsTreeItem") {
                    objectsToAdd = _db.GetProcedures();
                    icon = "procedure.png";
                } else if (expandedItem.Name == "_schemasTreeItem") {
                    objectsToAdd = _db.GetSchemas();
                    tag = "schema";
                    icon = "database.png";
                } else if (expandedItem.Tag != null && expandedItem.Tag.ToString().StartsWith("hasColumns")) {
                    string[] data = expandedItem.Tag.ToString().Split(':');
                    tag = GetHeaderText(expandedItem.Header);
                    if (data.Length > 1) {
                        objectsToAdd = _db.GetColumns(tag, data[2]);
                    } else {
                        objectsToAdd = _db.GetColumns(tag);
                    }
                    icon = "column.png";
                } else if (expandedItem.Tag != null && expandedItem.Tag.ToString().StartsWith("sub")) {
                    string[] data = expandedItem.Tag.ToString().Split(':');
                    if (data[1] == "table") {
                        objectsToAdd = _db.GetTables(data[2]);
                        icon = "table.png";
                    } else if (data[1] == "view") {
                        objectsToAdd = _db.GetViews(data[2]);
                        icon = "view.png";
                    } else {
                        objectsToAdd = _db.GetProcedures(data[2]);
                        icon = "procedure.png";
                    }
                    tag = "hasColumns:" + GetHeaderText(expandedItem.Header) + ":" + data[2];
                } else {
                    Cursor = null;
                    return;
                }

                foreach (string objectName in objectsToAdd) {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Header = BuildHeader(objectName, icon);
                    newItem.Expanded += HandleNavigationExpanded;
                    newItem.Tag = tag;
                    if (tag == "schema") {
                        AddSchemaChildren(newItem);
                    }

                    expandedItem.Items.Add(newItem);
                }
            }
            Cursor = null;
        }

        private StackPanel BuildHeader(string text, string iconName) {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;

            Image icon = new Image();
            icon.Source = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/SQLClient;component/Resources/" + iconName));
            icon.Width = 16;
            panel.Children.Add(icon);

            TextBlock block = new TextBlock();
            block.Text = text;
            panel.Children.Add(block);

            return panel;
        }

        private void AddSchemaChildren(TreeViewItem parent) {
            TreeViewItem tablesItem = new TreeViewItem();
            tablesItem.Header = "Tables";
            tablesItem.Expanded += HandleNavigationExpanded;
            tablesItem.Tag = "sub:table:" + parent.Header.ToString();
            parent.Items.Add(tablesItem);

            TreeViewItem viewsItem = new TreeViewItem();
            viewsItem.Header = "Views";
            viewsItem.Expanded += HandleNavigationExpanded;
            viewsItem.Tag = "sub:view:" + parent.Header.ToString();
            parent.Items.Add(viewsItem);

            TreeViewItem procsItem = new TreeViewItem();
            procsItem.Header = "Stored Procedures";
            procsItem.Expanded += HandleNavigationExpanded;
            procsItem.Tag = "sub:proc:" + parent.Header.ToString();
            parent.Items.Add(procsItem);
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
            //e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void HandleKey(object sender, KeyEventArgs e) {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) {
                SaveBuffer();
            }
        }

        private void HandleRefresh(object sender, RoutedEventArgs e) {
            MenuItem item = (MenuItem)sender;

            TreeViewItem itemToRefresh = null;
            if (item.Tag.ToString() == "table") {
                itemToRefresh = _tablesTreeItem;
            } else if (item.Tag.ToString() == "view") {
                itemToRefresh = _viewsTreeItem;
            } else if (item.Tag.ToString() == "proc") {
                itemToRefresh = _procsTreeItem;
            } else {
                throw new ApplicationException(string.Format("Unrecognized control requested a tree refresh. Tag: '{0}'", item.Tag));
            }

            itemToRefresh.Items.Clear();

            ExpandTreeItem(itemToRefresh);
        }
    }
}
