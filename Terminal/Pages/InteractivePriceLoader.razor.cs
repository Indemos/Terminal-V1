using Distribution.Services;
using Distribution.Stream;
using InteractiveBrokers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Terminal.Core.Services;

namespace Terminal.Pages
{
  public partial class InteractivePriceLoader
  {
    [Inject] IConfiguration Configuration { get; set; }

    protected virtual PageComponent View { get; set; }
    protected virtual Service Srv { get; set; } = new Service();
    protected virtual InstrumentModel Instrument { get; set; } = new InstrumentModel { Name = "ESH5", Exchange = "CME", Type = InstrumentEnum.Futures, TimeFrame = TimeSpan.FromMinutes(1) };

    protected override async Task OnAfterRenderAsync(bool setup)
    {
      if (setup)
      {
        View.OnPreConnect = CreateAccounts;
        View.OnPostConnect = async () =>
        {
          var stopDate = DateTime.UtcNow.AddDays(-3);
          var args = new PointScreenerModel
          {
            Count = 1000,
            MaxDate = DateTime.UtcNow,
            Instrument = Instrument
          };

          while (args.MaxDate is not null && args.MaxDate > stopDate)
          {
            args.MaxDate = await OnData(args);
          }
        };
      }

      await base.OnAfterRenderAsync(setup);
    }

    protected virtual void CreateAccounts()
    {
      var account = new Account
      {
        Descriptor = Configuration["InteractiveBrokers:Account"],
        Instruments = new ConcurrentDictionary<string, InstrumentModel>
        {
          [Instrument.Name] = Instrument
        }
      };

      View.Adapters["Demo"] = new Adapter
      {
        Account = account,
        Port = int.Parse(Configuration["InteractiveBrokers:Port"])
      };
    }

    protected async Task<DateTime?> OnData(PointScreenerModel screener)
    {
      try
      {
        var counter = 0;
        var adapter = View.Adapters["Demo"];
        var points = await adapter.GetPoints(screener, []);
        var storage = $"D:/Code/NET/Terminal/Data/{Instrument.Name}";

        Directory.CreateDirectory(storage);

        foreach (var point in points.Data)
        {
          var content = JsonSerializer.Serialize(point);
          var source = $"{storage}/{point?.Time?.Ticks}-{++counter}";

          await File.WriteAllTextAsync(source, content);
        }

        return points?.Data?.FirstOrDefault()?.Time;
      }
      catch (Exception e)
      {
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Error = e });
      }

      return null;
    }
  }
}
