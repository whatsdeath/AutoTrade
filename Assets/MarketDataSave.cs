using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
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
public class MarketDataSave : MonoBehaviour
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


    //세이브
    public void BinarySerialize<T>(T t, string fileName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream($"{Application.persistentDataPath}/{fileName}.bin", FileMode.Create);
        formatter.Serialize(stream, t);
        stream.Close();
    }

    //로드
    public T BinaryDeserialize<T>(string fileName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream($"{Application.persistentDataPath}/{fileName}.bin", FileMode.Open);
        T t = (T)formatter.Deserialize(stream);
        stream.Close();
        return t;
    }
}
