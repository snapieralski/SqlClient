using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLClient.Model {
    public enum TreeItemType {
        Table,
        View,
        Procedure,
        Catalog,
        Column
    }

    public interface ITreeItem {
        string Name { get; }
        TreeItemType TreeItemType { get; }
    }
}
