using Avro.Generic;
using Bankly.Sdk.Kafka;
using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.Mvc;
using Avro;
using Bankly.Sdk.Kafka.Avro;

namespace KafkaTest.Controllers
{
    public class PhoneDto
    {
        public string DDD { get; set; }
        public string Number { get; set; }
    }

    public class CustomerAvro
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        public PhoneDto Phone { get; set; }
    }


    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {

        private readonly IProducerMessage _producerMessage;
        private readonly ILogger<CustomerController> _logger;
        private readonly IGenericRecordConverter _genericRecordConverter;
        private static int count = 0;

        public CustomerController(ILogger<CustomerController> logger, IProducerMessage producerMessage, IGenericRecordConverter genericRecordConverter)
        {
            _logger = logger;
            _producerMessage = producerMessage;
            _genericRecordConverter = genericRecordConverter;
        }

        [HttpGet(Name = "GetCustomer")]
        public async Task<IEnumerable<WeatherForecast>> GetTest()
        {
            var topicName = "isa_hello";
            var customer1 = new CustomerAvro { Name = "Ivan", Address = "127.0.0.1", FatherName = "Ivan old", Phone = new PhoneDto { DDD = "11", Number = "123456789" } };

            var converted = await _genericRecordConverter.ParseToGenericRecordAsync(customer1, topicName);

            try
            {
                await _producerMessage.ProduceAsync(topicName, converted);
                //Console.WriteLine($"produced to: {dr.TopicPartitionOffset}");
            }
            catch (ProduceException<string, GenericRecord> ex)
            {
                Console.WriteLine($"error producing message: {ex}");
            }



            //var header = HeaderValue.Create();
            //header.AddCorrelationId(Guid.NewGuid().ToString());
            //header.AddResponseTopic("topic_to_response");

            //var notification = GetCustomerNotification();
            //await _producerMessage.ProduceWithBindNotificationAsync(notification.EntityId, notification, header);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
            }).ToArray();


            //int refer = count++;
            //private Customer GetCustomer()
            //{

            //    return new Customer
            //    {
            //        Name = $"Test Name {refer}",
            //        BirthDate = DateTime.Now,
            //        Created = DateTime.Now,
            //        MotherName = $"Test Mother {refer}",
            //        Status = refer % 2 == 0 ? CustomerStatusType.Simple : CustomerStatusType.Complete,
            //        DocumentNumber = $"DocumentNumber {refer}",
            //        Address = new Address
            //        {
            //            City = $"City {refer}",
            //            ZipCode = $"ZipCode {refer}",
            //            Complement = $"Complement {refer}",
            //            Neighborhood = $"Neighborhood {refer}",
            //            Number = $"Number {refer}",
            //            State = $"State {refer}",
            //            Street = $"Street {refer}"
            //        },
            //        Contacts = new List<Contact> { new Contact { Type = ContactType.Email, Value = $"Value {refer}" } }
            //    };
            //}


            //private CustomerNotification GetCustomerNotification()
            //{
            //    var notification = new CustomerNotification();
            //    var customer = GetCustomer();

            //    notification.Name = refer % 2 == 0 ? "CUSTOMER_WAS_CREATED" : "CUSTOMER_WAS_UPDATED";
            //    notification.Timestamp = DateTime.Now;
            //    notification.Data = customer;
            //    notification.EntityId = customer.DocumentNumber;
            //    notification.Context = Context.Account;
            //    notification.Metadata = new Dictionary<string, object>();
            //    notification.Metadata.Add("Test", "test");
            //    notification.CompanyKey = "BANKLY";

            //    return notification;
        }

    }
}