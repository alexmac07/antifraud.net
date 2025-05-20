using Antifraud.Common.Constant;
using Antifraud.Common.Globalization;
using Antifraud.Dto;
using Antifraud.Model;
using Antifraud.Repository.Interface;
using Antifraud.Service.Interface;
using Microsoft.Extensions.Logging;

namespace Antifraud.Service.Implementation
{
    public class AntifraudService : IAntifraudService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionEventRepository _transactionEventRepository;
        private readonly ILogger<AntifraudService> _logger;

        public AntifraudService(ITransactionRepository transactionRepository,
                                ITransactionEventRepository transactionEventRepository,
                                ILogger<AntifraudService> logger)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _transactionEventRepository = transactionEventRepository ?? throw new ArgumentNullException(nameof(transactionEventRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TransactionResult> VerifyTransaction(Guid eventId, Guid transactionId)
        {
            TransactionEventModel? eventInformation = new TransactionEventModel();
            var transactionResult = new TransactionResult();
            transactionResult.TransactionId = transactionId;
            transactionResult.EventId = eventId;

            while (true)
            {
                if (eventId == null || eventId.Equals(Guid.Empty))
                    throw new ArgumentException(Languages.ParameterRequired, nameof(eventId));

                var resultETI = await GetEventTransactionInformation(eventId, transactionId);
                eventInformation = resultETI.transactionEventModel;

                if (!resultETI.transactionResult.Success)
                {
                    transactionResult = resultETI.transactionResult;
                    break; // prevent continue with validation
                }

                var resultValueCheck = await RuleTransactionValueCheck(transactionId);
                transactionResult.Success = resultValueCheck.Success;
                transactionResult.ErrorMessage = resultValueCheck.ErrorMessage;
                break;
            }

            if (eventInformation != null)
            {
                eventInformation.IsProcessed = true;
                eventInformation.Status = transactionResult.Success ? TransactionStatusEnum.Approved : TransactionStatusEnum.Rejected;
                eventInformation.Messages = transactionResult.ErrorMessage;

                // save state
                await _transactionEventRepository.UpdateTransactionEvent(eventInformation);

                _logger.LogInformation($"Event {eventId} status changed to {eventInformation.Status} / Reason: {(eventInformation.Messages ?? "NA")}");
            }

            return transactionResult;
        }

        /// <summary>
        /// Business rules to prevent fraud in transactions.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <returns></returns>
        private async Task<TransactionResult> RuleTransactionValueCheck(Guid transactionId)
        {
            var transactionData = await _transactionRepository.SearchTransaction(transactionId);
            if (transactionData == null)
                return TransactionResult.Error(Languages.TransactionNotFound);

            // Rule 1. Transaction cannot be greater than 2,000
            if (transactionData.Value > 2000)
                return TransactionResult.Error(Languages.RejectionMaxValueExceeded);

            // search for sum of transactions for source account
            var transactions = await _transactionRepository.SearchTransactions(AccountPointerEnum.Source, transactionData.SourceAccountId);
            decimal? totalTransactionsSum = 0m;
            if (transactions != null)
            {
                // Rule 2. Transaction bag sended by origin (approved) cannot exceed sum of 20,000
                totalTransactionsSum = transactions.Where(x => x.Status == TransactionStatusEnum.Approved
                &&
                x.CreatedAt.Date == DateTimeOffset.Now.Date)?.Sum(x => x.Value);
            }

            if (totalTransactionsSum == null || totalTransactionsSum < 20000)
                return TransactionResult.Ok();
            else
                return TransactionResult.Error(Languages.RejectionMaxTransactionAllowed);
        }

        /// <summary>
        /// Retrieves transaction event information
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <returns></returns>
        private async Task<(TransactionResult transactionResult, TransactionEventModel? transactionEventModel)> GetEventTransactionInformation(Guid eventId, Guid transactionId)
        {
            var eventData = await _transactionEventRepository.GetTransactionEvent(eventId);

            if (eventData == null)
                return (TransactionResult.Error(Languages.TransactionEventNotFound), null);

            if (eventData.TransactionId != transactionId)
                return (TransactionResult.Error(eventData.Messages), null);

            // verify it's not already processed
            if (eventData.IsProcessed)
                return (TransactionResult.Error($"Transaction {transactionId} is already processed"), null);

            return (TransactionResult.Ok(), eventData);
        }


    }
}
