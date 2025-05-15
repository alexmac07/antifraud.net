using Antifraud.Common.Constant;

namespace Antifraud.Model
{
    public class TransactionEventModel
    {
        public Guid EventId { get; set; }
        public Guid TransactionId { get; set; }
        public TransactionStatusEnum Status { get; set; } = TransactionStatusEnum.Pending;
        public bool IsProcessed { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
