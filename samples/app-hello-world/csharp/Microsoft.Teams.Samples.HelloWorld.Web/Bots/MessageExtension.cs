using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using Bogus;
using Microsoft.Bot.Connector;
using AdaptiveCards;
using Newtonsoft.Json;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Teams.Samples.HelloWorld.Web.Bots;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class MessageExtension : TeamsActivityHandler
    {

        private readonly IConfiguration _config;
        private static ConversationReference _reference;

        public MessageExtension(IConfiguration config)
        {
            this._config = config;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value.ToString().Contains("DependencyAck"))
            {
                _ = ((BotAdapter)turnContext.Adapter).ContinueConversationAsync(
                    _config.GetValue<string>("MicrosoftAppId"),
                    _reference,
                    async (t, ct) =>
                    {
                        await t.SendActivityAsync(MessageFactory.Text("This will be the first response to the new thread"), ct);
                    },
                cancellationToken);
            }
            else
            {
                turnContext.Activity.RemoveRecipientMention();
                var text = turnContext.Activity.Text.Trim().ToLower();

                var replyText = $"You said: {text}";
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var title = "Hello";
            var attachments = new MessagingExtensionAttachment[5];

            for (int i = 0; i < 5; i++)
            {
                attachments[i] = GetAttachment(title);
            }


            var result = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments.ToList()
                },
            };
            return Task.FromResult(result);

        }

        private static MessagingExtensionAttachment GetAttachment(string title)
        {
            var card = new ThumbnailCard
            {
                Title = !string.IsNullOrWhiteSpace(title) ? title : new Faker().Lorem.Sentence(),
                Text = new Faker().Lorem.Paragraph(),
                Images = new List<CardImage> { new CardImage("http://lorempixel.com/640/480?rand=" + DateTime.Now.Ticks.ToString()) }
            };

            return card
                .ToAttachment()
                .ToMessagingExtensionAttachment();
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(
  ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            return await OnTeamsMessagingExtensionFetchTaskAsync(turnContext, action, cancellationToken);
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(
          ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            //handle the event
            var activityPreview = action.BotActivityPreview[0];
            var attachmentContent = activityPreview.Attachments[0].Content;
            var previewedCard = JsonConvert.DeserializeObject<AdaptiveCard>(attachmentContent.ToString(),
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            previewedCard.Version = "1.2";

            var responseActivity = Activity.CreateMessageActivity();
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = previewedCard
            };
            responseActivity.Attachments.Add(attachment);

            var membersToMention = ((AdaptiveTextBlock)previewedCard.Body.First()).Text;

            var teamId = turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id;
            var members = await TeamsInfo.GetTeamMembersAsync(turnContext, teamId, cancellationToken);

            List<Mention> membersToMentionList = new List<Mention>();
            foreach (var teamMember in members)
            {
                if (membersToMention.Contains(teamMember.Name))
                {
                    membersToMentionList.Add(new Mention
                    {
                        Mentioned = teamMember,
                        Text = $"<at>{teamMember.Name}</at>",
                    }
                    );
                }
            }

            var entities = new { entities = new List<Entity>(membersToMentionList) };
            previewedCard.AdditionalProperties.Add("msteams", entities);

            // Attribute the message to the user on whose behalf the bot is posting
            responseActivity.ChannelData = new
            {
                OnBehalfOf = new[]
                {
                    new
                    {
                        ItemId = 0,
                        MentionType = "person",
                        Mri = turnContext.Activity.From.Id,
                        DisplayName = turnContext.Activity.From.Name
                    }
                }
            };

            await turnContext.SendActivityAsync(responseActivity);

            // start for async message posting

            /* var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
             var message = MessageFactory.Text("This will start a new thread in a channel");

             var serviceUrl = turnContext.Activity.ServiceUrl;
             var credentials = new MicrosoftAppCredentials(_appId, _appPassword);

             var conversationParameters = new ConversationParameters
             {
                 IsGroup = true,
                 ChannelData = new { channel = new { id = teamsChannelId } },
                 Activity = (Activity)message,
             };*/
            var messageId = responseActivity.Id;
            var convesrationRef = turnContext.Activity.GetConversationReference();
            convesrationRef.Conversation.Id = convesrationRef.Conversation.Id + ";messageid=" + messageId;
            _reference = convesrationRef;
            
            return new MessagingExtensionActionResponse();
        }


        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
  ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {

            var dependencyInput = JsonConvert.DeserializeObject<DependencyInput>(((JObject)action.Data).ToString());
            dependencyInput.MembersAssignedList = new List<string>(dependencyInput.MembersAssigned.Split(","));

            //####################################################
            string serviceUrl = "https://smba.trafficmanager.net/emea/";

            //From the Bot Channel Registration
            string botClientID = _config.GetValue<string>("MicrosoftAppId");
            string botClientSecret = _config.GetValue<string>("MicrosoftAppPassword");

            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);

            var connectorClient = new ConnectorClient(new Uri(serviceUrl), new MicrosoftAppCredentials(botClientID, botClientSecret));
            var teamId = turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id;

            //var members = await TeamsInfo.GetTeamMembersAsync(turnContext, teamId, cancellationToken);

            var members = await TeamsInfo.GetTeamMembersAsync(turnContext, teamId, cancellationToken);

            List<Mention> membersToMentionList = new List<Mention>();
            string ListOfUsers = "";
            foreach (var member in members)
            {
                if (dependencyInput.MembersAssignedList.Contains(member.AadObjectId))
                {
                    ListOfUsers = ListOfUsers + member.Name + ",";
                    membersToMentionList.Add(new Mention
                    {
                        Mentioned = member,
                        Text = $"<at>{member.Name}</at>",
                    }
                    );
                }
            }

            //######################################################

            foreach (var member in dependencyInput.MembersAssignedList)
            {
                Console.WriteLine(member.ToString());
            }

            string mentionText = "";
            foreach (var member in membersToMentionList)
            {
                mentionText = mentionText + $" {member.Text} ";
            }
            AdaptiveTextBlock mentionBlock = new AdaptiveTextBlock()
            {
                Text = mentionText,
                Wrap = true
            };

            var adaptiveCard = new AdaptiveCard("1.2")
            {
                Body = new List<AdaptiveElement>()
                            {
                                mentionBlock,
                                new AdaptiveTextBlock() { Text = "Dependency", Size = AdaptiveTextSize.Large},
                                new AdaptiveTextBlock() { Text = dependencyInput.DepText, Wrap = true },
                                new AdaptiveTextBlock() { Text = "DeadLine", Size = AdaptiveTextSize.Large},
                                new AdaptiveTextBlock() { Text = dependencyInput.DeadLineDate.ToString() + " " + dependencyInput.DeadLineTime.ToString()  },
                                new AdaptiveTextInput() { Id = "ResolveComments", Placeholder = "Resolve with comments", IsMultiline = true},
                            },
                Height = AdaptiveHeight.Auto,
                Actions = new List<AdaptiveAction>()
                            {
                                new AdaptiveSubmitAction
                                {
                                    Type = AdaptiveSubmitAction.TypeName,
                                    Title = "Acknowledge",
                                    Data = new JObject { { "submitLocation", "DependencyAck" } },
                                },
                                new AdaptiveSubmitAction
                                {
                                    Type = AdaptiveSubmitAction.TypeName,
                                    Title = "Resolve",
                                    Data = new JObject { { "submitLocation", "DependencyResolve" } },
                                },
                            }
            };

            adaptiveCard.AdditionalProperties.Add("Users", ListOfUsers);

            var response = new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "botMessagePreview",
                    ActivityPreview = MessageFactory.Attachment(new Attachment
                    {
                        Content = adaptiveCard,
                        ContentType = AdaptiveCard.ContentType
                    }) as Activity
                }
            };
            return response;
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {

            var teamId = turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id;
            var members = await TeamsInfo.GetTeamMembersAsync(turnContext, teamId, cancellationToken);

            List<AdaptiveChoice> teamMembers = new List<AdaptiveChoice>();
            foreach (TeamsChannelAccount account in members)
            {
                if (string.Equals(turnContext.Activity.From.AadObjectId, account.AadObjectId))
                {
                    continue;
                }

                teamMembers.Add(new AdaptiveChoice() { Title = account.Name, Value = account.AadObjectId });
            }

            var response = new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = "large",
                        Width = "large",
                        Title = "Create Dependencies",
                        //Url = "https://1e43-103-139-35-182.ngrok.io/hello"
                        Card = new Attachment()
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = new AdaptiveCard("1.0")
                            {
                                Body = new List<AdaptiveElement>()
                                      {
                                          new AdaptiveTextBlock() { Text = "Pick Members responsible for this dependency"},
                                          new AdaptiveChoiceSetInput() { Id = "MembersAssigned", IsMultiSelect = true, Choices = teamMembers, Placeholder = "Select Members"},
                                          new AdaptiveTextBlock() { Text = "Enter Your Dependency below"},
                                          new AdaptiveTextInput() { Id = "DepText", Placeholder = "Describe your dependency", IsMultiline = true},
                                          new AdaptiveTextBlock() { Text = "Pick Date and Time you want this dependency to be completed"},
                                          new AdaptiveDateInput() { Id = "DeadLineDate", Placeholder = "Select the date when you need this dependency to get completed"},
                                          new AdaptiveTimeInput() { Id = "DeadLineTime", Placeholder = "Select the time when you need this dependency to get completed"},
                                      },
                                Actions = new List<AdaptiveAction>()
                                      {
                                          new AdaptiveSubmitAction()
                                          {
                                          Type = AdaptiveSubmitAction.TypeName,
                                          Title = "Preview and Post",
                                          },
                                      },
                            },
                        },
                    },
                },
            };
            return response;
            //handle fetch task
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = new MessagingExtensionAttachment[]{
                        new ThumbnailCard()
                            .ToAttachment()
                            .ToMessagingExtensionAttachment()
                    }
                },
            });
        }
    }
}
