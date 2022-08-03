using Bankly.Sdk.Kafka;
using Bankly.Sdk.Kafka.Notifications;
using Bankly.Sdk.Kafka.Values;
using KafkaTest.Models;
using KafkaTest.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace KafkaTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardWeatherForecastController : ControllerBase
    {
        static int count = 0;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IProducerMessage _producerMessage;
        private readonly ILogger<WeatherForecastController> _logger;

        public CardWeatherForecastController(ILogger<WeatherForecastController> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }

        [HttpGet(Name = "GetCard")]
        public async Task<IEnumerable<WeatherForecast>> GetCard()
        {
            var header = HeaderValue.Create();
            header.AddCorrelationId(Guid.NewGuid().ToString());

            var notification = new CardNotification();
            await _producerMessage.ProduceWithBindNotificationAsync("1346", notification, header);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

    }
}