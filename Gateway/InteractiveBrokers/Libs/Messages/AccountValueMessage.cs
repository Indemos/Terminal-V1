/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

namespace InteractiveBrokers.Messages
{
  public class AccountValueMessage
  {
    public AccountValueMessage(string key, string value, string currency, string accountName)
    {
      Key = key;
      Value = value;
      Currency = currency;
      AccountName = accountName;
    }

    public string Key { get; set; }

    public string Value { get; set; }

    public string Currency { get; set; }

    public string AccountName { get; set; }
  }
}
