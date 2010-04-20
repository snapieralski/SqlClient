using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using ICSharpCode.AvalonEdit;

namespace SQLClient {
    /// <summary>
    /// Interaction logic for FindDialog.xaml
    /// </summary>
    public partial class FindDialog : Window {
        private TextEditor _editor;
        private MatchCollection _matches;
        private int _matchesPosition;
        
        public FindDialog() {
            InitializeComponent();
        }

        public TextEditor Editor {
            get {
                return _editor;
            }
            set {
                _editor = value;
            }
        }

        private void DoFind(object sender, RoutedEventArgs e) {
            if (_matches == null) {
                _matches = Regex.Matches(_editor.Text, _findTextBox.Text);
                _matchesPosition = 0;
            }
            _editor.Select(_matches[_matchesPosition].Index, _matches[_matchesPosition].Length);
            _matchesPosition++;
            // flip back to the top
            if (_matchesPosition >= _matches.Count) {
                _matchesPosition = 0;
            }
        }

        private void DoReplace(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(_editor.SelectedText)) {
                int startPos = _editor.SelectionStart;
                int len = _editor.SelectionLength;
                _editor.Text = _editor.Text.Remove(startPos, len).Insert(startPos, _replaceTextBox.Text);
            }
            _matches = null;
            _matchesPosition = 0;
            DoFind(null, null);
        }

        private void DoReplaceAll(object sender, RoutedEventArgs e) {
            _editor.Text = Regex.Replace(_editor.Text, _findTextBox.Text, _replaceTextBox.Text);
        }

        private void DoClose(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e) {
            // when the user types something new in the find box, reset the list of matches
            _matches = null;
            _matchesPosition = 0;
        }

        private void HandleWindowActivated(object sender, EventArgs e) {
            _findTextBox.Focus();
        }
    }
}
