/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System.Collections.Generic;

namespace InteractiveBrokers.Messages
{
  public class ManagedAccountsMessage
  {
    public ManagedAccountsMessage(string managedAccounts)
    {
      ManagedAccounts = new List<string>(managedAccounts.Split(','));
    }

    public List<string> ManagedAccounts { get; set; }
  }
}
