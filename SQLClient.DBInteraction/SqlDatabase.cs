using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SQLClient.DBInteraction
{
    public class SqlDatabase : IDatabase
    {
        private SqlConnection _conn;

        public SqlDatabase(string connectionString) {
            _conn = new SqlConnection(connectionString);
        }
             
        private List<string> GetDbObjectsAsList(string type, string db) {
            return GetQueryResultAsList(string.Format("select name from sysobjects where xtype = '{0}' order by name", type), db);
        }

        private List<string> GetQueryResultAsList(string query, string db) {
            List<string> objects = new List<string>();
            try {
                _conn.Open();
                _conn.ChangeDatabase(db);

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = _conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader != null) {
                    while (reader.Read()) {
                        objects.Add(reader.GetString(0));
                    }
                    reader.Close();
                } else {
                    objects.Add("Error: Reader returned from SQL Server was NULL.");
                }
            } catch (Exception ex) {
                objects.Clear();
                objects.Add(string.Format("Couldn't get info : {0}", ex.Message));
            } finally {
                _conn.Close();
            }
            return objects;
        }

        private String CurrentDb {
            get {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.ConnectionString = _conn.ConnectionString;

                return builder.InitialCatalog;
            }
        }

        public List<string> GetTables() {
            return GetTables(CurrentDb);
        }

        public List<string> GetViews() {
            return GetViews(CurrentDb);
            
        }

        public List<string> GetProcedures() {
            return GetProcedures(CurrentDb);
        }

        public List<string> GetTables(string schema) {
            return GetDbObjectsAsList("U", schema);
        }

        public List<string> GetViews(string schema) {
            return GetDbObjectsAsList("V", schema);
        }

        public List<string> GetProcedures(string schema) {
            return GetDbObjectsAsList("P", schema);
        }

        public List<string> GetSchemas() {
            return GetQueryResultAsList("select name from sys.databases order by name", CurrentDb);
        }

        public List<string> GetColumns(string parentName) {
            return GetColumns(parentName, CurrentDb);
        }

        public List<string> GetColumns(string parentName, string databaseName) {
            List<string> objects = new List<string>();
            try {
                _conn.Open();
                _conn.ChangeDatabase(databaseName);

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = _conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format(@"select c.name + ' ' +  t.name + '(' + cast(c.length as varchar(10)) + ')' 
                                                    from syscolumns c 
                                                    join sysobjects o 
                                                    	on o.id = c.id 
                                                    join systypes t
                                                    	on t.xtype = c.xtype
                                                    where o.name = '{0}'", parentName);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader != null) {
                    while (reader.Read()) {
                        objects.Add(reader.GetString(0));
                    }
                    reader.Close();
                } else {
                    objects.Add("Error: Reader returned from SQL Server was NULL.");
                }
            } catch (Exception ex) {
                objects.Clear();
                objects.Add(string.Format("Couldn't get columns for {0} info : {1}", parentName, ex.Message));
            } finally {
                _conn.Close();
            }
            return objects;
        }

        public DataSet ExecuteQuery(string query) {
            DataSet result = new DataSet();

            try {
                _conn.Open();

                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, _conn);
                dataAdapter.Fill(result);

            } catch (SqlException oe) {
                result.Tables.Add("Error");
                result.Tables[0].Columns.Add("ErrorInfo");
                result.Tables[0].Rows.Add(oe.Message);
            } finally {
                _conn.Close();
            }
            return result;
        }


        
    }
}
