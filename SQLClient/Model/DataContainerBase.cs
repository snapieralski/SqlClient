using System;
using System.Collections.Generic;

namespace SQLClient.Model {
    public abstract class DataContainerBase : ITreeItem {
        private readonly List<Column> _columns;
        private readonly string _name;
        private readonly Catalog _parent;

        public DataContainerBase(string name, Catalog parent) {
            _name = name;
            _columns = new List<Column>();
            _parent = parent;
        }

        public Catalog Parent {
            get { return _parent; }
        }

        public List<Column> Columns {
            get { return _columns; }
        }

        public string Name {
            get { return _name; }
        }

        public abstract TreeItemType TreeItemType { get; }
    }
}