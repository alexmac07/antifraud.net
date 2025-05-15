using Antifraud.Common.Constant;
using Antifraud.Common.Settings;
using Antifraud.Model;
using Antifraud.Repository.Interface;
using Antifraud.Core.Repository.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Antifraud.Repository.Implementation
{
    public class TransactionRepository : DapperRepository<TransactionModel>, ITransactionRepository
    {
        public TransactionRepository(IOptions<AppSettings> config,
                                     ILogger<DapperRepository<TransactionModel>> logger) : base(config, logger)
        {
        }

        public async Task<int> RegisterTransaction(TransactionModel transaction)
        {
            var parameters = new
            {
                transaction.TransactionId,
                transaction.SourceAccountId,
                transaction.TargetAccountId,
                transaction.TransferType,
                transaction.Value,
                transaction.Status,
                CreatedAt = DateTime.Now
            };

            var command = @"INSERT INTO transactions(transaction_id, source_account_id, target_account_id, transfer_type, value, status, created_at)
                            values
                            (
                            @TransactionId,
                            @SourceAccountId,
                            @TargetAccountId,
                            @TransferType,
                            @Value,
                            @Status,
                            @CreatedAt
                            )";
            return await ExecuteAsync(command, parameters);
        }

        public async Task<TransactionModel> SearchTransaction(Guid transactionId, DateTime? createdAt = null)
        {
            var parameters = new { transactionId, createdAt };

            var command = @"SELECT transaction_id, source_account_id, target_account_id, transfer_type, value, status, created_at from transactions
                            WHERE transaction_id = @transactionId and created_at = @createdAt";

            return await QueryFirstOrDefaultAsync(command, parameters);
        }

        public async Task<IEnumerable<TransactionModel>> SearchTransactions(AccountPointerEnum accountPointer, Guid accountId)
        {
            var searchCommand = DefineAccountPointer(accountPointer);

            var parameters = new { searchCommand, accountId };

            var command = @"SELECT transaction_id, source_account_id, target_account_id, transfer_type, value, status, created_at from transactions
                            WHERE @searchCommand = @accountId";
            return await QueryAsync(command, parameters);
        }

        public async Task<bool> UpdateTransaction(TransactionModel transaction)
        {
            var parameters = new { transaction.TransactionId, transaction.Status };

            var command = @"UPDATE transactions SET status = @Status WHERE transaction_id = @TransactionId";

            return await ExecuteAsync(command, parameters) > 0;
        }

        #region .: Private Functions :.
        private string DefineAccountPointer(AccountPointerEnum accountPointer)
        {
            switch (accountPointer)
            {
                case AccountPointerEnum.Source:
                    return "source_account_id";
                case AccountPointerEnum.Target:
                    return "target_account_id";
                default:
                    throw new NotImplementedException($"{nameof(accountPointer)} not handled yet!");

            }
        }
        #endregion
    }
}
