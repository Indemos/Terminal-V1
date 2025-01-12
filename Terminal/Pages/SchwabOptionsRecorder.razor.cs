using Distribution.Services;
using Distribution.Stream;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Schwab;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Terminal.Components;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Terminal.Core.Services;

namespace Terminal.Pages
{
  public partial class SchwabOptionsRecorder
  {
    [Inject] IConfiguration Configuration { get; set; }

    protected virtual PageComponent View { get; set; }
    protected virtual Service Srv { get; set; } = new Service();
    protected virtual InstrumentModel Instrument { get; set; } = new InstrumentModel { Name = "SPY" };

    protected override async Task OnAfterRenderAsync(bool setup)
    {
      if (setup)
      {
        View.OnPreConnect = CreateAccounts;
        View.OnPostConnect = async () => await OnData();
      }

      await base.OnAfterRenderAsync(setup);
    }

    protected virtual void CreateAccounts()
    {
      var account = new Account
      {
        Descriptor = Configuration["Schwab:Account"],
        Instruments = new ConcurrentDictionary<string, InstrumentModel>
        {
          [Instrument.Name] = Instrument
        }
      };

      View.Adapters["Demo"] = new Adapter
      {
        Account = account,
        AccessToken = Configuration["Schwab:AccessToken"],
        RefreshToken = Configuration["Schwab:RefreshToken"],
        ClientId = Configuration["Schwab:ConsumerKey"],
        ClientSecret = Configuration["Schwab:ConsumerSecret"]
      };

      var interval = new Timer();

      interval.Elapsed += async (o, e) => await OnData();
      interval.Interval = 5000;
      interval.Enabled = true;
    }

    protected async Task OnData()
    {
      try
      {
        var adapter = View.Adapters["Demo"];

        var optionArgs = new InstrumentScreenerModel
        {
          Count = 100,
          MinDate = DateTime.Now,
          MaxDate = DateTime.Now.AddYears(1),
          Instrument = Instrument
        };

        var domArgs = new PointScreenerModel
        {
          Instrument = Instrument
        };

        var dom = await adapter.GetDom(domArgs, []);
        var options = await adapter.GetOptions(optionArgs, []);
        var message = dom.Data.Bids.First();
        var storage = $"D:/Code/NET/Terminal/Data/{Instrument.Name}/{DateTime.Now:yyyy-MM-dd}";

        message.Derivatives = new Dictionary<string, IList<InstrumentModel>>
        {
          [nameof(InstrumentEnum.Options)] = options.Data
        };

        Directory.CreateDirectory(storage);

        var content = JsonSerializer.Serialize(message, Srv.Options);
        var source = $"{storage}/{DateTime.UtcNow.Ticks}.zip";

        using var archive = ZipFile.Open(source, ZipArchiveMode.Create);
        using (var entry = archive.CreateEntry($"{DateTime.UtcNow.Ticks}").Open())
        {
          var bytes = Encoding.ASCII.GetBytes(content);
          entry.Write(bytes);
        }
      }
      catch (Exception e)
      {
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Error = e });
      }
    }
  }
}
