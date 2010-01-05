using System;
using System.Windows.Media.Imaging;

namespace SQLClient.Model {
    public class Procedure : CompletionDataBase, ITreeItem {
        
        public Procedure(string name) {
            Name = name;
        }

        public TreeItemType TreeItemType {
            get { return TreeItemType.Procedure; }
        }

        public override System.Windows.Media.ImageSource Image {
            get {
                return BitmapFrame.Create(new Uri("pack://application:,,,/SQLClient;component/Resources/procedure.png"));
            }
        }
    }
}
