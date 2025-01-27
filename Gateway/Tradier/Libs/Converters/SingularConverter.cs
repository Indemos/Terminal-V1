using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tradier.Converters
{
  public class SingularConverter<T> : JsonConverter<List<T>>
  {
    public override bool HandleNull => true;

    public override List<T> Read(ref Utf8JsonReader reader, Type dataType, JsonSerializerOptions options)
    {
      try
      {
        if (reader.TokenType is JsonTokenType.Null)
        {
          return default;
        }

        var response = new List<T>();

        if (reader.TokenType is JsonTokenType.StartArray)
        {
          while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray)
          {
            response.Add(JsonSerializer.Deserialize<T>(ref reader, options));
          }
        }
        else
        {
          response.Add(JsonSerializer.Deserialize<T>(ref reader, options));
        }

        return response;
      }
      catch (Exception)
      {
        return default;
      }
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
      writer.WriteStringValue($"{value}");
    }
  }
}
