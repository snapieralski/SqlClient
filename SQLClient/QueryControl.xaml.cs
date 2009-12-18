using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using Microsoft.Windows.Controls;
using SQLClient.Model;

namespace SQLClient {
    /// <summary>
    /// Interaction logic for QueryControl.xaml
    /// </summary>
    public partial class QueryControl : UserControl {
        private readonly BackgroundWorker _queryWorker;
        private QueryController _controller;

        public QueryControl() {
            InitializeComponent();

            Stream xshdStream =
                Assembly.GetAssembly(GetType()).GetManifestResourceStream("SQLClient.Resources.SQL-Mode.xshd");
            if (xshdStream != null) {
                XmlReader xshdReader = new XmlTextReader(xshdStream);
                _queryTextBox.SyntaxHighlighting = HighlightingLoader.Load(xshdReader, null);
            }
            _queryWorker = new BackgroundWorker();
            _queryWorker.DoWork += DoWorkForQuery;
            _queryWorker.RunWorkerCompleted += HandleQueryWorkerCompleted;
            _queryWorker.WorkerSupportsCancellation = true;
        }

        public QueryController Controller {
            get { return _controller; }
            set {
                _controller = value;
                _tablesTreeItem.Tag = _controller.Server.DefaultCatalog;
                _viewsTreeItem.Tag = _controller.Server.DefaultCatalog;
                _procsTreeItem.Tag = _controller.Server.DefaultCatalog;
            }
        }

        private void HandleQueryBoxKey(object sender, KeyEventArgs e) {
            if (e.Key == Key.F5) {
                RunQuery();
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control) {
                string text = _queryTextBox.Text;
                string[] lines = text.Split('\n');
                int pos = 0;
                string lineToDuplicate = string.Empty;
                foreach (string line in lines) {
                    pos += line.Length + 1;
                    if (_queryTextBox.CaretOffset <= pos) {
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

        private void DoWorkForQuery(object sender, DoWorkEventArgs e) {
            string queryToRun = (string) e.Argument;
            DataSet result = Controller.GetQueryResult(queryToRun);

            e.Result = result;
        }

        private void HandleQueryWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                // TODO: display error info
            }
            else if (!e.Cancelled) {
                DataSet result = (DataSet) e.Result;
                if (result.Tables.Count > 0) {
                    _resultsGrid.DataContext = result.Tables[0].DefaultView;
                }
                else {
                    MessageBox.Show("Query completed but no data was returned.", "Something weird happened.",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            _statusText.Text = "";
            _statusProgress.BeginAnimation(RangeBase.ValueProperty, null); // stop
            _cancelButton.IsEnabled = false;
        }

        private void HandleCancel(object sender, RoutedEventArgs e) {
            _queryWorker.CancelAsync();
        }

        private void HandleNavigationExpanded(object sender, RoutedEventArgs e) {
            TreeViewItem expandedItem = (TreeViewItem) sender;

            ExpandTreeItem(expandedItem);
        }

        private static string GetHeaderText(object header) {
            StackPanel panel = (StackPanel) header;

            foreach (object child in panel.Children) {
                if (child is TextBlock) {
                    return ((TextBlock) child).Text;
                }
            }
            return string.Empty;
        }

        private void ExpandTreeItem(TreeViewItem expandedItem) {
            if (!expandedItem.HasItems) {
                ForceCursor = true;
                Cursor = Cursors.Wait;
                List<ITreeItem> objectsToAdd = null;
                string icon = null;
                if (expandedItem.Tag != null && ((ITreeItem)expandedItem.Tag).TreeItemType == TreeItemType.Catalog) {
                    Catalog catalog = (Catalog) expandedItem.Tag;
                    if (GetHeaderText(expandedItem.Header) == "Tables") {
                        objectsToAdd = _controller.GetTables(catalog);
                        icon = "table.png";
                    }
                    else if (GetHeaderText(expandedItem.Header) == "Views") {
                        objectsToAdd = _controller.GetViews(catalog);
                        icon = "view.png";
                    }
                    else {
                        objectsToAdd = _controller.GetProcedures(catalog);
                        icon = "procedure.png";
                    }
                }
                else if (expandedItem.Name == "_schemasTreeItem") {
                    objectsToAdd = _controller.GetCatalogs();
                    icon = "database.png";
                } else if (expandedItem.Tag != null &&
                           (((ITreeItem)expandedItem.Tag).TreeItemType == TreeItemType.Table ||
                            ((ITreeItem)expandedItem.Tag).TreeItemType == TreeItemType.View)) {
                    objectsToAdd = _controller.GetColumns((DataContainerBase)expandedItem.Tag);
                    icon = "column.png";
                } else {
                    Cursor = null;
                    return;
                }

                foreach (ITreeItem item in objectsToAdd) {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Header = BuildHeader(item.Name, icon);
                    newItem.Expanded += HandleNavigationExpanded;
                    newItem.Tag = item;

                    if (item.TreeItemType == TreeItemType.Catalog) {
                        AddSchemaChildren(newItem, item);
                    }

                    expandedItem.Items.Add(newItem);
                }
            }
            Cursor = null;
        }

        private static StackPanel BuildHeader(string text, string iconName) {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;

            Image icon = new Image();
            icon.Source = BitmapFrame.Create(new Uri("pack://application:,,,/SQLClient;component/Resources/" + iconName));
            icon.Width = 16;
            panel.Children.Add(icon);

            TextBlock block = new TextBlock();
            block.Text = text;
            panel.Children.Add(block);

            return panel;
        }

        private void AddSchemaChildren(TreeViewItem parent, ITreeItem item) {
            TreeViewItem tablesItem = new TreeViewItem();
            tablesItem.Header = BuildHeader("Tables", "tables.png");
            tablesItem.Expanded += HandleNavigationExpanded;
            tablesItem.Tag = item;
            parent.Items.Add(tablesItem);

            TreeViewItem viewsItem = new TreeViewItem();
            viewsItem.Header = BuildHeader("Views", "views.png");
            viewsItem.Expanded += HandleNavigationExpanded;
            viewsItem.Tag = item;
            parent.Items.Add(viewsItem);

            TreeViewItem procsItem = new TreeViewItem();
            procsItem.Header = BuildHeader("Procedures", "procedures.png");
            procsItem.Expanded += HandleNavigationExpanded;
            procsItem.Tag = item;
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
            if (result == true) {
                string filename = openDialog.FileName;

                _queryTextBox.Text = File.ReadAllText(filename);
            }
        }

        /// <summary>
        /// Puts line numbers in the row header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleLoadingRow(object sender, DataGridRowEventArgs e) {
            //e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void HandleKey(object sender, KeyEventArgs e) {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) {
                SaveBuffer();
            }
        }

        private void HandleRefresh(object sender, RoutedEventArgs e) {
            MenuItem item = (MenuItem) sender;

            TreeViewItem itemToRefresh = null;
            if (item.Tag.ToString() == "table") {
                itemToRefresh = _tablesTreeItem;
            }
            else if (item.Tag.ToString() == "view") {
                itemToRefresh = _viewsTreeItem;
            }
            else if (item.Tag.ToString() == "proc") {
                itemToRefresh = _procsTreeItem;
            }
            else {
                throw new ApplicationException(string.Format(
                                                   "Unrecognized control requested a tree refresh. Tag: '{0}'", item.Tag));
            }

            itemToRefresh.Items.Clear();

            ExpandTreeItem(itemToRefresh);
        }
    }
}