﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Collections.Generic;
using CoreTweet;
using CoreTweet.Streaming;
namespace DaringChildhoodFriendBot
{
    public class Program
    {
        private static T load<T>(string path, T default_)
        {
            var x = new XmlSerializer(typeof(T));
            if (File.Exists(path))
            {
                using (var y = File.OpenRead(path))
                {
                    return (T)x.Deserialize(y);
                }
            }
            else
            {
                save(path, default_);
                return default_;
            }
        }
        private static void save<T>(string path, T obj)
        {
            var x = new XmlSerializer(typeof(T));
            File.Delete(path);
            using (var y = File.Open(path, FileMode.Create))
            {
                x.Serialize(y, obj);
            }
        }
        private static Tokens tokens_ = null;
        private static Tokens tokens
        {
            get
            {
                if (tokens_ == null)
                {
                    tokens_ = load("bot.xml", new Tokens());
                }
                return tokens_;
            }
        }
        [Serializable]
        public struct serializeable_pair<K, V>
        {
            public serializeable_pair(KeyValuePair<K, V> kv)
            {
                key = kv.Key;
                value = kv.Value;
            }
            public K key;
            public V value;
        }
        private static void exec_command(Status s, string command)
        {
            var regex = new Regex(@"([0-9a-zA-Z_]+)(.*)$");
            var match = regex.Match(command);
            if (match.Success)
            {
                if (match.Groups[1].Value.Equals("register_call_name"))
                {
                    register_call_name(s, match);
                }
                else if (match.Groups[1].Value.Equals("register_pre_developer"))
                {
                    register_pre_developer(s);
                }
                else if (match.Groups[1].Value.Equals("register_developer") && s.User.Id == tokens.Users.Show(screen_name => "FriendOfCpper").Id)
                {
                    register_developer(s, match);
                }
            }
        }

        private static void register_developer(Status s, Match match)
        {

            var developer_table = load("pre_developer.xml", new Dictionary<serializeable_pair<string, string>, long>().Select(kv => new serializeable_pair<serializeable_pair<string, string>, long>(kv)).ToList()).ToDictionary(kv => kv.key, kv => kv.value);
            var reg = new Regex(@" ([0-9a-zA-Z]+) ([0-9a-zA-Z]+)");
            var m = reg.Match(match.Groups[2].Value);
            if (m.Success)
            {
                var key = new serializeable_pair<string, string>(new KeyValuePair<string, string>(m.Groups[1].Value, m.Groups[2].Value));
                if (developer_table.ContainsKey(key))
                {
                    var new_developer = developer_table[key];
                    developer_table.Remove(key);
                    save("pre_developer.xml", developer_table.Select(kv => new serializeable_pair<serializeable_pair<string, string>, long>()).ToList());
                    var new_developer_table = load("developer.xml", new HashSet<long>());
                    new_developer_table.Add(new_developer);
                    save("developer.xml", new_developer_table);
                    tokens.Statuses.Update(status => "@" + tokens.Users.Show(user_id => new_developer).ScreenName + " registered with the developers you", in_reply_to_status_id => s.Id);
                }
            }
        }

        private static void register_pre_developer(Status s)
        {

            var random = new Random(DateTime.Now.Millisecond);
            var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            var key1 = BitConverter.ToString(sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(random.Next().ToString()))).Replace("-", "");
            var key2 = BitConverter.ToString(sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(random.Next().ToString()))).Replace("-", "");
            var developer_table = load("pre_developer.xml", new Dictionary<serializeable_pair<string, string>, long>().Select(kv => new serializeable_pair<serializeable_pair<string, string>, long>(kv)).ToList()).ToDictionary(kv => kv.key, kv => kv.value);
            if (developer_table.ContainsValue((long)s.User.Id))
            {
                developer_table = developer_table.Where(kv => kv.Value != (long)s.User.Id).ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            developer_table.Add(new serializeable_pair<string, string>(new KeyValuePair<string, string>(key1, key2)), (long)s.User.Id);
            save("pre_developer.xml", developer_table.Select(kv => new serializeable_pair<serializeable_pair<string, string>, long>(kv)).ToList());
            tokens.Statuses.Update(status => "@" + s.User.ScreenName + " register your pre developer code : " + key1, in_reply_to_status_id => s.Id);
            tokens.Statuses.Update(status => "@FriendOfCpper [command] developer_key : " + s.User.ScreenName + "'s pre developer key : " + key2, in_reply_to_status_id => s.Id);
        }

        private static void register_call_name(Status s, Match match)
        {

            var name_table = load("call_name.xml", new Dictionary<long, string>().Select(kv => new serializeable_pair<long, string>(kv)).ToList()).ToDictionary(kv => kv.key, kv => kv.value);
            if (name_table.ContainsKey((long)s.User.Id))
            {
                name_table.Remove((long)s.User.Id);
            }
            var name = match.Groups[2].Value.Substring(1);
            name_table.Add((long)s.User.Id, name);
            save("call_name.xml", name_table.Select(kv => new serializeable_pair<long, string>(kv)).ToList());
            tokens.Statuses.Update(status => "@" + s.User.ScreenName + " register your name : " + name, in_reply_to_status_id => s.Id);
        }
        private static void parse(Status s)
        {
            lock (tokens.Statuses)
            {
                try
                {
                    if (!(bool)s.IsRetweeted)
                    {
                        var regex = new Regex(@"^@FriendOfCpper \[command\] (.*)$");
                        var match = regex.Match(s.Text);
                        if (match.Success)
                        {
                            exec_command(s, match.Groups[1].Value);
                        }
                        else
                        {
                            do_reply(s);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static void do_reply(Status s)
        {

            var reply_tweet_table = load("reply_tweet.xml", new List<List<string>>());
            string tweet = "@" + s.User.ScreenName + " ";
            var name_table = new Dictionary<long, string>();
            name_table = load("call_name.xml", name_table.Select(kv => new serializeable_pair<long, string>(kv)).ToArray()).ToDictionary(kv => kv.key, kv => kv.value);
            foreach (var set in reply_tweet_table)
            {
                if (Regex.IsMatch(s.Text, set[0]))
                {
                    var add_str = replace_escape(s, name_table, DateTime.Now, set[1]);
                    tweet += add_str;
                    break;
                }
            }
            Console.WriteLine("Tweet.");
            Console.WriteLine(tweet);
            tokens.Statuses.Update(status => tweet, in_reply_to_status_id => s.Id);
        }

        private static string replace_escape(Status s, Dictionary<long, string> name_table, DateTime now, string add_str)
        {

            add_str = add_str.Replace("#{挨拶}", Greeting[time_table[now.Hour]]);
            if (s.User.Id != null && name_table.ContainsKey((long)s.User.Id))
            {
                add_str = add_str.Replace("#{名前}", name_table[(long)s.User.Id]);
            }
            else
            {
                add_str = add_str.Replace("#{名前}", s.User.ScreenName);
            }
            return add_str;
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
                foreach (var m in tokens.Streaming.StartStream(StreamingType.Public, new StreamingParameters(track => "@" + tokens.Account.VerifyCredentials().ScreenName)))
                {
                    if (m is StatusMessage)
                    {
                        reply((m as StatusMessage).Status);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private static System.Timers.Timer start_timer;
        private static System.Timers.Timer tweet_timer;
        private static int now_hour;
        private static void on_tweet(object source, ElapsedEventArgs e)
        {
            lock (tokens.Statuses)
            {
                try
                {
                    var auto_tweet_table = load("auto_tweet.xml", new List<List<string>>());
                    var x_ = new XmlSerializer(typeof(List<List<string>>));
                    using (var y = File.OpenRead("auto_tweet.xml"))
                    {
                        auto_tweet_table = (List<List<string>>)x_.Deserialize(y);
                    }
                    var random = new Random();
                    Console.WriteLine("Tweet.");
                    Console.WriteLine(auto_tweet_table[now_hour][random.Next(0, auto_tweet_table[now_hour].ToArray().Length)]);
                    tokens.Statuses.Update(status => auto_tweet_table[now_hour][random.Next(0, auto_tweet_table[now_hour].ToArray().Length)]);
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
        private static void tweet_thread()
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
        private enum time_traits
        {
            Morning,
            Noon,
            Evening,
            Night,
        }
        private static time_traits[] time_table =
        {
            time_traits.Night,//0
            time_traits.Night,//1
            time_traits.Night,//2
            time_traits.Night,//3
            time_traits.Night,//4
            time_traits.Night,//5
            time_traits.Morning,//6     
            time_traits.Morning,//7
            time_traits.Morning,//8
            time_traits.Morning,//9
            time_traits.Morning,//10
            time_traits.Noon,//11
            time_traits.Noon,//12
            time_traits.Noon,//13
            time_traits.Noon,//14
            time_traits.Noon,//15
            time_traits.Noon,//16
            time_traits.Evening,//17
            time_traits.Evening,//18
            time_traits.Night,//19
            time_traits.Night,//20
            time_traits.Night,//21
            time_traits.Night,//22
            time_traits.Night,//23
        };
        private static Dictionary<time_traits, string> Greeting_ = null;
        private static Dictionary<time_traits, string> Greeting
        {
            get
            {
                if (Greeting_ == null)
                {
                    Greeting_ = new Dictionary<time_traits, string>();
                    Greeting_.Add(time_traits.Morning, "おはよう");
                    Greeting_.Add(time_traits.Noon, "こんにちは");
                    Greeting_.Add(time_traits.Evening, "こんばんは");
                    Greeting_.Add(time_traits.Night, "こんばんは");
                }
                return Greeting_;
            }
        }
        private static void Main(string[] args)
        {
            Thread rt = new Thread(new ThreadStart(reply_thread));
            tweet_thread();
            rt.Start();
        }
    }
}
