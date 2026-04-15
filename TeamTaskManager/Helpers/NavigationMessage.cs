using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTaskManager.Helpers
{
    public class NavigationMessage
    {
        public object TargetView { get; }

        public NavigationMessage(object targetView)
        {
            TargetView = targetView;
        }
    }
}
