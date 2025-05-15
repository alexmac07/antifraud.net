namespace Antifraud.Dto
{
    public class TransactionEventKafkaDto
    {
        public Guid EventId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
