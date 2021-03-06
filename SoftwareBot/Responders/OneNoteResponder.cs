﻿using MargieBot;
using System.Text.RegularExpressions;

namespace SoftwareBot
{
    public class OneNoteResponder : ISBResponder
    {
        private static Regex ONENOTE_MASK = new Regex(@"onenote\:\<(?<OneNoteLink>http.+)\>");
        private static string USAGE = "Automatically detect keywords in message";
        private static string CLASSTOSTRING = "OneNote linker responder";
        private static string DESCRIPTION = "Post a link to directly open the OneNote page.";
        public bool CanRespond(ResponseContext context)
        {
            
            return ONENOTE_MASK.IsMatch(context.Message.Text);
        }
        public bool CanReact(ResponseContext context)
        {
            return false;
        }
        public BotReaction GetReaction(ResponseContext context) { return new BotReaction(); }
        public BotMessage GetResponse(ResponseContext context)
        {
            MatchCollection matches = ONENOTE_MASK.Matches(context.Message.Text);
            if (matches.Count > 0)
            {
                BotMessage message = new BotMessage();
                int i = 1;                
                foreach (Match match in matches)
                {
                    SlackAttachment attachment = new SlackAttachment();
                    attachment.Title = $"Open in OneNote ({i}/{matches.Count})";
                    string onenoteLink = match.Groups["OneNoteLink"].Value;
                    onenoteLink.Replace("&", "&amp;");
                    onenoteLink = "onenote://" + onenoteLink;
                    attachment.TitleLink = onenoteLink;
                    message.Attachments.Add(attachment);
                    i++;
                }
                
                return message;
            }
            else
            {
                return new BotMessage { Text = "Hmm I should reply but have not found one note link :|" };
            }
        }

        public string getUsage() => USAGE;
        public string getDescription() => DESCRIPTION;
        public override string ToString() => CLASSTOSTRING;
    }
}
