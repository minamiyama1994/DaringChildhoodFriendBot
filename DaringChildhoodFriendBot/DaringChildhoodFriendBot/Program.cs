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
        private static readonly XmlSerializer x = new XmlSerializer(typeof(Tokens));
        private static Tokens res = null;
        private static System.Timers.Timer start_timer;
        private static System.Timers.Timer tweet_timer;
        private static int now_hour;
        private static Random random = new Random();
        private static Tokens t()
        {
            if (res == null)
            {
                if (File.Exists("bot.xml"))
                {
                    using (var y = File.OpenRead("bot.xml"))
                    {
                        res = (Tokens)x.Deserialize(y);
                    }
                }
                else
                {
                    var se = OAuth.Authorize("xRXCNtYCXmRX5J8Cwtj9RA", "PEKcPY3gGjERFJOar5aF0yLVH9LFf3WerSevze4a5Y");
                    Console.WriteLine(se.AuthorizeUri);
                    Console.Write("pin> ");
                    var g = OAuth.GetTokens(Console.ReadLine(), se);
                    using (var y = File.Open("bot.xml", FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        x.Serialize(y, g);
                    }
                    res = g;
                }
            }
            return res;
        }
        private static void parse(Status s)
        {
            lock (t().Statuses)
            {
                try
                {
                    string tweet = "@" + s.User.ScreenName;
                    if (Regex.IsMatch(s.Text, @".*おはよう.*"))
                    {
                        tweet += " おはようっ！";
                    }
                    else if (Regex.IsMatch(s.Text, @".*はじめまして.*"))
                    {
                        tweet += " はじめまして！　かな？　なのかな？　はじめましてなのかな？　なにはともあれよろしくぅ！";
                    }
                    else if (Regex.IsMatch(s.Text, @".*(([vｖVＶ][cｃCＣ]([+＋][+＋])?)|([vｖVＶ][iｉIＩ][sｓSＳ][uＵUＵ][aаAА][lｌLＬ]\s*[cｃCＣ]\+\+)).*使.*"))
                    {
                        tweet += " えっ何Visual C++なんか使ってんの！？　わかった今からバールのようなものを持ってくるからそこでおとなしく待っててね！";
                    }
                    else if (Regex.IsMatch(s.Text, @".*(([vｖVＶ][cｃCＣ]([+＋][+＋])?)|([vｖVＶ][iｉIＩ][sｓSＳ][uＵUＵ][aаAА][lｌLＬ]\s+[cｃCＣ]\+\+)).*"))
                    {
                        tweet += " アレはC++処理系じゃないからね！　Visual C++処理系だからね！　使うのは勝手だけどC++じゃないからね！　C++使うつもりだったら絶対に使っちゃダメだよ！";
                    }
                    else if (Regex.IsMatch(s.Text, @".*[cｃCＣ].*言語.*"))
                    {
                        tweet += " 上位互換性は保証されてないからね！　「CのコードはC++でも動くはず」なんてそんなことはないからね！　もしそんなこと思ってるならCでしか動かない邪悪なコードを見せてやるから！";
                    }
                    else
                    {
                        tweet += " 何っ！？　呼んだ！？　ねえ今呼んだでしょなになになんの話聞かせて！";
                    }
                    Console.WriteLine("Tweet.");
                    Console.WriteLine(tweet);
                    t().Statuses.Update(status => tweet, in_reply_to_status_id => s.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        private static void reply(Status s)
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
        private static void reply_thread()
        {
            try
            {
                foreach (var m in t().Streaming.StartStream(new StreamingParameters(track => "@" + t().Account.VerifyCredentials().ScreenName), StreamingType.Public))
                {
                    if (m is StatusMessage)
                    {
                        reply(((StatusMessage)m).Status);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private static void on_tweet(object source, ElapsedEventArgs e)
        {
            lock (t().Statuses)
            {
                try
                {
                    string[][] auto_tweet_table;
                    var x_ = new XmlSerializer(typeof(string[][]));
                    using (var y = File.OpenRead("auto_tweet.xml"))
                    {
                        auto_tweet_table = (string[][])x_.Deserialize(y);
                    }
                    Console.WriteLine("Tweet.");
                    Console.WriteLine(auto_tweet_table[now_hour][random.Next(0, auto_tweet_table[now_hour].Length)]);
                    t().Statuses.Update(status => auto_tweet_table[now_hour][random.Next(0, auto_tweet_table[now_hour].Length)]);
                    ++now_hour;
                    now_hour %= 24;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        private static void on_tweet_init(object source, ElapsedEventArgs e)
        {
            tweet_timer = new System.Timers.Timer();
            tweet_timer.Elapsed += new ElapsedEventHandler(on_tweet);
            tweet_timer.AutoReset = true;
            tweet_timer.Interval = 60 * 60 * 1000;
            tweet_timer.Enabled = true;
            var current_event = new System.Timers.Timer();
            current_event.Elapsed += new ElapsedEventHandler(on_tweet);
            current_event.AutoReset = false;
            current_event.Interval = 1000;
            current_event.Enabled = true;
        }
        private static void tweet_thread()
        {
            Console.WriteLine("Auto Tweet Init.");
            start_timer = new System.Timers.Timer();
            start_timer.Elapsed += new ElapsedEventHandler(on_tweet_init);
            start_timer.AutoReset = false;
            var now = DateTime.Now;
            var tmp_next_time = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var next_time = tmp_next_time + new TimeSpan(1, 0, 0);
            start_timer.Interval = (next_time - now).TotalMilliseconds;
            start_timer.Enabled = true;
            now_hour = next_time.Hour;
            Console.WriteLine("Duration of first auto tweet is {0}min.", (next_time - now).Minutes);
            Console.WriteLine("First auto tweet is {0}hr.", now_hour);
        }
        static void Main(string[] args)
        {
            t();
            Thread rt = new Thread(new ThreadStart(reply_thread));
            tweet_thread();
            rt.Start();
        }
    }
}
