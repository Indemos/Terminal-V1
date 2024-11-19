using FluentValidation;
using Terminal.Core.Models;

namespace Terminal.Core.Validators
{
  /// <summary>
  /// Validation rules
  /// </summary>
  public class PointVolumeValidator : AbstractValidator<PointModel>
  {
    public PointVolumeValidator()
    {
      Include(new PointValidator());

      RuleFor(o => o.Volume).NotEmpty();
      RuleFor(o => o.BidSize).NotEmpty();
      RuleFor(o => o.AskSize).NotEmpty();
    }
  }
}
