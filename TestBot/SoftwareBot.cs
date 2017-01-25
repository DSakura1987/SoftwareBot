using MargieBot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace SoftwareBot
{
    public class SoftwareBot : Bot
    {
        public const string ADMIN_ID = "U0M4JPX6V";
        private string API_KEY = "";
        private BindingList<ScheduledItem> scheduledItems = new BindingList<ScheduledItem>();
        Timer timer;
        public SoftwareBot()
        {
            // Redirecting Console.Out to null because the MargieBot library likes to spam Console.Out with garbage, for some reason. 
            // Redirecting Console.Err to Console.Out so we can use "Console.Error.WriteLine()" instead of "Console.WriteLine()" to solve this problem.
            Console.SetError(Console.Out); 
            Console.SetOut(TextWriter.Null);
            Console.Error.WriteLine("Hello, I am SoftwareBot. -- I was unleashed upon the world by Adam Carruthers.\n");
            Console.Error.WriteLine("My build date is: " + Properties.Settings.Default.BUILD_DATE + "\n");

            System.IO.StreamReader file = new System.IO.StreamReader("api.key");
            API_KEY = file.ReadLine();

            /*
                timer = new Timer((e) => {
                CheckScheduledEvents();
            }, null, 0, 60 * 1000);

            //    loadScheduledItems(); *****************************************************
            scheduledItems.ListChanged += ScheduledItemsChanged;

            */
            TimeResponder timeResponder = new TimeResponder();
            ConnectionStatusChanged += (bool isConnected) =>
            {
                if (IsConnected)
                {
                    Console.Error.WriteLine("I have successfully connected to the Slack network.\n");
                } else
                {

                    Console.Error.WriteLine("I encountered errors with my connection to the Slack network.\n");
                    Console.Error.WriteLine("If these issues persist, please try restarting me.\n");
                }
                Console.Error.WriteLine("----------------------------------------------------------------\n");

                if (IsConnected)
                {

                    Console.Error.WriteLine("Begin transmission:\n");
                }
                // occurs every time your bot connects or disconnects
            };
            Aliases = new List<string>() { "SoftwareBot", "Software Bot" };

            Responders.Add(timeResponder);
            Responders.Add(new HelpResponder(Responders));
            //Responders.Add(new SchedulerResponder(scheduledItems));
            Responders.Add(new TaskResponder());
            Responders.Add(new ChatResponder());
            //Responders.Add(new AdminResponder(this));
            //Responders.Add(new TfsBuildResponder());
            Responders.Add(new OneNoteResponder());

            Console.Error.WriteLine("The features that are included in this build are:\n");
            foreach (IResponder r in Responders)
            {
                Console.Error.WriteLine("    -" + r.ToString() + "\n");
            }

            // DISPLAYS MESSAGES RECEIVED -- NEUTERED FOR PRIVACY
            MessageReceived += (string messageData) =>
            {
                JObject jObj = JObject.Parse(messageData);
                try
                {


                    string type = (string)jObj["type"];
                    if (type != null && type == "message")
                    {
                        string userID = (string)jObj["user"];

                        if (userID != null && timeResponder.userNameCache.ContainsKey(userID))
                        {
                            //  Console.Error.WriteLine("[" + DateTime.Now + "] - " + timeResponder.userNameCache[(string)jObj["user"]] + ": " + jObj["text"]);
                            Console.Error.WriteLine("[" + DateTime.Now + "] - " + timeResponder.userNameCache[(string)jObj["user"]] + " <MESSAGE RECEIVED>");
                        }
                        else
                        {

                            //  Console.Error.WriteLine("[" + DateTime.Now + "] - " + (string)jObj["user"] + ": " + jObj["text"]);
                            Console.Error.WriteLine("[" + DateTime.Now + "] - " + (string)jObj["user"] + " <MESSAGE RECEIVED>");
                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
                // the messageData parameter contains the complete json message the bot receives from Slack
                // this is fired each time your bot receives a message, which, let me tell you, will be often
            };




            Console.Error.WriteLine("Attempting to connect to Slack now...\n");
            try
            {
                Connect(API_KEY);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Connection attempt failed! Press ENTER if you would like me to try again.\n" + "[" + ex.Message + "]\n");
            }

        }
        private void CheckScheduledEvents()
        {
            /* try
             {

                 foreach (ScheduledItem si in scheduledItems.ToList())
                 {
                     if (si != null)
                         if (DateTime.Now > si.Date)
                         {

                             Say(new BotMessage() { Text = si.Content, ChatHub = si.ChatHub });
                             scheduledItems.Remove(si);
                             if (si.IsResechedulable)
                             {
                                 si.reschedule();
                                 scheduledItems.Add(si);
                             }
                         }
                 }


             } catch(InvalidOperationException ex)
             {
                 console.WriteLine("INVALID OPERATION EXCEPTION!");
             }*/
        }
        private void ScheduledItemsChanged(object sender, EventArgs e)
        {
            try
            {
                using (Stream stream = File.Open("schedule.SWB", FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, scheduledItems);
                }
                Console.Error.WriteLine("SAVING SCHEDULED ITEMS COMPLETE: " + scheduledItems.Count());
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
        private void LoadScheduledItems()
        {
            try
            {
                using (Stream stream = File.Open("schedule.SWB", FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    foreach (ScheduledItem si in (BindingList<ScheduledItem>)bin.Deserialize(stream))
                    {
                        scheduledItems.Add(si);
                    }

                }
                Console.Error.WriteLine("LOADING SCHEDULED ITEMS COMPLETE: " + scheduledItems.Count());
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
        public void DoCommand(string command)
        {
            if (!IsConnected)
            {
                Console.Error.WriteLine("There's a problem with my connection to Slack.\n");
                Console.Error.WriteLine("Trying to reconnect now.\n");
                try
                {
                    Connect(API_KEY);
                } catch(Exception ex)
                {
                    Console.Error.WriteLine("Connection attempt failed! Press ENTER if you would like me to try again.\n" + "[" + ex.Message + "]\n");
                }
            } else
            {

                Console.Error.WriteLine("I am connected to the Slack network.\n");
            }
            /*
               if (command != null && command != "")
            {
                switch (command)
                {
                    case ("close"):
                        break;
                    default:
                        BotMessage message = new BotMessage();
                        message.Text = command;

                        console.WriteLine("");
                        console.WriteLine("> Available channels:");

                        int counter = 1;
                        List<SlackChatHub> hubs = new List<SlackChatHub>();
                        foreach (SlackChatHub hub in ConnectedChannels)
                        {
                            console.WriteLine("> [" + counter + "] - " + hub.Name);
                            hubs.Add(hub);
                            counter++;
                        }
                        foreach (SlackChatHub dm in ConnectedDMs)
                        {
                            console.WriteLine("> [" + counter + "] - " + dm.Name);
                            hubs.Add(dm);
                            counter++;
                        }
                        int index = -1;
                        string channelInput = "";
                        while ((index < 1 || index >= counter) && !channelInput.Equals("close"))
                        {
                            console.WriteLine("");
                            console.WriteLine("> Please select a channel: (1-" + (counter - 1) + ")");
                            channelInput = console.ReadLine().ToLower();
                            Int32.TryParse(channelInput, out index);
                        }

                        if (channelInput.Equals("close"))
                        {
                            console.WriteLine("");
                            console.WriteLine("> Command cancelled.");
                        }
                        else
                        {
                            message.ChatHub = hubs[index - 1];
                            Say(message);
                        }

                        break;
                }
            }  

            ************* DISABLED BECAUSE WE CAN'T HAVE NICE THINGS*/

        }


    }
}
