using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLClient.Model {
    public class View : DataContainerBase {
        public View(string name, Catalog parent) : base(name, parent) {
        }

        public override TreeItemType TreeItemType {
            get { return TreeItemType.View; }
        }
    }
}
