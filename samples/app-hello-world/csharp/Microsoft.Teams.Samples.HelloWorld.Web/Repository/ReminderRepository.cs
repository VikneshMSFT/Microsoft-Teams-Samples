using Microsoft.Teams.Samples.HelloWorld.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Repository
{
    public class ReminderRepository : IRemainderRepository
    {
        private List<DependencyReminder> dependencyReminders;

        private Dictionary<string, DependencyReminder> dependencyByConvesrationId = new Dictionary<string, DependencyReminder>();

        public ReminderRepository()
        {
            this.dependencyReminders = new List<DependencyReminder>();
        }

        public ReminderRepository(List<DependencyReminder> dependencyReminders)
        {
            this.dependencyReminders = dependencyReminders;
        }

        public bool AddDependencyRemainder(DependencyReminder dependencyRemainder)
        {
            this.dependencyReminders.Add(dependencyRemainder);
            this.dependencyByConvesrationId.Add(dependencyRemainder.ConversationRefernce.Conversation.Id, dependencyRemainder);
            return true;
        }

        public List<DependencyReminder> GetAllDependencyRemainders()
        {
            return dependencyReminders;
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

        public DependencyReminder GetDependencyByConversationId(string conversationId)
        {
            DependencyReminder remainder = null;
            dependencyByConvesrationId.TryGetValue(conversationId, out remainder);
            return remainder;
        }
    }
}
