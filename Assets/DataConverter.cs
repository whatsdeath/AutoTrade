using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataConverter : MonoBehaviour
{
    Dictionary<string, TradingParameters> parametersDict = new Dictionary<string, TradingParameters>();

    void Start()
    {
        // TradingParameters 클래스를 포함하는 딕셔너리 생성
        parametersDict.Add("POLYX", new TradingParameters
        {
            name = "POLYX",

            stochasticK = 3,
            stochasticD = 7,
            stochasticStrength = 5,

            rsiStrength = 20,

            macdShort = 15,
            macdLong = 30,
            macdSignal = 15,

            overBuy = 72.0f,
            overSell = 21.0f,
            guideRsi = 47.0f,

            profitRate = 1.5f
        });

        parametersDict.Add("CHZ", new TradingParameters
        {
            name = "CHZ",

            stochasticK = 4,
            stochasticD = 6,
            stochasticStrength = 6,

            rsiStrength = 16,

            macdShort = 11,
            macdLong = 29,
            macdSignal = 15,

            overBuy = 70.0f,
            overSell = 25.0f,
            guideRsi = 50.0f,

            profitRate = 1.25f
        });
    }

    private void OnApplicationQuit()
    {
        SendDataConvert(parametersDict, TelegramBotType.DebugLog);
    }

    public void AddDataConvert(string jsonString)
    {
        Dictionary<string, TradingParameters> deserializedDict = JsonConvert.DeserializeObject<Dictionary<string, TradingParameters>>(jsonString);

        // 변환된 딕셔너리의 값을 출력
        foreach (var item in deserializedDict)
        {
            if (!parametersDict.ContainsKey(item.Key))
            {
                parametersDict.Add(item.Key, new TradingParameters(item.Value));
            }
            else
            {
                parametersDict[item.Key] = new TradingParameters(item.Value);
            }

            Debug.Log($"Key: {item.Key}, RsiPeriod: {item.Value.rsiStrength}, StochasticK: {item.Value.stochasticK}, ProfitRate: {item.Value.profitRate}");
        }
    }

    public void SendDataConvert(string market, TradingParameters data, TelegramBotType botType)
    {
        Dictionary<string, TradingParameters> tempParametersDict = new Dictionary<string, TradingParameters>
        {
            { market, new TradingParameters(data) }
        };

        // JSON 형식의 문자열로 변환
        string jsonString = JsonConvert.SerializeObject(tempParametersDict, Formatting.None);
        AppManager.Instance.TelegramMassage($"/SetCondition {jsonString}", botType);
    }

    public void SendDataConvert(Dictionary<string, TradingParameters> datas, TelegramBotType botType)
    {
        // JSON 형식의 문자열로 변환
        string jsonString = JsonConvert.SerializeObject(datas, Formatting.None);
        AppManager.Instance.TelegramMassage($"/SetCondition {jsonString}", botType);
    }
}
