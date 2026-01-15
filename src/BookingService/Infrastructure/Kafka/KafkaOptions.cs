namespace BookingService.Infrastructure.Kafka;

public class KafkaOptions
{
    public required string BootstrapServers { get; set; }

    public string PaymentRequestTopic { get; set; } = "payments.input";

    public string PaymentResultTopic { get; set; } = "payment.output";

    public string PaymentResultConsumerGroupId { get; set; } = "booking-service.payments-output";
}