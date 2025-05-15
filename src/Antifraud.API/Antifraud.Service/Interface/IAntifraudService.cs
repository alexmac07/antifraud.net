namespace Antifraud.Service.Interface;

public interface IAntifraudService
{
    /// <summary>
    /// Antifraud transaction verification service.
    /// </summary>
    /// <param name="eventId">Unique identifier of the event related to the transaction.</param>
    /// <returns></returns>
    Task<bool> VerifyTransaction(Guid eventId);
}
