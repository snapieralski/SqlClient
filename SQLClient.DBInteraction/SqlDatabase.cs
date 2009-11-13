using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SQLClient.DBInteraction
{
    public class SqlDatabase : IDatabase
    {
        public List<string> GetTables() {
            throw new System.NotImplementedException();
        }

        public List<string> GetViews() {
            throw new System.NotImplementedException();
        }

        public List<string> GetProcedures() {
            throw new System.NotImplementedException();
        }

        public List<string> GetColumns(string parentName) {
            throw new NotImplementedException();
        }

        public DataSet ExecuteQuery(string query) {
            throw new System.NotImplementedException();
        }
    }
}
