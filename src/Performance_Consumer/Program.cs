using Bankly.Sdk.Kafka;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Notifications;
using Performance_Consumer;

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
        
        var consumerBuilder = services.AddKafka(KafkaConnection.Create(kafkaBootstrapServers, kafkaIsPlaintext))
            .GetConsumerBuilder();

        var retry = RetryConfiguration.Create()
            .Add(RetryTime.Create(10))
            .Add(RetryTime.Create(30));

        var groupCustomerRequestId = BuilderName.GetGroupIdName("performance_test", "customer_request");
        consumerBuilder.CreateListener(customerTopic, groupCustomerRequestId, retry)
            .AddConsumer<CustomerConsumer>();

        var groupCustomerEvents = BuilderName.GetGroupIdName("performance_test", "customer_events");
        consumerBuilder.CreateListener(customerTopic, groupCustomerEvents, retry)
            .AddConsumer<CustomerNotificationConsumer>("CUSTOMER_WAS_CREATED")
            .AddConsumer<CustomerNotificationConsumer>("CUSTOMER_WAS_UPDATED");

        var groupCardRequest = BuilderName.GetGroupIdName("performance_test", "card_request");
        consumerBuilder.CreateListener(cardTopic, groupCardRequest, retry)
            .AddConsumer<CardConsumer>();
    })
    .Build();

await host.RunAsync();
