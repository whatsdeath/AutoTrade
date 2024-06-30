using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.Rendering;


[System.Serializable]
public class CandleData
{
    public CandlesParameters parameters;

    public CandleData(CandlesParameters parameters)
    {
        this.parameters = parameters;
    }

    public CandleData(CandleData candleData)
    {
        this.parameters = candleData.parameters;
    }
}

[System.Serializable]
public class TradingParameterData
{
    public string name;

    public int stochasticK;
    public int stochasticD;
    public int stochasticStrength;

    public int stochasticSellK;
    public int stochasticSellStrength   ;

    public int rsiStrength;
    public int rsiSellStrength;

    public int tradePriceEMALength;
    public float tradePriceConditionMul;
    public float tradeSellPriceConditionMul;

    public double amountStochastic;
    public double amountRSI;
    public double amountStoRsiTrade;

    public float winRateStochastic;
    public float winRateRSI;
    public float winRateStoRsiTrade;

    public DateTime lastUpdate;

    public TradingParameterData(TradingParameters parameters)
    {
        name = parameters.name;

        stochasticK = parameters.stochasticK;
        stochasticD = parameters.stochasticD;
        stochasticStrength = parameters.stochasticStrength;

        stochasticSellK = parameters.stochasticSellK;
        stochasticSellStrength = parameters.stochasticSellStrength;

        rsiStrength = parameters.rsiStrength;
        rsiSellStrength = parameters.rsiSellStrength;

        tradePriceEMALength = parameters.tradePriceEMALength;
        tradePriceConditionMul = parameters.tradePriceConditionMul;
        tradeSellPriceConditionMul = parameters.tradeSellPriceConditionMul;

        amountStochastic = parameters.amountStochastic;
        amountRSI = parameters.amountRSI;
        amountStoRsiTrade = parameters.amountStoRsiTrade;

        winRateStochastic = parameters.winRateStochastic;
        winRateRSI = parameters.winRateRSI;
        winRateStoRsiTrade = parameters.winRateStoRsiTrade;

        lastUpdate = TimeManager.Instance.nowTime;
    }
}


[System.Serializable]
public class MarketDataSave : BaseManager<MarketDataSave>
{
    [System.Serializable]
    public class CandleDataList
    {
        public CandlesParameters[] dataList = new CandlesParameters[GlobalValue.SAVE_DATA_MAX_COUNT];

        public CandleDataList() {}

        public CandleDataList(CandleDataList candleData)
        {
            dataList = candleData.dataList.ToArray();
        }

        public void SetDataList(CandleDataList candleData)
        {
            dataList = candleData.dataList.ToArray();
        }
        
        public void SetDataList(List<CandlesParameters> candleDatas)
        {
            while (candleDatas.Count > GlobalValue.SAVE_DATA_MAX_COUNT)
            {
                candleDatas.RemoveAt(GlobalValue.SAVE_DATA_MAX_COUNT);
            }

            dataList = candleDatas.ToArray();
        }
    }



    public void CandleSave(string market, List<CandlesParameters> candleDatas)
    {
        while (candleDatas.Count > GlobalValue.SAVE_DATA_MAX_COUNT)
        {
            candleDatas.RemoveAt(GlobalValue.SAVE_DATA_MAX_COUNT);
        }

        CandleDataList dataList = new CandleDataList();

        dataList.SetDataList(candleDatas);

        BinarySerialize(dataList, market);

        Debug.Log("저장 완료 " + dataList.dataList.Length);
    }

    public List<CandleData> CandleLoad(string market)
    {
        //dataByMarket[name].SetDataList(candleDatas);
        DataLoad(market);
        return null;
        //return dataByMarket[name].dataList.ToList();
    }

    /*
    public void DataSaveAndTestStart(string name)
    {
        BinarySerialize(dataByMarket[name], name);
    }
    */

    public CandleDataList DataLoad(string market)
    {
        return BinaryDeserialize<CandleDataList>(market);

        //dataByMarket[name].SetDataList(BinaryDeserialize<CandleDataList>(name));


    }

    public void SaveTradingParameter(TradingParameters parameters)
    {
        BinarySerialize(new TradingParameterData(parameters), parameters.name);
    }

    public TradingParameters LoadTradingParameter(string name)
    {
        try
        {
            return new TradingParameters(BinaryDeserialize<TradingParameterData>(name));
        }
        catch (Exception e) 
        { 
            Debug.LogException(e);
            return null;
        }
    }

    //세이브
    public void BinarySerialize<T>(T t, string fileName)
    {
        var directoryPath = $"{Application.dataPath}/09_Binaries/{typeof(T)}";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream($"{directoryPath}/{fileName}.bin", FileMode.Create);
        formatter.Serialize(stream, t);
        stream.Close();
    }

    //로드
    public T BinaryDeserialize<T>(string fileName)
    {
        var directoryPath = $"{Application.dataPath}/09_Binaries/{typeof(T)}";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream($"{directoryPath}/{fileName}.bin", FileMode.Open);
        T t = (T)formatter.Deserialize(stream);
        stream.Close();
        return t;
    }
}
