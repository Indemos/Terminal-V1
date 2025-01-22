using Simulation;
using System;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Terminal.Tests
{
  public class Positions : Adapter, IDisposable
  {
    public Positions()
    {
      Account = new Account
      {
        Descriptor = "Demo",
        Balance = 50000
      };
    }

    [Theory]
    [InlineData(OrderSideEnum.Long, OrderTypeEnum.Stop, 15.0, null, 25.0)]
    [InlineData(OrderSideEnum.Short, OrderTypeEnum.Stop, 15.0, null, 5.0)]
    [InlineData(OrderSideEnum.Long, OrderTypeEnum.Limit, 15.0, null, 5.0)]
    [InlineData(OrderSideEnum.Short, OrderTypeEnum.Limit, 15.0, null, 25.0)]
    [InlineData(OrderSideEnum.Long, OrderTypeEnum.StopLimit, 15.0, 20.0, 25.0)]
    [InlineData(OrderSideEnum.Short, OrderTypeEnum.StopLimit, 15.0, 10.0, 5.0)]
    public void CreatePendingOrder(
      OrderSideEnum orderSide,
      OrderTypeEnum orderType,
      double? price,
      double? activationPrice,
      double? orderPrice)
    {
      var point = new PointModel()
      {
        Bid = price,
        Ask = price,
        Last = price
      };

      var order = new OrderModel
      {
        Volume = 1,
        Side = orderSide,
        Type = orderType,
        Price = orderPrice,
        ActivationPrice = activationPrice,
        Transaction = new()
        {
          Instrument = new InstrumentModel()
          {
            Name = "X",
            Point = point,
            Points = [point]
          }
        }
      };

      base.CreateOrders(order);

      Assert.Empty(Account.Deals);
      Assert.Single(Account.Orders);
      Assert.Empty(Account.Positions);

      var outOrder = Account.Orders[order.Id];

      Assert.Equal(outOrder.Type, orderType);
      Assert.Equal(outOrder.Price, orderPrice);
      Assert.Equal(outOrder.TimeSpan, OrderTimeSpanEnum.Gtc);
      Assert.NotEmpty(outOrder.Transaction.Id);
      Assert.Equal(outOrder.Transaction.Id, order.Id);
      Assert.Equal(outOrder.Transaction.Status, OrderStatusEnum.Pending);
    }

    [Theory]
    [InlineData(OrderSideEnum.Long, OrderTypeEnum.Market, 10.0, 15.0, null)]
    [InlineData(OrderSideEnum.Short, OrderTypeEnum.Market, 10.0, 15.0, 10.0)]
    public void CreateMarketOrder(
      OrderSideEnum orderSide,
      OrderTypeEnum orderType,
      double? bid,
      double? ask,
      double? price)
    {
      var point = new PointModel()
      {
        Bid = bid,
        Ask = ask,
        Last = bid ?? ask
      };

      var instrument = new InstrumentModel()
      {
        Name = "X",
        Point = point,
        Points = [point]
      };

      var order = new OrderModel
      {
        Volume = 1,
        Price = price,
        Side = orderSide,
        Type = orderType,
        Transaction = new()
        {
          Descriptor = "Demo",
          Instrument = instrument
        }
      };

      base.CreateOrders(order);

      var position = Account.Positions[order.Name];
      var openPrice = position.Side is OrderSideEnum.Long ? point.Ask : point.Bid;

      Assert.Empty(Account.Deals);
      Assert.Empty(Account.Orders);
      Assert.Single(Account.Positions);

      Assert.Equal(position.Price, openPrice);
      Assert.Equal(position.Instruction, InstructionEnum.Side);
      Assert.Equal(position.Transaction.Price, openPrice);
      Assert.Equal(position.Type, OrderTypeEnum.Market);
      Assert.Equal(position.TimeSpan, OrderTimeSpanEnum.Gtc);
      Assert.Equal(position.Transaction.Status, OrderStatusEnum.Filled);
      Assert.Equal(position.Transaction.Id, order.Id);
      Assert.Equal(position.Volume, order.Volume);
      Assert.Equal(position.Transaction.Volume, order.Volume);
      Assert.NotEmpty(position.Transaction.Id);
      Assert.NotNull(position.Volume);
    }

    [Fact]
    public void CreateMarketOrderWithBrackets()
    {
      var price = 155;
      var point = new PointModel()
      {
        Bid = price,
        Ask = price,
        Last = price
      };

      var instrument = new InstrumentModel()
      {
        Name = "X",
        Point = point,
        Points = [point]
      };

      var TP = new OrderModel
      {
        Volume = 1,
        Price = price + 5,
        Side = OrderSideEnum.Short,
        Type = OrderTypeEnum.Stop,
        Instruction = InstructionEnum.Brace,
        Transaction = new()
        {
          Instrument = instrument
        }
      };

      var SL = new OrderModel
      {
        Volume = 1,
        Price = price - 5,
        Side = OrderSideEnum.Short,
        Type = OrderTypeEnum.Limit,
        Instruction = InstructionEnum.Brace,
        Transaction = new()
        {
          Instrument = instrument
        }
      };

      var order = new OrderModel
      {
        Volume = 1,
        Price = price,
        Side = OrderSideEnum.Long,
        Type = OrderTypeEnum.Market,
        Orders = [SL, TP],
        Transaction = new()
        {
          Instrument = instrument
        }
      };

      base.CreateOrders(order);

      Assert.Empty(Account.Deals);
      Assert.Equal(2, Account.Orders.Count);
      Assert.Single(Account.Positions);

      // Trigger SL

      base.SetupAccounts();

      var balance = Account.Balance;
      var newPoint = new PointModel
      {
        Bid = point.Bid - 15,
        Ask = point.Ask - 10,
        Last = point.Last - 15
      };

      instrument.Point = newPoint;
      instrument.Points.Add(newPoint);

      Assert.Equal(balance, 50000);

      base.OnPoint(null);

      Assert.Single(Account.Deals);
      Assert.Empty(Account.Orders);
      Assert.Empty(Account.Positions);
      Assert.Equal(Account.Balance, balance + (newPoint.Bid - point.Ask));
    }

    [Fact]
    public void CreateComplexMarketOrder()
    {
      var basis = new InstrumentModel
      {
        Name = "SPY",
        Point = new PointModel { Bid = 545, Ask = 550, Last = 550 }
      };

      var optionLong = new InstrumentModel
      {
        Name = "SPY 240814C00493000",
        Point = new PointModel { Bid = 1.45, Ask = 1.55, Last = 1.55 },
        Basis = basis
      };

      var optionShort = new InstrumentModel
      {
        Name = "SPY 240814P00493000",
        Point = new PointModel { Bid = 1.15, Ask = 1.25, Last = 1.25 },
        Basis = basis
      };

      var order = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        TimeSpan = OrderTimeSpanEnum.Day,
        Instruction = InstructionEnum.Group,
        Orders =
        [
          new OrderModel
          {
            Volume = 100,
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = basis },
          },
          new OrderModel
          {
            Volume = 1, 
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = optionLong }
          },
          new OrderModel
          {
            Volume = 2,
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = optionShort }
          }
        ]
      };

      base.CreateOrders(order);

      Assert.Empty(Account.Deals);
      Assert.Empty(Account.Orders);
      Assert.Equal(3, Account.Positions.Count);

      var openShare = Account.Positions[basis.Name];
      var openLong = Account.Positions[optionLong.Name];
      var openShort = Account.Positions[optionShort.Name];

      Assert.Equal(openShare.Side, OrderSideEnum.Long);
      Assert.Equal(openShare.Type, OrderTypeEnum.Market);
      Assert.Equal(openShare.Price, basis.Point.Ask);
      Assert.Equal(openShare.TimeSpan, OrderTimeSpanEnum.Day);
      Assert.NotNull(openShare.Transaction.Time);
      Assert.Equal(openShare.Volume, 100);
      Assert.Equal(openShare.Transaction.Volume, openShare.Volume);
      Assert.Equal(openShare.Transaction.Status, OrderStatusEnum.Filled);
      Assert.Equal(openShare.Transaction.Price, openShare.Price);

      Assert.Equal(openLong.Side, OrderSideEnum.Long);
      Assert.Equal(openLong.Type, OrderTypeEnum.Market);
      Assert.Equal(openLong.Price, optionLong.Point.Ask);
      Assert.Equal(openLong.TimeSpan, OrderTimeSpanEnum.Day);
      Assert.NotNull(openLong.Transaction.Time);
      Assert.Equal(openLong.Volume, 1);
      Assert.Equal(openLong.Transaction.Volume, openLong.Volume);
      Assert.Equal(openLong.Transaction.Status, OrderStatusEnum.Filled);
      Assert.Equal(openLong.Transaction.Price, openLong.Price);

      Assert.Equal(openShort.Side, OrderSideEnum.Long);
      Assert.Equal(openShort.Type, OrderTypeEnum.Market);
      Assert.Equal(openShort.Price, optionShort.Point.Ask);
      Assert.Equal(openShort.TimeSpan, OrderTimeSpanEnum.Day);
      Assert.NotNull(openShort.Transaction.Time);
      Assert.Equal(openShort.Volume, 2);
      Assert.Equal(openShort.Transaction.Volume, openShort.Volume);
      Assert.Equal(openShort.Transaction.Status, OrderStatusEnum.Filled);
      Assert.Equal(openShort.Transaction.Price, openShort.Price);
    }

    [Fact]
    public void UpdatePosition()
    {
      var basis = new InstrumentModel
      {
        Name = "SPY",
        Point = new PointModel { Bid = 545, Ask = 550, Last = 550 }
      };

      var optionLong = new InstrumentModel
      {
        Name = "SPY 240814C00493000",
        Point = new PointModel { Bid = 1.45, Ask = 1.55, Last = 1.55 },
        Basis = basis
      };

      var optionShort = new InstrumentModel
      {
        Name = "SPY 240814P00493000",
        Point = new PointModel { Bid = 1.15, Ask = 1.25, Last = 1.25 },
        Basis = basis
      };

      var order = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        TimeSpan = OrderTimeSpanEnum.Day,
        Instruction = InstructionEnum.Group,
        Orders =
        [
          new OrderModel
          {
            Volume = 100,
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = basis },
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = optionLong }
          },
          new OrderModel
          {
            Volume = 2,
            Side = OrderSideEnum.Long,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = optionShort }
          }
        ]
      };

      base.CreateOrders(order);

      // Increase

      var increase = new OrderModel
      {
        Volume = 50,
        Side = OrderSideEnum.Long,
        Type = OrderTypeEnum.Market,
        Transaction = new() { Instrument = basis },
      };

      base.CreateOrders(increase);

      Assert.Empty(Account.Deals);
      Assert.Empty(Account.Orders);
      Assert.Equal(3, Account.Positions.Count);

      var increaseShare = Account.Positions[basis.Name];

      Assert.Equal(increaseShare.Side, OrderSideEnum.Long);
      Assert.Equal(increaseShare.Type, OrderTypeEnum.Market);
      Assert.Equal(increaseShare.Price, basis.Point.Ask);
      Assert.Equal(increaseShare.TimeSpan, OrderTimeSpanEnum.Day);
      Assert.NotNull(increaseShare.Transaction.Time);
      Assert.Equal(increaseShare.Volume, 150);
      Assert.Equal(increaseShare.Transaction.Volume, increaseShare.Volume);
      Assert.Equal(increaseShare.Transaction.Status, OrderStatusEnum.Filled);
      Assert.Equal(increaseShare.Transaction.Price, increaseShare.Price);

      // Decrease

      var decrease = new OrderModel
      {
        Volume = 1,
        Side = OrderSideEnum.Short,
        Type = OrderTypeEnum.Market,
        Transaction = new() { Instrument = optionShort },
      };

      base.CreateOrders(decrease);

      Assert.Single(Account.Deals);
      Assert.Empty(Account.Orders);
      Assert.Equal(3, Account.Positions.Count);

      var decreaseShort = Account.Positions[optionShort.Name];

      Assert.Equal(decreaseShort.Side, OrderSideEnum.Long);
      Assert.Equal(decreaseShort.Type, OrderTypeEnum.Market);
      Assert.Equal(decreaseShort.Price, optionShort.Point.Ask);
      Assert.Equal(decreaseShort.TimeSpan, OrderTimeSpanEnum.Day);
      Assert.NotNull(decreaseShort.Transaction.Time);
      Assert.Equal(decreaseShort.Transaction.Volume, 1);
      Assert.Equal(decreaseShort.Transaction.Volume, decreaseShort.Volume);
      Assert.Equal(decreaseShort.Transaction.Status, OrderStatusEnum.Filled);
      Assert.Equal(decreaseShort.Transaction.Price, decreaseShort.Price);

      // Close side

      var close = new OrderModel
      {
        Side = OrderSideEnum.Short,
        Type = OrderTypeEnum.Market,
        Volume = Account.Positions[basis.Name].Volume,
        Transaction = new() { Instrument = basis },
      };

      base.CreateOrders(close);

      Assert.Equal(2, Account.Deals.Count);
      Assert.Empty(Account.Orders);
      Assert.Equal(2, Account.Positions.Count);

      // Close position

      var closePosition = new OrderModel
      {
        Type = OrderTypeEnum.Market,
        Instruction = InstructionEnum.Group,
        Orders =
        [
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Short,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = optionLong }
          },
          new OrderModel
          {
            Volume = 1,
            Side = OrderSideEnum.Short,
            Instruction = InstructionEnum.Side,
            Transaction = new() { Instrument = optionShort }
          }
        ]
      };

      base.CreateOrders(closePosition);

      Assert.Empty(Account.Positions);
      Assert.Empty(Account.Orders);
      Assert.Equal(4, Account.Deals.Count);
    }

    [Fact]
    public void ReversePosition()
    {
      var instrument = new InstrumentModel
      {
        Name = "MSFT",
        Point = new PointModel { Bid = 545, Ask = 550, Last = 550 }
      };

      var order = new OrderModel
      {
        Volume = 5,
        Side = OrderSideEnum.Long,
        Type = OrderTypeEnum.Market,
        Transaction = new() { Instrument = instrument },
      };

      base.CreateOrders(order);

      var reverse = new OrderModel
      {
        Volume = 10,
        Side = OrderSideEnum.Short,
        Type = OrderTypeEnum.Market,
        Transaction = new() { Instrument = instrument },
      };

      base.CreateOrders(reverse);

      Assert.Single(Account.Deals);
      Assert.Empty(Account.Orders);
      Assert.Single(Account.Positions);

      var reverseOrder = Account.Positions[instrument.Name];

      Assert.Equal(reverseOrder.Side, OrderSideEnum.Short);
      Assert.Equal(reverseOrder.Type, OrderTypeEnum.Market);
      Assert.Equal(reverseOrder.Price, instrument.Point.Bid);
      Assert.Equal(reverseOrder.TimeSpan, OrderTimeSpanEnum.Gtc);
      Assert.NotNull(reverseOrder.Transaction.Time);
      Assert.Equal(reverseOrder.Transaction.Volume, 5);
      Assert.Equal(reverseOrder.Transaction.Volume, reverseOrder.Volume);
      Assert.Equal(reverseOrder.Transaction.Status, OrderStatusEnum.Filled);
      Assert.Equal(reverseOrder.Transaction.Price, reverseOrder.Price);
    }

    [Fact]
    public void SeparatePosition()
    {
      var orderX = new OrderModel
      {
        Volume = 5,
        Side = OrderSideEnum.Long,
        Type = OrderTypeEnum.Market,
        Transaction = new()
        {
          Instrument = new InstrumentModel
          {
            Name = "SPY",
            Point = new PointModel { Bid = 545, Ask = 550, Last = 550 }
          }
        },
      };

      var orderY = new OrderModel
      {
        Volume = 5,
        Side = OrderSideEnum.Long,
        Type = OrderTypeEnum.Market,
        Transaction = new()
        {
          Instrument = new InstrumentModel
          {
            Name = "MSFT",
            Point = new PointModel { Bid = 145, Ask = 150, Last = 150 }
          }
        },
      };

      base.CreateOrders(orderX);
      base.CreateOrders(orderY);

      Assert.Empty(Account.Orders);
      Assert.Equal(2, Account.Positions.Count);
    }
  }
}
