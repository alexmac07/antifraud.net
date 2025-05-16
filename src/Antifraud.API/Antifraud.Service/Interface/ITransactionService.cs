using Antifraud.Dto;
using Antifraud.Model;

namespace Antifraud.Service.Interface;

public interface ITransactionService
{
    /// <summary>
    /// Creates a new transaction request.
    /// </summary>
    /// <param name="transaction">Transaction to be registred</param>
    /// <returns></returns>
    Task<TransactionEventModel> CreateTransaction(TransactionModel transaction);

    /// <summary>
    /// Updates a transaction given a antifraud transaction result.
    /// </summary>
    /// <param name="transactionEvent">Result related to transaction.</param>
    /// <returns></returns>
    Task<TransactionModel> UpdateTransaction(TransactionResult transactionResult);
}
