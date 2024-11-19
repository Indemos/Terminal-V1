/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class TickNewsMessage
  {
    public int TickerId { get; private set; }
    public long TimeStamp { get; private set; }
    public string ProviderCode { get; private set; }
    public string ArticleId { get; private set; }
    public string Headline { get; private set; }
    public string ExtraData { get; private set; }

    public TickNewsMessage(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData)
    {
      TickerId = tickerId;
      TimeStamp = timeStamp;
      ProviderCode = providerCode;
      ArticleId = articleId;
      Headline = headline;
      ExtraData = extraData;
    }
  }
}
