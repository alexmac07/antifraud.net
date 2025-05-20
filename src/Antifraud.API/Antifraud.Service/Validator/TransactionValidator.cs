using Antifraud.Common.Globalization;
using Antifraud.Model;
using FluentValidation;

namespace Antifraud.Service.Validator
{
    internal class TransactionValidator : AbstractValidator<TransactionModel>
    {

        public TransactionValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage(string.Format(Languages.ParameterRequired, nameof(TransactionModel)));

            RuleFor(x => x.SourceAccountId)
                .NotNull().WithMessage(Languages.InvalidSourceAccount);

            RuleFor(x => x.TargetAccountId)
                .NotNull().WithMessage(Languages.InvalidTargetAccount);

            RuleFor(x => x.Value)
                .GreaterThan(0).WithMessage(Languages.InvalidTransactionAmount);
        }
    }
}
