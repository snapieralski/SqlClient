using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;

namespace SQLClient.DBInteraction
{
    public class OracleDatabase : IDatabase {
        private OracleConnection _conn;

        public OracleDatabase(string connectionString) {
            _conn = new OracleConnection(connectionString);
        }

        

        private List<string> GetDbObjectsAsList(string type) {
            return GetDbObjectsAsList(type, CurrentUser);
        }

        private string CurrentUser {
            get {
                OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();
                builder.ConnectionString = _conn.ConnectionString;
                string userId = builder.UserID.ToUpper();
                return userId;
            }
        }

        private List<string> GetDbObjectsAsList(string type, string userId) {
            return GetQueryResultsAsList(string.Format("select object_name from all_objects where object_type = '{0}' and owner = '{1}' order by object_name", type, userId));
        }

        private List<string> GetQueryResultsAsList(string query) {
            List<string> objects = new List<string>();
            try {
                _conn.Open();

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = _conn;
                cmd.CommandType = CommandType.Text;

                cmd.CommandText = query;

                OracleDataReader reader = cmd.ExecuteReader();

                while(reader.Read()) {
                    objects.Add(reader.GetString(0));
                }

                reader.Close();
            }
            catch (Exception ex) {
                objects.Clear();
                objects.Add(string.Format("Couldn't get info : {0}", ex.Message));
            }
            finally {
                _conn.Close();
            }
            return objects;
        }
        public List<string> GetTables() {
            return GetDbObjectsAsList("TABLE");
        }

        public List<string> GetViews() {
            return GetDbObjectsAsList("VIEW");
        }

        public List<string> GetProcedures() {
            return GetDbObjectsAsList("PROCEDURE");
        }

        public List<string> GetTables(string schema) {
            return GetDbObjectsAsList("TABLE", schema);
        }

        public List<string> GetViews(string schema) {
            return GetDbObjectsAsList("VIEW", schema);
        }

        public List<string> GetProcedures(string schema) {
            return GetDbObjectsAsList("PROCEDURE", schema);
        }

        public List<string> GetSchemas() {
            return GetQueryResultsAsList("select username from all_users order by username");
        }

        public List<string> GetColumns(string parentName) {
            return GetColumns(parentName, CurrentUser);
        }

        private const int COLUMN_NAME = 0;
        private const int DATA_TYPE = 1;
        private const int PRECISION = 2;
        private const int SCALE = 3;
        private const int CHAR_COL_LEN = 4;
        private const int NULLABLE = 5;

        public List<string> GetColumns(string parentName, string schema) {
            List<string> objects = new List<string>();
            try {
                _conn.Open();

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = _conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("select column_name, data_type, data_precision, data_scale, char_col_decl_length, nullable from all_tab_columns where table_name = '{0}' and owner = '{1}'", parentName, schema);

                OracleDataReader reader = cmd.ExecuteReader();

                StringBuilder columnText = new StringBuilder();
                while(reader.Read()) {
                    // clear out the last column
                    if( columnText.Length > 0 ) {
                        columnText.Remove(0, columnText.Length);
                    }
                    // build text for this column
                    columnText.AppendFormat("{0} {1}", reader.GetString(COLUMN_NAME), reader.GetString(DATA_TYPE));
                    if (!reader.IsDBNull(PRECISION)) {
                        columnText.AppendFormat(" ({0}", reader.GetInt32(PRECISION));
                        if (!reader.IsDBNull(SCALE)) {
                            columnText.AppendFormat(", {0}", reader.GetInt32(SCALE));
                        }
                        columnText.Append(")");
                    } else if (!reader.IsDBNull(CHAR_COL_LEN)) {
                        columnText.AppendFormat("({0})", reader.GetInt32(CHAR_COL_LEN));
                    }

                    columnText.AppendFormat("{0} NULL", reader.GetString(NULLABLE) == "Y" ? string.Empty : " NOT");
                    objects.Add(columnText.ToString());
                }

                reader.Close();
            }
            catch (Exception ex) {
                objects.Clear();
                objects.Add(string.Format("Couldn't get {0} column info for: {1}", parentName, ex.Message));
            }
            finally {
                _conn.Close();
            }
            return objects;
        }

        public DataSet ExecuteQuery(string query) {
            DataSet result = new DataSet();

            string[] statements = query.Split(';');

            try {
                _conn.Open();

                foreach (string stmt in statements)
                {
                    if (stmt.ToUpperInvariant().StartsWith("INSERT") || stmt.ToUpperInvariant().StartsWith("UPDATE") || stmt.ToUpperInvariant().StartsWith("DELETE"))
                    {
                        OracleCommand cmd = new OracleCommand(stmt, _conn);
                        int itemsAffected = cmd.ExecuteNonQuery();
                        DataTable updateTable = result.Tables.Add("Update");
                        updateTable.Columns.Add("Info");
                        updateTable.Rows.Add(itemsAffected + " rows affected");
                    }
                    else
                    {
                        OracleDataAdapter dataAdapter = new OracleDataAdapter(stmt, _conn);
                        dataAdapter.Fill(result);

                        // if we didn't get any data back
                        if (result.Tables.Count <= 0)
                        {
                            DataTable infoTable = result.Tables.Add("Info");
                            infoTable.Columns.Add("Info");
                            infoTable.Rows.Add("Query executed successfully but no results were returned.");
                        }
                    }
                }

            } catch(OracleException oe) {
                DataTable errorTable = result.Tables.Add("Error");
                errorTable.Columns.Add("ErrorInfo");
                errorTable.Rows.Add(oe.Message);
            } finally {
                _conn.Close();
            }
            return result;
        }


        


        

    }
}
