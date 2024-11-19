using MudBlazor;
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

    protected TableGroupDefinition<PositionRecord> GroupDefinition = new()
    {
      GroupName = "Group",
      Indentation = false,
      Expandable = true,
      IsInitiallyExpanded = true,
      Selector = (e) => e.Group
    };

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
        Update = InvokeAsync(StateHasChanged);
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
        Group = o.Descriptor,
        Time = o.Transaction.Time,
        Name = o.Transaction.Instrument.Name,
        Side = o.Side ?? OrderSideEnum.None,
        Size = o.Transaction.CurrentVolume ?? 0,
        OpenPrice = o.Price ?? 0,
        ClosePrice = o.GetCloseEstimate() ?? 0,
        Gain = o.GetGainEstimate() ?? 0
      };
    }
  }
}
