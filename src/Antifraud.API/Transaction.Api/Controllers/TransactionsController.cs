using Antifraud.Model;
using Antifraud.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Transaction.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<ActionResult> CreateTransaction([FromBody] TransactionModel transaction)
        {
           
            var result = await _transactionService.CreateTransaction(transaction);

            return CreatedAtAction(nameof(CreateTransaction), new { transactionId = result.TransactionId }, transaction);
        }

        [HttpPost("message")]
        public async Task<ActionResult> CreateTransactionMessage()
        {
            var result = await _transactionService.SendMessage();
            return Created();
        }
    }
}
