using FluentValidation;
using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Terminal.Core.Validators
{
  /// <summary>
  /// Validation rules for limit orders
  /// </summary>
  public class OrderPriceValidator : AbstractValidator<OrderModel>
  {
    public OrderPriceValidator()
    {
      Include(new OrderValidator());

      When(o => o.Side is OrderSideEnum.Long && o.Type is OrderTypeEnum.Stop, () => RuleFor(o => o.Price).GreaterThanOrEqualTo(o => o.Transaction.Instrument.Points.Last().Ask));
      When(o => o.Side is OrderSideEnum.Short && o.Type is OrderTypeEnum.Stop, () => RuleFor(o => o.Price).LessThanOrEqualTo(o => o.Transaction.Instrument.Points.Last().Bid));
      When(o => o.Side is OrderSideEnum.Long && o.Type is OrderTypeEnum.Limit, () => RuleFor(o => o.Price).LessThanOrEqualTo(o => o.Transaction.Instrument.Points.Last().Ask));
      When(o => o.Side is OrderSideEnum.Short && o.Type is OrderTypeEnum.Limit, () => RuleFor(o => o.Price).GreaterThanOrEqualTo(o => o.Transaction.Instrument.Points.Last().Bid));

      When(o => o.Side is OrderSideEnum.Long && o.Type is OrderTypeEnum.StopLimit, () =>
      {
        RuleFor(o => o.ActivationPrice).NotEmpty();
        RuleFor(o => o.ActivationPrice).GreaterThanOrEqualTo(o => o.Transaction.Instrument.Points.Last().Ask);
        RuleFor(o => o.Price).GreaterThanOrEqualTo(o => o.ActivationPrice);
      });

      When(o => o.Side is OrderSideEnum.Short && o.Type is OrderTypeEnum.StopLimit, () =>
      {
        RuleFor(o => o.ActivationPrice).NotEmpty();
        RuleFor(o => o.ActivationPrice).LessThanOrEqualTo(o => o.Transaction.Instrument.Points.Last().Bid);
        RuleFor(o => o.Price).LessThanOrEqualTo(o => o.ActivationPrice);
      });
    }
  }
}
