/* Copyright (C) 2024 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace IBApi
{
  internal class EDecoder : IDecoder
  {
    private readonly EClientMsgSink eClientMsgSink;
    private readonly EWrapper eWrapper;
    private int serverVersion;
    private BinaryReader dataReader;
    private int nDecodedLen;

    public EDecoder(int serverVersion, EWrapper callback, EClientMsgSink sink = null)
    {
      this.serverVersion = serverVersion;
      eWrapper = callback;
      eClientMsgSink = sink;
    }

    public int ParseAndProcessMsg(byte[] buf)
    {
      dataReader?.Dispose();

      dataReader = new BinaryReader(new MemoryStream(buf));
      nDecodedLen = 0;

      if (serverVersion == 0)
      {
        ProcessConnectAck();

        return nDecodedLen;
      }

      return ProcessIncomingMessage(ReadInt()) ? nDecodedLen : -1;
    }

    private void ProcessConnectAck()
    {
      serverVersion = ReadInt();

      if (serverVersion == -1)
      {
        var srv = ReadString();

        serverVersion = 0;

        eClientMsgSink?.redirect(srv);

        return;
      }

      var serverTime = "";

      if (serverVersion >= 20) serverTime = ReadString();

      eClientMsgSink?.serverVersion(serverVersion, serverTime);

      eWrapper.connectAck();
    }

    private bool ProcessIncomingMessage(int incomingMessage)
    {
      if (incomingMessage == IncomingMessage.NotValid) return false;

      switch (incomingMessage)
      {
        case IncomingMessage.TickPrice:
          TickPriceEvent();
          break;

        case IncomingMessage.TickSize:
          TickSizeEvent();
          break;

        case IncomingMessage.Tickstring:
          TickStringEvent();
          break;

        case IncomingMessage.TickGeneric:
          TickGenericEvent();
          break;

        case IncomingMessage.TickEFP:
          TickEFPEvent();
          break;

        case IncomingMessage.TickSnapshotEnd:
          TickSnapshotEndEvent();
          break;

        case IncomingMessage.Error:
          ErrorEvent();
          break;

        case IncomingMessage.CurrentTime:
          CurrentTimeEvent();
          break;

        case IncomingMessage.ManagedAccounts:
          ManagedAccountsEvent();
          break;

        case IncomingMessage.NextValidId:
          NextValidIdEvent();
          break;

        case IncomingMessage.DeltaNeutralValidation:
          DeltaNeutralValidationEvent();
          break;

        case IncomingMessage.TickOptionComputation:
          TickOptionComputationEvent();
          break;

        case IncomingMessage.AccountSummary:
          AccountSummaryEvent();
          break;

        case IncomingMessage.AccountSummaryEnd:
          AccountSummaryEndEvent();
          break;

        case IncomingMessage.AccountValue:
          AccountValueEvent();
          break;

        case IncomingMessage.PortfolioValue:
          PortfolioValueEvent();
          break;

        case IncomingMessage.AccountUpdateTime:
          AccountUpdateTimeEvent();
          break;

        case IncomingMessage.AccountDownloadEnd:
          AccountDownloadEndEvent();
          break;

        case IncomingMessage.OrderStatus:
          OrderStatusEvent();
          break;

        case IncomingMessage.OpenOrder:
          OpenOrderEvent();
          break;

        case IncomingMessage.OpenOrderEnd:
          OpenOrderEndEvent();
          break;

        case IncomingMessage.ContractData:
          ContractDataEvent();
          break;

        case IncomingMessage.ContractDataEnd:
          ContractDataEndEvent();
          break;

        case IncomingMessage.ExecutionData:
          ExecutionDataEvent();
          break;

        case IncomingMessage.ExecutionDataEnd:
          ExecutionDataEndEvent();
          break;

        case IncomingMessage.CommissionsReport:
          CommissionReportEvent();
          break;

        case IncomingMessage.FundamentalData:
          FundamentalDataEvent();
          break;

        case IncomingMessage.HistoricalData:
          HistoricalDataEvent();
          break;

        case IncomingMessage.MarketDataType:
          MarketDataTypeEvent();
          break;

        case IncomingMessage.MarketDepth:
          MarketDepthEvent();
          break;

        case IncomingMessage.MarketDepthL2:
          MarketDepthL2Event();
          break;

        case IncomingMessage.NewsBulletins:
          NewsBulletinsEvent();
          break;

        case IncomingMessage.Position:
          PositionEvent();
          break;

        case IncomingMessage.PositionEnd:
          PositionEndEvent();
          break;

        case IncomingMessage.RealTimeBars:
          RealTimeBarsEvent();
          break;

        case IncomingMessage.ScannerParameters:
          ScannerParametersEvent();
          break;

        case IncomingMessage.ScannerData:
          ScannerDataEvent();
          break;

        case IncomingMessage.ReceiveFA:
          ReceiveFAEvent();
          break;

        case IncomingMessage.BondContractData:
          BondContractDetailsEvent();
          break;

        case IncomingMessage.VerifyMessageApi:
          VerifyMessageApiEvent();
          break;

        case IncomingMessage.VerifyCompleted:
          VerifyCompletedEvent();
          break;

        case IncomingMessage.DisplayGroupList:
          DisplayGroupListEvent();
          break;

        case IncomingMessage.DisplayGroupUpdated:
          DisplayGroupUpdatedEvent();
          break;

        case IncomingMessage.VerifyAndAuthMessageApi:
          VerifyAndAuthMessageApiEvent();
          break;

        case IncomingMessage.VerifyAndAuthCompleted:
          VerifyAndAuthCompletedEvent();
          break;

        case IncomingMessage.PositionMulti:
          PositionMultiEvent();
          break;

        case IncomingMessage.PositionMultiEnd:
          PositionMultiEndEvent();
          break;

        case IncomingMessage.AccountUpdateMulti:
          AccountUpdateMultiEvent();
          break;

        case IncomingMessage.AccountUpdateMultiEnd:
          AccountUpdateMultiEndEvent();
          break;

        case IncomingMessage.SecurityDefinitionOptionParameter:
          SecurityDefinitionOptionParameterEvent();
          break;

        case IncomingMessage.SecurityDefinitionOptionParameterEnd:
          SecurityDefinitionOptionParameterEndEvent();
          break;

        case IncomingMessage.SoftDollarTier:
          SoftDollarTierEvent();
          break;

        case IncomingMessage.FamilyCodes:
          FamilyCodesEvent();
          break;

        case IncomingMessage.SymbolSamples:
          SymbolSamplesEvent();
          break;

        case IncomingMessage.MktDepthExchanges:
          MktDepthExchangesEvent();
          break;

        case IncomingMessage.TickNews:
          TickNewsEvent();
          break;

        case IncomingMessage.TickReqParams:
          TickReqParamsEvent();
          break;

        case IncomingMessage.SmartComponents:
          SmartComponentsEvent();
          break;

        case IncomingMessage.NewsProviders:
          NewsProvidersEvent();
          break;

        case IncomingMessage.NewsArticle:
          NewsArticleEvent();
          break;

        case IncomingMessage.HistoricalNews:
          HistoricalNewsEvent();
          break;

        case IncomingMessage.HistoricalNewsEnd:
          HistoricalNewsEndEvent();
          break;

        case IncomingMessage.HeadTimestamp:
          HeadTimestampEvent();
          break;

        case IncomingMessage.HistogramData:
          HistogramDataEvent();
          break;

        case IncomingMessage.HistoricalDataUpdate:
          HistoricalDataUpdateEvent();
          break;

        case IncomingMessage.RerouteMktDataReq:
          RerouteMktDataReqEvent();
          break;

        case IncomingMessage.RerouteMktDepthReq:
          RerouteMktDepthReqEvent();
          break;

        case IncomingMessage.MarketRule:
          MarketRuleEvent();
          break;

        case IncomingMessage.PnL:
          PnLEvent();
          break;

        case IncomingMessage.PnLSingle:
          PnLSingleEvent();
          break;

        case IncomingMessage.HistoricalTick:
          HistoricalTickEvent();
          break;

        case IncomingMessage.HistoricalTickBidAsk:
          HistoricalTickBidAskEvent();
          break;

        case IncomingMessage.HistoricalTickLast:
          HistoricalTickLastEvent();
          break;

        case IncomingMessage.TickByTick:
          TickByTickEvent();
          break;

        case IncomingMessage.OrderBound:
          OrderBoundEvent();
          break;

        case IncomingMessage.CompletedOrder:
          CompletedOrderEvent();
          break;

        case IncomingMessage.CompletedOrdersEnd:
          CompletedOrdersEndEvent();
          break;

        case IncomingMessage.ReplaceFAEnd:
          ReplaceFAEndEvent();
          break;

        case IncomingMessage.WshMetaData:
          ProcessWshMetaData();
          break;

        case IncomingMessage.WshEventData:
          ProcessWshEventData();
          break;

        case IncomingMessage.HistoricalSchedule:
          ProcessHistoricalScheduleEvent();
          break;

        case IncomingMessage.UserInfo:
          ProcessUserInfoEvent();
          break;

        default:
          eWrapper.error(IncomingMessage.NotValid, EClientErrors.UNKNOWN_ID.Code, EClientErrors.UNKNOWN_ID.Message, "");
          return false;
      }

      return true;
    }

    private void CompletedOrderEvent()
    {
      var contract = new Contract();
      var order = new Order();
      var orderState = new OrderState();
      var eOrderDecoder = new EOrderDecoder(this, contract, order, orderState, int.MaxValue, serverVersion);

      // read contract fields
      eOrderDecoder.readContractFields();

      // read order fields
      eOrderDecoder.readAction();
      eOrderDecoder.readTotalQuantity();
      eOrderDecoder.readOrderType();
      eOrderDecoder.readLmtPrice();
      eOrderDecoder.readAuxPrice();
      eOrderDecoder.readTIF();
      eOrderDecoder.readOcaGroup();
      eOrderDecoder.readAccount();
      eOrderDecoder.readOpenClose();
      eOrderDecoder.readOrigin();
      eOrderDecoder.readOrderRef();
      eOrderDecoder.readPermId();
      eOrderDecoder.readOutsideRth();
      eOrderDecoder.readHidden();
      eOrderDecoder.readDiscretionaryAmount();
      eOrderDecoder.readGoodAfterTime();
      eOrderDecoder.readFAParams();
      eOrderDecoder.readModelCode();
      eOrderDecoder.readGoodTillDate();
      eOrderDecoder.readRule80A();
      eOrderDecoder.readPercentOffset();
      eOrderDecoder.readSettlingFirm();
      eOrderDecoder.readShortSaleParams();
      eOrderDecoder.readBoxOrderParams();
      eOrderDecoder.readPegToStkOrVolOrderParams();
      eOrderDecoder.readDisplaySize();
      eOrderDecoder.readSweepToFill();
      eOrderDecoder.readAllOrNone();
      eOrderDecoder.readMinQty();
      eOrderDecoder.readOcaType();
      eOrderDecoder.readTriggerMethod();
      eOrderDecoder.readVolOrderParams(false);
      eOrderDecoder.readTrailParams();
      eOrderDecoder.readComboLegs();
      eOrderDecoder.readSmartComboRoutingParams();
      eOrderDecoder.readScaleOrderParams();
      eOrderDecoder.readHedgeParams();
      eOrderDecoder.readClearingParams();
      eOrderDecoder.readNotHeld();
      eOrderDecoder.readDeltaNeutral();
      eOrderDecoder.readAlgoParams();
      eOrderDecoder.readSolicited();
      eOrderDecoder.readOrderStatus();
      eOrderDecoder.readVolRandomizeFlags();
      eOrderDecoder.readPegToBenchParams();
      eOrderDecoder.readConditions();
      eOrderDecoder.readStopPriceAndLmtPriceOffset();
      eOrderDecoder.readCashQty();
      eOrderDecoder.readDontUseAutoPriceForHedge();
      eOrderDecoder.readIsOmsContainer();
      eOrderDecoder.readAutoCancelDate();
      eOrderDecoder.readFilledQuantity();
      eOrderDecoder.readRefFuturesConId();
      eOrderDecoder.readAutoCancelParent();
      eOrderDecoder.readShareholder();
      eOrderDecoder.readImbalanceOnly();
      eOrderDecoder.readRouteMarketableToBbo();
      eOrderDecoder.readParentPermId();
      eOrderDecoder.readCompletedTime();
      eOrderDecoder.readCompletedStatus();
      eOrderDecoder.readPegBestPegMidOrderAttributes();
      eOrderDecoder.readCustomerAccount();
      eOrderDecoder.readProfessionalCustomer();

      eWrapper.completedOrder(contract, order, orderState);
    }

    private void CompletedOrdersEndEvent() => eWrapper.completedOrdersEnd();

    private void OrderBoundEvent()
    {
      var orderId = ReadLong();
      var apiClientId = ReadInt();
      var apiOrderId = ReadInt();

      eWrapper.orderBound(orderId, apiClientId, apiOrderId);
    }

    private void TickByTickEvent()
    {
      var reqId = ReadInt();
      var tickType = ReadInt();
      var time = ReadLong();
      BitMask mask;

      switch (tickType)
      {
        case 0: // None
          break;
        case 1: // Last
        case 2: // AllLast
          var price = ReadDouble();
          var size = ReadDecimal();
          mask = new BitMask(ReadInt());
          var tickAttribLast = new TickAttribLast
          {
            PastLimit = mask[0],
            Unreported = mask[1]
          };
          var exchange = ReadString();
          var specialConditions = ReadString();
          eWrapper.tickByTickAllLast(reqId, tickType, time, price, size, tickAttribLast, exchange, specialConditions);
          break;
        case 3: // BidAsk
          var bidPrice = ReadDouble();
          var askPrice = ReadDouble();
          var bidSize = ReadDecimal();
          var askSize = ReadDecimal();
          mask = new BitMask(ReadInt());
          var tickAttribBidAsk = new TickAttribBidAsk
          {
            BidPastLow = mask[0],
            AskPastHigh = mask[1]
          };
          eWrapper.tickByTickBidAsk(reqId, time, bidPrice, askPrice, bidSize, askSize, tickAttribBidAsk);
          break;
        case 4: // MidPoint
          var midPoint = ReadDouble();
          eWrapper.tickByTickMidPoint(reqId, time, midPoint);
          break;
      }
    }

    private void HistoricalTickLastEvent()
    {
      var reqId = ReadInt();
      var nTicks = ReadInt();
      var ticks = new HistoricalTickLast[nTicks];

      for (var i = 0; i < nTicks; i++)
      {
        var time = ReadLong();
        var mask = new BitMask(ReadInt());
        var tickAttribLast = new TickAttribLast
        {
          PastLimit = mask[0],
          Unreported = mask[1]
        };
        var price = ReadDouble();
        var size = ReadDecimal();
        var exchange = ReadString();
        var specialConditions = ReadString();

        ticks[i] = new HistoricalTickLast(time, tickAttribLast, price, size, exchange, specialConditions);
      }

      var done = ReadBoolFromInt();

      eWrapper.historicalTicksLast(reqId, ticks, done);
    }

    private void HistoricalTickBidAskEvent()
    {
      var reqId = ReadInt();
      var nTicks = ReadInt();
      var ticks = new HistoricalTickBidAsk[nTicks];

      for (var i = 0; i < nTicks; i++)
      {
        var time = ReadLong();
        var mask = new BitMask(ReadInt());
        var tickAttribBidAsk = new TickAttribBidAsk
        {
          AskPastHigh = mask[0],
          BidPastLow = mask[1]
        };
        var priceBid = ReadDouble();
        var priceAsk = ReadDouble();
        var sizeBid = ReadDecimal();
        var sizeAsk = ReadDecimal();

        ticks[i] = new HistoricalTickBidAsk(time, tickAttribBidAsk, priceBid, priceAsk, sizeBid, sizeAsk);
      }

      var done = ReadBoolFromInt();

      eWrapper.historicalTicksBidAsk(reqId, ticks, done);
    }

    private void HistoricalTickEvent()
    {
      var reqId = ReadInt();
      var nTicks = ReadInt();
      var ticks = new HistoricalTick[nTicks];

      for (var i = 0; i < nTicks; i++)
      {
        var time = ReadLong();
        ReadInt(); // for consistency
        var price = ReadDouble();
        var size = ReadDecimal();

        ticks[i] = new HistoricalTick(time, price, size);
      }

      var done = ReadBoolFromInt();

      eWrapper.historicalTicks(reqId, ticks, done);
    }

    private void MarketRuleEvent()
    {
      var marketRuleId = ReadInt();
      var priceIncrements = Array.Empty<PriceIncrement>();
      var nPriceIncrements = ReadInt();

      if (nPriceIncrements > 0)
      {
        Array.Resize(ref priceIncrements, nPriceIncrements);

        for (var i = 0; i < nPriceIncrements; ++i)
        {
          priceIncrements[i] = new PriceIncrement(ReadDouble(), ReadDouble());
        }
      }

      eWrapper.marketRule(marketRuleId, priceIncrements);
    }

    private void RerouteMktDepthReqEvent()
    {
      var reqId = ReadInt();
      var conId = ReadInt();
      var exchange = ReadString();

      eWrapper.rerouteMktDepthReq(reqId, conId, exchange);
    }

    private void RerouteMktDataReqEvent()
    {
      var reqId = ReadInt();
      var conId = ReadInt();
      var exchange = ReadString();

      eWrapper.rerouteMktDataReq(reqId, conId, exchange);
    }

    private void HistoricalDataUpdateEvent()
    {
      var requestId = ReadInt();
      var barCount = ReadInt();
      var date = ReadString();
      var open = ReadDouble();
      var close = ReadDouble();
      var high = ReadDouble();
      var low = ReadDouble();
      var WAP = ReadDecimal();
      var volume = ReadDecimal();

      eWrapper.historicalDataUpdate(requestId, new Bar(date, open, high, low, close, volume, barCount, WAP));
    }


    private void PnLSingleEvent()
    {
      var reqId = ReadInt();
      var pos = ReadDecimal();
      var dailyPnL = ReadDouble();
      var unrealizedPnL = double.MaxValue;
      var realizedPnL = double.MaxValue;

      if (serverVersion >= MinServerVer.UNREALIZED_PNL) unrealizedPnL = ReadDouble();

      if (serverVersion >= MinServerVer.REALIZED_PNL) realizedPnL = ReadDouble();

      var value = ReadDouble();

      eWrapper.pnlSingle(reqId, pos, dailyPnL, unrealizedPnL, realizedPnL, value);
    }

    private void PnLEvent()
    {
      var reqId = ReadInt();
      var dailyPnL = ReadDouble();
      var unrealizedPnL = double.MaxValue;
      var realizedPnL = double.MaxValue;

      if (serverVersion >= MinServerVer.UNREALIZED_PNL) unrealizedPnL = ReadDouble();

      if (serverVersion >= MinServerVer.REALIZED_PNL) realizedPnL = ReadDouble();

      eWrapper.pnl(reqId, dailyPnL, unrealizedPnL, realizedPnL);
    }

    private void HistogramDataEvent()
    {
      var reqId = ReadInt();
      var n = ReadInt();
      var data = new HistogramEntry[n];

      for (var i = 0; i < n; i++)
      {
        data[i].Price = ReadDouble();
        data[i].Size = ReadDecimal();
      }

      eWrapper.histogramData(reqId, data);
    }

    private void HeadTimestampEvent()
    {
      var reqId = ReadInt();
      var headTimestamp = ReadString();

      eWrapper.headTimestamp(reqId, headTimestamp);
    }

    private void HistoricalNewsEvent()
    {
      var requestId = ReadInt();
      var time = ReadString();
      var providerCode = ReadString();
      var articleId = ReadString();
      var headline = ReadString();

      eWrapper.historicalNews(requestId, time, providerCode, articleId, headline);
    }

    private void HistoricalNewsEndEvent()
    {
      var requestId = ReadInt();
      var hasMore = ReadBoolFromInt();

      eWrapper.historicalNewsEnd(requestId, hasMore);
    }

    private void NewsArticleEvent()
    {
      var requestId = ReadInt();
      var articleType = ReadInt();
      var articleText = ReadString();

      eWrapper.newsArticle(requestId, articleType, articleText);
    }

    private void NewsProvidersEvent()
    {
      var newsProviders = Array.Empty<NewsProvider>();
      var nNewsProviders = ReadInt();

      if (nNewsProviders > 0)
      {
        Array.Resize(ref newsProviders, nNewsProviders);

        for (var i = 0; i < nNewsProviders; ++i)
        {
          newsProviders[i] = new NewsProvider(ReadString(), ReadString());
        }
      }

      eWrapper.newsProviders(newsProviders);
    }

    private void SmartComponentsEvent()
    {
      var reqId = ReadInt();
      var n = ReadInt();
      var theMap = new Dictionary<int, KeyValuePair<string, char>>();

      for (var i = 0; i < n; i++)
      {
        var bitNumber = ReadInt();
        var exchange = ReadString();
        var exchangeLetter = ReadChar();

        theMap.Add(bitNumber, new KeyValuePair<string, char>(exchange, exchangeLetter));
      }

      eWrapper.smartComponents(reqId, theMap);
    }

    private void TickReqParamsEvent()
    {
      var tickerId = ReadInt();
      var minTick = ReadDouble();
      var bboExchange = ReadString();
      var snapshotPermissions = ReadInt();

      eWrapper.tickReqParams(tickerId, minTick, bboExchange, snapshotPermissions);
    }

    private void TickNewsEvent()
    {
      var tickerId = ReadInt();
      var timeStamp = ReadLong();
      var providerCode = ReadString();
      var articleId = ReadString();
      var headline = ReadString();
      var extraData = ReadString();

      eWrapper.tickNews(tickerId, timeStamp, providerCode, articleId, headline, extraData);
    }

    private void SymbolSamplesEvent()
    {
      var reqId = ReadInt();
      var contractDescriptions = Array.Empty<ContractDescription>();
      var nContractDescriptions = ReadInt();

      if (nContractDescriptions > 0)
      {
        Array.Resize(ref contractDescriptions, nContractDescriptions);

        for (var i = 0; i < nContractDescriptions; ++i)
        {
          // read contract fields
          var contract = new Contract
          {
            ConId = ReadInt(),
            Symbol = ReadString(),
            SecType = ReadString(),
            PrimaryExch = ReadString(),
            Currency = ReadString()
          };

          // read derivative sec types list
          var derivativeSecTypes = Array.Empty<string>();
          var nDerivativeSecTypes = ReadInt();
          if (nDerivativeSecTypes > 0)
          {
            Array.Resize(ref derivativeSecTypes, nDerivativeSecTypes);
            for (var j = 0; j < nDerivativeSecTypes; ++j)
            {
              derivativeSecTypes[j] = ReadString();
            }
          }
          if (serverVersion >= MinServerVer.MIN_SERVER_VER_BOND_ISSUERID)
          {
            contract.Description = ReadString();
            contract.IssuerId = ReadString();
          }

          var contractDescription = new ContractDescription(contract, derivativeSecTypes);
          contractDescriptions[i] = contractDescription;
        }
      }

      eWrapper.symbolSamples(reqId, contractDescriptions);
    }

    private void FamilyCodesEvent()
    {
      var familyCodes = Array.Empty<FamilyCode>();
      var nFamilyCodes = ReadInt();

      if (nFamilyCodes > 0)
      {
        Array.Resize(ref familyCodes, nFamilyCodes);

        for (var i = 0; i < nFamilyCodes; ++i)
        {
          familyCodes[i] = new FamilyCode(ReadString(), ReadString());
        }
      }

      eWrapper.familyCodes(familyCodes);
    }

    private void MktDepthExchangesEvent()
    {
      var depthMktDataDescriptions = Array.Empty<DepthMktDataDescription>();
      var nDescriptions = ReadInt();

      if (nDescriptions > 0)
      {
        Array.Resize(ref depthMktDataDescriptions, nDescriptions);

        for (var i = 0; i < nDescriptions; i++)
        {
          if (serverVersion >= MinServerVer.SERVICE_DATA_TYPE)
          {
            depthMktDataDescriptions[i] = new DepthMktDataDescription(ReadString(), ReadString(), ReadString(), ReadString(), ReadIntMax());
          }
          else
          {
            depthMktDataDescriptions[i] = new DepthMktDataDescription(ReadString(), ReadString(), "", ReadBoolFromInt() ? "Deep2" : "Deep", int.MaxValue);
          }
        }
      }

      eWrapper.mktDepthExchanges(depthMktDataDescriptions);
    }

    private void SoftDollarTierEvent()
    {
      var reqId = ReadInt();
      var nTiers = ReadInt();
      var tiers = new SoftDollarTier[nTiers];

      for (var i = 0; i < nTiers; i++)
      {
        tiers[i] = new SoftDollarTier(ReadString(), ReadString(), ReadString());
      }

      eWrapper.softDollarTiers(reqId, tiers);
    }

    private void SecurityDefinitionOptionParameterEndEvent()
    {
      var reqId = ReadInt();

      eWrapper.securityDefinitionOptionParameterEnd(reqId);
    }

    private void SecurityDefinitionOptionParameterEvent()
    {
      var reqId = ReadInt();
      var exchange = ReadString();
      var underlyingConId = ReadInt();
      var tradingClass = ReadString();
      var multiplier = ReadString();
      var expirationsSize = ReadInt();
      var expirations = new HashSet<string>();
      var strikes = new HashSet<double>();

      for (var i = 0; i < expirationsSize; i++)
      {
        expirations.Add(ReadString());
      }

      var strikesSize = ReadInt();

      for (var i = 0; i < strikesSize; i++)
      {
        strikes.Add(ReadDouble());
      }

      eWrapper.securityDefinitionOptionParameter(reqId, exchange, underlyingConId, tradingClass, multiplier, expirations, strikes);
    }

    private void DisplayGroupUpdatedEvent()
    {
      _ = ReadInt(); //msgVersion
      var reqId = ReadInt();
      var contractInfo = ReadString();

      eWrapper.displayGroupUpdated(reqId, contractInfo);
    }

    private void DisplayGroupListEvent()
    {
      _ = ReadInt(); //msgVersion
      var reqId = ReadInt();
      var groups = ReadString();

      eWrapper.displayGroupList(reqId, groups);
    }

    private void VerifyCompletedEvent()
    {
      _ = ReadInt(); //msgVersion
      var isSuccessful = string.Equals(ReadString(), "true", StringComparison.OrdinalIgnoreCase);
      var errorText = ReadString();

      eWrapper.verifyCompleted(isSuccessful, errorText);
    }

    private void VerifyMessageApiEvent()
    {
      _ = ReadInt(); //msgVersion
      var apiData = ReadString();

      eWrapper.verifyMessageAPI(apiData);
    }

    private void VerifyAndAuthCompletedEvent()
    {
      _ = ReadInt(); //msgVersion
      var isSuccessful = string.Equals(ReadString(), "true", StringComparison.OrdinalIgnoreCase);
      var errorText = ReadString();

      eWrapper.verifyAndAuthCompleted(isSuccessful, errorText);
    }

    private void VerifyAndAuthMessageApiEvent()
    {
      _ = ReadInt(); //msgVersion
      var apiData = ReadString();
      var xyzChallenge = ReadString();

      eWrapper.verifyAndAuthMessageAPI(apiData, xyzChallenge);
    }

    private void TickPriceEvent()
    {
      var msgVersion = ReadInt();
      var requestId = ReadInt();
      var tickType = ReadInt();
      var price = ReadDouble();
      decimal size = 0;

      if (msgVersion >= 2) size = ReadDecimal();

      var attr = new TickAttrib();

      if (msgVersion >= 3)
      {
        var attrMask = ReadInt();

        attr.CanAutoExecute = attrMask == 1;

        if (serverVersion >= MinServerVer.PAST_LIMIT)
        {
          var mask = new BitMask(attrMask);

          attr.CanAutoExecute = mask[0];
          attr.PastLimit = mask[1];

          if (serverVersion >= MinServerVer.PRE_OPEN_BID_ASK) attr.PreOpen = mask[2];
        }
      }


      eWrapper.tickPrice(requestId, tickType, price, attr);

      if (msgVersion < 2) return;
      var sizeTickType = -1; //not a tick
      switch (tickType)
      {
        case TickType.BID:
          sizeTickType = TickType.BID_SIZE;
          break;
        case TickType.ASK:
          sizeTickType = TickType.ASK_SIZE;
          break;
        case TickType.LAST:
          sizeTickType = TickType.LAST_SIZE;
          break;
        case TickType.DELAYED_BID:
          sizeTickType = TickType.DELAYED_BID_SIZE;
          break;
        case TickType.DELAYED_ASK:
          sizeTickType = TickType.DELAYED_ASK_SIZE;
          break;
        case TickType.DELAYED_LAST:
          sizeTickType = TickType.DELAYED_LAST_SIZE;
          break;
      }
      if (sizeTickType != -1) eWrapper.tickSize(requestId, sizeTickType, size);
    }

    private void TickSizeEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var tickType = ReadInt();
      var size = ReadDecimal();
      eWrapper.tickSize(requestId, tickType, size);
    }

    private void TickStringEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var tickType = ReadInt();
      var value = ReadString();
      eWrapper.tickString(requestId, tickType, value);
    }

    private void TickGenericEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var tickType = ReadInt();
      var value = ReadDouble();
      eWrapper.tickGeneric(requestId, tickType, value);
    }

    private void TickEFPEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var tickType = ReadInt();
      var basisPoints = ReadDouble();
      var formattedBasisPoints = ReadString();
      var impliedFuturesPrice = ReadDouble();
      var holdDays = ReadInt();
      var futureLastTradeDate = ReadString();
      var dividendImpact = ReadDouble();
      var dividendsToLastTradeDate = ReadDouble();
      eWrapper.tickEFP(requestId, tickType, basisPoints, formattedBasisPoints, impliedFuturesPrice, holdDays, futureLastTradeDate, dividendImpact, dividendsToLastTradeDate);
    }

    private void TickSnapshotEndEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      eWrapper.tickSnapshotEnd(requestId);
    }

    private void ErrorEvent()
    {
      var msgVersion = ReadInt();
      if (msgVersion < 2)
      {
        var msg = ReadString();
        eWrapper.error(msg);
      }
      else
      {
        var id = ReadInt();
        var errorCode = ReadInt();
        var errorMsg = serverVersion >= MinServerVer.ENCODE_MSG_ASCII7 ? Regex.Unescape(ReadString()) : ReadString();
        var advancedOrderRejectJson = "";
        if (serverVersion >= MinServerVer.ADVANCED_ORDER_REJECT)
        {
          var tempStr = ReadString();
          if (!Util.StringIsEmpty(tempStr)) advancedOrderRejectJson = Regex.Unescape(tempStr);
        }
        eWrapper.error(id, errorCode, errorMsg, advancedOrderRejectJson);
      }
    }

    private void CurrentTimeEvent()
    {
      _ = ReadInt(); //msgVersion
      var time = ReadLong();
      eWrapper.currentTime(time);
    }

    private void ManagedAccountsEvent()
    {
      _ = ReadInt(); //msgVersion
      var accountsList = ReadString();
      eWrapper.managedAccounts(accountsList);
    }

    private void NextValidIdEvent()
    {
      _ = ReadInt(); //msgVersion
      var orderId = ReadInt();
      eWrapper.nextValidId(orderId);
    }

    private void DeltaNeutralValidationEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var deltaNeutralContract = new DeltaNeutralContract
      {
        ConId = ReadInt(),
        Delta = ReadDouble(),
        Price = ReadDouble()
      };
      eWrapper.deltaNeutralValidation(requestId, deltaNeutralContract);
    }

    private void TickOptionComputationEvent()
    {
      var msgVersion = serverVersion >= MinServerVer.PRICE_BASED_VOLATILITY ? int.MaxValue : ReadInt();

      var requestId = ReadInt();
      var tickType = ReadInt();
      var tickAttrib = int.MaxValue;
      if (serverVersion >= MinServerVer.PRICE_BASED_VOLATILITY)
      {
        tickAttrib = ReadInt();
      }
      var impliedVolatility = ReadDouble();
      if (impliedVolatility.Equals(-1))
      { // -1 is the "not yet computed" indicator
        impliedVolatility = double.MaxValue;
      }
      var delta = ReadDouble();
      if (delta.Equals(-2))
      { // -2 is the "not yet computed" indicator
        delta = double.MaxValue;
      }
      var optPrice = double.MaxValue;
      var pvDividend = double.MaxValue;
      var gamma = double.MaxValue;
      var vega = double.MaxValue;
      var theta = double.MaxValue;
      var undPrice = double.MaxValue;
      if (msgVersion >= 6 || tickType == TickType.MODEL_OPTION || tickType == TickType.DELAYED_MODEL_OPTION)
      {
        optPrice = ReadDouble();
        if (optPrice.Equals(-1))
        { // -1 is the "not yet computed" indicator
          optPrice = double.MaxValue;
        }
        pvDividend = ReadDouble();
        if (pvDividend.Equals(-1))
        { // -1 is the "not yet computed" indicator
          pvDividend = double.MaxValue;
        }
      }
      if (msgVersion >= 6)
      {
        gamma = ReadDouble();
        if (gamma.Equals(-2))
        { // -2 is the "not yet computed" indicator
          gamma = double.MaxValue;
        }
        vega = ReadDouble();
        if (vega.Equals(-2))
        { // -2 is the "not yet computed" indicator
          vega = double.MaxValue;
        }
        theta = ReadDouble();
        if (theta.Equals(-2))
        { // -2 is the "not yet computed" indicator
          theta = double.MaxValue;
        }
        undPrice = ReadDouble();
        if (undPrice.Equals(-1))
        { // -1 is the "not yet computed" indicator
          undPrice = double.MaxValue;
        }
      }

      eWrapper.tickOptionComputation(requestId, tickType, tickAttrib, impliedVolatility, delta, optPrice, pvDividend, gamma, vega, theta, undPrice);
    }

    private void AccountSummaryEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var account = ReadString();
      var tag = ReadString();
      var value = ReadString();
      var currency = ReadString();
      eWrapper.accountSummary(requestId, account, tag, value, currency);
    }

    private void AccountSummaryEndEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      eWrapper.accountSummaryEnd(requestId);
    }

    private void AccountValueEvent()
    {
      var msgVersion = ReadInt();
      var key = ReadString();
      var value = ReadString();
      var currency = ReadString();
      string accountName = null;
      if (msgVersion >= 2) accountName = ReadString();
      eWrapper.updateAccountValue(key, value, currency, accountName);
    }

    private void BondContractDetailsEvent()
    {
      var msgVersion = 6;
      if (serverVersion < MinServerVer.SIZE_RULES) msgVersion = ReadInt();
      var requestId = -1;
      if (msgVersion >= 3) requestId = ReadInt();

      var contract = new ContractDetails();

      contract.Contract.Symbol = ReadString();
      contract.Contract.SecType = ReadString();
      contract.Cusip = ReadString();
      contract.Coupon = ReadDouble();
      readLastTradeDate(contract, true);
      contract.IssueDate = ReadString();
      contract.Ratings = ReadString();
      contract.BondType = ReadString();
      contract.CouponType = ReadString();
      contract.Convertible = ReadBoolFromInt();
      contract.Callable = ReadBoolFromInt();
      contract.Putable = ReadBoolFromInt();
      contract.DescAppend = ReadString();
      contract.Contract.Exchange = ReadString();
      contract.Contract.Currency = ReadString();
      contract.MarketName = ReadString();
      contract.Contract.TradingClass = ReadString();
      contract.Contract.ConId = ReadInt();
      contract.MinTick = ReadDouble();
      if (serverVersion >= MinServerVer.MD_SIZE_MULTIPLIER && serverVersion < MinServerVer.SIZE_RULES) ReadInt(); // MdSizeMultiplier - not used anymore
      contract.OrderTypes = ReadString();
      contract.ValidExchanges = ReadString();
      if (msgVersion >= 2)
      {
        contract.NextOptionDate = ReadString();
        contract.NextOptionType = ReadString();
        contract.NextOptionPartial = ReadBoolFromInt();
        contract.Notes = ReadString();
      }
      if (msgVersion >= 4) contract.LongName = ReadString();
      if (msgVersion >= 6)
      {
        contract.EvRule = ReadString();
        contract.EvMultiplier = ReadDouble();
      }
      if (msgVersion >= 5)
      {
        var secIdListCount = ReadInt();
        if (secIdListCount > 0)
        {
          contract.SecIdList = new List<TagValue>();
          for (var i = 0; i < secIdListCount; ++i)
          {
            var tagValue = new TagValue
            {
              Tag = ReadString(),
              Value = ReadString()
            };
            contract.SecIdList.Add(tagValue);
          }
        }
      }
      if (serverVersion >= MinServerVer.AGG_GROUP) contract.AggGroup = ReadInt();
      if (serverVersion >= MinServerVer.MARKET_RULES) contract.MarketRuleIds = ReadString();
      if (serverVersion >= MinServerVer.SIZE_RULES)
      {
        contract.MinSize = ReadDecimal();
        contract.SizeIncrement = ReadDecimal();
        contract.SuggestedSizeIncrement = ReadDecimal();
      }

      eWrapper.bondContractDetails(requestId, contract);
    }

    private void PortfolioValueEvent()
    {
      var msgVersion = ReadInt();
      var contract = new Contract();
      if (msgVersion >= 6) contract.ConId = ReadInt();
      contract.Symbol = ReadString();
      contract.SecType = ReadString();
      contract.LastTradeDateOrContractMonth = ReadString();
      contract.Strike = ReadDouble();
      contract.Right = ReadString();
      if (msgVersion >= 7)
      {
        contract.Multiplier = ReadString();
        contract.PrimaryExch = ReadString();
      }
      contract.Currency = ReadString();
      if (msgVersion >= 2) contract.LocalSymbol = ReadString();
      if (msgVersion >= 8) contract.TradingClass = ReadString();

      var position = ReadDecimal();
      var marketPrice = ReadDouble();
      var marketValue = ReadDouble();
      var averageCost = 0.0;
      var unrealizedPNL = 0.0;
      var realizedPNL = 0.0;
      if (msgVersion >= 3)
      {
        averageCost = ReadDouble();
        unrealizedPNL = ReadDouble();
        realizedPNL = ReadDouble();
      }

      string accountName = null;
      if (msgVersion >= 4) accountName = ReadString();

      if (msgVersion == 6 && serverVersion == 39) contract.PrimaryExch = ReadString();

      eWrapper.updatePortfolio(contract, position, marketPrice, marketValue, averageCost, unrealizedPNL, realizedPNL, accountName);
    }

    private void AccountUpdateTimeEvent()
    {
      _ = ReadInt(); //msgVersion
      var timestamp = ReadString();
      eWrapper.updateAccountTime(timestamp);
    }

    private void AccountDownloadEndEvent()
    {
      _ = ReadInt(); //msgVersion
      var account = ReadString();
      eWrapper.accountDownloadEnd(account);
    }

    private void OrderStatusEvent()
    {
      var msgVersion = serverVersion >= MinServerVer.MARKET_CAP_PRICE ? int.MaxValue : ReadInt();
      var id = ReadInt();
      var status = ReadString();
      var filled = ReadDecimal();
      var remaining = ReadDecimal();
      var avgFillPrice = ReadDouble();

      var permId = 0;
      if (msgVersion >= 2) permId = ReadInt();

      var parentId = 0;
      if (msgVersion >= 3) parentId = ReadInt();

      double lastFillPrice = 0;
      if (msgVersion >= 4) lastFillPrice = ReadDouble();

      var clientId = 0;
      if (msgVersion >= 5) clientId = ReadInt();

      string whyHeld = null;
      if (msgVersion >= 6) whyHeld = ReadString();

      var mktCapPrice = double.MaxValue;

      if (serverVersion >= MinServerVer.MARKET_CAP_PRICE) mktCapPrice = ReadDouble();

      eWrapper.orderStatus(id, status, filled, remaining, avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld, mktCapPrice);
    }

    private void OpenOrderEvent()
    {
      var msgVersion = serverVersion < MinServerVer.ORDER_CONTAINER ? ReadInt() : serverVersion;

      var contract = new Contract();
      var order = new Order();
      var orderState = new OrderState();
      var eOrderDecoder = new EOrderDecoder(this, contract, order, orderState, msgVersion, serverVersion);

      // read order id
      eOrderDecoder.readOrderId();

      // read contract fields
      eOrderDecoder.readContractFields();

      // read order fields
      eOrderDecoder.readAction();
      eOrderDecoder.readTotalQuantity();
      eOrderDecoder.readOrderType();
      eOrderDecoder.readLmtPrice();
      eOrderDecoder.readAuxPrice();
      eOrderDecoder.readTIF();
      eOrderDecoder.readOcaGroup();
      eOrderDecoder.readAccount();
      eOrderDecoder.readOpenClose();
      eOrderDecoder.readOrigin();
      eOrderDecoder.readOrderRef();
      eOrderDecoder.readClientId();
      eOrderDecoder.readPermId();
      eOrderDecoder.readOutsideRth();
      eOrderDecoder.readHidden();
      eOrderDecoder.readDiscretionaryAmount();
      eOrderDecoder.readGoodAfterTime();
      eOrderDecoder.skipSharesAllocation();
      eOrderDecoder.readFAParams();
      eOrderDecoder.readModelCode();
      eOrderDecoder.readGoodTillDate();
      eOrderDecoder.readRule80A();
      eOrderDecoder.readPercentOffset();
      eOrderDecoder.readSettlingFirm();
      eOrderDecoder.readShortSaleParams();
      eOrderDecoder.readAuctionStrategy();
      eOrderDecoder.readBoxOrderParams();
      eOrderDecoder.readPegToStkOrVolOrderParams();
      eOrderDecoder.readDisplaySize();
      eOrderDecoder.readOldStyleOutsideRth();
      eOrderDecoder.readBlockOrder();
      eOrderDecoder.readSweepToFill();
      eOrderDecoder.readAllOrNone();
      eOrderDecoder.readMinQty();
      eOrderDecoder.readOcaType();
      eOrderDecoder.skipETradeOnly();
      eOrderDecoder.skipFirmQuoteOnly();
      eOrderDecoder.skipNbboPriceCap();
      eOrderDecoder.readParentId();
      eOrderDecoder.readTriggerMethod();
      eOrderDecoder.readVolOrderParams(true);
      eOrderDecoder.readTrailParams();
      eOrderDecoder.readBasisPoints();
      eOrderDecoder.readComboLegs();
      eOrderDecoder.readSmartComboRoutingParams();
      eOrderDecoder.readScaleOrderParams();
      eOrderDecoder.readHedgeParams();
      eOrderDecoder.readOptOutSmartRouting();
      eOrderDecoder.readClearingParams();
      eOrderDecoder.readNotHeld();
      eOrderDecoder.readDeltaNeutral();
      eOrderDecoder.readAlgoParams();
      eOrderDecoder.readSolicited();
      eOrderDecoder.readWhatIfInfoAndCommission();
      eOrderDecoder.readVolRandomizeFlags();
      eOrderDecoder.readPegToBenchParams();
      eOrderDecoder.readConditions();
      eOrderDecoder.readAdjustedOrderParams();
      eOrderDecoder.readSoftDollarTier();
      eOrderDecoder.readCashQty();
      eOrderDecoder.readDontUseAutoPriceForHedge();
      eOrderDecoder.readIsOmsContainer();
      eOrderDecoder.readDiscretionaryUpToLimitPrice();
      eOrderDecoder.readUsePriceMgmtAlgo();
      eOrderDecoder.readDuration();
      eOrderDecoder.readPostToAts();
      eOrderDecoder.readAutoCancelParent(MinServerVer.AUTO_CANCEL_PARENT);
      eOrderDecoder.readPegBestPegMidOrderAttributes();
      eOrderDecoder.readCustomerAccount();
      eOrderDecoder.readProfessionalCustomer();
      eOrderDecoder.readBondAccruedInterest();

      eWrapper.openOrder(order.OrderId, contract, order, orderState);
    }

    private void OpenOrderEndEvent()
    {
      _ = ReadInt(); //msgVersion
      eWrapper.openOrderEnd();
    }

    private void ContractDataEvent()
    {
      var msgVersion = 8;
      if (serverVersion < MinServerVer.SIZE_RULES) msgVersion = ReadInt();
      var requestId = -1;
      if (msgVersion >= 3) requestId = ReadInt();
      var contract = new ContractDetails();
      contract.Contract.Symbol = ReadString();
      contract.Contract.SecType = ReadString();
      readLastTradeDate(contract, false);
      if (serverVersion >= MinServerVer.MIN_SERVER_VER_LAST_TRADE_DATE) contract.Contract.LastTradeDate = ReadString();
      contract.Contract.Strike = ReadDouble();
      contract.Contract.Right = ReadString();
      contract.Contract.Exchange = ReadString();
      contract.Contract.Currency = ReadString();
      contract.Contract.LocalSymbol = ReadString();
      contract.MarketName = ReadString();
      contract.Contract.TradingClass = ReadString();
      contract.Contract.ConId = ReadInt();
      contract.MinTick = ReadDouble();
      if (serverVersion >= MinServerVer.MD_SIZE_MULTIPLIER && serverVersion < MinServerVer.SIZE_RULES) ReadInt(); // MdSizeMultiplier - not used anymore
      contract.Contract.Multiplier = ReadString();
      contract.OrderTypes = ReadString();
      contract.ValidExchanges = ReadString();
      if (msgVersion >= 2) contract.PriceMagnifier = ReadInt();
      if (msgVersion >= 4) contract.UnderConId = ReadInt();
      if (msgVersion >= 5)
      {
        contract.LongName = serverVersion >= MinServerVer.ENCODE_MSG_ASCII7 ? Regex.Unescape(ReadString()) : ReadString();
        contract.Contract.PrimaryExch = ReadString();
      }
      if (msgVersion >= 6)
      {
        contract.ContractMonth = ReadString();
        contract.Industry = ReadString();
        contract.Category = ReadString();
        contract.Subcategory = ReadString();
        contract.TimeZoneId = ReadString();
        contract.TradingHours = ReadString();
        contract.LiquidHours = ReadString();
      }
      if (msgVersion >= 8)
      {
        contract.EvRule = ReadString();
        contract.EvMultiplier = ReadDouble();
      }
      if (msgVersion >= 7)
      {
        var secIdListCount = ReadInt();
        if (secIdListCount > 0)
        {
          contract.SecIdList = new List<TagValue>(secIdListCount);
          for (var i = 0; i < secIdListCount; ++i)
          {
            var tagValue = new TagValue
            {
              Tag = ReadString(),
              Value = ReadString()
            };
            contract.SecIdList.Add(tagValue);
          }
        }
      }
      if (serverVersion >= MinServerVer.AGG_GROUP) contract.AggGroup = ReadInt();
      if (serverVersion >= MinServerVer.UNDERLYING_INFO)
      {
        contract.UnderSymbol = ReadString();
        contract.UnderSecType = ReadString();
      }
      if (serverVersion >= MinServerVer.MARKET_RULES) contract.MarketRuleIds = ReadString();
      if (serverVersion >= MinServerVer.REAL_EXPIRATION_DATE) contract.RealExpirationDate = ReadString();
      if (serverVersion >= MinServerVer.STOCK_TYPE) contract.StockType = ReadString();
      if (serverVersion >= MinServerVer.FRACTIONAL_SIZE_SUPPORT && serverVersion < MinServerVer.SIZE_RULES) ReadDecimal(); // SizeMinTick - not used anymore
      if (serverVersion >= MinServerVer.SIZE_RULES)
      {
        contract.MinSize = ReadDecimal();
        contract.SizeIncrement = ReadDecimal();
        contract.SuggestedSizeIncrement = ReadDecimal();
      }
      if (serverVersion >= MinServerVer.MIN_SERVER_VER_FUND_DATA_FIELDS && contract.Contract.SecType == "FUND")
      {
        contract.FundName = ReadString();
        contract.FundFamily = ReadString();
        contract.FundType = ReadString();
        contract.FundFrontLoad = ReadString();
        contract.FundBackLoad = ReadString();
        contract.FundBackLoadTimeInterval = ReadString();
        contract.FundManagementFee = ReadString();
        contract.FundClosed = ReadBoolFromInt();
        contract.FundClosedForNewInvestors = ReadBoolFromInt();
        contract.FundClosedForNewMoney = ReadBoolFromInt();
        contract.FundNotifyAmount = ReadString();
        contract.FundMinimumInitialPurchase = ReadString();
        contract.FundSubsequentMinimumPurchase = ReadString();
        contract.FundBlueSkyStates = ReadString();
        contract.FundBlueSkyTerritories = ReadString();
        contract.FundDistributionPolicyIndicator = CFundDistributionPolicyIndicator.getFundDistributionPolicyIndicator(ReadString());
        contract.FundAssetType = CFundAssetType.getFundAssetType(ReadString());
      }

      if (serverVersion >= MinServerVer.MIN_SERVER_VER_INELIGIBILITY_REASONS)
      {
        var ineligibilityReasonCount = ReadInt();
        if (ineligibilityReasonCount > 0)
        {
          contract.IneligibilityReasonList = new List<IneligibilityReason>();
          for (var i = 0; i < ineligibilityReasonCount; ++i)
          {
            var ineligibilityReason = new IneligibilityReason
            {
              Id = ReadString(),
              Description = ReadString()
            };
            contract.IneligibilityReasonList.Add(ineligibilityReason);
          }
        }
      }

      eWrapper.contractDetails(requestId, contract);
    }


    private void ContractDataEndEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      eWrapper.contractDetailsEnd(requestId);
    }

    private void ExecutionDataEvent()
    {
      var msgVersion = serverVersion;

      if (serverVersion < MinServerVer.LAST_LIQUIDITY) msgVersion = ReadInt();

      var requestId = -1;
      if (msgVersion >= 7) requestId = ReadInt();
      var orderId = ReadInt();
      var contract = new Contract();
      if (msgVersion >= 5) contract.ConId = ReadInt();
      contract.Symbol = ReadString();
      contract.SecType = ReadString();
      contract.LastTradeDateOrContractMonth = ReadString();
      contract.Strike = ReadDouble();
      contract.Right = ReadString();
      if (msgVersion >= 9) contract.Multiplier = ReadString();
      contract.Exchange = ReadString();
      contract.Currency = ReadString();
      contract.LocalSymbol = ReadString();
      if (msgVersion >= 10) contract.TradingClass = ReadString();

      var exec = new Execution
      {
        OrderId = orderId,
        ExecId = ReadString(),
        Time = ReadString(),
        AcctNumber = ReadString(),
        Exchange = ReadString(),
        Side = ReadString(),
        Shares = ReadDecimal(),
        Price = ReadDouble()
      };
      if (msgVersion >= 2) exec.PermId = ReadInt();
      if (msgVersion >= 3) exec.ClientId = ReadInt();
      if (msgVersion >= 4) exec.Liquidation = ReadInt();
      if (msgVersion >= 6)
      {
        exec.CumQty = ReadDecimal();
        exec.AvgPrice = ReadDouble();
      }
      if (msgVersion >= 8) exec.OrderRef = ReadString();
      if (msgVersion >= 9)
      {
        exec.EvRule = ReadString();
        exec.EvMultiplier = ReadDouble();
      }
      if (serverVersion >= MinServerVer.MODELS_SUPPORT) exec.ModelCode = ReadString();

      if (serverVersion >= MinServerVer.LAST_LIQUIDITY) exec.LastLiquidity = new Liquidity(ReadInt());

      if (serverVersion >= MinServerVer.MIN_SERVER_VER_PENDING_PRICE_REVISION) exec.PendingPriceRevision = ReadBoolFromInt();

      eWrapper.execDetails(requestId, contract, exec);
    }

    private void ExecutionDataEndEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      eWrapper.execDetailsEnd(requestId);
    }

    private void CommissionReportEvent()
    {
      _ = ReadInt(); //msgVersion
      var commissionReport = new CommissionReport
      {
        ExecId = ReadString(),
        Commission = ReadDouble(),
        Currency = ReadString(),
        RealizedPNL = ReadDouble(),
        Yield = ReadDouble(),
        YieldRedemptionDate = ReadInt()
      };
      eWrapper.commissionReport(commissionReport);
    }

    private void FundamentalDataEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var fundamentalData = ReadString();
      eWrapper.fundamentalData(requestId, fundamentalData);
    }

    private void HistoricalDataEvent()
    {
      var msgVersion = int.MaxValue;

      if (serverVersion < MinServerVer.SYNT_REALTIME_BARS) msgVersion = ReadInt();

      var requestId = ReadInt();
      var startDateStr = "";
      var endDateStr = "";

      if (msgVersion >= 2)
      {
        startDateStr = ReadString();
        endDateStr = ReadString();
      }

      var itemCount = ReadInt();

      for (var ctr = 0; ctr < itemCount; ctr++)
      {
        var date = ReadString();
        var open = ReadDouble();
        var high = ReadDouble();
        var low = ReadDouble();
        var close = ReadDouble();
        var volume = ReadDecimal();
        var WAP = ReadDecimal();

        if (serverVersion < MinServerVer.SYNT_REALTIME_BARS)
        {
          /*string hasGaps = */
          ReadString();
        }

        var barCount = -1;

        if (msgVersion >= 3) barCount = ReadInt();

        eWrapper.historicalData(requestId, new Bar(date, open, high, low, close, volume, barCount, WAP));
      }

      // send end of dataset marker.
      eWrapper.historicalDataEnd(requestId, startDateStr, endDateStr);
    }

    private void MarketDataTypeEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var marketDataType = ReadInt();
      eWrapper.marketDataType(requestId, marketDataType);
    }

    private void MarketDepthEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var position = ReadInt();
      var operation = ReadInt();
      var side = ReadInt();
      var price = ReadDouble();
      var size = ReadDecimal();
      eWrapper.updateMktDepth(requestId, position, operation, side, price, size);
    }

    private void MarketDepthL2Event()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var position = ReadInt();
      var marketMaker = ReadString();
      var operation = ReadInt();
      var side = ReadInt();
      var price = ReadDouble();
      var size = ReadDecimal();

      var isSmartDepth = false;
      if (serverVersion >= MinServerVer.SMART_DEPTH) isSmartDepth = ReadBoolFromInt();

      eWrapper.updateMktDepthL2(requestId, position, marketMaker, operation, side, price, size, isSmartDepth);
    }

    private void NewsBulletinsEvent()
    {
      _ = ReadInt(); //msgVersion
      var newsMsgId = ReadInt();
      var newsMsgType = ReadInt();
      var newsMessage = ReadString();
      var originatingExch = ReadString();
      eWrapper.updateNewsBulletin(newsMsgId, newsMsgType, newsMessage, originatingExch);
    }

    private void PositionEvent()
    {
      var msgVersion = ReadInt();
      var account = ReadString();
      var contract = new Contract
      {
        ConId = ReadInt(),
        Symbol = ReadString(),
        SecType = ReadString(),
        LastTradeDateOrContractMonth = ReadString(),
        Strike = ReadDouble(),
        Right = ReadString(),
        Multiplier = ReadString(),
        Exchange = ReadString(),
        Currency = ReadString(),
        LocalSymbol = ReadString()
      };
      if (msgVersion >= 2) contract.TradingClass = ReadString();

      var pos = ReadDecimal();
      double avgCost = 0;
      if (msgVersion >= 3) avgCost = ReadDouble();
      eWrapper.position(account, contract, pos, avgCost);
    }

    private void PositionEndEvent()
    {
      _ = ReadInt(); //msgVersion
      eWrapper.positionEnd();
    }

    private void RealTimeBarsEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var time = ReadLong();
      var open = ReadDouble();
      var high = ReadDouble();
      var low = ReadDouble();
      var close = ReadDouble();
      var volume = ReadDecimal();
      var wap = ReadDecimal();
      var count = ReadInt();
      eWrapper.realtimeBar(requestId, time, open, high, low, close, volume, wap, count);
    }

    private void ScannerParametersEvent()
    {
      _ = ReadInt(); //msgVersion
      var xml = ReadString();
      eWrapper.scannerParameters(xml);
    }

    private void ScannerDataEvent()
    {
      var msgVersion = ReadInt();
      var requestId = ReadInt();
      var numberOfElements = ReadInt();
      for (var i = 0; i < numberOfElements; i++)
      {
        var rank = ReadInt();
        var conDet = new ContractDetails();
        if (msgVersion >= 3) conDet.Contract.ConId = ReadInt();
        conDet.Contract.Symbol = ReadString();
        conDet.Contract.SecType = ReadString();
        conDet.Contract.LastTradeDateOrContractMonth = ReadString();
        conDet.Contract.Strike = ReadDouble();
        conDet.Contract.Right = ReadString();
        conDet.Contract.Exchange = ReadString();
        conDet.Contract.Currency = ReadString();
        conDet.Contract.LocalSymbol = ReadString();
        conDet.MarketName = ReadString();
        conDet.Contract.TradingClass = ReadString();
        var distance = ReadString();
        var benchmark = ReadString();
        var projection = ReadString();
        string legsStr = null;
        if (msgVersion >= 2) legsStr = ReadString();
        eWrapper.scannerData(requestId, rank, conDet, distance, benchmark, projection, legsStr);
      }
      eWrapper.scannerDataEnd(requestId);
    }

    private void ReceiveFAEvent()
    {
      _ = ReadInt(); //msgVersion
      var faDataType = ReadInt();
      var faData = ReadString();
      eWrapper.receiveFA(faDataType, faData);
    }

    private void PositionMultiEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var account = ReadString();
      var contract = new Contract
      {
        ConId = ReadInt(),
        Symbol = ReadString(),
        SecType = ReadString(),
        LastTradeDateOrContractMonth = ReadString(),
        Strike = ReadDouble(),
        Right = ReadString(),
        Multiplier = ReadString(),
        Exchange = ReadString(),
        Currency = ReadString(),
        LocalSymbol = ReadString(),
        TradingClass = ReadString()
      };
      var pos = ReadDecimal();
      var avgCost = ReadDouble();
      var modelCode = ReadString();
      eWrapper.positionMulti(requestId, account, modelCode, contract, pos, avgCost);
    }

    private void PositionMultiEndEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      eWrapper.positionMultiEnd(requestId);
    }

    private void AccountUpdateMultiEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      var account = ReadString();
      var modelCode = ReadString();
      var key = ReadString();
      var value = ReadString();
      var currency = ReadString();
      eWrapper.accountUpdateMulti(requestId, account, modelCode, key, value, currency);
    }

    private void AccountUpdateMultiEndEvent()
    {
      _ = ReadInt(); //msgVersion
      var requestId = ReadInt();
      eWrapper.accountUpdateMultiEnd(requestId);
    }

    private void ReplaceFAEndEvent()
    {
      var reqId = ReadInt();
      var text = ReadString();
      eWrapper.replaceFAEnd(reqId, text);
    }

    private void ProcessWshMetaData()
    {
      var reqId = ReadInt();
      var dataJson = ReadString();

      eWrapper.wshMetaData(reqId, dataJson);
    }

    private void ProcessWshEventData()
    {
      var reqId = ReadInt();
      var dataJson = ReadString();
      eWrapper.wshEventData(reqId, dataJson);
    }

    private void ProcessHistoricalScheduleEvent()
    {
      var reqId = ReadInt();
      var startDateTime = ReadString();
      var endDateTime = ReadString();
      var timeZone = ReadString();

      var sessionsCount = ReadInt();
      var sessions = new HistoricalSession[sessionsCount];

      for (var i = 0; i < sessionsCount; i++)
      {
        var sessionStartDateTime = ReadString();
        var sessionEndDateTime = ReadString();
        var sessionRefDate = ReadString();

        sessions[i] = new HistoricalSession(sessionStartDateTime, sessionEndDateTime, sessionRefDate);
      }

      eWrapper.historicalSchedule(reqId, startDateTime, endDateTime, timeZone, sessions);
    }

    private void ProcessUserInfoEvent()
    {
      var reqId = ReadInt();
      var whiteBrandingId = ReadString();

      eWrapper.userInfo(reqId, whiteBrandingId);
    }

    public double ReadDouble()
    {
      var doubleAsstring = ReadString();
      if (string.IsNullOrEmpty(doubleAsstring) || doubleAsstring == "0") return 0;
      return double.Parse(doubleAsstring, System.Globalization.NumberFormatInfo.InvariantInfo);
    }

    public double ReadDoubleMax()
    {
      var str = ReadString();
      return string.IsNullOrEmpty(str) ? double.MaxValue : str == Constants.INFINITY_STR ? double.PositiveInfinity : double.Parse(str, System.Globalization.NumberFormatInfo.InvariantInfo);
    }

    public decimal ReadDecimal()
    {
      var str = ReadString();
      return Util.StringToDecimal(str);
    }

    public long ReadLong()
    {
      var longAsstring = ReadString();
      if (string.IsNullOrEmpty(longAsstring) || longAsstring == "0") return 0;
      return long.Parse(longAsstring);
    }

    public int ReadInt()
    {
      var intAsstring = ReadString();
      if (string.IsNullOrEmpty(intAsstring) ||
          intAsstring == "0")
      {
        return 0;
      }
      return int.Parse(intAsstring);
    }

    public int ReadIntMax()
    {
      var str = ReadString();
      return string.IsNullOrEmpty(str) ? int.MaxValue : int.Parse(str);
    }

    public bool ReadBoolFromInt()
    {
      var str = ReadString();
      return str != null && int.Parse(str) != 0;
    }

    public char ReadChar()
    {
      var str = ReadString();
      return str == null ? '\0' : str[0];
    }

    public string ReadString()
    {
      var b = dataReader.ReadByte();

      nDecodedLen++;

      if (b == 0) return null;
      var strBuilder = new StringBuilder();
      strBuilder.Append((char)b);
      while (true)
      {
        b = dataReader.ReadByte();
        if (b == 0) break;
        strBuilder.Append((char)b);
      }

      nDecodedLen += strBuilder.Length;

      return strBuilder.ToString();
    }

    private void readLastTradeDate(ContractDetails contract, bool isBond)
    {
      var lastTradeDateOrContractMonth = ReadString();
      if (lastTradeDateOrContractMonth != null)
      {
        var splitted = lastTradeDateOrContractMonth.Contains("-") ? Regex.Split(lastTradeDateOrContractMonth, "-") : Regex.Split(lastTradeDateOrContractMonth, "\\s+");
        if (splitted.Length > 0)
        {
          if (isBond) contract.Maturity = splitted[0];
          else contract.Contract.LastTradeDateOrContractMonth = splitted[0];
        }
        if (splitted.Length > 1) contract.LastTradeTime = splitted[1];
        if (isBond && splitted.Length > 2) contract.TimeZoneId = splitted[2];
      }
    }
  }
}
