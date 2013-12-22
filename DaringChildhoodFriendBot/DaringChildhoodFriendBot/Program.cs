using System;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using CoreTweet;
using CoreTweet.Streaming;

namespace DaringChildhoodFriendBot
{
    class Program
    {
        static XmlSerializer x = new XmlSerializer(typeof(Tokens));
        static Tokens res = null;
        static Tokens t()
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
        static void parse(Status s)
        {
            lock (t().Statuses)
            {
                if (Regex.IsMatch(s.Text, @".*おはよう.*"))
                {
                    t().Statuses.Update(status => "@" + s.User.ScreenName + " おはようっ！", in_reply_to_status_id => s.Id);

                }
                else if (Regex.IsMatch(s.Text, @".*(([vｖVＶ][cｃCＣ]?([+＋][+＋]))|([vｖVＶ][iｉIＩ][sｓSＳ][uＵUＵ][aаAА][lｌLＬ]\s+[cｃCＣ]\+\+)).*使.*"))
                {
                    t().Statuses.Update(status => "@" + s.User.ScreenName + " えっ何Visual C++なんか使ってんの！？　わかった今からバールのようなものを持ってくるからそこでおとなしく待っててね！", in_reply_to_status_id => s.Id);
                }
                else if (Regex.IsMatch(s.Text, @".*(([vｖVＶ][cｃCＣ]?([+＋][+＋]))|([vｖVＶ][iｉIＩ][sｓSＳ][uＵUＵ][aаAА][lｌLＬ]\s+[cｃCＣ]\+\+)).*"))
                {
                    t().Statuses.Update(status => "@" + s.User.ScreenName + " アレはC++処理系じゃないからね！　Visual C++処理系だからね！　使うのは勝手だけどC++じゃないからね！　C++使うつもりだったら絶対に使っちゃダメだよ！", in_reply_to_status_id => s.Id);
                }
                else if (Regex.IsMatch(s.Text, @".*[cｃCＣ].*言語.*"))
                {
                    t().Statuses.Update(status => "@" + s.User.ScreenName + " 上位互換性は保証されてないからね！　「CのコードはC++でも動くはず」なんてそんなことはないからね！　もしそんなこと思ってるならCでしか動かない邪悪なコードを見せてやるから！", in_reply_to_status_id => s.Id);
                }
                else
                {
                    t().Statuses.Update(status => "@" + s.User.ScreenName + " 何っ！？　呼んだ！？　ねえ今呼んだでしょなになになんの話聞かせて！", in_reply_to_status_id => s.Id);
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
                Console.WriteLine("err: failed to send at " + DateTime.Now.ToString() + ", to " + s.Id.ToString() + "\n" + e.ToString());
            }
        }
        static void reply_thread()
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
                Console.WriteLine("err: lost connection");
                throw;
            }
        }
        static void tweet_thread()
        {
        }
        static void Main(string[] args)
        {
            t();
            Thread rt = new Thread(new ThreadStart(reply_thread));
            Thread tt = new Thread(new ThreadStart(tweet_thread));
            rt.Start();
            tt.Start();
        }
    }
}
