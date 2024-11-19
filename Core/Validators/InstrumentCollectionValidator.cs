using FluentValidation;
using Terminal.Core.Domains;

namespace Terminal.Core.Validators
{
  /// <summary>
  /// Validation rules
  /// </summary>
  public class InstrumentCollectionValidator : AbstractValidator<InstrumentModel>
  {
    public InstrumentCollectionValidator()
    {
      Include(new InstrumentValidator());

      RuleFor(o => o.Points).NotEmpty();
      RuleFor(o => o.PointGroups).NotEmpty();
    }
  }
}
