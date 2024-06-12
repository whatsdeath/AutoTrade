using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;


[Serializable]
public class TradingParameters
{
    public string name;

    public int stochasticK;
    public int stochasticD;
    public int stochasticStrength;

    public int rsiStrength;

    public int tradePriceEMALength;
    public float tradePriceConditionMul;

    public TradingParameters() { }
    public TradingParameters(TradingParameters other)
    {
        name = other.name;

        stochasticK = other.stochasticK;
        stochasticD = other.stochasticD;
        stochasticStrength = other.stochasticStrength;

        rsiStrength = other.rsiStrength;

        tradePriceEMALength = other.tradePriceEMALength;
        tradePriceConditionMul = other.tradePriceConditionMul;        
    }
    /*
    public MarketList market { get => Enum.Parse<MarketList>(name); }
    public string KRWmarket { get => $"KRW-{name}"; }

    public int stochasticK { get => stochasticK; }
    public int stochasticD { get => stochasticD; }
    public int stochasticStrength { get => stochasticStrength; }

    public int rsiStrength { get => rsiStrength; }

    public int macdShort { get => macdShort; }
    public int macdLong { get => macdLong; }
    public int macdSignal { get => macdSignal; }

    public float overBuy { get => overBuy; }
    public float overSell { get => overSell; }
    public float guideRsi { get => guideRsi; }

    public double profitRate { get => profitRate; }*/
}


public class Trade : MonoBehaviour
{
    private Dictionary<string, AccountParam> accountInfoByMarket { get => TradeManager.Instance.accountInfoByMarket; }
    private Dictionary<MarketList, TradingParameters> conditionByMarket { get => TradeManager.Instance.conditionByMarket; }
        

    public void TradeByMarket(MarketList market)
    {
        //�ش� ���Ͽ� ���� ������ �����Ǿ� ���� ���� ���
        if (!conditionByMarket.ContainsKey(market))
        {
            DebugByPlatform.Debug.Log($"{market} : �ش� ������ ������ �����Ǿ� ���� �ʽ��ϴ�.");
            return;
        }

        //�ش� ���Ͽ� ���� �����Ͱ� �������� ���� ���
        if (!CandleManager.Instance.ChkCandleDataByMarket(market))
        {
            DebugByPlatform.Debug.Log($"{market} :�ش� ������ ĵ�鵥���Ͱ� �������� �ʽ��ϴ�.");
            return;
        }

        var datas = CandleManager.Instance.GetCandleData(market);

        //Stochastic Calculation
        Dictionary<CandlesParameters, float> kValueDic, dValueDic;
        IndicatorVolumeCalculater.CalculateStochasticSlow(datas,
            conditionByMarket[market].stochasticK, conditionByMarket[market].stochasticD, conditionByMarket[market].stochasticStrength,
            out kValueDic, out dValueDic);

        //rsiStrength Calculation
        float rsi = IndicatorVolumeCalculater.CalcRSI(datas, conditionByMarket[market].rsiStrength, 1);

        //tradePriceEMA Calculation
        double tradePriceEMA = IndicatorVolumeCalculater.CalculateEMATradePriceAvg(datas, conditionByMarket[market].tradePriceEMALength, 1);

        if (accountInfoByMarket.ContainsKey(market.ToString())
            && accountInfoByMarket[market.ToString()].balance * accountInfoByMarket[market.ToString()].avg_buy_price >= 5000)
        {
            SellTrade(market, kValueDic[datas[1]], rsi, tradePriceEMA, conditionByMarket[market].tradePriceConditionMul);
        }
        else
        {
            //RSIVisualization(market, rsiStrength, buyRSI, sellRSI);/ 
            BuyTrade(market, kValueDic[datas[1]], dValueDic[datas[1]], rsi, tradePriceEMA, conditionByMarket[market].tradePriceConditionMul);
        }
    }


    private void BuyTrade(MarketList market, float k, float d, float rsi, double tradePriceEMA, float multi)
    {
        var datas = CandleManager.Instance.GetCandleData(market);

        DebugByPlatform.Debug.LogOnlyEditer($"���������� Ž���մϴ�. : {market} ::: TradePrice : {datas[1].trade_price} // {k} / {d} // {rsi} // {datas[1].candle_acc_trade_price} : {tradePriceEMA} * {multi}");

        int score = 0;

        if (ChkBuyConditionStochastic(k, d))
        {
            score++;
        }

        if (ChkBuyConditionRSI(rsi))
        {
            score++;
        }

        if (ChkBuyConditionFinal(datas[1], tradePriceEMA, multi))
        {
            score++;
        }

        if (score >= 2)
        {
            TradeManager.Instance.BuyOrder(market, datas[0].trade_price);
        }         
    }

    private bool ChkBuyConditionStochastic(float k, float d)
    {
        if (k <= 20.0f && d <= 20.0f && (k > d))
        {
            return true;
        }

        return false;
    }

    private bool ChkBuyConditionRSI(float rsi)
    {
        if (rsi <= 30.0f)
        {
            return true;
        }

        return false;
    }

    private bool ChkBuyConditionFinal(CandlesParameters parameter, double tradePriceAvg, float multi)
    {
        if (parameter.candle_acc_trade_price >= tradePriceAvg * multi)
        {
            return true;
        }

        return false;
    }



    private void SellTrade(MarketList market, float k, float rsi, double tradePriceEMA, float multi)
    {
        var datas = CandleManager.Instance.GetCandleData(market);

        DebugByPlatform.Debug.LogOnlyEditer($"�Ǹ������� Ž���մϴ�. : {market} ::: TradePrice : {datas[1].trade_price} // {k}  // {rsi} // {datas[1].candle_acc_trade_price} : {tradePriceEMA} * {multi}");

        int score = 0;

        if (ChkSellConditionStochastic(k))
        {
            score++;
        }

        if (ChkSellConditionRSI(rsi))
        {
            score++;
        }

        if (ChkSellConditionFinal(datas[1], tradePriceEMA, multi))
        {
            score++;
        }

        if (score >= 2)
        {
            TradeManager.Instance.SellOrder(market, datas[0].trade_price);
        }
    }

    private bool ChkSellConditionStochastic(float k)
    {
        if (k >= 80.0f)
        {
            return true;
        }

        return false;
    }


    private bool ChkSellConditionRSI(float rsi)
    {
        if (rsi >= 70.0f)
        {
            return true;
        }

        return false;
    }

    private bool ChkSellConditionFinal(CandlesParameters parameter, double tradePriceAvg, float multi)
    {
        if (parameter.candle_acc_trade_price >= tradePriceAvg * multi)
        {
            return true;
        }

        return false;
    }

}


[Serializable]
public class MarketData
{
    public string market;
    public double losscut;
    public double profitcut;

    int stochasticK;
    int stochasticD;
    int stochasticPower;
    int rsiPower;

    float overBuy;
    float overSell;

    float guideRsi;

    int macdShort;
    int macdLong;
    int macdSignal;



    float profitRate;

    // �߰� �ʵ� ����

    public MarketData(string market, double losscut, double profitcut /* �߰� �ʵ� */)
    {
        this.market = market;
        this.losscut = losscut;
        this.profitcut = profitcut;
        // �߰� �ʵ� �ʱ�ȭ
    }
}

public class DataManager
{
    public void SaveMarketData()
    {

    }

    public void SaveData(MarketData[] data, string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            formatter.Serialize(fileStream, data);
        }
    }

    public MarketData[] LoadData(string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            return (MarketData[])formatter.Deserialize(fileStream);
        }
    }
}
