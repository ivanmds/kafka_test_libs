using Kafka;
using Kafka.Values;
using Microsoft.AspNetCore.Mvc;

namespace KafkaTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IProducerMessage _producerMessage;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }



        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var header = HeaderValue.Create("answer_to_topic", "test_local_response");
            header.PutKeyValue("test_another_head", "yes, It´s work");


            await _producerMessage.ProduceAsync("test.temp", "001", new { Message = "Hello, world" }, header, default);

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