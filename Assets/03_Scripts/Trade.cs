using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;


[Serializable]
public class TradingParameters
{
    public string name;

    public int stochasticK;
    public int stochasticD;
    public int stochasticStrength;

    public int rsiStrength;

    public int macdShort;
    public int macdLong;
    public int macdSignal;

    public float overBuy;
    public float overSell;
    public float guideRsi;

    public double profitRate;

    public double lossCut;
    public double profitCut;

    public TradingParameters() { }
    public TradingParameters(TradingParameters other)
    {
        name = other.name;

        stochasticK = other.stochasticK;
        stochasticD = other.stochasticD;
        stochasticStrength = other.stochasticStrength;

        rsiStrength = other.rsiStrength;

        macdShort = other.macdShort;
        macdLong = other.macdLong;
        macdSignal = other.macdSignal;

        overBuy = other.overBuy;
        overSell = other.overSell;
        guideRsi = other.guideRsi;

        profitRate = other.profitRate;

        lossCut = other.lossCut;
        profitCut = other.profitCut;        
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

        //MACD Calculation
        Dictionary<CandlesParameters, double> macdLineDic, signalLineDic;
        IndicatorVolumeCalculater.CalculateMACD(datas,
            conditionByMarket[market].macdShort, conditionByMarket[market].macdLong, conditionByMarket[market].macdSignal, out macdLineDic, out signalLineDic);

        if (accountInfoByMarket.ContainsKey(market.ToString())
            && accountInfoByMarket[market.ToString()].balance * accountInfoByMarket[market.ToString()].avg_buy_price >= 5000)
        {
            //RSIVisualization(market, rsiStrength, buyRSI, sellRSI, true);/
            if(conditionByMarket[market].lossCut > 0)
            {
                SellTrade(market, conditionByMarket[market].lossCut, conditionByMarket[market].profitCut);
            }
        }
        else
        {
            //RSIVisualization(market, rsiStrength, buyRSI, sellRSI);/ 
            BuyTrade(market, kValueDic, dValueDic, rsi, macdLineDic[datas[1]], signalLineDic[datas[1]]);
        }
        /*
        //NERSI ǥ��
        double lowNEMA = CalcMA(datas, 0, conditionByMarket[market].MA1Power);
        double highNEMA = CalcMA(datas, 0, conditionByMarket[market].MA2Power);

        bool isNEBullMarket = ChkMAUpCondition(lowNEMA, highNEMA);

        float buyNERSI = isNEBullMarket ? conditionByMarket[market].upBuyRSI : conditionByMarket[market].downBuyRSI;
        float sellNERSI = isNEBullMarket ? conditionByMarket[market].upSellRSI : conditionByMarket[market].downSellRSI;
        int NERSIPower = isNEBullMarket ? conditionByMarket[market].upRSIPower : conditionByMarket[market].downRSIPower;

        float NERSI = CalcRSI(datas, 0, NERSIPower);

        //PriceVisualization(market, datas[0].trade_price, isBullMarket);
        NERSIVisualization(market, NERSI, buyNERSI, sellNERSI);*/
    }

    public void PriceVisualization(MarketList market, double price, bool isBullMarket)
    {
        UIManager.Instance.VisualizationPriceByMarket(market, price, isBullMarket ? "55FF55" : "FF5555");
    }

    public void RSIVisualization(MarketList market, float RSI, float buyRSI, float sellRSI, bool isSell = false)
    {
        if (RSI >= sellRSI)
        {
            UIManager.Instance.VisualizationRSIByMarket(market, RSI, isSell ? sellRSI : buyRSI, "55FF55", isSell);
        }
        else if (RSI <= buyRSI)
        {
            UIManager.Instance.VisualizationRSIByMarket(market, RSI, isSell ? sellRSI : buyRSI, "FF5555", isSell);
        }
        else
        {
            UIManager.Instance.VisualizationRSIByMarket(market, RSI, isSell ? sellRSI : buyRSI, isSell);
        }


    }

    public void NERSIVisualization(MarketList market, float NERSI, float buyNERSI, float sellNERSI)
    {
        if (NERSI >= sellNERSI)
        {
            UIManager.Instance.VisualizationNERSIByMarket(market, NERSI, "55FF55");
        }
        else if (NERSI <= buyNERSI)
        {
            UIManager.Instance.VisualizationNERSIByMarket(market, NERSI, "FF5555");
        }
        else
        {
            UIManager.Instance.VisualizationNERSIByMarket(market, NERSI);
        }
    }

    private void BuyTrade(MarketList market,
        Dictionary<CandlesParameters, float> kValues,
        Dictionary<CandlesParameters, float> dValues, 
        float rsi, double macd, double signal)
    {
        var datas = CandleManager.Instance.GetCandleData(market);

        bool isPositionOpen = false;
        int firstOpenPoint = 0;

        //�ش��ϴ� ��ǥ�� �ϳ��� �������� ������ ����
        if (!kValues.ContainsKey(datas[1]) || !dValues.ContainsKey(datas[1]))
        {
            return;
        }

        DebugByPlatform.Debug.LogOnlyEditer($"���������� Ž���մϴ�. : {market} ::: TradePrice : {datas[1].trade_price} // {kValues[datas[1]]} / {dValues[datas[1]]} // {rsi} // {macd} / {signal}");

        //����ĳ��ƽ �ŷ� ������ ���� ���ΰ��
        for (int i = 1; i < datas.Count; i++)
        {
            //���� �ϳ��� ���� ������
            if (!kValues.ContainsKey(datas[i]) || !dValues.ContainsKey(datas[i]))
            {
                return;
            }

            //������ ���� ��, ���� �������� Ž��
            if (isPositionOpen)
            {
                if (kValues[datas[i]] > conditionByMarket[market].overSell || dValues[datas[i]] > conditionByMarket[market].overSell)
                {
                    break;
                }

                firstOpenPoint = i;
            }

            if (!dValues.ContainsKey(datas[i]))
            {
                return;
            }

            //���ż� ������ �� ���µ��� ���������� ���μ��� ����
            if (kValues[datas[i]] >= conditionByMarket[market].overBuy)
            {
                return;
            }

            //K, D�� ���ŵ� �����Ͻ� ������ ����
            if (kValues[datas[i]] <= conditionByMarket[market].overSell && dValues[datas[i]] <= conditionByMarket[market].overSell)
            {
                isPositionOpen = true;
                firstOpenPoint = i;
            }
        }

        if (isPositionOpen)
        {
            //RSI�� ����ġ �̸��̶�� ����
            if (rsi < conditionByMarket[market].guideRsi)
            {
                return;
            }

            //RSI�� ����ġ �̻��� ��� MACDüũ
            //MACD ������ �ñ׳� ���� �Ʒ���� ����.
            if (macd < signal)
            {                
                return;
            }

            double minPrice = double.MaxValue;

            for (int i = 1; i <= Mathf.Max(firstOpenPoint, 24); i++)
            {
                if (i >= datas.Count)
                {
                    break;
                }

                if (datas[i].trade_price < minPrice)
                {
                    minPrice = datas[i].trade_price;
                }
            }

            conditionByMarket[market].lossCut = minPrice;
            conditionByMarket[market].profitCut = datas[1].trade_price + ((datas[1].trade_price - minPrice) * conditionByMarket[market].profitRate);

            TradeManager.Instance.BuyOrder(market, datas[0].trade_price);
            
        }
    }



    /*
    private void BuyTrade(MarketList market, float rsiStrength, float buyRSI)
    {
        DebugByPlatform.Debug.LogOnlyEditer($"���������� Ž���մϴ�. : {market} (rsiStrength : {rsiStrength})");

        if (rsiStrength <= buyRSI)
        {
            var datas = CandleManager.Instance.GetCandleData(market);

            TradeManager.Instance.BuyOrder(market, rsiStrength, datas[0].trade_price);
        }
    }*/


    private void SellTrade(MarketList market, double lossCut, double profitCut)
    {
        var datas = CandleManager.Instance.GetCandleData(market);

        DebugByPlatform.Debug.LogOnlyEditer($"�Ǹ������� Ž���մϴ�. : {market} ::: TradePrice : {datas[1].trade_price} / Loss : {lossCut} / Profit {profitCut}  // (NeerTradePrice : {datas[0].trade_price})");

        //�ν��� ���ų� �������� ����
        if (datas[1].trade_price < lossCut || datas[1].trade_price > profitCut)
        {
            TradeManager.Instance.SellOrder(market, datas[0].trade_price, datas[1].trade_price > profitCut);
        }
    }


    /*
    private void SellTrade(MarketList market, float rsiStrength, float sellRSI, float downRSI, float downBuyRSI, bool deadCross = false)
    {
        DebugByPlatform.Debug.LogOnlyEditer($"�Ǹ������� Ž���մϴ�. : {market} (rsiStrength : {rsiStrength})");

        if ((deadCross && downRSI > downBuyRSI)
            || (rsiStrength >= sellRSI)) //�϶��϶��� �϶���
        {
            var datas = CandleManager.Instance.GetCandleData(market);

            TradeManager.Instance.SellOrder(market, rsiStrength, datas[0].trade_price);
        }
    */
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
