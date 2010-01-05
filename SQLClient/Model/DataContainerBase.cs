using System;
using System.Collections.Generic;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace SQLClient.Model {
    public abstract class DataContainerBase : CompletionDataBase, ITreeItem {
        private readonly List<Column> _columns;
        private readonly Catalog _parent;

        public DataContainerBase(string name, Catalog parent) {
            Name = name;
            _columns = new List<Column>();
            _parent = parent;
        }

        public Catalog Parent {
            get { return _parent; }
        }

        public List<Column> Columns {
            get { return _columns; }
        }

        public abstract TreeItemType TreeItemType { get; }


        
    }
}