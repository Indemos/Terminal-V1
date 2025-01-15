using Distribution.Services;
using Serilog;

namespace Terminal.Services
{
  public class RecordService
  {
    /// <summary>
    /// Logger instance
    /// </summary>
    public ILogger Recorder => Log.Logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public RecordService()
    {
      var setup = InstanceService<SetupService>.Instance.Setup;

      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        //.WriteTo.Console()
        .WriteTo.File($"{setup["Logs:Source"]}")
        .CreateLogger();
    }
  }
}
