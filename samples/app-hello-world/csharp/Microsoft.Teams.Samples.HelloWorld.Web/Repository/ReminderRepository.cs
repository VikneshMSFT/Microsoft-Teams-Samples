using Microsoft.Teams.Samples.HelloWorld.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Repository
{
    public class ReminderRepository
    {
        private List<DependencyReminder> dependencyReminders;

        public ReminderRepository(List<DependencyReminder> dependencyReminders)
        {
            this.dependencyReminders = dependencyReminders;
        }

        public List<DependencyReminder> GetAllRemindersCreatedByUser(string alias)
        {
            var filteredReminders = new List<DependencyReminder>(this.dependencyReminders.FindAll(a => a.CreatedBy.ToLower() == alias.ToLower()));
            return filteredReminders;
        }

        public List<DependencyReminder> GetAllRemindersCreatedForUser(string alias)
        {
            var filteredReminders = new List<DependencyReminder>(this.dependencyReminders.FindAll(a => a.ReminderAliases.Any(a => a.ToLower() == alias.ToLower())));
            return filteredReminders;
        }

        public List<DependencyReminder> GetAllRemindersForTeamsChannel(string teamId, string channelId)
        {
            var filteredReminders = new List<DependencyReminder>(this.dependencyReminders.FindAll(a => a.ChannelId.ToLower() == channelId.ToLower() && a.TeamId.ToLower() == teamId.ToLower()));
            return filteredReminders;
        }
    }
}
