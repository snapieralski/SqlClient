using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLClient.Model {
    public class Table : DataContainerBase {
        public Table(string name, Catalog parent) : base(name, parent) {
        }

        public override TreeItemType TreeItemType {
            get { return TreeItemType.Table; }
        }
    }
}
