using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace SQLClient.Model {
    public abstract class CompletionDataBase : ICompletionData {
        public string Name { get; protected set; }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs) {
            textArea.Document.Replace(completionSegment, " " + Text);
        }

        public abstract ImageSource Image { get; }

        public virtual string Text {
            get { return Name; }
        }

        public virtual object Content {
            get { return Text; }
        }

        public virtual object Description {
            get { return null; }
        }
    }
}
