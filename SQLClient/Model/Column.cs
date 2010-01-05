using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace SQLClient.Model {
    public class Column : CompletionDataBase, ITreeItem {
        
        public Column(string name) {
            Name = name;
        }

        public string DataType { get; set; }

        public string Scale { get; set; }

        public string Precision { get; set; }

        public TreeItemType TreeItemType {
            get { return TreeItemType.Column; }
        }

        public override ImageSource Image {
            get {
                return BitmapFrame.Create(new Uri("pack://application:,,,/SQLClient;component/Resources/column.png"));
            }
        }

        public override string Text {
            get {
                return Name.Split(' ')[0];
            }
        }

        public override object Description {
            get {
                return Name;
            }
        }
    }
}
