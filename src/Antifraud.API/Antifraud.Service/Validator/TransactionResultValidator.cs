using Antifraud.Common.Globalization;
using Antifraud.Dto;
using FluentValidation;

namespace Antifraud.Service.Validator
{
    public class TransactionResultValidator : AbstractValidator<TransactionResult>
    {
        public TransactionResultValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage(string.Format(Languages.ParameterRequired, nameof(TransactionResult)));

            RuleFor(x => x.TransactionId)
                .NotNull().WithMessage(string.Format(Languages.ParameterRequired, nameof(TransactionResult.TransactionId)));
        }
    }
}
