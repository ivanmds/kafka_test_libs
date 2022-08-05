using Bankly.Sdk.Kafka;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Notifications;
using Performance_Producer;
using Performance_Producer.Models;
using Performance_Producer.Notification;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {

        var builder = new ConfigurationBuilder();
        builder.AddJsonFile("appsettings.json");
        builder.AddEnvironmentVariables();

        var configuration = builder.Build();

        var customerTopic = BuilderName.GetTopicNameRPC(true, Context.Account, "customers", "create_customer");
        var cardTopic = BuilderName.GetTopicNameRPC(true, Context.Card, "cards", "create_card");
        var customerEventsTopic = BuilderName.GetTopicName(true, Context.Account, "customers");
        var cardEventsTopic = BuilderName.GetTopicName(true, Context.Card, "cards");

        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka_BootstrapServers");
        var kafkaIsPlaintext = configuration.GetValue<bool>("Kafka_IsPlaintext");

        services.AddKafka(KafkaConnection.Create(kafkaBootstrapServers, kafkaIsPlaintext))
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
