using Kafka;
using Kafka.Notifications;
using KafkaTest.Models;
using Microsoft.AspNetCore.Mvc;

namespace KafkaTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestWeatherForecastController : ControllerBase
    {
        static int count = 0;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IProducerMessage _producerMessage;
        private readonly ILogger<WeatherForecastController> _logger;

        public TestWeatherForecastController(ILogger<WeatherForecastController> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }

        [HttpGet(Name = "GetTest")]
        public async Task<IEnumerable<WeatherForecast>> GetTest()
        {
            var customer = GetCustomerNotification();
            await _producerMessage.ProduceAsync("bankly.event.customers", "001", customer);

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


        private CustomerNotification GetCustomerNotification()
        {
            var notification = new CustomerNotification();
            var customer = GetCustomer();
            
            notification.Name = "CUSTOMER_WAS_CREATED";
            notification.Timestamp = DateTime.Now;
            notification.Data = customer;
            notification.EntityId = customer.DocumentNumber;
            notification.Context = "ACCOUNT";
            notification.Metadata = new Dictionary<string, object>();
            notification.Metadata.Add("Test", "test");
            notification.CompanyKey = "BANKLY";

            return notification;
        }

    }

    public class CustomerNotification : IEventNotification<Customer>
    {
        public string EntityId { get; set; }

        public string CompanyKey { get; set; }

        public string Context { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

        public Customer Data { get; set; }
    }
}