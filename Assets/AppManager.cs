using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;

public class AppManager : BaseManager<AppManager>
{
    public string machineName { get => Environment.MachineName; }
    public string ip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RuntimeInitialize()
    {
        GameObject newGameObject = new GameObject();
        newGameObject.name = $"@{typeof(AppManager)}";
        AppManager manager = newGameObject.AddComponent<AppManager>();
        DontDestroyOnLoad(manager);
        Instance.Call();
    }

    TelegramLogger logger;
    TelegramCommandListener commandListener;
    DataConverter dataConverter;
    FireStore fireStore;
    protected override void Init()
    {
        //TestManager.Instance.Call(transform);
        UIManager.Instance.Call();
        TimeManager.Instance.Call();

        CandleManager.Instance.Call();
        TradeManager.Instance.Call();

        logger = CreateComponentObjectInChildrenAndReturn<TelegramLogger>();
        commandListener = CreateComponentObjectInChildrenAndReturn<TelegramCommandListener>();
        dataConverter = CreateComponentObjectInChildrenAndReturn<DataConverter>();

        fireStore = CreateComponentObjectInChildrenAndReturn<FireStore>();

        ip = GetIpAddress();
    }

    public string GetIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            Debug.Log($"{ip} ::: {ip.AddressFamily}");
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();                
            }
        }

        return string.Empty;
    }

    public async void TelegramMassage(string massage, TelegramBotType type)
    {
        await logger.SendMessageToTelegram(massage, type);
    }

    public void AddData(string jsonString)
    {
        dataConverter.AddDataConvert(jsonString);
    }

    public void SaveData(MarketList market, TradingParameters parameters)
    {
        fireStore.AddOrUpdateTradingParameter(market, parameters);
    }

    public void SaveData(string market, TradingParameters parameters)
    {
        fireStore.AddOrUpdateTradingParameter(market, parameters);
    }

    public void ReloadConditionByMarkets()
    {
        fireStore.ReloadConditionByMarkets();
    }

    public Dictionary<MarketList, TradingParameters> LoadData() 
    {
        return fireStore.GetTradingParameters();
    }
}
