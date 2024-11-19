using FluentValidation;
using Terminal.Core.Models;

namespace Terminal.Core.Validators
{
  /// <summary>
  /// Validation rules
  /// </summary>
  public class PointValidator : AbstractValidator<PointModel>
  {
    public PointValidator()
    {
      RuleFor(o => o.Bid).NotEmpty();
      RuleFor(o => o.Ask).NotEmpty();
      RuleFor(o => o.Last).NotEmpty();
      RuleFor(o => o.Time).NotEmpty();
    }
  }
}
