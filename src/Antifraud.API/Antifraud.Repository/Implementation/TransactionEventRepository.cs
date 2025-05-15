using Antifraud.Common.Settings;
using Antifraud.Model;
using Antifraud.Repository.Interface;
using Antifraud.Core.Repository.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Antifraud.Repository.Implementation
{
    public class TransactionEventRepository : DapperRepository<TransactionEventModel>, ITransactionEventRepository
    {
        public TransactionEventRepository(IOptions<AppSettings> config,
                              ILogger<DapperRepository<TransactionEventModel>> logger) : base(config, logger)
        {
        }

        public async Task<int> CreateTransactionEvent(TransactionEventModel transactionLog)
        {
            var parameters = new
            {
                transactionLog.EventId,
                transactionLog.TransactionId,
                transactionLog.Status,
                transactionLog.IsProcessed,
                CreatedAt = DateTime.Now
            };

            var command = @"INSERT INTO transaction_events (event_id, transaction_id, status, is_processed, created_at)
                            VALUES
                            (
                            @EventId,
                            @TransactionId,
                            @Status,
                            @IsProcessed,
                            @CreatedAt
                            )";

            return await ExecuteAsync(command, parameters);
        }

        public async Task<IEnumerable<TransactionEventModel>> GetTransactionEvent(Guid eventId)
        {
            var parameters = new { eventId };

            var command = @"SELECT event_id, transaction_id, status, is_processed, created_at FROM transaction_events
                            WHERE event_id = @eventId";

            return await QueryAsync(command, parameters);
        }

        public async Task<bool> UpdateTransactionEvent(TransactionEventModel transactionLog)
        {
            var parameters = new {transactionLog.EventId, transactionLog.TransactionId, transactionLog.Status, transactionLog.IsProcessed };

            var command = @"UPDATE transaction_events SET status = @Status, is_processed = @IsProcessed
                            WHERE event_id = @EventId and transaction_id = @TransactionId";

            return await ExecuteAsync(command, parameters) > 0;
        }
    }
}
