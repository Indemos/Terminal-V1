using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Terminal.Records;

namespace Terminal.Components
{
  public partial class OrdersComponent
  {
    protected Task Update { get; set; }

    /// <summary>
    /// Table records
    /// </summary>
    protected virtual IList<OrderRecord> Items { get; set; } = [];

    /// <summary>
    /// Update table records 
    /// </summary>
    /// <param name="items"></param>
    public virtual void UpdateItems(IEnumerable<OrderModel> items)
    {
      if (Update is null || Update.IsCompleted)
      {
        Items = [.. items.Select(GetRecord)];
        Update = Task.WhenAll([InvokeAsync(StateHasChanged), Task.Delay(100)]);
      }
    }

    /// <summary>
    /// Clear records
    /// </summary>
    public virtual void Clear() => UpdateItems([]);

    /// <summary>
    /// Map
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    private static OrderRecord GetRecord(OrderModel o)
    {
      return new OrderRecord
      {
        Name = o.Name,
        Type = $"{o.Type}",
        Time = o.Transaction.Time,
        Group = o.BasisName ?? o.Name,
        Side = o.Side ?? OrderSideEnum.None,
        Size = o.Transaction.Volume ?? 0,
        Price = o.Price ?? 0,
      };
    }
  }
}
