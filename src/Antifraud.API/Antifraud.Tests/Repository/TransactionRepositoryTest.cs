using Antifraud.Common.Constant;
using Antifraud.Common.Settings;
using Antifraud.Core.Repository.Implementation;
using Antifraud.Model;
using Antifraud.Repository.Implementation;
using Antifraud.Repository.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Antifraud.Tests;


public class TransactionRepositoryTest
{
    private readonly ITransactionRepository transactionRepository;
    private readonly Mock<IOptions<AppSettings>> mockAppSettings;
    private readonly Mock<IOptions<KafkaSettings>> mockKafkaSettings;
    private readonly Mock<ILogger<DapperRepository<TransactionModel>>> mockLogger;
    public TransactionRepositoryTest()
    {
        mockAppSettings = new Mock<IOptions<AppSettings>>();
        mockAppSettings.Setup(x => x.Value).Returns(new AppSettings
        {
            DBConnection = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=antifraud",

        });



        mockKafkaSettings = new Mock<IOptions<KafkaSettings>>();
        mockKafkaSettings.Setup(x => x.Value).Returns(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            GroupId = "transaction-consumer-group-debug",
            TransactionsTopic = "transactions-debug",
            ConfirmationTopic = "transactions-confirmed-debug",
            AutoOffsetReset = "Earliest"
        });

        mockLogger = new Mock<ILogger<DapperRepository<TransactionModel>>>();

        transactionRepository = new TransactionRepository(mockAppSettings.Object, mockLogger.Object);
    }

    #region Repository
    [Fact]
    public async Task RegisterTransaction_ShouldInsertTransaction()
    {
        // Arrange
        var result = await RegisterNewTransaction();
        // Assert
        Assert.NotNull(result); // Successfully registered transaction
        // Clean up
        var deleteResult = await transactionRepository.DeleteTransaction(result.TransactionId);
        Assert.True(deleteResult, "Transaction was deleted successfully.");
    }

    [Fact]
    public async Task RegisterTransaction_ThenUpdateIt_RetrieveUpdatedInformation_ShouldUpdateTransaction()
    {
        // Arrange
        var transaction = await RegisterNewTransaction();
        Assert.NotNull(transaction); // Successfully registered transaction
        // Act
        transaction.Status = TransactionStatusEnum.Approved;
        var updateResult = await transactionRepository.UpdateTransaction(transaction);
        Assert.True(updateResult, "Transaction was updated successfully.");
        var newData = await transactionRepository.SearchTransaction(transaction.TransactionId);
        Assert.NotNull(newData); // Successfully retrieved transaction
        Assert.Equal(transaction.Status, newData.Status); // Check if the status was updated
        // Clean up
        var deleteResult = await transactionRepository.DeleteTransaction(transaction.TransactionId);
        Assert.True(deleteResult, "Transaction was deleted successfully.");
    }

    [Fact]
    public async Task GetAllTransactions_ShouldListCreatedTransactions()
    {
        // Arrange
        var transaction = await RegisterNewTransaction();
        Assert.NotNull(transaction); // Successfully registered transaction
        // Act
        var transactions = await transactionRepository.SearchTransactions(AccountPointerEnum.Source, transaction.SourceAccountId);
        // Assert
        Assert.NotNull(transactions); // Successfully retrieved transactions
        Assert.NotEmpty(transactions); // Check if the list is not empty
        // Clean up
        var deleteResult = await transactionRepository.DeleteTransaction(transaction.TransactionId);
    }
    #endregion



    #region .: Shared steps :.
    private async Task<TransactionModel?> RegisterNewTransaction()
    {
        // Arrange
        var transaction = new TransactionModel
        {
            TransactionId = Guid.NewGuid(),
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferType = 1,
            Value = 100.00m,
            Status = TransactionStatusEnum.Pending,
        };
        // Act
        return await transactionRepository.RegisterTransaction(transaction) > 0 ? transaction : null;
    }
    #endregion
}
