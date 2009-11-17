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

            try {
                _conn.Open();

                if (query.ToUpperInvariant().StartsWith("INSERT") || query.ToUpperInvariant().StartsWith("UPDATE") || query.ToUpperInvariant().StartsWith("DELETE")) {
                    OracleCommand cmd = new OracleCommand(query, _conn);
                    int itemsAffected = cmd.ExecuteNonQuery();
                    result.Tables.Add("Update");
                    result.Tables[0].Columns.Add("Info");
                    result.Tables[0].Rows.Add(itemsAffected + " rows affected");
                } else {
                    OracleDataAdapter dataAdapter = new OracleDataAdapter(query, _conn);
                    dataAdapter.Fill(result);

                    // if we didn't get any data back
                    if (result.Tables.Count <= 0) {
                        result.Tables.Add("Info");
                        result.Tables[0].Columns.Add("Info");
                        result.Tables[0].Rows.Add("Query executed successfully but no results were returned.");
                    }
                } 

            } catch(OracleException oe) {
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
