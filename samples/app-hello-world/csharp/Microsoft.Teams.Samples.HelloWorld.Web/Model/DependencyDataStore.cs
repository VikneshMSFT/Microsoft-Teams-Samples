using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Model
{
    public static class DependencyDataStore
    {
        public static List<DependencyReminder> RemindersListDataStore { get; set; } = new List<DependencyReminder>();
    }
}
