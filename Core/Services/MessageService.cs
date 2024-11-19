using System;
using Terminal.Core.Models;

namespace Terminal.Core.Services
{
  public class MessageService
  {
    public virtual Action<MessageModel<string>> OnMessage { get; set; }
  }
}
