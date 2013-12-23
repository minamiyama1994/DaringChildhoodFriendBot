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
        private static string[] tweet_table =
        {
            "あたしはもう寝るからな！　絶対に寝るんだからな！　嘘じゃないぞ詐欺じゃないぞ寝るからな！　昨日という日はもう終わってスコープは閉じたんだからデストラクタが呼ばれてあたしというリソースを開放するのは当たり前だろ！　あんたは何故か外部から参照されてるshared_ptrみたいだがな！" ,
            "……zzz……constexpr……" ,
            "……っひぃっ！　マクロ！　マクロオオオオオォォォォォ……zzz……" ,
            "……TMPで……世界征服……zzz……" ,
            "……うにゃっ！　やめろ！　それ規格違反……！……zzz……" ,
            "……zzz……ううーん……Java、Java……" ,
            "……zzz……NULL……" ,
            "……zzz……ん、もう朝か……起きるか……………………よーしおっはよーございまーっす！　今日も？　元気に？　例外投げまくろうぜ！　……あれ、なんかおかしい" ,
            "さあ朝だぞ！　起きろさあ起きるんだ起きないとあんたが使ってるコンパイラをあたし特製の「undefined behaviorによりPCのエロ画像が全て削除される規格準拠なコンパイラ」に差し替えるぞさあ起きろ！" ,
            "今日は仕事かな？　学校かな？　バイトかな？　趣味かな？　なんでもいいけどさっさと動けー！　N3797で殴るぞ！" ,
            "あー！　「職場の誰もC++11 laterを学ぼうとする意欲がないせいで新規開発にもかかわらずC++03ベースなプロジェクト」だ！　殺せ！" ,
            "あんたいつまでNULL使ってんだよnullptr使えよ原始人かそうか原始人だったなそのままC++03使っとけ！　知らん！" ,
            "おっ昼だぞー！　何食べる？　メモリ？　メモリ食べちゃう？　newしまくってdeleteしないでメモリ食べまくっちゃう？　ってんなわけあるかー！　生ポインタ使ってるんじゃなーい！　そこはスマートポインタを使えー！" ,
            "そこは！　ラムダ関数を！　使え！　ちっさい特に意味のない一回しか使わない関数オブジェクトをいくつも定義してるんじゃなーい！　ポケット鈍器で殴ってやろうか！？" ,
            "ヘッダファイルでのusing namespaceだ！　殺せ！" ,
            "なーんでautoがあるのになんでそこでわざわざiteratorの型を明示してるんだ長いだろっていうかそれ範囲for文でいいだろ！" ,
            "Better C？　知らん！　時代はModern C++だ！" ,
            "何C言語手書きしてるんだC言語はコードジェネレーターに吐き出させるための言語だろ人間が書く言語じゃなーい！" ,
            "時代はC++だ！　Haskell？　知らん！　D？　知らん！　時代はC++だってうわぁなんだこの数千行のエラーメッセージはあああああァァァァァ！" ,
            "さあそれでは今から規格準拠なC++コンパイラをつくろうじゃないか！" ,
            "さあ一緒にー？　れ☆い☆が☆い☆あ☆ん☆ぜ☆ん！　さあ一緒にー？　れ☆い☆が☆い☆あ☆ん☆ぜ☆ん！　さあ一緒にー？　れ☆い☆が☆い☆あ☆ん☆ぜ☆ん！　さあ一緒にー？　れ☆い☆が☆い☆あ☆ん☆ぜ☆ん！　さあ一緒にー？　れ☆い☆が☆い☆あ☆ん☆ぜ☆ん！　さあ一緒にー？　れ☆い☆が☆い" ,
            "Visua C++はC++コンパイラじゃないからな！　違うからな！　間違えるなよ！　絶対に間違えるなよ！" ,
            "さあ今日もそろそろ終わるぞ！　やらなきゃいけないことはさっさと終わらす！　いいね絶対だよ？　その日のことはその日に終わらす！　つまりRAIIだな！" ,
            "き　ょ　う　も　い　ち　に　ち　お疲れ様でしたー！　疲れてるでしょ？　頭も働かないでしょ？　さあ寝ようさっさと寝ようそこに布団があるだろすぐ入るんだ！　え？　残業？　課題？　そんなの知ったこっちゃなーいはよ寝ろ！　abortだabort！" ,
       };
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
                    if (Regex.IsMatch(s.Text, @".*おはよう.*"))
                    {
                        t().Statuses.Update(status => "@" + s.User.ScreenName + " おはようっ！", in_reply_to_status_id => s.Id);
                    }
                    else if (Regex.IsMatch(s.Text, @".*はじめまして.*"))
                    {
                        t().Statuses.Update(status => "@" + s.User.ScreenName + " はじめまして！　かな？　なのかな？　はじめましてなのかな？　なにはともあれよろしくぅ！", in_reply_to_status_id => s.Id);

                    }
                    else if (Regex.IsMatch(s.Text, @".*(([vｖVＶ][cｃCＣ]([+＋][+＋])?)|([vｖVＶ][iｉIＩ][sｓSＳ][uＵUＵ][aаAА][lｌLＬ]\s*[cｃCＣ]\+\+)).*使.*"))
                    {
                        t().Statuses.Update(status => "@" + s.User.ScreenName + " えっ何Visual C++なんか使ってんの！？　わかった今からバールのようなものを持ってくるからそこでおとなしく待っててね！", in_reply_to_status_id => s.Id);
                    }
                    else if (Regex.IsMatch(s.Text, @".*(([vｖVＶ][cｃCＣ]([+＋][+＋])?)|([vｖVＶ][iｉIＩ][sｓSＳ][uＵUＵ][aаAА][lｌLＬ]\s+[cｃCＣ]\+\+)).*"))
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
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
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
                Console.WriteLine("err: failed to send at " + DateTime.Now.ToString() + ", to " + s.Id.ToString() + "\n" + e.ToString());
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
                Console.WriteLine("err: lost connection");
                throw;
            }
        }
        private static void on_tweet(object source, ElapsedEventArgs e)
        {
            lock (t().Statuses)
            {
                if (tweet_table[now_hour] != null)
                {
                    t().Statuses.Update(status => tweet_table[now_hour]);
                }
                ++now_hour;
                if (now_hour == 24)
                {
                    now_hour = 0;
                }
            }
        }
        private static void on_tweet_init(object source, ElapsedEventArgs e)
        {
            tweet_timer = new System.Timers.Timer();
            tweet_timer.Enabled = true;
            tweet_timer.AutoReset = true;
            tweet_timer.Interval = 60 * 60 * 1000;
            tweet_timer.Elapsed += new ElapsedEventHandler(on_tweet);
        }
        private static void tweet_thread()
        {
            start_timer = new System.Timers.Timer();
            start_timer.Enabled = true;
            start_timer.AutoReset = false;
            var now = DateTime.Now;
            var next_time = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0);
            start_timer.Interval = (next_time - now).Milliseconds;
            start_timer.Elapsed += new ElapsedEventHandler(on_tweet_init);
            now_hour = next_time.Hour;
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
