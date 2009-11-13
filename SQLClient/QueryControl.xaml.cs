using System;
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
                _resultsGrid.DataContext = result.Tables[0].DefaultView;
            }
            _statusText.Text = "";
            _statusProgress.BeginAnimation(RangeBase.ValueProperty, null); // stop
            _cancelButton.IsEnabled = false;
        }

        private void HandleCancel(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            if (button.IsEnabled) {
                _queryWorker.CancelAsync();
            }
        }

        private bool VerifyConnection() {
            if (_db == null) {
                MessageBox.Show("How do you expect to run a query when you aren't connected?", "Hey Dummy!",
                                MessageBoxButton.OK, MessageBoxImage.Hand);
                return false;
            }
            return true;
        }
    }
}
