using Antifraud.Common.Constant;

namespace Antifraud.Model;

/// <summary>
/// Class representation of the transactions table
/// </summary>
public class TransactionModel
{
    /// <summary>
    /// The unique identifier of the transaction
    /// </summary>
    public Guid TransactionId { get; set; }
    /// <summary>
    /// The unique identifier of the source account
    /// </summary>
    public Guid SourceAccountId { get; set; }
    /// <summary>
    /// The unique identifier of the target account
    /// </summary>
    public Guid TargetAccountId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int TransferType { get; set; }
    /// <summary>
    /// Amount to be transfered
    /// </summary>
    public decimal Value { get; set; }
    public TransactionStatusEnum Status { get; set; } = TransactionStatusEnum.Pending;
    /// <summary>
    /// Creation date
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
