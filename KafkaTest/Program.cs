using Bankly.Sdk.Kafka.Configuration;
using KafkaTest.Consumers;
using KafkaTest.Notifications;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var consumerBuilder = builder.Services.AddKafka(KafkaConnection.Create("b-2.acsstg-msk.z25ji9.c7.kafka.us-east-1.amazonaws.com:9092,b-1.acsstg-msk.z25ji9.c7.kafka.us-east-1.amazonaws.com:9092,b-3.acsstg-msk.z25ji9.c7.kafka.us-east-1.amazonaws.com:9092"));

//var retry = RetryConfiguration.Create()
//    .Add(RetryTime.Create(2))
//    .Add(RetryTime.Create(15))
//    .Add(RetryTime.Create(60))
//    .Add(RetryTime.Create(120));

//consumerBuilder.CreateListener("bankly.event.customers", "event_customer", retry)
//    .AddSkippedMessage<SkippedMessage>()
//    .AddConsumer<CustomerCreatedConsumer>("CUSTOMER_WAS_CREATED")
//    .AddConsumer<CustomerUpdatedConsumer>("CUSTOMER_WAS_UPDATED");

//consumerBuilder.CreateListener("test.temp", "anothers_consumer")
//    .AddConsumer<AnotherConsumer>();


consumerBuilder.CreateListener("bankly.event.card", "test_card_process_events_01")
    .AddSkippedMessage<SkippedMessage>()
    .AddConsumer<CardWasIssuedConsumer>("CARD_WAS_ISSUED")
    .AddConsumer<CardProgramWasCreated>("PROGRAM_WAS_CREATED");


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