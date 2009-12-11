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

				if( _integratedAuthCheckBox.IsChecked.GetValueOrDefault(false) ) {
                    info.Username = "IntegratedAuthentication";
				} else {
		            info.Username = _usernameTextBox.Text;
		            info.Password = _passwordTextBox.Text;
		            
				}
                return info;
            }
            set
            {
                if (value.Username == "IntegratedAuthentication") {
                    _usernameTextBox.IsEnabled = false;
                    _passwordTextBox.IsEnabled = false;

                    _integratedAuthCheckBox.IsChecked = true;
                } else {
                    _usernameTextBox.Text = value.Username;
                    _passwordTextBox.Text = value.Password;

                    _usernameTextBox.IsEnabled = true;
                    _passwordTextBox.IsEnabled = true;

                    _ssAuthCheckBox.IsChecked = true;
                }
                _serverTextBox.Text = value.Server;
                _initialCatalogTextBox.Text = value.InitialCatalog;
            }
        }

        private void HandleTypeChanged(object sender, RoutedEventArgs e) {
            // when building initial UI, skip handling
            if (_usernameTextBox == null || _passwordTextBox == null) {
                return;
            }

            if (sender == _integratedAuthCheckBox && _integratedAuthCheckBox.IsChecked.GetValueOrDefault(false)) {
                _usernameTextBox.IsEnabled = false;
                _passwordTextBox.IsEnabled = false;
            } else {
                _usernameTextBox.IsEnabled = true;
                _passwordTextBox.IsEnabled = true;
            }
        }

    }
}
