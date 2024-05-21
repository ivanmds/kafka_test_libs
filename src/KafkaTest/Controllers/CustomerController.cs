using Avro.Generic;
using Bankly.Sdk.Kafka;
using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using Microsoft.AspNetCore.Mvc;
using Avro;
using Bankly.Sdk.Kafka.Extensions;
using System.Text.Json;

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
        private static int count = 0;

        public CustomerController(ILogger<CustomerController> logger, IProducerMessage producerMessage)
        {
            _logger = logger;
            _producerMessage = producerMessage;
        }

        [HttpGet(Name = "GetCustomer")]
        public async Task<IEnumerable<WeatherForecast>> GetTest()
        {
            var topicName = "isa_hello";

            var schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig
            {
                Url = "http://ksr.root-platform.insecure.bankly-staging-us-east-1-aws.internal"
            });

            var subjectName = $"{topicName}-value";
            var schema = await schemaRegistry.GetLatestSchemaAsync(subjectName);
            //var temp = await schemaRegistry.GetLatestSchemaAsync("test_ivan");
            var temp2 = (RecordSchema)RecordSchema.Parse(await schemaRegistry.GetSchemaAsync(schema.Subject, schema.Version));

            using (var producer =
                new ProducerBuilder<string, GenericRecord>(new ProducerConfig { BootstrapServers = "localhost:9092" })
                    .SetValueSerializer(new AvroSerializer<GenericRecord>(schemaRegistry))
                    .Build())
            {
                Console.WriteLine($"{producer.Name} producing on {topicName}. Enter user names, q to exit.");
                var customer1 = new CustomerAvro { Name = "Ivan", Address = "127.0.0.1", FatherName = "Ivan old", Phone = new PhoneDto { DDD = "11", Number = "123456789" } };

                //var jsonString = JsonSerializer.Serialize(customer1);
                var record1 = customer1.ParseToGenericRecord(temp2);

                var record = new GenericRecord(temp2);
                record.Add("Name", "Ivan");
                record.Add("Address", "123456");
                record.Add("MotherName", null);
                record.Add("FatherName", "Ivan");

                var temp3 = temp2.Fields.Where(p => p.Name == "Phone").FirstOrDefault();

                var recordPhone = new GenericRecord((RecordSchema)temp3.Schema);
                recordPhone.Add("DDD", "12");
                recordPhone.Add("Number", "123456789");
                record.Add("Phone", recordPhone);

                try
                {
                    var dr = await producer.ProduceAsync(topicName, new Message<string, GenericRecord> { Key = "123", Value = record1 });
                    Console.WriteLine($"produced to: {dr.TopicPartitionOffset}");
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
                })
            .ToArray();
            }

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