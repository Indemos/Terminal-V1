using FluentValidation;
using Terminal.Core.Domains;

namespace Terminal.Core.Validators
{
  /// <summary>
  /// Validation rules
  /// </summary>
  public class InstrumentValidator : AbstractValidator<InstrumentModel>
  {
    public InstrumentValidator()
    {
      RuleFor(o => o.Name).NotEmpty();
      RuleFor(o => o.Commission).NotEmpty();
      RuleFor(o => o.ContractSize).NotEmpty();
      RuleFor(o => o.StepSize).NotEmpty();
      RuleFor(o => o.StepValue).NotEmpty();
      RuleFor(o => o.Leverage).NotEmpty();
      RuleFor(o => o.Point).NotEmpty().SetValidator(new PointValidator());
    }
  }
}
