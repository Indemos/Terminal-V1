using FluentValidation;
using Terminal.Core.Models;

namespace Terminal.Core.Validators
{
  /// <summary>
  /// Validation rules
  /// </summary>
  public class BarValidator : AbstractValidator<BarModel>
  {
    public BarValidator()
    {
      RuleFor(o => o.Low).NotEmpty();
      RuleFor(o => o.High).NotEmpty();
      RuleFor(o => o.Open).NotEmpty();
      RuleFor(o => o.Close).NotEmpty();
    }
  }
}
