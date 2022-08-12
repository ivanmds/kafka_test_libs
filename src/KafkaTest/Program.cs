using Bankly.Sdk.Kafka;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Notifications;
using KafkaTest.Consumers;
using KafkaTest.Notifications;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var topicNameCustomerEvent = BuilderName.GetTopicName(true, Context.Account, "customers");
var topicNameCardEvent = BuilderName.GetTopicName(true, Context.Card, "Cards");

var connection = "localhost:9092";
var consumerBuilder = builder.Services.AddKafka(KafkaConnection.Create(connection))
        .AddSkippedMessage<SkippedMessage>()
        .AddConsumerErrorFatal<ConsumerErrorFatal>()
        .Bind<CustomerNotification>(topicNameCustomerEvent)
        .Bind<CardNotification>(topicNameCardEvent)
        .GetConsumerBuilder();


var retry = RetryConfiguration.Create()
    .Add(RetryTime.Create(2))
    .Add(RetryTime.Create(15));

var groupId = BuilderName.GetGroupIdName("test_kafka", "customer_events");
consumerBuilder.CreateListener("bankly.event.account.customers", groupId, retry)
    .AddConsumer<CustomerCreatedConsumer>("CUSTOMER_WAS_CREATED")
    .AddConsumer<CustomerUpdatedConsumer>("CUSTOMER_WAS_UPDATED");

consumerBuilder.CreateListener("test.temp", "anothers_consumer")
    .AddConsumer<AnotherConsumer>();


//consumerBuilder.CreateListener("bankly.event.card", "test_card_process_events_01")
//    .AddSkippedMessage<SkippedMessage>()
//    .AddConsumer<CardWasIssuedConsumer>("CARD_WAS_ISSUED")
//    .AddConsumer<CardProgramWasCreated>("PROGRAM_WAS_CREATED");



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();