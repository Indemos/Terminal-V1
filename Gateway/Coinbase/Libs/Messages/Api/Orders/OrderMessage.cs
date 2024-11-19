using System;
using System.Text.Json.Serialization;

namespace Coinbase.Messages
{
  public class OrderMessage
  {
    [JsonPropertyName("order_id")]
    public string Id { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("order_configuration")]
    public OrderConfigurationMessage OrderConfiguration { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("client_order_id")]
    public string ClientOrderId { get; set; }

    [JsonPropertyName("cumulative_quantity")]
    public double? CumulativeQuantity { get; set; }

    [JsonPropertyName("leaves_quantity")]
    public double? LeavesQuantity { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("time_in_force")]
    public string TimeInForce { get; set; }

    [JsonPropertyName("created_time")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("completion_percentage")]
    public double? CompletionPercentage { get; set; }

    [JsonPropertyName("filled_size")]
    public double? FilledSize { get; set; }

    [JsonPropertyName("average_filled_price")]
    public double? AverageFilledPrice { get; set; }

    [JsonPropertyName("fee")]
    public double? Fee { get; set; }

    [JsonPropertyName("number_of_fills")]
    public double? NumberOfFills { get; set; }

    [JsonPropertyName("filled_value")]
    public double? FilledValue { get; set; }

    [JsonPropertyName("pending_cancel")]
    public bool PendingCancel { get; set; }

    [JsonPropertyName("size_in_quote")]
    public bool SizeInQuote { get; set; }

    [JsonPropertyName("avg_price")]
    public double? AveragePrice { get; set; }

    [JsonPropertyName("total_fees")]
    public double? TotalFees { get; set; }

    [JsonPropertyName("size_inclusive_of_fees")]
    public bool SizeInclusiveOfFees { get; set; }

    [JsonPropertyName("total_value_after_fees")]
    public double? TotalValueAfterFees { get; set; }

    [JsonPropertyName("trigger_status")]
    public string TriggerStatus { get; set; }

    [JsonPropertyName("order_type")]
    public string OrderType { get; set; }

    [JsonPropertyName("reject_reason")]
    public string RejectReason { get; set; }

    [JsonPropertyName("settled")]
    public bool Settled { get; set; }

    [JsonPropertyName("product_type")]
    public string ProductType { get; set; }

    [JsonPropertyName("reject_message")]
    public string RejectMessage { get; set; }

    [JsonPropertyName("cancel_message")]
    public string CancelMessage { get; set; }

    [JsonPropertyName("order_placement_source")]
    public string OrderPlacementSource { get; set; }

    [JsonPropertyName("outstanding_hold_amount")]
    public string OutstandingHoldAmount { get; set; }

    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }
  }
}
