using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Diagnostics;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
//get facebook channel data models
using Azure_Bot_Generic_CSharp.Models;
using Microsoft.Bot.Connector.DirectLine;
using System.Collections.Generic;

namespace Azure_Bot_Generic_CSharp
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public static string directLineSecret = "5zGk8WRvJl8.cwA.bHM.sq8NJ4ZSXRu7cZ9jzKysBGeeekhBecby6_amhE2ANy0";
        public static string botId = "Hbevalp39";

        public static Dictionary<string, UserStuff> users = new Dictionary<string, UserStuff>();

        //public static DirectLineClient client;
        //public static Microsoft.Bot.Connector.DirectLine.Conversation dconversation;
        //public static string watermark = null;
        //public static ConnectorClient connector;
        //public static Microsoft.Bot.Connector.Activity activity;

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Microsoft.Bot.Connector.Activity int_activity)
        {
            UserStuff user = null;
            if (users.ContainsKey(int_activity.From.Id))
            {
                user = users[int_activity.From.Id];
            }
            else
            {
                users[int_activity.From.Id] = new UserStuff()
                {
                    activity = int_activity
                };
                user = users[int_activity.From.Id];
            }
            
            try
            {
                if (int_activity.Type == Microsoft.Bot.Connector.ActivityTypes.Message)
                {
                    Trace.WriteLine("Got Message");
                    Trace.WriteLine(int_activity.ToString());
                    //create connector service
                    user.connector = new ConnectorClient(new Uri(int_activity.ServiceUrl));

                    //var rply = int_activity.CreateReply($"Successful connection for {JsonConvert.SerializeObject(int_activity, Formatting.Indented)}");
                    //await user.connector.Conversations.ReplyToActivityAsync(rply);
                    //conv = await connector.Conversations.CreateDirectConversationAsync(activity.Recipient, activity.From);
                    //activity.Conversation.Id = conv.Id;

                    if (user.client == null)
                    {
                        user.client = new DirectLineClient(directLineSecret);
                    }
                    
                    if (user.dconversation == null)
                    {
                        user.dconversation = await user.client.Conversations.StartConversationAsync();
                        new System.Threading.Thread(async () => await user.ReadBotMessagesAsync()).Start();
                    }

                    // send user's input to health service bot
                    Microsoft.Bot.Connector.DirectLine.Activity dact = new Microsoft.Bot.Connector.DirectLine.Activity()
                    {
                        From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount("some user"),
                        Text = int_activity.Text,
                        Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message
                    };
                    await user.client.Conversations.PostActivityAsync(user.dconversation.ConversationId, dact);
                }
                else
                {
                    HandleSystemMessage(int_activity);
                }
                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        //private async Task ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        //{
        //    try
        //    {
        //        while (true)
        //        {
                    
        //            // get response from heath service bot
        //            var activitySet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
        //            watermark = activitySet?.Watermark;

        //            var activities = from x in activitySet.Activities
        //                             where x.From.Id == botId
        //                             select x;

        //            // respond to user
        //            foreach (Microsoft.Bot.Connector.DirectLine.Activity dactivity in activities)
        //            {
        //                Console.WriteLine(dactivity.Text);

        //                Microsoft.Bot.Connector.Activity reply = activity.CreateReply(dactivity.Text);

        //                if (dactivity.Attachments != null)
        //                {
        //                    foreach (Microsoft.Bot.Connector.DirectLine.Attachment attachment in dactivity.Attachments)
        //                    {
        //                        switch (attachment.ContentType)
        //                        {
        //                            case "application/vnd.microsoft.card.hero":
        //                                var heroCard = JsonConvert.DeserializeObject<Microsoft.Bot.Connector.HeroCard>(attachment.Content.ToString());
        //                                reply.Attachments.Add(heroCard.ToAttachment());
        //                                break;
        //                            default:
        //                                break;
        //                        }
        //                    }
        //                }

        //                //await connector.Conversations.SendToConversationAsync(reply);

        //                await connector.Conversations.ReplyToActivityAsync(reply);
        //            }

        //            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw;
        //    }
        //}

        private Microsoft.Bot.Connector.Activity HandleSystemMessage(Microsoft.Bot.Connector.Activity message)
        {
            if (message.Type == Microsoft.Bot.Connector.ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == Microsoft.Bot.Connector.ActivityTypes.ConversationUpdate)
            {
                //if (connector != null)
                //{
                //    connector.Dispose();
                //    connector = null;
                //}
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == Microsoft.Bot.Connector.ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == Microsoft.Bot.Connector.ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == Microsoft.Bot.Connector.ActivityTypes.Ping)
            {
            }

            return null;
        }
    }

    public class UserStuff
    {
        public DirectLineClient client { get; set; }
        public Microsoft.Bot.Connector.DirectLine.Conversation dconversation { get; set; }
        public string watermark { get; set; } = null;
        public ConnectorClient connector { get; set; }
        public Microsoft.Bot.Connector.Activity activity { get; set; }

        public async Task ReadBotMessagesAsync()
        {
            try
            {
                while (true)
                {

                    // get response from heath service bot
                    var activitySet = await this.client.Conversations.GetActivitiesAsync(this.dconversation.ConversationId, this.watermark);
                    this.watermark = activitySet?.Watermark;

                    var activities = from x in activitySet.Activities
                                     where x.From.Id == MessagesController.botId
                                     select x;

                    // respond to user
                    foreach (Microsoft.Bot.Connector.DirectLine.Activity dactivity in activities)
                    {
                        Console.WriteLine(dactivity.Text);

                        Microsoft.Bot.Connector.Activity reply = activity.CreateReply(dactivity.Text);

                        if (dactivity.Attachments != null)
                        {
                            foreach (Microsoft.Bot.Connector.DirectLine.Attachment attachment in dactivity.Attachments)
                            {
                                switch (attachment.ContentType)
                                {
                                    case "application/vnd.microsoft.card.hero":
                                        var heroCard = JsonConvert.DeserializeObject<Microsoft.Bot.Connector.HeroCard>(attachment.Content.ToString());
                                        reply.Attachments.Add(heroCard.ToAttachment());
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        //await connector.Conversations.SendToConversationAsync(reply);

                        await this.connector.Conversations.ReplyToActivityAsync(reply);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}