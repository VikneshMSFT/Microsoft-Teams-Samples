using Microsoft.Teams.Samples.HelloWorld.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Repository
{
    public interface IRemainderRepository
    {
        public List<DependencyReminder> GetAllRemindersCreatedByUser(string alias);

        public List<DependencyReminder> GetAllRemindersCreatedForUser(string alias);

        public List<DependencyReminder> GetAllRemindersForTeamsChannel(string teamId, string channelId);

        public bool AddDependencyRemainder(DependencyReminder dependencyRemainder);

        public List<DependencyReminder> GetAllDependencyRemainders();

        public DependencyReminder GetDependencyByConversationId(string conversationId);
    }

}
