using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLClient
{
    [Serializable]
    public class ConnectionInfo
    {
        public string Name { get; set; }
        public string Server { get; set; }
        public string InitialCatalog { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            bool result = false;
            ConnectionInfo compareTo = obj as ConnectionInfo;
            if (compareTo != null)
            {
                result = Name.Equals(compareTo.Name);
            }
            return result;
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }
    }
}
