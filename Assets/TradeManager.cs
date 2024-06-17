using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


public class TradeManager : BaseManager<TradeManager>
{
    private Accounts accountInfo;
    private Trade trade;
    private Order order;
    private LastTrade lastTrade;

    public AccountParamClass accountParam = new AccountParamClass();

    public double balanceKRW { get => accountParam.balanceKRW; }
    public double myProperty { get => accountParam.myProperty; }


    Dictionary<MarketList, TradingParameters> _conditionByMarket = new Dictionary<MarketList, TradingParameters>();
    public Dictionary<MarketList, TradingParameters> conditionByMarket { get => _conditionByMarket; }
    public Dictionary<string, AccountParam> accountInfoByMarket { get => accountInfo.parameters; }

    Dictionary<MarketList, DateTime> _lastTradeTimeByMarket = new Dictionary<MarketList, DateTime>();
    Dictionary<MarketList, DateTime> lastTradeTimeByMarket { get => _lastTradeTimeByMarket; }


    public AccountParam KRWinfo { get => accountInfoByMarket["KRW"]; }


    public bool isReady { get; set; }
    public bool tradeMode { get; private set; }


    protected override void Init()
    {
        accountInfo = CreateComponentObjectInChildrenAndReturn<Accounts>();
        trade = CreateComponentObjectInChildrenAndReturn<Trade>(); 
        order = CreateComponentObjectInChildrenAndReturn<Order>();
        lastTrade = CreateComponentObjectInChildrenAndReturn<LastTrade>();
    }

    private void Start()
    {
        accountInfo.GetAccountInfo();
        CandleManager.Instance.FirstSearchAll();
        SetTradeMode(false);
    }

    public void SetConditionByMarket(Dictionary<MarketList, TradingParameters> datas)
    {
        _conditionByMarket = new Dictionary<MarketList, TradingParameters>(datas);

        if (!_conditionByMarket.Count.Equals((int)MarketList.MaxCount))
        {
            AppManager.Instance.TelegramMassage($"마켓 데이터 수가 부족합니다. 확인을 요망합니다. 마켓 ::: {(int)MarketList.MaxCount} // 데이터 ::: {_conditionByMarket}", TelegramBotType.DebugLog);
        }
    }

    public void SetConditionByMarket(MarketList market, TradingParameters data)
    {
        if (_conditionByMarket == null || !_conditionByMarket.ContainsKey(market))
        {
            AppManager.Instance.TelegramMassage($"해당 마켓의 기존 데이터가 존재하지 않습니다. ::: ({market})", TelegramBotType.DebugLog);
            return;
        }

        _conditionByMarket[market] = new TradingParameters(data);
        AppManager.Instance.TelegramMassage($"해당 마켓의 데이터가 갱신되었습니다. ::: ({market})", TelegramBotType.DebugLog);
    }


    public void SetTradeReady(bool value)
    {
        if (isReady.Equals(value))
        {
            return;
        }

        if (!value && tradeMode)
        {
            SetTradeMode(false);
        }

        isReady = value;
    }

    public bool ChkTradeCondition(MarketList market)
    {
        return _conditionByMarket.ContainsKey(market);
    }

    public void SetTradeMode(bool value)
    {
        if (!isReady && value)
        {
            DebugByPlatform.Debug.LogOnlyEditer("거래에 필요한 데이터가 준비되지 않았습니다.");
            return;
        }

        if (value.Equals(tradeMode))
        {
            UIManager.Instance.VisualizationTradeMode(value);
            return;        
        }

        tradeMode = value;
        UIManager.Instance.VisualizationTradeMode(tradeMode);

        if (tradeMode)
        {
            StartTradeMode();
        }
        else
        {
            StopTradeMode();
        }
    }

    private void StartTradeMode()
    {
        CandleManager.Instance.StartAutoSearchTradeMode();
    }

    private void StopTradeMode()
    {
        CandleManager.Instance.StopAutoSearchTradeMode();
    }

    public void TradeByMarket(MarketList market)
    {
        trade.TradeByMarket(market);
    }

    public void BuyOrder(MarketList market, double unitPrice)
    {
        if (!accountParam.isAcoountInfoSync)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Account 정보를 동기화중입니다.");
            return;
        }
        
        if (balanceKRW / (int)MarketList.MaxCount >= 5000.0f)
        {
            double price;

            if(balanceKRW < myProperty / 5.0f)
            {
                price = balanceKRW - 5000;
            }
            else
            {
                price = myProperty / 5.0f;
            }

            AppManager.Instance.TelegramMassage($"<i>[{TimeManager.Instance.nowTime}]</i>\n<b>[구매시도] <u>{market}[{conditionByMarket[market].tradeTerms}] : {myProperty.ToString("#,###")}KRW</u></b>\nUnitPrice : {unitPrice.ToString("#,##0.0####")}\nOrderBalance : {price.ToString("#,###")}", TelegramBotType.Trade);
            order.BuyOrder(market, price);

            accountParam.AccountParamSyncEnd();
        }
    }

    public void SellOrder(MarketList market, double unitPrice, bool isWin = false)
    {
        if (!accountParam.isAcoountInfoSync)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Account 정보를 동기화중입니다.");
            return;
        }

        double volume = accountInfoByMarket[market.ToString()].balance * 0.9999f;

        AppManager.Instance.TelegramMassage($"<i>[{TimeManager.Instance.nowTime}]</i>\n<b>[판매시도] <u>{market}[{conditionByMarket[market].tradeTerms}] : {myProperty.ToString("#,###")}KRW</u></b>\nWin & Loss : {(accountInfoByMarket[market.ToString()].avg_buy_price < (unitPrice * 0.998f) ? "WIN" : "LOSS")} / UnitPrice : {accountInfoByMarket[market.ToString()].avg_buy_price} >> {unitPrice.ToString("#,##0.0####")}\nOrderVolume : {volume.ToString("#,###")}", TelegramBotType.Trade);        
        order.SellOrder(market, volume);

        accountParam.AccountParamSyncEnd();

    }

    public void SaveDataByAfterBuy(MarketList market)
    {
        //AppManager.Instance.SaveData(market, TradeManager.Instance.conditionByMarket[market]);
    }

    public void SaveDataByAfterSell(MarketList market)
    {
        //AppManager.Instance.SaveData(market, TradeManager.Instance.conditionByMarket[market]);
    }

    IEnumerator updateAccountInfo;

    public void UpdateAccountInfo()
    {
        accountInfo.GetAccountInfo();
    }

    public void DelayedUpdateAccountInfo(float delay)
    {
        if (updateAccountInfo != null)
        {
            StopCoroutine(updateAccountInfo);
            updateAccountInfo = null;
        }

        updateAccountInfo = UpdateAccountInfo(delay);
        StartCoroutine(updateAccountInfo);
    }

    private IEnumerator UpdateAccountInfo(float delay)
    {
        yield return new WaitForSeconds(delay);
        accountInfo.GetAccountInfo();
        updateAccountInfo = null;
    }
}
