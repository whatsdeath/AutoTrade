using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct CandlesParameters
{
    public string market;                   //���ϸ�
    public string candle_date_time_utc;     //ĵ�� ���� �ð�(UTC ����) ����: yyyy-MM-dd'T'HH:mm:ss
    public string candle_date_time_kst;     //ĵ�� ���� �ð�(KST ����) ����: yyyy-MM-dd'T'HH:mm:ss
    public double opening_price;            //�ð�
    public double high_price;               //��
    public double low_price;                //����
    public double trade_price;              //����
    public long timestamp;                  //������ ƽ�� ����� �ð�
    public double candle_acc_trade_price;   //���� �ŷ� �ݾ�
    public double candle_acc_trade_volume;  //���� �ŷ���
}

public enum MinuteUnit
{
    Minutes_1 = 1,
    Minutes_3 = 3,
    Minutes_5 = 5,
    Minutes_15 = 15,
    Minutes_10 = 10,
    Minutes_30 = 30,
    Hours_1 = 60,
    Hours_4 = 240
}

public abstract class BaseCandleSearch : BaseWebRequest<CandlesParameters>
{
    private List<string> marketList { get => TestManager.Instance.marketList; }

    abstract protected override string additionalUrl { get; }
    abstract protected override string apiEndPoint { get; }    
    protected MarketList market { get; set; }


    Dictionary<string, List<CandlesParameters>> _candleListDic = new Dictionary<string, List<CandlesParameters>>();
    public Dictionary<string, List<CandlesParameters>> candleListDic { get => _candleListDic; }

    public void Start()
    {
        Preparation();
    }

    public void Preparation()
    {
        for (int i = 0; i < marketList.Count; i++)
        {
            _candleListDic.Add(marketList[i], new List<CandlesParameters>());
        }
    }

    protected void CandleSearchByMarket(MarketList market)
    {
        this.market = market;
        StartCoroutine(Access(apiEndPoint));
    }

    protected void CandleSearch()
    {
        StartCoroutine(Access(apiEndPoint));
    }

    protected override bool ChkIgnoreCondition(object data)
    {
        return GlobalValue.IGNORE_CURRENCYS.Contains(((CandlesParameters)data).candle_date_time_kst);
    }

    protected override string SetParamsKey(object data)
    {
        return ((CandlesParameters)data).candle_date_time_kst;
    }
    
    protected override void AfterProcess()
    {
        DebugByPlatform.Debug.LogOnlyEditer($"{market} : Ž����");

        List<CandlesParameters> candleList = DictionaryFunction.GetValueList(parameters);

        candleList.Sort((p1, p2) => p2.timestamp.CompareTo(p1.timestamp));

        if(candleList.Count > 0)
        {
            CandleManager.Instance.SetCandleData(market, candleList);
            TradeManager.Instance.accountParam.SetUnitPriceByMarket(market, candleList[0].trade_price);
        }

        if (TradeManager.Instance.tradeMode)
        {
            TradeManager.Instance.TradeByMarket(market);
        }
    }
}
