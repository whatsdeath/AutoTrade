using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataConverter : MonoBehaviour
{
    Dictionary<string, TradingParameters> parametersDict = new Dictionary<string, TradingParameters>();

    public void AddDataConvert(string jsonString)
    {
        Dictionary<string, TradingParameters> deserializedDict = JsonConvert.DeserializeObject<Dictionary<string, TradingParameters>>(jsonString);

        // ��ȯ�� ��ųʸ��� ���� ���
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
        }
    }
}
