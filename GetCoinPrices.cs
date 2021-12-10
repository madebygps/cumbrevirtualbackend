using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class GetCoinPrices
    {
        private readonly ILogger _logger;

        public GetCoinPrices(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetCoinPrices>();
        }

        [Function("GetCoinPrices")]
        public async static Task<MyOutputType> Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] MyInfo myTimer,FunctionContext context)
        {
            var _logger = context.GetLogger("GetCoinPrices");
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        
            Coin [] prices;

            using (var httpClient = new HttpClient())
            {
                var coinData = await httpClient.GetAsync("https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=100&page=1&sparkline=false&price_change_percentage=1h");
                var body = await coinData.Content.ReadAsStringAsync();
                prices = JsonSerializer.Deserialize<Coin[]>(body);
                
            }
            foreach(var price in prices)
                {
                    _logger.LogInformation(price.name);
                }

                MySignalRMessage mySignalRMessage = new MySignalRMessage()
                {
                    Target = "updated",
                    Arguments = new object []{prices}
                };


                return new MyOutputType()
                {
                    Coins = prices,
                    NewMessage = mySignalRMessage
                };
            

        }
    }

    public class MyOutputType
    {
        [CosmosDBOutput("%DatabaseName%", "%CollectionName%", Connection = "CosmosDBConnectionString")]
        public Coin[] Coins {get; set;}

        [SignalROutput(HubName = "coins", ConnectionStringSetting = "AzureSignalRConnectionString")]
        public MySignalRMessage NewMessage {get; set;}
    }

    public class MySignalRMessage 
    {
        public string Target {get; set;}
        public object Arguments {get; set;}
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
