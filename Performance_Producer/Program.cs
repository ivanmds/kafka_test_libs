using Bankly.Sdk.Kafka;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Notifications;
using Performance_Producer;
using Performance_Producer.Models;
using Performance_Producer.Notification;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {

        var customerTopic = BuilderName.GetTopicNameRPC(true, Context.Account, "customers", "create_customer");
        var cardTopic = BuilderName.GetTopicNameRPC(true, Context.Card, "cards", "create_card");
        var customerEventsTopic = BuilderName.GetTopicName(true, Context.Account, "customers");
        var cardEventsTopic = BuilderName.GetTopicName(true, Context.Card, "cards");

        services.AddKafka(KafkaConnection.Create("localhost:9092"))
            .Bind<Customer>(customerTopic)
            .Bind<Card>(cardTopic)
            .Bind<CustomerNotification>(customerEventsTopic)
            .Bind<CardNotification>(cardEventsTopic);

        
        services.AddHostedService<Worker_P1>();
        services.AddHostedService<Worker_P2>();
        services.AddHostedService<Worker_P3>();
        services.AddHostedService<Worker_P4>();
    })
    .Build();

await host.RunAsync();
