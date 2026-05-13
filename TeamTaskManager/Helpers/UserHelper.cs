using System;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using User = TeamTaskManager.Models.Entities.User;
using Task = TeamTaskManager.Models.Entities.Task;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;
using TeamTaskManager.Services;
using System.Windows;

namespace TeamTaskManager.Helpers
{
    public static class UserHelper
    {
        public static bool HasAdminPowers()
        {
            if (App.CurrentUser == null) return false;

            return App.CurrentUser?.OrgRole == OrgRole.Admin || App.CurrentUser?.OrgRole == OrgRole.HeadAdmin;
        }
    }
}
