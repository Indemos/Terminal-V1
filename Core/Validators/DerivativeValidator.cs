using FluentValidation;
using Terminal.Core.Models;

namespace Terminal.Core.Validators
{
  /// <summary>
  /// Validation rules
  /// </summary>
  public class DerivativeValidator : AbstractValidator<DerivativeModel>
  {
    public DerivativeValidator()
    {
      RuleFor(o => o.Side).NotEmpty();
      RuleFor(o => o.Strike).NotEmpty();
      RuleFor(o => o.TradeDate).NotEmpty();
      RuleFor(o => o.ExpirationDate).NotEmpty();
    }
  }
}
