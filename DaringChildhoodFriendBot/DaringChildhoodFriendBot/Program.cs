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
        static string[][] tweet_table = new [ ]
        {
                "あたしはもう寝るからな！　寝るんだからな！　嘘じゃないぞ詐欺じゃないぞ寝るからな！　昨日という日はもう終わってスコープは閉じたんだからデストラクタが呼ばれてあたしというリソースを開放するのは当たり前だろ！　あんたは何故か外部から参照されてるshared_ptrみたいだがな！" ,
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
        }.Select(x => new []{x}).ToArray();
        
        #region reply

        static void parse(Status s)
        {
            lock (tokens.Statuses)
            {
                try
                {
                    var tweet = "@" + s.User.ScreenName;
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

        #endregion

        #region tweet

        static System.Timers.Timer start_timer;
        static System.Timers.Timer tweet_timer;
        static int now_hour;

        static void on_tweet(object source, ElapsedEventArgs e)
        {
            lock (tokens.Statuses)
            {
                try
                {
                    var random = new Random();
                    Console.WriteLine("Tweet.");
                    Console.WriteLine(tweet_table[now_hour][random.Next(0, tweet_table[now_hour].Length)]);
                    tokens.Statuses.Update(status => tweet_table[now_hour][random.Next(0, tweet_table[now_hour].Length)]);
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

        #endregion

        static void Main(string[] args)
        {
            var _ = tokens;
            Thread rt = new Thread(new ThreadStart(reply_thread));
            tweet_thread();
            rt.Start();
        }
    }
}
