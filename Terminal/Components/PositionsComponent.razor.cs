using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Terminal.Records;

namespace Terminal.Components
{
  public partial class PositionsComponent
  {
    protected Task Update { get; set; }

    /// <summary>
    /// Table records
    /// </summary>
    protected virtual IList<PositionRecord> Items { get; set; } = [];

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
    private static PositionRecord GetRecord(OrderModel o)
    {
      return new PositionRecord
      {
        Name = o.Name,
        Group = o.BasisName ?? o.Name,
        Time = o.Transaction.Time,
        Side = o.Side ?? OrderSideEnum.None,
        Size = o.Transaction.Volume ?? 0,
        OpenPrice = o.Price ?? 0,
        ClosePrice = o.GetCloseEstimate() ?? 0,
        Gain = o.GetGainEstimate() ?? o.Gain ?? 0
      };
    }
  }
}
