using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tradier.Messages.MarketData
{
  public class CorporateCalendarCoreMessage
  {
    [JsonPropertyName("request")]
    public string Request { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("results")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CorporateCalendarDataMessage> Results { get; set; }
  }

  public class CorporateCalendarDataMessage
  {
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("tables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CorporateCalendarTableMessage Tables { get; set; }
  }

  public class CorporateCalendarTableMessage
  {
    [JsonPropertyName("corporate_calendars")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CorporateCalendarMessage> CorporateCalendars { get; set; }
  }

  public class CorporateCalendarMessage
  {
    [JsonPropertyName("company_id")]
    public string CompanyId { get; set; }

    [JsonPropertyName("begin_date_time")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? BeginDateTime { get; set; }

    [JsonPropertyName("end_date_time")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? EndDateTime { get; set; }

    [JsonPropertyName("event_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EventType { get; set; }

    [JsonPropertyName("estimated_date_for_next_event")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? EstimatedDateForNextEvent { get; set; }

    [JsonPropertyName("event")]
    public string Event { get; set; }

    [JsonPropertyName("event_fiscal_year")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EventFiscalYear { get; set; }

    [JsonPropertyName("event_status")]
    public string EventStatus { get; set; }

    [JsonPropertyName("time_zone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? TimeZone { get; set; }
  }
}
