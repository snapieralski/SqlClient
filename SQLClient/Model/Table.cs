using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace SQLClient.Model {
    public class Table : DataContainerBase {
        public Table(string name, Catalog parent) : base(name, parent) {
        }

        public override TreeItemType TreeItemType {
            get { return TreeItemType.Table; }
        }

        public override System.Windows.Media.ImageSource Image {
            get {
                return BitmapFrame.Create(new Uri("pack://application:,,,/SQLClient;component/Resources/table.png"));
            }
        }
    }
}
