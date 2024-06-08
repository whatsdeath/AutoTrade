using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : BaseManager<AppManager>
{
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
    }

    public async void TelegramMassage(string massage, TelegramBotType type)
    {
        await logger.SendMessageToTelegram(massage, type);
    }

    public void AddData(string jsonString)
    {
        dataConverter.AddDataConvert(jsonString);
    }

    public void SendData(string market, TradingParameters data, TelegramBotType botType)
    {
        dataConverter.SendDataConvert(market, data, botType);
    }

    public void SendData(Dictionary<string, TradingParameters> datas, TelegramBotType botType)
    {
        dataConverter.SendDataConvert(datas, botType);
    }

    public void SaveData(MarketList market, TradingParameters parameters)
    {
        fireStore.AddOrUpdateTradingParameter(market, parameters);
    }


    public Dictionary<MarketList, TradingParameters> LoadData() 
    {
        return fireStore.GetTradingParameters();
    }
}
