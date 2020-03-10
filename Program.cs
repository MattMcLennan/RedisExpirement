using System;
using System.Diagnostics;
using System.Linq;
using ServiceStack.Redis;

namespace RedisExpirement {
    class Program {
        public static void Main (string[] args) {
            using (var redis = new RedisClient ()) {
                // Performance to Test
                // Adding full list to redis
                // Get all indexes as a time table
                // want to see how time changes for different items in the list

                redis.FlushDb ();
                var data = new MockedData (300000);

                var list = new RedisList (Guid.NewGuid ().ToString (), redis, data);
                var set = new RedisSortedSet (Guid.NewGuid ().ToString (), redis, data);

                Console.WriteLine ("List Testing");
                PerformanceTesting.TimeOperation ("Insert all data into a list", list.AddAllRecords);
                PerformanceTesting.TimeOperation ("Lookup all records sequentially - one by one", list.LookupRecordsSequentially);

                Console.WriteLine ("++++++++++============================================++++++++++");

                Console.WriteLine ("Sorted Set Testing");
                PerformanceTesting.TimeOperation ("Insert all data into a sortedset", set.AddAllRecords);
                PerformanceTesting.TimeOperation ("Lookup all records sequentially - one by one", set.LookupRecordsSequentially);
            }
        }

        public static class PerformanceTesting {
            public static TimeSpan TimeOperation (string message, Action action) {
                var sw = Stopwatch.StartNew ();
                action ();
                sw.Stop ();

                Console.WriteLine ($"Testing {message} - Elapsed = {sw.Elapsed}");
                return sw.Elapsed;
            }
        }

        public interface IRedis {
            void AddAllRecords (MockedData data);
            void LookupRecord (int n);
            void LookupRecordsSequentially ();
        }

        public abstract class Redis {
            protected string Identifier { get; set; }
            protected long[] Records { get; set; }
            protected RedisClient RedisClient { get; set; }
            public Redis (string identifier, RedisClient redisClient, MockedData data) {
                Identifier = identifier;
                Records = data.Records;
                RedisClient = redisClient;
            }

            public abstract void AddAllRecords ();
            public abstract void LookupRecord (int index);

            public void LookupRecordsSequentially () {
                for (int i = 0; i < Records.Length; i++)
                    LookupRecord (i);
            }
        }

        public class RedisList : Redis {
            public RedisList (string identifier, RedisClient redisClient, MockedData data) : base (identifier, redisClient, data) { }

            public override void AddAllRecords () {
                var result = Records.Select (x => x.ToString ()).ToList ();
                RedisClient.AddRangeToList ("1", result);
            }

            public override void LookupRecord (int index) {
                var record = RedisClient.GetItemFromList (Identifier, index);
            }
        }

        public class RedisSortedSet : Redis {
            public RedisSortedSet (string identifier, RedisClient redisClient, MockedData data) : base (identifier, redisClient, data) { }

            public override void AddAllRecords () {
                for (var i = 0; i < Records.Length; i++)
                    RedisClient.AddItemToSortedSet (Identifier, Records[i].ToString (), i);
            }

            public override void LookupRecord (int index) {
                var record = RedisClient.GetRangeFromSortedSet (Identifier, index, index);
            }
        }

        public class MockedData {
            public long[] Records { get; set; }
            public MockedData (long numOfRecords) {
                CreateRecords (numOfRecords);
            }

            private void CreateRecords (long numOfRecords) {
                Records = new long[numOfRecords];
                for (var i = 0; i < Records.Length; i++) {
                    Records[i] = i * 1000;
                }
            }
        }
    }
}