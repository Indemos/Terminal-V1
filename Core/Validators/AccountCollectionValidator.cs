using FluentValidation;
using Terminal.Core.Domains;

namespace Terminal.Core.Validators
{
    /// <summary>
    /// Validation rules
    /// </summary>
    public class AccountCollectionValidator : AbstractValidator<IAccount>
  {
    public AccountCollectionValidator()
    {
      Include(new AccountValidator());

      RuleFor(o => o.Instruments).NotEmpty();
    }
  }
}
