using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLClient.Model {
    public class Procedure : ITreeItem {
        private readonly string _name;

        public Procedure(string name) {
            _name = name;
        }

        public string Name {
            get { return _name; }
        }

        public TreeItemType TreeItemType {
            get { return TreeItemType.Procedure; }
        }
    }
}
