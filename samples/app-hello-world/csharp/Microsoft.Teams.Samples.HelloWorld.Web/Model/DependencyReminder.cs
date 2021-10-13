using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Model
{
    public class DependencyReminder
    {
        public string ReminderAliases { get; set; }

        public string Message { get; set; }

        public DateTime DeadlineDateTime { get; set; }
    }
}
