using Antifraud.Model;

namespace Antifraud.Repository.Interface
{
    public interface ITransactionEventRepository
    {
        Task<int> CreateTransactionEvent(TransactionEventModel transactionLog);

        Task<bool> UpdateTransactionEvent(TransactionEventModel transactionLog);

        Task<IEnumerable<TransactionEventModel>> GetTransactionEvent(Guid eventId);

    }
}
