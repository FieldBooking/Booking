using BookingService.Infrastructure.Kafka;
using BookingService.Infrastructure.Kafka.Consumer;
using BookingService.Infrastructure.Kafka.Producer;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Extensions;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<KafkaOptions>(config.GetSection("Kafka"));

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            KafkaOptions o = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = o.BootstrapServers,
                EnableIdempotence = true,
                Acks = Acks.All,
            };
            return new ProducerBuilder<string, string>(producerConfig).Build();
        });

        services.AddSingleton<IPaymentRequestedProducerService, PaymentRequestedProducerService>();
        services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
        services.AddScoped<IKafkaMessageHandler, PaymentResultHandler>();
        services.AddHostedService<KafkaConsumerHostedService>();

        return services;
    }
}