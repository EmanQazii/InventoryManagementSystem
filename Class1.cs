using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_ex
{
        public static class NotificationManager
        {
            public static Action<string> ShowNotifications;

            public static void Notify(string message)
            {
                ShowNotifications?.Invoke(message);
            }
        }
}
