using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Model
{
    public class DependencyReminder
    {
        public Guid ReminderId { get; set; }
        public List<string> ReminderAliases { get; set; }

        public string Message { get; set; }

        public string DependencyText { get; set; }

        public DateTime DeadlineDateTime { get; set; }

        public int Interval { get; set; }

        public string CreatedBy { get; set; }

        public DateTime LastTriggeredDateTime { get; set; }

        public List<string> ObjectID { get; set; }

        public string TeamId { get; set; }
        public string ThreadId { get; set; }
        public string ChannelId { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Mention> UsersAssigned;

        public TeamsChannelAccount CreatedByUser;

        public ConversationReference ConversationRefernce;

        public bool Notified = false;

        public bool Acknowledge = false;

        public bool Resolved = false;

        public TeamsChannelAccount AcknowledgedByUser;

        public TeamsChannelAccount ResolvedByUser;

        public string ResolveText;

        public string AdaptiveCardData;

        public bool deadlineMissNotificationSent;
    }
}
