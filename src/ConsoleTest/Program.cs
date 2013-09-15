using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DynamicProperties();
        }

        public static void DynamicProperties()
        {
            ChuckNorrisJokeResponse jokeResponse = null;

            SimpleWebApi.WebApiClient.Get<ChuckNorrisJokeResponse>("http://api.icndb.com/jokes/random", null,
                queryStringParameters: new {firstName = "John", lastName = "Doe"})
                .ContinueWith((taskResult) =>
                {
                    jokeResponse = taskResult.Result;
                    Console.WriteLine("Got result.");
                }).Wait(TimeSpan.FromMinutes(1));
                

            Console.WriteLine("Done.");
            Console.ReadLine();

        }

        public class ChuckNorrisJoke
        {
            public int id { get; set; }
            public string joke { get; set; }
        }

        public class ChuckNorrisJokeResponse
        {
            public string type { get; set; }
            public ChuckNorrisJoke value { get; set; }
        }

    }
}
