using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SchwabSignIn.Controllers
{
  [ApiController]
  public class HomeController(IConfiguration configuration) : ControllerBase
  {
    IConfiguration Configuration { get; set; } = configuration;

    [HttpGet]
    [Route("")]
    public async Task<ActionResult<string>> Get(string code)
    {
      var url = "https://api.schwabapi.com/v1/oauth/token";
      var clientId = $"{Configuration["Schwab:ClientId"]}";
      var clientSecret = $"{Configuration["Schwab:ClientSecret"]}";

      if (string.IsNullOrEmpty(code))
      {
        return RedirectToAction("SignIn", "Home", new { clientId, clientSecret });
      }

      using (var client = new HttpClient())
      {
        var formData = new Dictionary<string, string>
        {
          ["code"] = code,
          ["grant_type"] = "authorization_code",
          ["client_id"] = clientId,
          ["redirect_uri"] = "https://127.0.0.1"
        };

        var content = new FormUrlEncodedContent(formData);
        var baseCode = Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}");
        var authHeader = Convert.ToBase64String(baseCode);

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        client.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeader}");

        var response = await client.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
          return await response.Content.ReadAsStringAsync();
        }
      }

      return "End";
    }

    [HttpGet]
    [Route("signin")]
    public ActionResult SignIn(string clientId, string clientSecret)
    {
      var query = HttpUtility.ParseQueryString(string.Empty);

      query.Add("response_type", "code");
      query.Add("client_id", clientId);
      query.Add("scope", "readonly");
      query.Add("redirect_uri", "https://127.0.0.1");

      var url = $"https://api.schwabapi.com/v1/oauth/authorize?{query}";

      return Redirect(url);
    }
  }
}
