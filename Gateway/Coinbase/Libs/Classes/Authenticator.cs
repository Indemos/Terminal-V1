using Jose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Coinbase.Classes
{
  public class Authenticator
  {
    protected Random _generator;

    /// <summary>
    /// Constructor
    /// </summary>
    public Authenticator()
    {
      _generator = new Random();
    }

    /// <summary>
    /// Get JWT token
    /// </summary>
    /// <param name="name"></param>
    /// <param name="secret"></param>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public virtual string GetToken(string name, string secret, string endpoint)
    {
      var privateKeyBytes = Convert.FromBase64String(secret);
      var payload = new Dictionary<string, object>
      {
        { "sub", name },
        { "iss", "coinbase-cloud" },
        { "nbf", Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds) },
        { "exp", Convert.ToInt64((DateTime.UtcNow.AddMinutes(1) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds) },
        { "uri", endpoint }
      };

      var extraHeaders = new Dictionary<string, object>
      {
        { "kid", name },
        { "nonce", GetNonce(10) },
        { "typ", "JWT"}
      };

      using (var key = ECDsa.Create())
      {
        key.ImportECPrivateKey(privateKeyBytes, out _);
        return JWT.Encode(payload, key, JwsAlgorithm.ES256, extraHeaders);
      }
    }

    /// <summary>
    /// Get nonce
    /// </summary>
    /// <param name="digits"></param>
    /// <returns></returns>
    protected virtual string GetNonce(int digits)
    {
      var buffer = new byte[digits / 2];

      _generator.NextBytes(buffer);

      var result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());

      if (digits % 2 == 0)
      {
        return result;
      }

      return result + _generator.Next(16).ToString("X");
    }
  }
}
