using Bankly.Sdk.Kafka;
using Bankly.Sdk.Kafka.Values;
using KafkaTest.Models;
using Microsoft.AspNetCore.Mvc;

namespace KafkaTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        static int count = 0;
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

            var customer = GetCustomer();
            await _producerMessage.ProduceAsync("test.temp", customer, header);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        private Customer GetCustomer()
        {
            int refer = count++;
            return new Customer
            {
                Name = $"Test Name {refer}",
                BirthDate = DateTime.Now,
                Created = DateTime.Now,
                MotherName = $"Test Mother {refer}",
                Status = refer % 2 == 0 ? CustomerStatusType.Simple : CustomerStatusType.Complete,
                DocumentNumber = $"DocumentNumber {refer}",
                Address = new Address
                {
                    City = $"City {refer}",
                    ZipCode = $"ZipCode {refer}",
                    Complement = $"Complement {refer}",
                    Neighborhood = $"Neighborhood {refer}",
                    Number = $"Number {refer}",
                    State = $"State {refer}",
                    Street = $"Street {refer}"
                },
                Contacts = new List<Contact> { new Contact { Type = ContactType.Email, Value = $"Value {refer}" } }
            };
        }
    }
}