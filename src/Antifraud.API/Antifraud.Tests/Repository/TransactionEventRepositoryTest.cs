using Antifraud.Common.Constant;
using Antifraud.Common.Settings;
using Antifraud.Core.Repository.Implementation;
using Antifraud.Model;
using Antifraud.Repository.Implementation;
using Antifraud.Repository.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Antifraud.Tests.Repository
{
    public class TransactionEventRepositoryTest
    {
        private readonly ITransactionEventRepository _transactionEventRepository;
        private readonly Mock<IOptions<AppSettings>> _mockAppSettings;
        private readonly Mock<IOptions<KafkaSettings>> _mockKafkaSettings;
        private readonly Mock<ILogger<DapperRepository<TransactionEventModel>>> _mockLogger;
        public TransactionEventRepositoryTest()
        {
            _mockAppSettings = new Mock<IOptions<AppSettings>>();
            _mockAppSettings.Setup(x => x.Value).Returns(new AppSettings
            {
                DBConnection = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=antifraud",
            });
            _mockLogger = new Mock<ILogger<DapperRepository<TransactionEventModel>>>();
            _transactionEventRepository = new TransactionEventRepository(_mockAppSettings.Object,
                                                                         new Mock<ILogger<DapperRepository<TransactionEventModel>>>().Object);
        }

        [Fact]
        public async Task CreateTransactionEvent_ShouldInsertTransactionEvent()
        {
           var result = await CreateTransactionEventModel();
            // Assert
            Assert.NotNull(result);
            // Clean up
            var deleteResult = await _transactionEventRepository.DeleteTransactionEvent(result.EventId);
        }

        [Fact]
        public async Task RegisterTransactionEvent_ThenUpdateIt_RetrieveUpdatedInformation_ShouldUpdateTransaction()
        {
            var transactionData = await CreateTransactionEventModel();
            Assert.NotNull(transactionData); // Successfully registered transaction
            transactionData.Status = TransactionStatusEnum.Approved;
            transactionData.IsProcessed = true;
            // Act
            var result = await _transactionEventRepository.UpdateTransactionEvent(transactionData);
            Assert.True(result); // Successfully updated transaction
            var updatedTransaction = await _transactionEventRepository.GetTransactionEvent(transactionData.EventId);
            Assert.NotNull(updatedTransaction); // Successfully retrieved updated transaction
            Assert.Equal(transactionData.Status, updatedTransaction.Status); // Check if the status is updated
        }

        #region .: SHARED STEPS :.
        private async Task<TransactionEventModel?> CreateTransactionEventModel()
        {
            // Arrange
            var transactionEvent = new TransactionEventModel
            {
                EventId = Guid.NewGuid(),
                TransactionId = Guid.NewGuid(),
                Status = TransactionStatusEnum.Pending,
                CreatedAt = DateTimeOffset.Now.ToUniversalTime(),
                IsProcessed = false,
            };
            // Act
            var result = await _transactionEventRepository.CreateTransactionEvent(transactionEvent);
            return result > 0 ? transactionEvent : null;
        }
        #endregion
    }
}
