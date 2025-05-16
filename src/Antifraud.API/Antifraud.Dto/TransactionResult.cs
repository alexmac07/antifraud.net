namespace Antifraud.Dto;

public class TransactionResult
{
    public Guid TransactionId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public static TransactionResult Ok() => new() { Success = true };
    public static TransactionResult Error(string message) => new()
    {
        Success = false,
        ErrorMessage = message,
    };

}
