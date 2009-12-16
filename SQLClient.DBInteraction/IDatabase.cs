using System.Collections.Generic;
using System.Data;

namespace SQLClient.DBInteraction {
    public interface IDatabase {
        List<string> GetTables();
        List<string> GetViews();
        List<string> GetProcedures();
        List<string> GetTables(string schema);
        List<string> GetViews(string schema);
        List<string> GetProcedures(string schema);
        List<string> GetColumns(string parentName);
        List<string> GetColumns(string parentName, string schema);
        List<string> GetSchemas();
        DataSet ExecuteQuery(string query);
    }
}