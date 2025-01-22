namespace Tradier.Messages.MarketData
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public class CompanyDataCoreMessage
  {
    public CompanyDataCoreMessage() => Results = [];

    [JsonPropertyName("request")]
    public string Request { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("results")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CompanyDataMessage> Results { get; set; }
  }

  public class CompanyDataMessage
  {
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("tables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CompanyDataTableMessage Tables { get; set; }
  }

  public class CompanyDataTableMessage
  {
    [JsonPropertyName("company_profile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CompanyProfileMessage CompanyProfile { get; set; }

    [JsonPropertyName("asset_classification")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AssetClassificationMessage AssetClassification { get; set; }

    [JsonPropertyName("historical_asset_classification")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HistoricalAssetClassificationMessage HistoricalAssetClassification { get; set; }

    [JsonPropertyName("long_descriptions")]
    public string LongDescriptions { get; set; }
  }

  public class CompanyProfileMessage
  {
    [JsonPropertyName("company_id")]
    public string CompanyId { get; set; }

    [JsonPropertyName("average_employee_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AverageEmployeeNumber { get; set; }

    [JsonPropertyName("contact_email")]
    public string ContactEmail { get; set; }

    [JsonPropertyName("is_head_office_same_with_registered_office_flag")]
    public bool IsHeadOfficeSameWithRegisteredOfficeFlag { get; set; }

    [JsonPropertyName("total_employee_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalEmployeeNumber { get; set; }

    [JsonPropertyName("TotalEmployeeNumber.asOfDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? TotalEmployeeNumberAsOfDate { get; set; }

    [JsonPropertyName("headquarter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CompanyHeadquarterMessage Headquarter { get; set; }
  }

  public class CompanyHeadquarterMessage
  {
    [JsonPropertyName("address_line1")]
    public string AddressLine1 { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("fax")]
    public string Fax { get; set; }

    [JsonPropertyName("homepage")]
    public string Homepage { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    [JsonPropertyName("province")]
    public string Province { get; set; }
  }

  public abstract class AssetClassificationBaseMessage
  {
    [JsonPropertyName("company_id")]
    public string CompanyId { get; set; }

    [JsonPropertyName("financial_health_grade")]
    public string FinancialHealthGrade { get; set; }

    [JsonPropertyName("morningstar_economy_sphere_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MorningstarEconomySphereCode { get; set; }

    [JsonPropertyName("morningstar_industry_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MorningstarIndustryCode { get; set; }

    [JsonPropertyName("morningstar_industry_group_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MorningstarIndustryGroupCode { get; set; }

    [JsonPropertyName("morningstar_sector_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MorningstarSectorCode { get; set; }

    [JsonPropertyName("profitability_grade")]
    public string ProfitabilityGrade { get; set; }

    [JsonPropertyName("size_score")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SizeScore { get; set; }

    [JsonPropertyName("stock_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StockType { get; set; }

    [JsonPropertyName("style_box")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StyleBox { get; set; }

    [JsonPropertyName("style_score")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? StyleScore { get; set; }

    [JsonPropertyName("value_score")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ValueScore { get; set; }
  }

  public class AssetClassificationMessage : AssetClassificationBaseMessage
  {
    [JsonPropertyName("c_a_n_n_a_i_c_s")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Cannaics { get; set; }

    [JsonPropertyName("FinancialHealthGrade.asOfDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? FinancialHealthGradeAsOfDate { get; set; }

    [JsonPropertyName("growth_grade")]
    public string GrowthGrade { get; set; }

    [JsonPropertyName("GrowthGrade.asOfDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? GrowthGradeAsOfDate { get; set; }

    [JsonPropertyName("n_a_c_e")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Nace { get; set; }

    [JsonPropertyName("n_a_i_c_s")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Naics { get; set; }

    [JsonPropertyName("ProfitabilityGrade.asOfDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? ProfitabilityGradeAsOfDate { get; set; }

    [JsonPropertyName("s_i_c")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Sic { get; set; }

    [JsonPropertyName("StockType.asOfDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? StockTypeAsOfDate { get; set; }

    [JsonPropertyName("StyleBox.asOfDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? StyleBoxAsOfDate { get; set; }
  }

  public class HistoricalAssetClassificationMessage : AssetClassificationBaseMessage
  {
    [JsonPropertyName("as_of_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? AsOfDate { get; set; }
  }
}
