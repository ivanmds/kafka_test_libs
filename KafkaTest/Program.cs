using Bankly.Sdk.Kafka.Configuration;
using KafkaTest.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var consumerBuilder = builder.Services.AddKafka(KafkaConnection.Create("localhost:9092"));

consumerBuilder.CreateListener("bankly.event.customers", "event_customer")
    .AddConsumer<CustomerCreatedConsumer>("CUSTOMER_WAS_CREATED")
    .AddConsumer<CustomerUpdatedConsumer>("CUSTOMER_WAS_UPDATED");

consumerBuilder.CreateListener("test.temp", "anothers_consumer")
    .AddConsumer<AnotherConsumer>();

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