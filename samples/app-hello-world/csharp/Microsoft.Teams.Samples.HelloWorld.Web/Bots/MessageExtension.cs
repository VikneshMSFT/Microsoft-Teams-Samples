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

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class MessageExtension : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            var replyText = $"You said: {text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            /*var title = "";
            var titleParam = query.Parameters?.FirstOrDefault(p => p.Name == "cardTitle");
            if (titleParam != null)
            {
                title = titleParam.Value.ToString();
            }

            if (query == null || query.CommandId != "getRandomText")
            {
                // We only process the 'getRandomText' queries with this message extension
                throw new NotImplementedException($"Invalid CommandId: {query.CommandId}");
            }
            */
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

            previewedCard.Version = "1.0";

            var responseActivity = Activity.CreateMessageActivity();
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = previewedCard
            };
            responseActivity.Attachments.Add(attachment);

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

            return new MessagingExtensionActionResponse();
        }


        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
  ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            /*var response = new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                },
            };
            var card = new HeroCard
            {
                Title = "Test",
                Subtitle = "Test",
                Text = "Test",
            };
            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = HeroCard.ContentType,
                Preview = card.ToAttachment(),
            });
            response.ComposeExtension.Attachments = attachments;
            return response;*/

            dynamic createCardData = ((JObject)action.Data).ToObject(typeof(JObject));
            var response = new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "botMessagePreview",
                    ActivityPreview = MessageFactory.Attachment(new Attachment
                    {
                        Content = new AdaptiveCard("1.0")
                        {
                            Body = new List<AdaptiveElement>()
          {
            new AdaptiveTextBlock() { Text = "FormField1 value was:", Size = AdaptiveTextSize.Large },
            new AdaptiveTextBlock() { Text = "Check"  }
          },
                            Height = AdaptiveHeight.Auto,
                            Actions = new List<AdaptiveAction>()
          {
            new AdaptiveSubmitAction
            {
              Type = AdaptiveSubmitAction.TypeName,
              Title = "Submit",
              Data = new JObject { { "submitLocation", "messagingExtensionFetchTask" } },
            },
          }
                        },
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

                teamMembers.Add(new AdaptiveChoice() { Title = account.Name, Value = account.UserPrincipalName });
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
                                          new AdaptiveChoiceSetInput() { IsMultiSelect = true, Choices = teamMembers, Placeholder = "Select Members"},
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
