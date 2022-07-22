using Kafka.Configuration;
using KafkaTest.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var consumerBuilder = builder.Services.AddKafka(KafkaConnection.Create("localhost:9092"));

consumerBuilder.CreateListener("bankly.event.customers", "event_customer")
    .AddConsumer<SimpleConsumer>("CUSTOMER_WAS_CREATED")
    .AddConsumer<SecondConsumer>("CUSTOMER_WAS_UPDATED");

consumerBuilder.CreateListener("test.temp", "test_consumer_2")
    .AddConsumer<SecondConsumer>();

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