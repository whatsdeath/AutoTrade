using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CandleDataDownload : BaseWebRequest<CandlesParameters>
{
    private List<string> marketList { get => TestManager.Instance.marketList; }

    protected MinuteUnit _timeUnit;

    protected override string additionalUrl { get => $"v1/candles/minutes/{timeUnit}"; }
    protected override string apiEndPoint { get => $"market={market}&to={requestLastDate}T{requestLastTime}&count={count}"; }
    protected string apiEndPointStartOnly { get => $"market={market}&count={count}"; }

    protected int timeUnit { get => (int)_timeUnit; }

    protected int count { get => 200; }

    protected string requestLastDate { get => lastTime.AddMinutes(-timeUnit).ToString("yyyy-MM-dd"); }
    protected string requestLastTime { get => lastTime.AddMinutes(-timeUnit).ToString("HH:mm:ss"); }

    private DateTime lastTime;
    protected string market { get; private set; }
    Dictionary<string, List<CandlesParameters>> _candleListDic = new Dictionary<string, List<CandlesParameters>>();
    public Dictionary<string, List<CandlesParameters>> candleListDic { get => _candleListDic; }

    private IEnumerator candleSearchLoop;

    bool isNextSearchReady;


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

    public void CandlsSearch()
    {
       // StartCandleDataDownload("KRW-BTC");
    }

    public void SetLastTime(DateTime dateTime)
    {
        lastTime = dateTime;
    }

    public void StartCandleDataDownload(string market)
    {
        StartCandleDataDownload(market, (MinuteUnit)GlobalValue.CAMDLE_MINUTE_UNIT);
    }

    public void StartCandleDataDownload(string market, MinuteUnit minuteUnit)
    {
        _timeUnit = minuteUnit;
        if (!_candleListDic.ContainsKey(market))
        {
            _candleListDic.Add(market, new List<CandlesParameters>());
        }
        else
        {
            _candleListDic[market].Clear();
        }
        

        if (candleSearchLoop != null)
        {
            StopCoroutine(candleSearchLoop);
            candleSearchLoop = null;
        }

        candleSearchLoop = DataDownload(market);
        StartCoroutine(candleSearchLoop);
    }

    protected IEnumerator DataDownload(string market)
    {
        float nextAccessTime = Time.realtimeSinceStartup + GlobalValue.ACCESS_INTERVAL;
        this.market = market;

        StartCoroutine(Access(apiEndPointStartOnly));
        isNextSearchReady = false;

        while (true)
        {
            if (nextAccessTime > Time.realtimeSinceStartup)
            {
                yield return new WaitForSeconds(nextAccessTime - Time.realtimeSinceStartup);
            }

            while(!isNextSearchReady)
            {
                yield return null;
            }

            nextAccessTime = Time.realtimeSinceStartup + GlobalValue.ACCESS_INTERVAL;

            StartCoroutine(Access(apiEndPoint));
            isNextSearchReady = false;
        }
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
        List<CandlesParameters> candleList = DictionaryFunction.GetValueList(parameters);

        candleList.Sort((p1, p2) => p2.timestamp.CompareTo(p1.timestamp));

        for (int i = 0;  i < candleList.Count; i++)
        {
            _candleListDic[market].Add(candleList[i]);
        }

        lastTime = Convert.ToDateTime(_candleListDic[market][_candleListDic[market].Count - 1].candle_date_time_utc);

        Debug.Log(lastTime + "   " + _candleListDic[market].Count);

        if(_candleListDic[market].Count >= GlobalValue.SAVE_DATA_MAX_COUNT)
        {
            StopCoroutine(candleSearchLoop);

            //CreateScriptableObject.DataSet(name, _candleListDic[name]);

            Debug.Log("종료");
            TestManager.Instance.DataSaveAndTestStart(market, _candleListDic[market]);
        }

        isNextSearchReady = true;
    }

    protected override void ErrorProcess()
    {
        StopCoroutine(candleSearchLoop);

        //CreateScriptableObject.DataSet(name, _candleListDic[name]);

        Debug.Log("종료");
        TestManager.Instance.DataSaveAndTestStart(market, _candleListDic[market]);

        isNextSearchReady = true;
    }
}
