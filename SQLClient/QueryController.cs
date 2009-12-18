using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using SQLClient.DBInteraction;
using SQLClient.Model;

namespace SQLClient {
    public class QueryController {
        private IDatabase _db;
        private DatabaseServer _server;
        //private ConnectionInfo _connnectionInfo;

        public QueryController(ConnectionInfo info) {
            //_connnectionInfo = info;
            
            if (info.Type == "Oracle") {
                _db = new OracleDatabase(info.ConnectionString);
            } else if (info.Type == "SQL Server") {
                _db = new SqlDatabase(info.ConnectionString);
            } else {
                throw new ApplicationException("Unable to connect to DB Type: '" + info.Type + "'");
            }

            _server = new DatabaseServer(info.Name);
            _server.DefaultCatalog = new Catalog(info.InitialCatalog);
        }

        public DatabaseServer Server {
            get { return _server; }
        }

        /// <summary>
        /// Gets the data returned by some query.
        /// </summary>
        /// <param name="queryToRun">SQL text of the query to run.</param>
        /// <returns>Results of the query.</returns>
        public DataSet GetQueryResult(string queryToRun) {
            return _db.ExecuteQuery(queryToRun);
        }

        public List<ITreeItem> GetTables(Catalog catalog) {
            if( catalog.Tables.Count == 0 ) {
                catalog.Tables.AddRange(_db.GetTables(catalog.Name).ConvertAll(t => new Table(t, catalog)));
            }
            return catalog.Tables.Cast<ITreeItem>().ToList();
        }

        public List<ITreeItem> GetViews(Catalog catalog) {
            if( catalog.Views.Count == 0) {
                catalog.Views.AddRange(_db.GetViews(catalog.Name).ConvertAll(t => new View(t, catalog)));
            }
            return catalog.Views.Cast<ITreeItem>().ToList();
        }

        public List<ITreeItem> GetColumns(DataContainerBase table) {
            if( table.Columns.Count == 0 ) {
                table.Columns.AddRange(_db.GetColumns(table.Name, table.Parent.Name).ConvertAll(t => new Column(t)));
            }
            return table.Columns.Cast<ITreeItem>().ToList();
        }

        public List<ITreeItem> GetProcedures(Catalog catalog) {
            if( catalog.Procedures.Count == 0) {
                catalog.Procedures.AddRange(_db.GetProcedures(catalog.Name).ConvertAll(t => new Procedure(t)));
            }
            return catalog.Procedures.Cast<ITreeItem>().ToList();
        }

        public List<ITreeItem> GetCatalogs() {
            if( _server.Catalogs.Count == 1) {
                _server.Catalogs.AddRange(_db.GetCatalogs().FindAll(c => c != _server.DefaultCatalog.Name).ConvertAll( c => new Catalog(c)));
            }
            return _server.Catalogs.Cast<ITreeItem>().ToList();
        }
    }
}
