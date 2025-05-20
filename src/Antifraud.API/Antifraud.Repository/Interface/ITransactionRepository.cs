using Antifraud.Common.Constant;
using Antifraud.Model;

namespace Antifraud.Repository.Interface
{
    public interface ITransactionRepository
    {
        /// <summary>
        /// Creates a new registry on the database.
        /// </summary>
        /// <param name="transaction">The transaction model.</param>
        /// <returns></returns>
        /// 
        Task<int> RegisterTransaction(TransactionModel transaction);
        /// <summary>
        /// Fetch a transaction.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction</param>
        /// <param name="createdAt">Creation date</param>
        /// <returns></returns>
        Task<TransactionModel> SearchTransaction(Guid transactionId, DateTime? createdAt = null);

        /// <summary>
        /// Search for all transactions given a <paramref name="accountPointer"/> and a accountId
        /// </summary>
        /// <param name="accountPointer">Tells searcher to look inside source or target account</param>
        /// <param name="accountId">The unique identifier of the account</param>
        /// <returns></returns>
        Task<IEnumerable<TransactionModel>> SearchTransactions(AccountPointerEnum accountPointer, Guid accountId);

        /// <summary>
        /// Updates transaction
        /// </summary>
        /// <param name="transaction">Transaction model</param>
        /// <returns></returns>
        Task<bool> UpdateTransaction(TransactionModel transaction);

        
        /// Deletes a transaction from the database.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction to delete.</param>
        /// <returns>True if the transaction was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteTransaction(Guid transactionId);
    }
}
