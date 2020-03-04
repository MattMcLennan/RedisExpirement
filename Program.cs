using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace RedisExpirement {
    class Program {
        public static void Main (string[] args) {
            using (var redis = new RedisClient ()) {
                // Performance to Test
                // Adding full list to redis
                // Get all indexes as a time table
                // want to see how time changes for different items in the list
                // https://stackoverflow.com/questions/969290/exact-time-measurement-for-performance-testing

                var mockedData = new MockedData (300000);
                List<string> result = mockedData.Records.Select (x => x.ToString ()).ToList ();
                redis.AddRangeToList ("1", result);

                var listCount = redis.GetListCount ("1");
                Console.WriteLine (listCount);
            }
        }

        public static class Redis {

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