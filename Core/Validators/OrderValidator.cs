using FluentValidation;
using System.Linq;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Terminal.Core.Validators
{
  /// <summary>
  /// Validation rules
  /// </summary>
  public class OrderValidator : AbstractValidator<OrderModel>
  {
    public OrderValidator()
    {
      RuleFor(o => o.Type).NotEmpty();
      RuleFor(o => o.Descriptor).NotEmpty();

      When(o => o.Instruction is InstructionEnum.Group || o.Orders.Where(o => o.Instruction is InstructionEnum.Side).Any(), () =>
      {
        RuleFor(o => o.Transaction).Empty();
        RuleFor(o => o.Side).Empty();
      });

      When(o => o.Transaction is not null, () =>
      {
        RuleFor(o => o.Side).NotEmpty();
        RuleFor(o => o.Transaction.Status).Empty();
        RuleFor(o => o.Transaction.Volume).Empty();
        RuleFor(o => o.Transaction).SetValidator(new TransactionValidator());
      });
    }
  }
}
