using Microsoft.Extensions.Configuration;

namespace Terminal.Services
{
  public class SetupService
  {
    /// <summary>
    /// Logger instance
    /// </summary>
    public IConfiguration Setup { get; set; }
  }
}
