using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Bots
{
    public class RemainderNotifierTimer
    {
        private readonly IRemainderRepository _remainderRepository;
        private readonly IBotFrameworkHttpAdapter _botAdapter;
        private readonly IConfiguration _config;

        public RemainderNotifierTimer(IRemainderRepository remainderRepository, 
            IBotFrameworkHttpAdapter botAdapter,
            IConfiguration config)
        {
            this._remainderRepository = remainderRepository;
            this._botAdapter = botAdapter;
            this._config = config;
        }

        public void StartNotificationTimer()
        {
            Timer reminderTimer = new Timer(60000);
            reminderTimer.Enabled = true;
            reminderTimer.AutoReset = true;
            reminderTimer.Elapsed += this.Notify;
        }

        private void Notify(object sender, ElapsedEventArgs e)
        {
            foreach (var reminder in this._remainderRepository.GetAllDependencyRemainders())
            {
                if (reminder.Notified || reminder.Resolved)
                {
                    continue;
                }

                var deadLineDateTime = reminder.DeadlineDateTime;

                if ((deadLineDateTime - DateTime.Now).Minutes <= 1 && !reminder.deadlineMissNotificationSent)
                {
                    Console.WriteLine("Sending reminder");
                    _ = ((BotAdapter)this._botAdapter).ContinueConversationAsync(
                    this._config.GetValue<string>("MicrosoftAppId"),
                    reminder.ConversationRefernce,
                    async (t, ct) =>
                    {
                        var previewedCard = new AdaptiveCard("1.2");
                        var responseActivity = Activity.CreateMessageActivity();

                        string mentionText = String.Join(" , ", reminder.UsersAssigned.Select(user => user.Text).ToArray());
                        var mentionTextBlock =
                            new AdaptiveTextBlock() { Wrap = true, Text = $"{mentionText} :  this dependency crossed dead line time. Take approproate actions", Size = AdaptiveTextSize.Medium };
                        previewedCard.Body.Add(mentionTextBlock);

                        var entities = new { entities = new List<Entity>(reminder.UsersAssigned) };
                        previewedCard.AdditionalProperties.Add("msteams", entities);

                        Attachment attachment = new Attachment()
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = previewedCard
                        };
                        responseActivity.Attachments.Add(attachment);
                        await t.SendActivityAsync(responseActivity);
                    },
                new System.Threading.CancellationToken());
                    reminder.deadlineMissNotificationSent = true;
                }

                if ((deadLineDateTime - DateTime.Now).Minutes <= 5)
                {
                    Console.WriteLine("Sending reminder");
                    _ = ((BotAdapter)this._botAdapter).ContinueConversationAsync(
                    this._config.GetValue<string>("MicrosoftAppId"),
                    reminder.ConversationRefernce,
                    async (t, ct) =>
                    {
                        var previewedCard = new AdaptiveCard("1.2");
                        var responseActivity = Activity.CreateMessageActivity();

                        string mentionText = String.Join(" , ", reminder.UsersAssigned.Select(user => user.Text).ToArray());
                        var mentionTextBlock =
                            new AdaptiveTextBlock() { Wrap = true, Text = $"{mentionText} :  remainder to complete this dependecny", Size = AdaptiveTextSize.Medium };
                        previewedCard.Body.Add(mentionTextBlock);

                        var entities = new { entities = new List<Entity>(reminder.UsersAssigned) };
                        previewedCard.AdditionalProperties.Add("msteams", entities);

                        Attachment attachment = new Attachment()
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = previewedCard
                        };
                        responseActivity.Attachments.Add(attachment);
                        await t.SendActivityAsync(responseActivity);
                    },
                new System.Threading.CancellationToken());
                    reminder.Notified = true;
                    // send a reminder on teams channel
                }
                Console.WriteLine(reminder.Message);
            }
        }

    }
}
