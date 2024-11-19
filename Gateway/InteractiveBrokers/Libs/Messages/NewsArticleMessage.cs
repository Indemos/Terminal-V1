/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class NewsArticleMessage
  {
    public int RequestId { get; private set; }
    public int ArticleType { get; private set; }
    public string ArticleText { get; private set; }

    public NewsArticleMessage(int requestId, int articleType, string articleText)
    {
      RequestId = requestId;
      ArticleType = articleType;
      ArticleText = articleText;
    }
  }
}
