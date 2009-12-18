using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLClient.Model {
    
    public class DatabaseServer {
        private Catalog _defaultCatalog;
        private readonly List<Catalog> _catalogs;
        private readonly string _name;

        public DatabaseServer(string name) {
            _name = name;
            _catalogs = new List<Catalog>();
        }

        public string Name {
            get { return _name; }
        }

        public List<Catalog> Catalogs {
            get { return _catalogs; }
        }

        public Catalog DefaultCatalog {
            get { return _defaultCatalog; }
            set {
                if (!_catalogs.Contains(value)) {
                    _catalogs.Add(value);
                }
                _defaultCatalog = value;
            }
        }

        public override string ToString() {
            return _name;
        }

    }
}
