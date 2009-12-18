using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLClient.Model {
    public class Column : ITreeItem {
        
        public Column(string name) {
            Name = name;
        }

        public string DataType { get; set; }

        public string Scale { get; set; }

        public string Precision { get; set; }

        public string Name { get;
            private set;
        }

        public TreeItemType TreeItemType {
            get { return TreeItemType.Column; }
        }
    }
}
