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

        public List<string> GetTables() {
            return GetDbObjectsAsList("TABLE");
        }

        private List<string> GetDbObjectsAsList(string type) {
            List<string> objects = new List<string>();
            try {
                _conn.Open();

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = _conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("select object_name from user_objects where object_type = '{0}' order by object_name", type);

                OracleDataReader reader = cmd.ExecuteReader();

                while(reader.Read()) {
                    objects.Add(reader.GetString(0));
                }

                reader.Close();
            }
            catch (Exception ex) {
                objects.Clear();
                objects.Add(string.Format("Couldn't get {0} info : {1}", type, ex.Message));
            }
            finally {
                _conn.Close();
            }
            return objects;
        }

        public List<string> GetViews() {
            return GetDbObjectsAsList("VIEW");
        }

        public List<string> GetProcedures() {
            return GetDbObjectsAsList("PROCEDURE");
        }

        public List<string> GetColumns(string parentName) {
            List<string> objects = new List<string>();
            try {
                _conn.Open();

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = _conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("select column_name || ' ' || data_type || '(' || data_length || ')' from user_tab_columns where table_name = '{0}'", parentName);

                OracleDataReader reader = cmd.ExecuteReader();

                while(reader.Read()) {
                    objects.Add(reader.GetString(0));
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
