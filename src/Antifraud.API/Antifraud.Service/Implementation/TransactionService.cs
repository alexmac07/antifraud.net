using Antifraud.Common.Constant;
using Antifraud.Common.Globalization;
using Antifraud.Common.Settings;
using Antifraud.Dto;
using Antifraud.Model;
using Antifraud.Repository.Interface;
using Antifraud.Service.Interface;
using Antifraud.Service.KProducers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Transactions;

namespace Antifraud.Service.Implementation;

public class TransactionService : ITransactionService
{
    private readonly IValidator<TransactionModel> _transactionValidator;
    private readonly IValidator<TransactionResult> _transactionResultValidator;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionEventRepository _transactionEvent;
    private readonly ILogger<TransactionService> _logger;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly KafkaSettings _kafkaSettings;

    public TransactionService(IValidator<TransactionModel> transactionValidator,
                              IValidator<TransactionResult> transactionResultValidator,
                              ITransactionRepository transactionRepository,
                              ITransactionEventRepository transactionEvent,
                              ILogger<TransactionService> logger,
                              IKafkaProducer kafkaProducer,
                              IOptions<KafkaSettings> kafkaSettings
                              )
    {
        _transactionValidator = transactionValidator ?? throw new ArgumentNullException(nameof(transactionValidator));
        _transactionResultValidator = transactionResultValidator ?? throw new ArgumentNullException(nameof(transactionResultValidator));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _transactionEvent = transactionEvent ?? throw new ArgumentNullException(nameof(transactionEvent));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
        _kafkaSettings = kafkaSettings.Value ?? throw new ArgumentNullException(nameof(kafkaSettings));
    }

    public async Task<TransactionEventModel> CreateTransaction(TransactionModel transaction)
    {
        // Verifies required information to create the transaction
        ValidationResult vResult = await _transactionValidator.ValidateAsync(transaction);
        if (vResult.IsValid)
        {
            using (TransactionScope tsn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {

                await RegisterTransaction(transaction);
                var eventInformation = await RegisterEventTransaction(transaction.TransactionId);

                await _kafkaProducer.PublishAsync(_kafkaSettings.TransactionsTopic, new TransactionEventKafkaDto
                {
                    EventId = eventInformation.EventId,
                    TransactionId = eventInformation.TransactionId,
                });

                tsn.Complete();
                return eventInformation;
            }

        }
        throw new ValidationException(vResult.Errors);
    }

    public async Task<TransactionModel> UpdateTransaction(TransactionResult transactionResult)
    {
        ValidationResult vResult = _transactionResultValidator.Validate(transactionResult);
        if (vResult.IsValid)
        {
            // retrieve event trasction information
            var eventInformation = await _transactionEvent.GetTransactionEvent(transactionResult.EventId);
            var transactionInformation = await _transactionRepository.SearchTransaction(transactionResult.TransactionId);

            if (eventInformation != null
                && eventInformation.IsProcessed == true)
            {
                if (transactionInformation != null)
                {
                    transactionInformation.Status = eventInformation.Status;
                    await _transactionRepository.UpdateTransaction(transactionInformation);
                    return transactionInformation;
                }
                else
                {
                    throw new Exception(Languages.TransactionNotFound);
                }
            }
            else
            {
                throw new Exception(Languages.TransactionEventNotFound);
            }
        }
        throw new ValidationException(vResult.Errors);
    }


    #region .: Private Functions :.
    private async Task<TransactionEventModel> RegisterEventTransaction(Guid transactionId)
    {
        try
        {
            TransactionEventModel transactionEvent = new TransactionEventModel
            {
                EventId = Guid.NewGuid(),
                TransactionId = transactionId,
            };

            var result = await _transactionEvent.CreateTransactionEvent(transactionEvent);
            return result > 0 ? transactionEvent : throw new Exception($"Unable to register event using {JsonConvert.SerializeObject(transactionEvent)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(RegisterTransaction));
            throw;
        }
    }

    private async Task RegisterTransaction(TransactionModel transaction)
    {
        try
        {
            transaction.TransactionId = Guid.NewGuid();
            var result = await _transactionRepository.RegisterTransaction(transaction);
            if (result <= 0)
                throw new Exception($"Unable to register transaction using {JsonConvert.SerializeObject(transaction)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(RegisterTransaction));
            throw;
        }
    }

    #endregion
}
