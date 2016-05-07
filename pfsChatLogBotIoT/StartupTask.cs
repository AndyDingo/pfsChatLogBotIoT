using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Windows.ApplicationModel.Background;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace pfsChatLogBotIoT
{
    public sealed class StartupTask : IBackgroundTask
    {
        // Declare Variables
        private static string logfile = Directory.GetCurrentDirectory() + @"\pfsChatLogBot.log";
        private static string cfgfile = Directory.GetCurrentDirectory() + @"\pfsChatLogBot.cfg"; // Main config
        private BackgroundTaskDeferral _deferral;
        private static string dateformat = "yyyyMMdd";
        private static string timeformat = "HH:mm";

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //


            _deferral = taskInstance.GetDeferral();

            RunBot().Wait(-1);

            _deferral.Complete();

        }

        private static async Task RunBot()
        {
            var Bot = new Api("203067277:AAGzj4MXygP0FIWjoC80Oog0nTOQJGKXbEI");

            var me = await Bot.GetMe();

            var offset = 0;

            while (true)
            {
                var updates = await Bot.GetUpdates(offset);

                foreach (var update in updates)
                {
                    switch (update.Type)
                    {
                        case UpdateType.MessageUpdate:
                            var message = update.Message;

                            DateTime m = update.Message.Date.ToLocalTime();
                            string filename = @"\logs_tg\#perthfurs" + "." + dateformat + ".log";
                            FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.ReadWrite);

                            // remove unsightly characters from usernames.
                            string s_cleanname = update.Message.From.FirstName;
                            s_cleanname = Regex.Replace(s_cleanname, @"[^\u0000-\u007F]", string.Empty);

                            switch (message.Type)
                            {
                                case MessageType.TextMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(timeformat) + "] " + "<" + s_cleanname + "> " + update.Message.Text);
                                    }
                                    break;
                                case MessageType.StickerMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(timeformat) + "] * " + s_cleanname + " has posted an unknown sticker!");
                                    }
                                    break;
                                case MessageType.PhotoMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        // download the caption for the image, if there is one.
                                        string s = update.Message.Caption;

                                        // check to see if the caption string is empty or not
                                        if (s == string.Empty || s == null || s == "" || s == "/n")
                                        {
                                            sw.WriteLine("[" + m.ToString(timeformat) + "] " + "* " + s_cleanname + " has posted a photo message with no caption.");
                                        }
                                        else
                                        {
                                            sw.WriteLine("[" + m.ToString(timeformat) + "] " + "* " + s_cleanname + " has posted a photo message with the caption '" + s + "'.");
                                        }
                                    }
                                    break;
                            }
                            break;
                    }

                    offset = update.Id + 1;
                }
            }
        }
    }
}
