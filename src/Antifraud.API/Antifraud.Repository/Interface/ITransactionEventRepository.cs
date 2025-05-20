using Antifraud.Model;

namespace Antifraud.Repository.Interface
{
    public interface ITransactionEventRepository
    {
        /// <summary>
        /// Creates a new transaction event in the repository.
        /// </summary>
        /// <param name="transactionLog">The transaction event model to create.</param>
        /// <returns>The identifier of the created transaction event.</returns>
        Task<int> CreateTransactionEvent(TransactionEventModel transactionLog);

        /// <summary>
        /// Updates an existing transaction event in the repository.
        /// </summary>
        /// <param name="transactionLog">The transaction event model with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateTransactionEvent(TransactionEventModel transactionLog);

        /// <summary>
        /// Retrieves a transaction event by its unique identifier.
        /// </summary>
        /// <param name="eventId">The unique identifier of the transaction event.</param>
        /// <returns>The transaction event model if found; otherwise, null.</returns>
        Task<TransactionEventModel> GetTransactionEvent(Guid eventId);

        /// <summary>
        /// Deletes a transaction event from the repository by its unique identifier.
        /// </summary>
        /// <param name="eventId">The unique identifier of the transaction event to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteTransactionEvent(Guid eventId);
    }
}
