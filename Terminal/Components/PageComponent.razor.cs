using Distribution.Models;
using Distribution.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;
using Terminal.Core.Services;

namespace Terminal.Components
{
  public partial class PageComponent
  {
    [Parameter] public virtual RenderFragment FramesView { get; set; } = default;
    [Parameter] public virtual RenderFragment EstimatesView { get; set; } = default;
    [Parameter] public virtual RenderFragment PositionsMetricsView { get; set; } = default;
    [Parameter] public virtual RenderFragment StrikesView { get; set; } = default;

    public virtual StatusEnum ConnectionState { get; set; }
    public virtual StatusEnum SubscriptionState { get; set; }
    public virtual ChartsComponent ChartsView { get; set; }
    public virtual ChartsComponent ReportsView { get; set; }
    public virtual DealsComponent DealsView { get; set; }
    public virtual OrdersComponent OrdersView { get; set; }
    public virtual PositionsComponent PositionsView { get; set; }
    public virtual StatementsComponent StatementsView { get; set; }
    public virtual Action OnPreConnect { get; set; } = () => { };
    public virtual Action OnPostConnect { get; set; } = () => { };
    public virtual Action OnDisconnect { get; set; } = () => { };
    public virtual IDictionary<string, IGateway> Adapters { get; set; } = new Dictionary<string, IGateway>();

    public virtual async Task Connect()
    {
      try
      {
        ConnectionState = StatusEnum.Progress;

        await Disconnect();

        OnPreConnect();

        await Task.WhenAll(Adapters.Values.Select(o => o.Connect()));

        ConnectionState = StatusEnum.Success;
        SubscriptionState = StatusEnum.Success;

        OnPostConnect();
      }
      catch (Exception e)
      {
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Error = e });
      }
    }

    public virtual async Task Disconnect()
    {
      try
      {
        await Task.WhenAll(Adapters.Values.Select(o => o.Disconnect()));

        InstanceService<ScheduleService>.Instance.Send(() =>
        {
          ChartsView.Clear();
          ReportsView.Clear();
          DealsView.Clear();
          OrdersView.Clear();
          PositionsView.Clear();

          OnDisconnect();

        }, new OptionModel { IsRemovable = false });

        if (ConnectionState is not StatusEnum.Progress)
        {
          ConnectionState = StatusEnum.None;
          SubscriptionState = StatusEnum.None;
        }
      }
      catch (Exception e)
      {
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Error = e });
      }
    }

    public virtual async Task Subscribe()
    {
      try
      {
        await Task.WhenAll(Adapters
          .Values
          .SelectMany(adapter => adapter
            .Account
            .Instruments
            .Values
            .Select(o => adapter.Subscribe(o))));

        SubscriptionState = StatusEnum.Success;
      }
      catch (Exception e)
      {
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Error = e });
      }
    }

    public virtual async Task Unsubscribe()
    {
      try
      {
        await Task.WhenAll(Adapters
          .Values
          .SelectMany(adapter => adapter
            .Account
            .Instruments
            .Values
            .Select(o => adapter.Unsubscribe(o))));

        SubscriptionState = StatusEnum.None;
      }
      catch (Exception e)
      {
        InstanceService<MessageService>.Instance.OnMessage(new MessageModel<string> { Error = e });
      }
    }

    public virtual void OpenState()
    {
      StatementsView.UpdateItems(Adapters.Values.Select(o => o.Account).ToList());
    }
  }
}
