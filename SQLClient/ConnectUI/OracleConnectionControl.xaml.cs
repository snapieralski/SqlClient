using System;
using System.Collections.Generic;
using System.Data;
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

namespace SQLClient.ConnectUI
{
    /// <summary>
    /// Interaction logic for OracleConnectionControl.xaml
    /// </summary>
    public partial class OracleConnectionControl : UserControl, IConnectionControl
    {
        public OracleConnectionControl()
        {
            InitializeComponent();
        }


        public ConnectionInfo ConnectionInfo
        {
            get {
                ConnectionInfo info = new ConnectionInfo();
                info.Server = _serverTextBox.Text;
                info.Username = _usernameTextBox.Text;
                info.Password = _passwordTextBox.Text;
                info.InitialCatalog = info.Username;
                info.Type = "Oracle";

                return info;
            }
            set {
                _serverTextBox.Text = value.Server;
                _usernameTextBox.Text = value.Username;
                _passwordTextBox.Text = value.Password;
            }
        }
    }
}
