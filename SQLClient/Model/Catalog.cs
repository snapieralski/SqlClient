using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace SQLClient.Model {
    public class Catalog : CompletionDataBase, ITreeItem {
        private readonly List<Table> _tables;
        private readonly List<View> _views;
        private readonly List<Procedure> _procedures;

        public Catalog(string name) {
            Name = name;
            _tables = new List<Table>();
            _views = new List<View>();
            _procedures = new List<Procedure>();
        }

        public List<Table> Tables {
            get { return _tables; }
        }

        public List<View> Views {
            get { return _views; }
        }

        public List<Procedure> Procedures {
            get { return _procedures; }
        }

        public TreeItemType TreeItemType {
            get { return TreeItemType.Catalog; }
        }

        public override System.Windows.Media.ImageSource Image {
            get {
                return BitmapFrame.Create(new Uri("pack://application:,,,/SQLClient;component/Resources/database.png"));
            }
        }

        public override string ToString() {
            return Name;
        }

        public override bool Equals(object obj) {
            return ToString().Equals(obj.ToString());
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }
    }
}
