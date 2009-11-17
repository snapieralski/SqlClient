using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SQLClient
{
    [Serializable]
    public class ConnectionInfo
    {
        public string Name { get; set; }
        public string Server { get; set; }
        public string InitialCatalog { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }

        public string ConnectionString
        {
            get
            {
                string connString;
                if (Type == "Oracle")
                {
                    OracleConnectionStringBuilder connStrBuilder = new OracleConnectionStringBuilder();
                    connStrBuilder.DataSource = Server;
                    connStrBuilder.UserID = Username;
                    connStrBuilder.Password = Password;
                    connStrBuilder.IntegratedSecurity = false;

                    connString = connStrBuilder.ConnectionString;
                }
                else if (Type == "SQL Server")
                {
                    SqlConnectionStringBuilder connStrBuilder = new SqlConnectionStringBuilder();
                    connStrBuilder.DataSource = Server;
                    connStrBuilder.InitialCatalog = InitialCatalog;
                    connStrBuilder.UserID = Username;
                    connStrBuilder.Password = Password;
                    connStrBuilder.IntegratedSecurity = false;

                    connString = connStrBuilder.ConnectionString;
                } else {
                    throw new ApplicationException("Unknown server type: " + Type);
                }
                return connString;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            bool result = false;
            ConnectionInfo compareTo = obj as ConnectionInfo;
            if (compareTo != null)
            {
                result = Name.Equals(compareTo.Name);
            }
            return result;
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }
    }
}
