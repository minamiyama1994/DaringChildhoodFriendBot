using System;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using CoreTweet;
using CoreTweet.Streaming;

namespace DaringChildhoodFriendBot
{
    class Program
    {
        static Tokens _tokens;
        static Tokens tokens
        {
            get
            {
                var x = new XmlSerializer(typeof(Tokens));
                if (_tokens == null)
                {
                    if (File.Exists("bot.xml"))
                        using (var y = File.OpenRead("bot.xml"))
                            _tokens = (Tokens)x.Deserialize(y);
                    else
                    {
                        var se = OAuth.Authorize("xRXCNtYCXmRX5J8Cwtj9RA", "PEKcPY3gGjERFJOar5aF0yLVH9LFf3WerSevze4a5Y");
                        Console.WriteLine(se.AuthorizeUri);
                        Console.Write("pin> ");
                        _tokens = OAuth.GetTokens(Console.ReadLine(), se);
                        using (var y = File.Open("bot.xml", FileMode.OpenOrCreate, FileAccess.Write))
                            x.Serialize(y, _tokens);
                    }
                }
                return _tokens;
            }
        }
        static void parse(Status s)
        {
            lock (tokens.Statuses)
            {
                try
                {
                    string[][] reply_tweet_table;
                    var x_ = new XmlSerializer(typeof(string[][]));
                    using (var y = File.OpenRead("reply_tweet.xml"))
                    {
                        reply_tweet_table = (string[][])x_.Deserialize(y);
                    }
                    string tweet = "@" + s.User.ScreenName + " ";
                    foreach (var set in reply_tweet_table)
                    {
                        if (Regex.IsMatch(s.Text, set[0]) && s.RetweetCount == 0)
                        {
                            tweet += set[1];
                            break;
                        }
                    }
                    Console.WriteLine("Tweet.");
                    Console.WriteLine(tweet);
                    tokens.Statuses.Update(status => tweet, in_reply_to_status_id => s.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        static void reply(Status s)
        {
            try
            {
                parse(s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        static void reply_thread()
        {
            try
            {
                foreach (var m in tokens.Streaming.StartStream(new StreamingParameters(track => "@" + tokens.Account.VerifyCredentials().ScreenName), StreamingType.Public))
                    if (m is StatusMessage)
                        reply(((StatusMessage)m).Status);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        static System.Timers.Timer start_timer;
        static System.Timers.Timer tweet_timer;
        static int now_hour;
        static void on_tweet(object source, ElapsedEventArgs e)
        {
            lock (tokens.Statuses)
            {
                try
                {
                    string[][] auto_tweet_table;
                    var x_ = new XmlSerializer(typeof(string[][]));
                    using (var y = File.OpenRead("auto_tweet.xml"))
                    {
                        auto_tweet_table = (string[][])x_.Deserialize(y);
                    }
                    var random = new Random();
                    Console.WriteLine("Tweet.");
                    Console.WriteLine(auto_tweet_table[now_hour][random.Next(0, auto_tweet_table[now_hour].Length)]);
                    tokens.Statuses.Update(status => auto_tweet_table[now_hour][random.Next(0, auto_tweet_table[now_hour].Length)]);
                    ++now_hour;
                    now_hour %= 24;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        static void on_tweet_init(object source, ElapsedEventArgs e)
        {
            tweet_timer = new System.Timers.Timer();
            tweet_timer.Elapsed += on_tweet;
            tweet_timer.AutoReset = true;
            tweet_timer.Interval = 60 * 60 * 1000;
            tweet_timer.Enabled = true;
            var current_event = new System.Timers.Timer();
            current_event.Elapsed += on_tweet;
            current_event.AutoReset = false;
            current_event.Interval = 1000;
            current_event.Enabled = true;
        }
        static void tweet_thread()
        {
            Console.WriteLine("Auto Tweet Init.");
            start_timer = new System.Timers.Timer();
            start_timer.Elapsed += on_tweet_init;
            start_timer.AutoReset = false;
            var next_time = DateTime.Now.Date + new TimeSpan(DateTime.Now.Hour + 1, 0, 0);
            start_timer.Interval = (next_time - DateTime.Now).TotalMilliseconds;
            start_timer.Enabled = true;
            now_hour = next_time.Hour;
            Console.WriteLine("Duration of first auto tweet is {0}min.", (next_time - DateTime.Now).Minutes);
            Console.WriteLine("First auto tweet is {0}hr.", now_hour);
        }
        static void Main(string[] args)
        {
            var _ = tokens;
            Thread rt = new Thread(new ThreadStart(reply_thread));
            tweet_thread();
            rt.Start();
        }
    }
}
