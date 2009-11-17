using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLClient.ConnectUI
{
    public interface IConnectionControl
    {

        ConnectionInfo ConnectionInfo
        {
            get;
            set;
        }
    }
}
