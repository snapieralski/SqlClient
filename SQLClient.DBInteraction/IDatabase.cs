using System.Collections.Generic;
using System.Data;

namespace SQLClient.DBInteraction {
    public interface IDatabase {
        List<string> GetTables();
        List<string> GetViews();
        List<string> GetProcedures();
        List<string> GetColumns(string parentName);
        DataSet ExecuteQuery(string query);
    }
}