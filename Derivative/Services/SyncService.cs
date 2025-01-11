using Distribution.Services;
using Microsoft.Extensions.Configuration;
using Schwab;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Terminal.Core.Domains;
using Terminal.Core.Models;

namespace Derivative.Services
{
  public class SyncService
  {
    public bool IsActive { get; set; }
    public Adapter Connector { get; set; }
    public IConfiguration Configuration { get; set; }
    public IDictionary<string, IList<InstrumentModel>> Options { get; set; } = new ConcurrentDictionary<string, IList<InstrumentModel>>();

    public SyncService(IConfiguration configuration) => Configuration = configuration;

    public async Task Connect()
    {
      IsActive = true;

      var account = new Account
      {
        Descriptor = Configuration.GetValue<string>("Schwab:Account")
      };

      Connector = new Adapter
      {
        Account = account,
        AccessToken = Configuration["Schwab:AccessToken"],
        RefreshToken = Configuration["Schwab:RefreshToken"],
        ClientId = Configuration["Schwab:ConsumerKey"],
        ClientSecret = Configuration["Schwab:ConsumerSecret"]
      };

      var interval = new Timer(TimeSpan.FromMinutes(1));
      var scheduler = InstanceService<ScheduleService>.Instance;

      await Connector.Connect();

      scheduler.Send(Update);

      interval.Enabled = true;
      interval.Elapsed += (sender, e) => scheduler.Send(() => Update());
    }

    public async Task<IList<InstrumentModel>> GetOptions(string name, DateTime minDate, DateTime maxDate)
    {
      if (Options.TryGetValue($"{name}", out var items) is false)
      {
        Options[name] = [];
        await Update();
      }

      var rangeItems = Options[name].Where(o =>
      {
        var min = o.Derivative.ExpirationDate.Value.Date >= minDate.Date;
        var max = o.Derivative.ExpirationDate.Value.Date <= maxDate.Date;

        return min && max;

      }).ToList();

      return rangeItems;
    }

    protected async Task Update()
    {
      if (IsActive is false)
      {
        await Connector.Connect();
      }

      foreach (var asset in Options.Keys)
      {
        var date = DateTime.Now;
        var options = await Connector.GetOptions(new InstrumentScreenerModel(), new Hashtable
        {
          ["strikeCount"] = 50,
          ["symbol"] = asset.ToUpper(),
          ["fromDate"] = $"{date:yyyy-MM-dd}",
          ["toDate"] = $"{date.AddYears(1):yyyy-MM-dd}"
        });

        Options[asset] = options.Data;
      }
    }
  }
}
