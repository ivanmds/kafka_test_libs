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
    public class CustomerWeatherForecastController : ControllerBase
    {
        static int count = 0;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IProducerMessage _producerMessage;
        private readonly ILogger<WeatherForecastController> _logger;

        public CustomerWeatherForecastController(ILogger<WeatherForecastController> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }

        [HttpGet(Name = "GetCustomer")]
        public async Task<IEnumerable<WeatherForecast>> GetTest()
        {
            var header = HeaderValue.Create();
            header.AddCorrelationId(Guid.NewGuid().ToString());
            header.AddResponseTopic(""); //responseTopic

            var notification = GetCustomerNotification();
            await _producerMessage.ProduceWithBindNotificationAsync(notification.EntityId, notification, header);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        int refer = count++;
        private Customer GetCustomer()
        {
            
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


        private CustomerNotification GetCustomerNotification()
        {
            var notification = new CustomerNotification();
            var customer = GetCustomer();
            
            notification.Name = refer % 2 == 0 ? "CUSTOMER_WAS_CREATED" : "CUSTOMER_WAS_UPDATED";
            notification.Timestamp = DateTime.Now;
            notification.Data = customer;
            notification.EntityId = customer.DocumentNumber;
            notification.Context = Context.Account;
            notification.Metadata = new Dictionary<string, object>();
            notification.Metadata.Add("Test", "test");
            notification.CompanyKey = "BANKLY";

            return notification;
        }

    }
}