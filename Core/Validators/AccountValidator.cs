using FluentValidation;
using Terminal.Core.Domains;

namespace Terminal.Core.Validators
{
    /// <summary>
    /// Validation rules
    /// </summary>
    public class AccountValidator : AbstractValidator<IAccount>
  {
    public AccountValidator()
    {
      RuleFor(o => o.Descriptor).NotEmpty();
      RuleFor(o => o.Balance).NotEmpty();
      RuleFor(o => o.InitialBalance).NotEmpty();
    }
  }
}
