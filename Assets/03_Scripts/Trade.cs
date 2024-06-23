using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;


public enum TradeTerms
{ 
    Stochastic, RSI, StoRsiTrade
}


[Serializable]
public class TradingParameters
{
    public string name;

    public int stochasticK = 6;
    public int stochasticD = 6;
    public int stochasticStrength = 10;

    public int stochasticSellK = 6;
    public int stochasticSellStrength = 10;

    public int rsiStrength = 15;
    public int rsiSellStrength = 15;

    public int tradePriceEMALength = 30;
    public float tradePriceConditionMul = 2.0f;
    public float tradeSellPriceConditionMul = 2.0f;

    public double amountStochastic = 0;
    public double amountRSI = 0;
    public double amountStoRsiTrade = 0;

    public float winRateStochastic = 0;
    public float winRateRSI = 0;
    public float winRateStoRsiTrade = 0;

    public DateTime lastUpdate = new DateTime();

    public TradeTerms tradeTerms { get => CalcTradeTerms(); }

    public TradingParameters() { }
    public TradingParameters(TradingParameters other)
    {
        name = other.name;

        stochasticK = other.stochasticK;
        stochasticD = other.stochasticD;
        stochasticStrength = other.stochasticStrength;

        stochasticSellK = other.stochasticSellK;
        stochasticSellStrength = other.stochasticSellStrength;

        rsiStrength = other.rsiStrength;
        rsiSellStrength = other.rsiSellStrength;

        tradePriceEMALength = other.tradePriceEMALength;
        tradePriceConditionMul = other.tradePriceConditionMul;
        tradeSellPriceConditionMul = other.tradeSellPriceConditionMul;

        amountStochastic = other.amountStochastic;
        amountRSI = other.amountRSI;
        amountStoRsiTrade = other.amountStoRsiTrade;

        winRateStochastic = other.winRateStochastic;
        winRateRSI = other.winRateRSI;
        winRateStoRsiTrade = other.winRateStoRsiTrade;

        lastUpdate = other.lastUpdate;
    }

    private TradeTerms CalcTradeTerms()
    {
        if (amountStochastic > amountRSI)
        {
            if (amountStochastic > amountStoRsiTrade)
            {
                return TradeTerms.Stochastic;
            }            
        }
        else
        {
            if (amountRSI > amountStoRsiTrade)
            {
                return TradeTerms.RSI;
            }
        }

        return TradeTerms.StoRsiTrade;
    }
}


public class Trade : MonoBehaviour
{
    private Dictionary<string, AccountParam> accountInfoByMarket { get => TradeManager.Instance.accountInfoByMarket; }
    private Dictionary<MarketList, TradingParameters> conditionByMarket { get => TradeManager.Instance.conditionByMarket; }
        

    public void TradeByMarket(MarketList market)
    {
        //해당 마켓에 대한 조건이 설정되어 있지 않을 경우
        if (!conditionByMarket.ContainsKey(market))
        {
            DebugByPlatform.Debug.Log($"{market} : 해당 마켓은 조건이 설정되어 있지 않습니다.");
            return;
        }

        //해당 마켓에 대한 데이터가 존재하지 않을 경우
        if (!CandleManager.Instance.ChkCandleDataByMarket(market))
        {
            DebugByPlatform.Debug.Log($"{market} :해당 마켓은 캔들데이터가 존재하지 않습니다.");
            return;
        }

        var datas = CandleManager.Instance.GetCandleData(market);

        //Stochastic Calculation
        Dictionary<CandlesParameters, float> kValueDic, dValueDic;
        IndicatorVolumeCalculater.CalculateStochasticSlow(datas,
            conditionByMarket[market].stochasticK, conditionByMarket[market].stochasticD, conditionByMarket[market].stochasticStrength,
            out kValueDic, out dValueDic);

        Dictionary<CandlesParameters, float> kSellValueDic, dSellValueDic;
        IndicatorVolumeCalculater.CalculateStochasticSlow(datas,
            conditionByMarket[market].stochasticSellK, conditionByMarket[market].stochasticD, conditionByMarket[market].stochasticSellStrength,
            out kSellValueDic, out dSellValueDic);

        //rsiStrength Calculation
        float rsiBuy = IndicatorVolumeCalculater.CalcRSI(datas, conditionByMarket[market].rsiStrength, 1);
        float rsiSell = IndicatorVolumeCalculater.CalcRSI(datas, conditionByMarket[market].rsiSellStrength, 1);

        //tradePriceEMA Calculation
        double tradePriceEMA = IndicatorVolumeCalculater.CalculateEMATradePriceAvg(datas, conditionByMarket[market].tradePriceEMALength, 1);

        if (accountInfoByMarket.ContainsKey(market.ToString())
            && accountInfoByMarket[market.ToString()].balance * accountInfoByMarket[market.ToString()].avg_buy_price >= 5000)
        {
            SellTrade(market, kSellValueDic[datas[1]], rsiSell, tradePriceEMA, conditionByMarket[market].tradeSellPriceConditionMul);
        }
        else
        {
            //RSIVisualization(market, rsiStrength, buyRSI, sellRSI);/ 
            BuyTrade(market, kValueDic[datas[1]], dValueDic[datas[1]], rsiBuy, tradePriceEMA, conditionByMarket[market].tradePriceConditionMul);
        }
    }


    private void BuyTrade(MarketList market, float k, float d, float rsi, double tradePriceEMA, float multi)
    {
        var datas = CandleManager.Instance.GetCandleData(market);

        DebugByPlatform.Debug.LogOnlyEditer($"구매조건을 탐색합니다. : {market}[{conditionByMarket[market].tradeTerms}] ::: TradePrice : {datas[1].trade_price} // {k.ToString("##0.0")}/{d.ToString("##0.0")}(20.0) // {rsi.ToString("##0.0#")}(30) // {(datas[1].candle_acc_trade_price / tradePriceEMA).ToString("##0.00")}({multi})");

        int score = 0;
        //이득 못볼 것 같은 종목은 거래하지 말자.
        //1달이면 5%는 챙겨야지.
        if (!ChkTradeSimulationResult(conditionByMarket[market], 1.05f))
        {
            return;
        }

        if (ChkBuyConditionStochastic(k, d))
        {
            score++;
            //스토캐스틱의 수익이 가장 많은 경우
            if (conditionByMarket[market].tradeTerms.Equals(TradeTerms.Stochastic))
            {
                TradeManager.Instance.BuyOrder(market, datas[0].trade_price);
                return;
            }
        }

        if (ChkBuyConditionRSI(rsi))
        {
            score++;
            //RSI의 수익이 가장 많은 경우
            if (conditionByMarket[market].tradeTerms.Equals(TradeTerms.RSI))
            {
                TradeManager.Instance.BuyOrder(market, datas[0].trade_price);
                return;
            }
        }

        if (ChkBuyConditionFinal(datas[1], tradePriceEMA, multi))
        {
            score++;
        }

        if (score >= 2)
        {            
            //종합지표(스토캐스틱/RSI/거래량)의 수익이 가장 많은 경우
            if (conditionByMarket[market].tradeTerms.Equals(TradeTerms.StoRsiTrade))
            {
                TradeManager.Instance.BuyOrder(market, datas[0].trade_price);
            }
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

        DebugByPlatform.Debug.LogOnlyEditer($"판매조건을 탐색합니다. : {market}[{conditionByMarket[market].tradeTerms}] ::: TradePrice : {datas[1].trade_price} // {k.ToString("##0.0")}(80)  // {rsi.ToString("##0.0")}(70) // {(datas[1].candle_acc_trade_price / tradePriceEMA).ToString("##0.00")}({multi})");

        int score = 0;

        //본전도 못찾는 종목은 일단 비우고 이득각이 보일때까지 거래 보류하자.
        //자칫 계속 사고팔 수 있으므로 사는 가격에 비해 리미트를 낮게 책정
        if (!ChkTradeSimulationResult(conditionByMarket[market], 1.01f))
        {
            TradeManager.Instance.SellOrder(market, datas[0].trade_price);
            return;
        }


        if (ChkSellConditionStochastic(k))
        {
            score++;
            //스토캐스틱의 수익이 가장 많은 경우
            if (conditionByMarket[market].tradeTerms.Equals(TradeTerms.Stochastic))
            {
                TradeManager.Instance.SellOrder(market, datas[0].trade_price);
                return;
            }
        }

        if (ChkSellConditionRSI(rsi))
        {
            score++;
            //RSI의 수익이 가장 많은 경우
            if (conditionByMarket[market].tradeTerms.Equals(TradeTerms.RSI))
            {
                TradeManager.Instance.SellOrder(market, datas[0].trade_price);
                return;
            }
        }

        if (ChkSellConditionFinal(datas[1], tradePriceEMA, multi))
        {
            score++;
        }

        if (score >= 2)
        {
            //종합지표(스토캐스틱/RSI/거래량)의 수익이 가장 많은 경우
            if (conditionByMarket[market].tradeTerms.Equals(TradeTerms.StoRsiTrade))
            {
                TradeManager.Instance.SellOrder(market, datas[0].trade_price);
            }
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

    private bool ChkTradeSimulationResult(TradingParameters parameters, float limit)
    {
        //이득이 아닌 종목은 일단 비우고 이득각이 보일때까지 거래 보류하자.
        switch (parameters.tradeTerms)
        {
            case TradeTerms.Stochastic:
                return parameters.amountStochastic > TestManager.Instance.testMoney * limit;

            case TradeTerms.RSI:
                return parameters.amountRSI > TestManager.Instance.testMoney * limit;

            case TradeTerms.StoRsiTrade:
                return parameters.amountStoRsiTrade > TestManager.Instance.testMoney * limit;

            default:
                return false;
        }
    }
}


