/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class HistoricalNewsMessage
  {
    public int RequestId { get; private set; }
    public string Time { get; private set; }
    public string ProviderCode { get; private set; }
    public string ArticleId { get; private set; }
    public string Headline { get; private set; }

    public HistoricalNewsMessage(int requestId, string time, string providerCode, string articleId, string headline)
    {
      RequestId = requestId;
      Time = time;
      ProviderCode = providerCode;
      ArticleId = articleId;
      Headline = headline;
    }
  }
}
