using Antifraud.Dto;

namespace Antifraud.Service.Interface;

public interface IAntifraudService
{
    /// <summary>
    /// Antifraud transaction verification service.
    /// </summary>
    /// <param name="eventId">Unique identifier of the event related to the transaction.</param>
    /// <param name="transactionId">Unique identifier of the transaction.</param>
    /// <returns></returns>
    Task<TransactionResult> VerifyTransaction(Guid eventId, Guid transactionId);
}
