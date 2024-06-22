using DebugByPlatform;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public struct AccountParam
{
    public string currency;         //화폐를 의미하는 영문 대문자 코드
    public double balance;           //주문가능 금액/수량
    public double locked;            //주문 중 묶여있는 금액/수량
    public double avg_buy_price;     //매수평균가
    public string unit_currency;    //평단가 기준 화폐
}

public class AccountParamClass
{
    public double balanceKRW { get; private set; } 
    public double totalConversionKRW { get; private set; }
    public double myProperty { get => balanceKRW + totalConversionKRW; }

    private double[] balanceByMarket = new double[(int)MarketList.MaxCount];
    private double[] ConversionKRWByMarket = new double[(int)MarketList.MaxCount];

    public bool isAcoountInfoSync;

    public void SetAccountParam(AccountParam param)
    {
        balanceKRW = param.balance;

        isAcoountInfoSync = true;
        UIManager.Instance.VisualizationKRWBalance(balanceKRW);
    }

    public void SetBalanceByMarket(MarketList market, double balance)
    {
        balanceByMarket[(int)market] = balance;
        UIManager.Instance.VisualizationTotalKRW(myProperty);
    }

    public void SetUnitPriceByMarket(MarketList market, double price)
    {
        ConversionKRWByMarket[(int)market] = balanceByMarket[(int)market] * price;

        double tempConversionKRW = 0;

        for (int i = 0; i < (int)MarketList.MaxCount; i++) 
        {
            tempConversionKRW += ConversionKRWByMarket[i];
        }

        totalConversionKRW = tempConversionKRW;
        UIManager.Instance.VisualizationTotalKRW(myProperty);
    }

    public void AccountParamSyncEnd()
    {
        isAcoountInfoSync = false;
        TradeManager.Instance.DelayedUpdateAccountInfo(2.0f);
    }
}

public class Accounts : BaseWebRequest<AccountParam>
{
    protected override string additionalUrl { get => "v1/accounts"; }


    public void GetAccountInfo()
    {
        StartCoroutine(Access());
    }


    override protected bool ChkIgnoreCondition(object data)
    {
        return GlobalValue.IGNORE_CURRENCYS.Contains(((AccountParam)data).currency);
    }

    protected override string SetParamsKey(object data)
    {
        return ((AccountParam)data).currency;
    }

    protected override void AfterProcess()
    {
        TradeManager.Instance.accountParam.SetAccountParam(parameters["KRW"]);

        for (int i = 0; i < (int)MarketList.MaxCount; i++)
        {
            MarketList market = (MarketList)i;
            string marketString = market.ToString();

            if (parameters.ContainsKey(marketString))
            {
                UIManager.Instance.VisualizationBalanceByMarket(market, parameters[marketString].balance);
                TradeManager.Instance.accountParam.SetBalanceByMarket(market, parameters[marketString].balance);
            }
            else
            {
                UIManager.Instance.VisualizationBalanceByMarket(market, 0.0f);
                TradeManager.Instance.accountParam.SetBalanceByMarket(market, 0.0f);
            }
        }
    }

    protected override void ErrorProcess()
    {
        AppManager.Instance.TelegramMassage($"자산정보의 갱신이 실패하였습니다.", TelegramBotType.DebugLog);
    }
}

