using System;
using System.Collections.Generic;
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
    /// Interaction logic for SqlServerConnectionControl.xaml
    /// </summary>
    public partial class SqlServerConnectionControl : UserControl, IConnectionControl
    {
        public SqlServerConnectionControl()
        {
            InitializeComponent();
        }

        public ConnectionInfo ConnectionInfo
        {
            get
            {
				ConnectionInfo info = new ConnectionInfo();
				info.Server = _serverTextBox.Text;
		        info.InitialCatalog = _initialCatalogTextBox.Text;
		        info.Type = "SQL Server";    

				if( _integratedAuthCheckBox.Checked ) {

				} else {
		            info.Username = _usernameTextBox.Text;
		            info.Password = _passwordTextBox.Text;
		            
				}
                return info;
            }
            set
            {
                _serverTextBox.Text = value.Server;
                _usernameTextBox.Text = value.Username;
                _passwordTextBox.Text = value.Password;
                _initialCatalogTextBox.Text = value.InitialCatalog;
            }
        }

    }
}
