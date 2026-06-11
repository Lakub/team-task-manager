using CommunityToolkit.Mvvm.Messaging.Messages;
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

    public class TaskUpdatedMessage : ValueChangedMessage<int>
    {
        public TaskUpdatedMessage(int taskId) : base(taskId)
        {
        }
    }
}
