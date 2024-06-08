using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Unity.Jobs;

public class TestManager : BaseManager<TestManager>
{
    private MarketAll marketAll;
    private CandleDataDownload dataDownload;
    private MarketDataSave dataSave;

    public List<string> marketList { get => marketAll.marketList; }

    public Dictionary<string,List<CandleData>> datasByMarket = new Dictionary<string, List<CandleData>>();


    public List<CandlesParameters> parameters = new List<CandlesParameters>();



    protected override void Init()
    {
        marketAll = CreateComponentObjectInChildrenAndReturn<MarketAll>();
        dataDownload = CreateComponentObjectInChildrenAndReturn<CandleDataDownload>();
        dataSave = CreateComponentObjectInChildrenAndReturn<MarketDataSave>();

        //LoadDataBase<MarketData, MarketDataBase>(out marketDatabase);

        //datasByMarket.Add("KRW-DOGE", new List<pa>());
        //datasByMarket["KRW-DOGE"] = marketDatabase.GetData("KRW-DOGE").candleDatas.ToList();

        //Debug.Log(marketDatabase.GetData("KRW-DOGE").candleDatas.Count + "  " + marketDatabase.GetData("KRW-DOGE").dataCount);


        if (startTest)
        {
            parameters = dataSave.DataLoad(testMarket).dataList.ToList();  // marketDatabase.GetData("KRW-DOGE").candleDatas.ToList();
            parameters.Sort((p1, p2) => p2.timestamp.CompareTo(p1.timestamp));

            Debug.Log(parameters.Count);

            //StartBackTestNew();
            //BacktestStrategy(true, 3, 7, 5, 11, 80, 25, 50, 12, 30, 5, 1.6f);

            //{ k} / { d} / { stPower} 
            // {rsiPower} 
            // {macdShort} / {macdLong} / {macdSignal} 
            // {overBuy} / {overSell} / {guideRSI} 
            // {profita}", TelegramBotType.BackTest);
            //StartBackTestPhase2(5, 5, 8, 11);

            StartCoroutine(DataSetting(false));

        }
        //BackTest.BackTestingCoverRSI(prices);
        //BackTest.BackTestingCoverRSI(prices, 34.7f, 66.6f, 7);
    }
    string tmarket { get => "PYTH"; }
    //BTC SOL
    bool startTest { get => false; }
    //bool startTest { get => true; }

    string testMarket { get => $"KRW-{tmarket}"; }

    float penalty { get => 0.998f; }

    #region data  
    public void DataDownload()
    {
        dataDownload.StartCandleDataDownload(testMarket);
    }


    public void DataSet(string market, List<CandleData> candleDatas)
    {
        if (!datasByMarket.ContainsKey(market))
        {
            datasByMarket.Add(market, new List<CandleData>());
        }

        datasByMarket[market] = candleDatas.ToList();
    }


    public void DataSave(string market, List<CandlesParameters> candleDatas)
    {
        dataSave.CandleSave(market, candleDatas);
    }
    #endregion

    public int RsiPeriod = 14; // RSI 기간
    public int StochasticK = 3; // 스토캐스틱 기간
    public int StochasticD = 3; // 스토캐스틱 기간
    public int StochasticPeriod = 14; // 스토캐스틱 기간
    public int MacdShortPeriod = 12; // MACD 단기 이동평균 기간
    public int MacdLongPeriod = 26; // MACD 장기 이동평균 기간
    public int MacdSignalPeriod = 9; // MACD 시그널 라인 기간    

    public float OverBuyPeriod = 70.0f;
    public float OverSellPeriod = 30.0f;

    public float GuideRsiPeriod = 50.0f;

    public double ProfitRate = 1.5;

    double money = 3000000;
    double beforeMoney = 0;

    float tradeCount = 0;
    int failCount = 0;

    double buyPrice = 0;
    double buyUnitCount = 0;

    double lossCutLine = 0.0f;
    double profitCutLine = 0.0f;

    DateTime buyDateTime = new DateTime();
    DateTime sellDateTime = new DateTime();

    
    public void StartBackTestNew()
    {
        StartCoroutine(BackTesting_Stochastic_RSI());
    }
    
    IEnumerator BackTesting_Stochastic_RSI()
    {
        int counta = 0;

        for (int k = minStochasticK; k <= maxStochasticK; k++)
        {
            for (int d = minStochasticD; d <= maxStochasticD; d++)
            {
                for (int power = minStochasticPower; power <= maxStochasticPower; power++)
                {
                    for (int rsiPower = minRSIPower; rsiPower <= maxRSIPower; rsiPower++)
                    {
                        counta++;
                    }                   
                }
            }
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        int powera = 0;
        int ka = 0;
        int da = 0;
        int rsipowera = 0;
        //int macdSignalPerioda = 0;

        for (int k = minStochasticK; k <= maxStochasticK; k++)
        {
            for (int d = minStochasticD; d <= maxStochasticD; d++)
            {
                for (int power = minStochasticPower; power <= maxStochasticPower; power++)
                {
                    for (int rsiPower = minRSIPower; rsiPower <= maxRSIPower; rsiPower++)
                    {
                        count++;
                        float winRate = 0.0f;
                        var aa = BacktestStrategy(out winRate, false, k, d, power, rsiPower);

                        if (maxMoney < aa)
                        {
                            winRatea = winRate;
                            maxMoney = aa;
                            powera = power;
                            ka = k;
                            da = d;
                            rsipowera = rsiPower;
                            //macdSignalPerioda = macdSignal;
                        }
                        Debug.Log($"{count} / {counta} ::: {aa} / {winRate} / {k} / {d} / {power} // {rsiPower}");
                        yield return null;
                    }
                }
            }
        }

        AppManager.Instance.TelegramMassage($"[{testMarket}] 1페이즈 [{count}/{counta}] {maxMoney} / {winRatea} ::: {ka} / {da} / {powera} // {rsipowera}", TelegramBotType.BackTest);

        StartCoroutine(BackTesting_MACD(ka, da, powera, rsipowera));
    }

    IEnumerator BackTesting_MACD(int k, int d, int stPower, int rsiPower)
    {
        int counta = 0;

        for (int macdShort = minMacdShort; macdShort <= maxMacdShort; macdShort++)
        {
            for (int macdLong = minMacdLong; macdLong <= maxMacdLong; macdLong++)
            {
                for (int macdSignal = minMacdSignal; macdSignal <= maxMacdSignal; macdSignal++)
                {
                    counta++;
                }
            }
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        int macdShorta = 0;
        int macdLonga = 0;
        int macdSignala = 0;


        for (int macdShort = minMacdShort; macdShort <= maxMacdShort; macdShort++)
        {
            for (int macdLong = minMacdLong; macdLong <= maxMacdLong; macdLong++)
            {
                for (int macdSignal = minMacdSignal; macdSignal <= maxMacdSignal; macdSignal++)
                {
                    count++;
                    float winRate;
                    var aa = BacktestStrategy(out winRate, true, k, d, stPower, rsiPower, macdShort, macdLong, macdSignal);

                    if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
                    {
                        winRatea = winRate;
                        maxMoney = aa;
                        macdShorta = macdShort;
                        macdLonga = macdLong;
                        macdSignala = macdSignal;
                    }
                    Debug.Log($"{count} / {counta} ::: {aa} / {winRate} // {k} / {d} / {stPower} // {rsiPower} //  {macdShort} / {macdLong} / {macdSignal}");
                    yield return null;
                }
            }
        }

        AppManager.Instance.TelegramMassage($"[{testMarket}] 2페이즈 [{count}/{counta}] {maxMoney} / {winRatea} ::: {k} / {d} / {stPower} // {rsiPower} //  {macdShorta} / {macdLonga} / {macdSignala}", TelegramBotType.BackTest);

        StartCoroutine(BackTesting_Guide(k, d, stPower, rsiPower, macdShorta, macdLonga, macdSignala));
    }



    IEnumerator BackTesting_Guide(int k, int d, int stPower, int rsiPower, int macdShort, int macdLong, int macdSignal)
    {
        int counta = 0;

        for (float overBuy = 70.0f; overBuy <= 90.0f; overBuy++)
        {
            for (float overSell = 20.0f; overSell <= 40.0f; overSell++)
            {
                for (float guideRsi = 30.0f; guideRsi <= 50.0f; guideRsi++)
                {
                    counta++;
                }
            }
        }

        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        float overBuya = 0;
        float overSella = 0;
        float guideRsia = 0;

        for (float overBuy = 70.0f; overBuy <= 90.0f; overBuy++)
        {
            for (float overSell = 20.0f; overSell <= 40.0f; overSell++)
            {
                for (float guideRsi = 30.0f; guideRsi <= 50.0f; guideRsi++)
                {
                    count++;
                    float winRate = 0.0f;
                    var aa = BacktestStrategy(out winRate, true, k, d, stPower, rsiPower, macdShort, macdLong, macdSignal, overBuy, overSell, guideRsi);

                    if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
                    {
                        winRatea = winRate;
                        maxMoney = aa;
                        overBuya = overBuy;
                        overSella = overSell;
                        guideRsia = guideRsi;
                    }
                    Debug.Log($"{count} / {counta} ::: {aa} / {winRate} // {k} / {d} / {stPower} // {rsiPower} // {macdShort} / {macdLong} / {macdSignal} // {overBuy} / {overSell} / {guideRsi}");
                    yield return null;
                }
            }
        }

        AppManager.Instance.TelegramMassage($"[{testMarket}] 3페이즈 [{count}/{counta}] {maxMoney} / {winRatea} ::: {k} / {d} / {stPower} // {rsiPower} // {macdShort} / {macdLong} / {macdSignal} // {overBuya} / {overSella} / {guideRsia} ", TelegramBotType.BackTest);

        StartCoroutine(BackTesting_ProfitRate(k, d, stPower, rsiPower, macdShort, macdLong, macdSignal, overBuya, overSella, guideRsia));
    }

    IEnumerator BackTesting_ProfitRate(int k, int d, int stPower, int rsiPower, int macdShort, int macdLong, int macdSignal, float overBuy, float overSell, float guideRSI)
    {
        int counta = 0;

        for (float profit = 1.0f; profit <= 3.0f; profit = profit + 0.05f)
        {
            counta++;
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        float profita = 0;


        for (float profit = 1.0f; profit <= 3.0f; profit = profit + 0.05f)
        {
            count++;
            float winRate = float.MinValue;
            var aa = BacktestStrategy(out winRate, true, k, d, stPower, rsiPower, macdShort, macdLong, macdSignal, overBuy, overSell, guideRSI, profit);

            if (winRatea < winRate || (winRatea == winRate && maxMoney <= aa))
            {
                winRatea = winRate;
                maxMoney = aa;
                profita = profit;
            }
            Debug.Log($"{count} / {counta} ::: {aa} / {winRate} // {k} / {d} / {stPower} // {rsiPower} // {macdShort} / {macdLong} / {macdSignal} // {overBuy} / {overSell} / {guideRSI} // {profit}");
            yield return null;
        }

        AppManager.Instance.TelegramMassage($"[{testMarket}] 4페이즈 [{count}/{counta}] {maxMoney} / {winRatea} ::: {k} / {d} / {stPower} // {rsiPower} // {macdShort} / {macdLong} / {macdSignal} // {overBuy} / {overSell} / {guideRSI} // {profita}", TelegramBotType.BackTest);
        AppManager.Instance.SendData(tmarket, new TradingParameters 
        {
            name = tmarket,

            stochasticK = k,
            stochasticD = d,
            stochasticStrength = stPower,

            rsiStrength = rsiPower,

            macdShort = macdShort,
            macdLong = macdLong,
            macdSignal = macdSignal,

            overBuy = overBuy,
            overSell = overSell,
            guideRsi = guideRSI,

            profitRate = profita
        }, TelegramBotType.BackTest);
           
    }

    public void Retest()
    {
        StartCoroutine(Retesting_MACD(4, 6, 6, 11, 77, 22, 48));
    }

    IEnumerator Retesting_MACD(int k, int d, int stPower, int rsiPower, float overBuy, float overSell, float guideRSI)
    {
        int counta = 0;

        for (int macdShort = minMacdShort; macdShort <= 15; macdShort++)
        {
            for (int macdLong = minMacdLong; macdLong <= 30; macdLong++)
            {
                for (int macdSignal = minMacdSignal; macdSignal <= 15; macdSignal++)
                {
                    counta++;
                }
            }
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        int macdShorta = 0;
        int macdLonga = 0;
        int macdSignala = 0;


        for (int macdShort = minMacdShort; macdShort <= 15; macdShort++)
        {
            for (int macdLong = minMacdLong; macdLong <= 30; macdLong++)
            {
                for (int macdSignal = minMacdSignal; macdSignal <= 15; macdSignal++)
                {
                    count++;
                    float winRate;
                    var aa = BacktestStrategy(out winRate, true, k, d, stPower, rsiPower, macdShort, macdLong, macdSignal, overBuy, overSell, guideRSI);

                    if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
                    {
                        winRatea = winRate;
                        maxMoney = aa;
                        macdShorta = macdShort;
                        macdLonga = macdLong;
                        macdSignala = macdSignal;
                    }
                    Debug.Log($"{count} / {counta} :::  {aa} / {winRate}  // {k} / {d} / {stPower} // {rsiPower} //  {macdShort} / {macdLong} / {macdSignal} //{overBuy} / {overSell} / {guideRSI}");
                    yield return null;
                }
            }
        }

        AppManager.Instance.TelegramMassage($"ReMACD [{count}/{counta}] {maxMoney} / {winRatea} ::: {k} / {d} / {stPower} // {rsiPower} //  {macdShorta} / {macdLonga} / {macdSignala} // {overBuy} / {overSell} / {guideRSI}", TelegramBotType.BackTest);

        StartCoroutine(Retesting_ProfitRate(k, d, stPower, rsiPower, macdShorta, macdLonga, macdSignala, overBuy, overSell, guideRSI));
    }

    IEnumerator Retesting_ProfitRate(int k, int d, int stPower, int rsiPower, int macdShort, int macdLong, int macdSignal, float overBuy, float overSell, float guideRSI)
    {
        int counta = 0;

        for (float profit = 1.0f; profit <= 3.0f; profit = profit + 0.05f)
        {
            counta++;
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = 0.0f;
        float profita = 0;


        for (float profit = 1.0f; profit <= 3.0f; profit = profit + 0.05f)
        {
            count++;
            float winRate;
            var aa = BacktestStrategy(out winRate, true, k, d, stPower, rsiPower, macdShort, macdLong, macdSignal, overBuy, overSell, guideRSI, profit);

            if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
            {
                winRatea = winRate;
                maxMoney = aa;
                profita = profit;
            }
            Debug.Log($"{count} / {counta} ::: {aa} // {k} / {d} / {stPower} // {rsiPower} // {macdShort} / {macdLong} / {macdSignal} // {overBuy} / {overSell} / {guideRSI} // {profit}");
            yield return null;
        }

        AppManager.Instance.TelegramMassage($"ReProfitRate [{count}/{counta}] {maxMoney} / {winRatea} ::: {k} / {d} / {stPower} // {rsiPower} // {macdShort} / {macdLong} / {macdSignal} // {overBuy} / {overSell} / {guideRSI} // {profita}", TelegramBotType.BackTest);
    }


    private double BacktestStrategy(out float winRate, bool MACD, 
    int stochasticK = 3,
    int stochasticD = 3,
    int stochasticPeriod = 14,// 스토캐스틱 기간
    int rsiPeriod = 14,// RSI 기간 

    int macdShortPeriod = 12, // MACD 단기 이동평균 기간
    int macdLongPeriod = 26, // MACD 장기 이동평균 기간
    int macdSignalPeriod = 9, // MACD 시그널 라인 기간

    float overBuyPeriod = 70.0f,
    float overSellPeriod = 30.0f,

    float guideRsiPeriod = 50.0f,

    float profitRate = 1.5f) // 2% 손절

    {
        //변수 세팅
        StochasticK = stochasticK; // 스토캐스틱 기간
        StochasticD = stochasticD; // 스토캐스틱 기간
        StochasticPeriod = stochasticPeriod; // 스토캐스틱 기간
        RsiPeriod = rsiPeriod; // RSI 기간
        MacdShortPeriod = macdShortPeriod; // MACD 단기 이동평균 기간
        MacdLongPeriod = macdLongPeriod; // MACD 장기 이동평균 기간
        MacdSignalPeriod = macdSignalPeriod; // MACD 시그널 라인 기간

        OverBuyPeriod = overBuyPeriod;
        OverSellPeriod = overSellPeriod;

        GuideRsiPeriod = guideRsiPeriod;

        ProfitRate = profitRate;

        //자금계산을 위한 변수모음
        money = 3000000;
        beforeMoney = 0;

        tradeCount = 0;
        failCount = 0;

        buyPrice = 0;
        buyUnitCount = 0;
        /*
        if (!stochasticKValues.ContainsKey(stochasticK))
        {
            Dictionary<CandlesParameters, float> kValueDic, dValueDic;
            CalculateStochasticSlow(parameters, stochasticK, stochasticD, stochasticStrength, out kValueDic, out dValueDic);

            stochasticKValues.Add(stochasticK, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticKValues[stochasticK].Add(stochasticD, new Dictionary<int, Dictionary<CandlesParameters, float>>());
            stochasticKValues[stochasticK][stochasticD].Add(stochasticStrength, new Dictionary<CandlesParameters, float>(kValueDic));

            stochasticDValues.Add(stochasticK, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticDValues[stochasticK].Add(stochasticD, new Dictionary<int, Dictionary<CandlesParameters, float>>());
            stochasticDValues[stochasticK][stochasticD].Add(stochasticStrength, new Dictionary<CandlesParameters, float>(dValueDic));

            Dictionary<CandlesParameters, float> rsiValueDic;
            CalculateRSI(parameters, rsiStrength, out rsiValueDic);

            rsiValues.Add(rsiStrength, new Dictionary<CandlesParameters, float>(rsiValueDic));


            Dictionary<CandlesParameters, double> macdLineDic, signalLineDic;
            CalculateMACD(parameters, macdShortPeriod, macdLong, macdSignal, out macdLineDic, out signalLineDic);

            macdMACDValues.Add(macdShortPeriod, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdMACDValues[macdShortPeriod].Add(macdLong, new Dictionary<int, Dictionary<CandlesParameters, double>>());
            macdMACDValues[macdShortPeriod][macdLong].Add(macdSignal, new Dictionary<CandlesParameters, double>(macdLineDic));

            macdSignalValues.Add(macdShortPeriod, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdSignalValues[macdShortPeriod].Add(macdLong, new Dictionary<int, Dictionary<CandlesParameters, double>>());
            macdSignalValues[macdShortPeriod][macdLong].Add(macdSignal, new Dictionary<CandlesParameters, double>(signalLineDic));
        }*/

        //Debug.Log("오디ㄲㅈ ㅇㄴㅂㅈ");

        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            //구매한게 있으면 매도관련 판단
            if (buyUnitCount > 0)
            {
                TestSellProcess(parameters, i);
            }
            else
            //구매한게 없으면 매수관련 판단
            {
                TestBuyProcess(parameters, rsiValues[RsiPeriod], 
                    stochasticKValues[StochasticK][StochasticD][StochasticPeriod],
                    stochasticDValues[StochasticK][StochasticD][StochasticPeriod],
                    macdMACDValues[MacdShortPeriod][MacdLongPeriod][MacdSignalPeriod], 
                    macdSignalValues[MacdShortPeriod][MacdLongPeriod][MacdSignalPeriod], i, MACD);
            }
        }

        Debug.Log($"최종액수 {money} 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}");

        if (MACD)
        {
            if (tradeCount >= 10 && (1 - (failCount / tradeCount)) >= 0.7f) //&& (money + (buyPrice * buyUnitCount)) >= 5500000)
            {
                AppManager.Instance.TelegramMassage($"[{testMarket}] [ k : {stochasticK}, d : {stochasticD}, power : {stochasticPeriod} // s : {macdShortPeriod}, l : {macdLongPeriod}, signal : {macdSignalPeriod} // rsipower : {rsiPeriod} ] 최종액수 {money} 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}", TelegramBotType.BackTest);
            }
        }

        /*
        if (tradeCount >= 10 && (1 - (failCount / tradeCount)) >= 0.7f)
        {
            AppManager.Instance.TelegramMessage($"[{testMarket}] [ k : {stochasticK}, d : {stochasticD}, power : {stochasticStrength}, rsipower : {rsiStrength} ] 최종액수 {money} 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}", TelegramBotType.BackTest);
        }*/
        /*
        if (tradeCount >= 2 && 
            ((1 - (failCount / tradeCount)) >= 0.9f && (money + (buyPrice * buyUnitCount)) >= 4500000)
            || ((1 - (failCount / tradeCount)) >= 0.8f && (money + (buyPrice * buyUnitCount)) >= 5500000))*/
        if (tradeCount >= 10 &&
           (1 - (failCount / tradeCount)) >= 0.70f)//  (money + (buyPrice * buyUnitCount)) >= 4500000)
        {
            AppManager.Instance.TelegramMassage($"[{testMarket}] [ k : {stochasticK}, d : {stochasticD}, power : {stochasticPeriod}, rsipower : {rsiPeriod} // buy : {overBuyPeriod}, sell : {overSellPeriod}, guide : {guideRsiPeriod} ] 최종액수 {money} 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}", TelegramBotType.BackTest);
        }
        else
        {

        }

        winRate = 1 - (failCount / tradeCount);
        return money + (buyPrice * buyUnitCount);
    }

    private void TestSellProcess(List<CandlesParameters> parameters, int index)
    {
        if (parameters[index].trade_price < lossCutLine || parameters[index].trade_price > profitCutLine)
        {
            money += (parameters[index].trade_price * buyUnitCount) * penalty;
            sellDateTime = Convert.ToDateTime(parameters[index].candle_date_time_kst);

            if (beforeMoney < money)
            {
                //Debug.Log($"거래 성공? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")} :: {lossCutLine} / {profitCutLine}");
            }
            else
            {
                //Debug.Log($"거래 실패? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")} :: {lossCutLine} / {profitCutLine}");
                failCount++;
            }

            buyPrice = 0;
            buyUnitCount = 0;
        }
    }


    private void TestBuyProcess(List<CandlesParameters> parameters, 
        Dictionary<CandlesParameters, float> rsiValues,
        Dictionary<CandlesParameters, float> kValues,
        Dictionary<CandlesParameters, float> dValues,
        Dictionary<CandlesParameters, double> macdLine,
        Dictionary<CandlesParameters, double> signalLine,
        int index, bool MACD = false)
    {
        bool isPositionOpen = false;
        int firstOpenPoint = 0;

        //해당하는 지표가 하나라도 존재하지 않으면 종료
        if (!rsiValues.ContainsKey(parameters[index]) 
            || !kValues.ContainsKey(parameters[index]) || !dValues.ContainsKey(parameters[index]))
        {
            return;
        }

        if (MACD)
        {
            if (!macdLine.ContainsKey(parameters[index]) || !signalLine.ContainsKey(parameters[index]))
            {
                return;
            }
        }

        //Debug.Log("여기 오나요?");

        //스토캐스틱 거래 포지션 오픈 여부계산
        for (int i = index; i < parameters.Count; i++)
        {
            //둘중 하나라도 값이 없으면
            if (!kValues.ContainsKey(parameters[i]) || !dValues.ContainsKey(parameters[i]))
            {
                return;
            }

            //포지션 오픈 시, 최초 오픈지점 탐색
            if (isPositionOpen)
            {
                if (kValues[parameters[i]] > OverSellPeriod || dValues[parameters[i]] > OverSellPeriod)
                {
                    break;
                }

                firstOpenPoint = i;
            }

            if (!dValues.ContainsKey(parameters[i]))
            {
                return;
            }             

            //과매수 구간일 시 오픈되지 않은것으로 프로세스 종료
            if (kValues[parameters[i]] >= OverBuyPeriod)
            {
                return;
            }

            //K, D가 과매도 구간일시 포지션 오픈
            if (kValues[parameters[i]] <= OverSellPeriod && dValues[parameters[i]] <= OverSellPeriod)
            {
                isPositionOpen = true;
                firstOpenPoint = i;
            }         
        }

        //거래 포지션이 열려있다면 
        if(isPositionOpen)
        {
            //RSI가 기준치 미만이라면 종료
            if (rsiValues[parameters[index]] < GuideRsiPeriod)
            {
                return;
            }

            //RSI가 기준치 이상일 경우 MACD체크
            //MACD 라인이 시그널 라인 아래라면 종료.
            if (MACD)
            {
                if (macdLine[parameters[index]] < signalLine[parameters[index]])
                {
                    //Debug.Log($"안됍니다~~~~ {parameters[index].candle_date_time_kst} ::: {macdLineDic[parameters[index]]} / {signalLineDic[parameters[index]]}");
                    return;
                }
            }

            buyPrice = parameters[index].trade_price;
            buyUnitCount = (money / buyPrice) * penalty;
            beforeMoney = money;
            money = 0;
            buyDateTime = Convert.ToDateTime(parameters[index].candle_date_time_kst);
            tradeCount++;

            double minPrice = double.MaxValue;
            int minIndex = -1;

            for (int i = index; i <= Mathf.Max(firstOpenPoint, index + 24); i++)
            {
                if (i >= parameters.Count)
                {
                    break;
                }

                //Debug.Log($"저가까지 봐야겠다.{j} :: {parameters[j].trade_price}");
                if (parameters[i].trade_price < minPrice)
                {
                    minPrice = parameters[i].trade_price;
                    minIndex = i;
                }                    
            }

            lossCutLine = minPrice;
            profitCutLine = parameters[index].trade_price + ((parameters[index].trade_price - minPrice) * ProfitRate);

            //Debug.Log($"구매 시도 [now({index}), first({firstOpenPoint}), min({minIndex}) // RSI({rsiValues[parameters[index]]})] : ({buyDateTime}){buyPrice} * {buyUnitCount} :: {lossCutLine} / {parameters[index].trade_price} / {profitCutLine}");            
        }
    }


    //Stochastic
    int minStochasticK = 3, maxStochasticK = 10;
    int minStochasticD = 3, maxStochasticD = 10;
    int minStochasticPower = 5, maxStochasticPower = 20;

    //RSI
    int minRSIPower = 5, maxRSIPower = 20;

    //MACD
    int minMacdShort = 10, maxMacdShort = 15;
    int minMacdLong = 20, maxMacdLong = 30;
    int minMacdSignal = 5, maxMacdSignal = 15;


    Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>>
        stochasticKValues = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>>(),
        stochasticDValues = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>>();

    Dictionary<int, Dictionary<CandlesParameters, float>>
        rsiValues = new Dictionary<int, Dictionary<CandlesParameters, float>>();


    Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>>
        macdMACDValues = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>>(),
        macdSignalValues = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>>();



    IEnumerator DataSetting(bool retest)
    {
        #region 데이터 생성 횟수
        int counta = 0;

        for (int k = minStochasticK; k <= maxStochasticK; k++)
        {
            for (int d = minStochasticD; d <= maxStochasticD; d++)
            {
                for (int power = minStochasticPower; power <= maxStochasticPower; power++)
                {
                    counta++;
                }
            }
        }

        for (int rsiPower = minRSIPower; rsiPower <= maxRSIPower; rsiPower++)
        {
            counta++;
        }

        for (int macdShort = minMacdShort; macdShort <= maxMacdShort; macdShort++)
        {
            for (int macdLong = minMacdLong; macdLong <= maxMacdLong; macdLong++)
            {
                for (int macdSignal = minMacdSignal; macdSignal <= maxMacdSignal; macdSignal++)
                {
                    counta++;
                }
            }
        }
        #endregion

        int count = 0;

        #region Stochastic 
        stochasticKValues.Clear();
        stochasticDValues.Clear();

        for (int k = minStochasticK; k <= maxStochasticK; k++)
        {
            stochasticKValues.Add(k, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticDValues.Add(k, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());

            for (int d = minStochasticD; d <= maxStochasticD; d++)
            {
                stochasticKValues[k].Add(d, new Dictionary<int, Dictionary<CandlesParameters, float>>());
                stochasticDValues[k].Add(d, new Dictionary<int, Dictionary<CandlesParameters, float>>());

                for (int power = minStochasticPower; power <= maxStochasticPower; power++)
                {
                    Dictionary<CandlesParameters, float> kValueDic, dValueDic;

                    CalculateStochasticSlow(parameters, k, d, power, out kValueDic, out dValueDic);

                    stochasticKValues[k][d].Add(power, new Dictionary<CandlesParameters, float>(kValueDic));
                    stochasticDValues[k][d].Add(power, new Dictionary<CandlesParameters, float>(dValueDic));

                    count++;

                    Debug.Log($"데이터 생성중 :: {count} / {counta} = {stochasticKValues[k][d][power][parameters[2]]} / {stochasticDValues[k][d][power][parameters[2]]} ");
                    yield return null;
                }
            }
        }
        #endregion

        #region RSI
        rsiValues.Clear();

        for (int rsiPower = minRSIPower; rsiPower <= maxRSIPower; rsiPower++)
        {
            Dictionary<CandlesParameters, float> rsiValueDic;
            CalculateRSI(parameters, rsiPower, out rsiValueDic);

            rsiValues.Add(rsiPower, new Dictionary<CandlesParameters, float>(rsiValueDic));
            count++;
            Debug.Log($"데이터 생성중 :: {count} / {counta} = {rsiValues[rsiPower][parameters[2]]}");
            yield return null;
        }
        #endregion

        #region MACD
        macdMACDValues.Clear();
        macdSignalValues.Clear();

        for (int macdShort = minMacdShort; macdShort <= maxMacdShort; macdShort++)
        {
            macdMACDValues.Add(macdShort, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdSignalValues.Add(macdShort, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());

            for (int macdLong = minMacdLong; macdLong <= maxMacdLong; macdLong++)
            {
                macdMACDValues[macdShort].Add(macdLong, new Dictionary<int, Dictionary<CandlesParameters, double>>());
                macdSignalValues[macdShort].Add(macdLong, new Dictionary<int, Dictionary<CandlesParameters, double>>());

                for (int macdSignal = minMacdSignal; macdSignal <= maxMacdSignal; macdSignal++)
                {
                    Dictionary<CandlesParameters, double> macdLineDic, signalLineDic;

                    CalculateMACD(parameters, macdShort, macdLong, macdSignal, out macdLineDic, out signalLineDic);

                    macdMACDValues[macdShort][macdLong].Add(macdSignal, new Dictionary<CandlesParameters, double>(macdLineDic));
                    macdSignalValues[macdShort][macdLong].Add(macdSignal, new Dictionary<CandlesParameters, double>(signalLineDic));

                    count++;
                    Debug.Log($"데이터 생성중 :: {count} / {counta} = {macdMACDValues[macdShort][macdLong][macdSignal][parameters[2]]} / {macdSignalValues[macdShort][macdLong][macdSignal][parameters[2]]}");
                    yield return null;
                }
            }
        }

        #endregion

        if (retest)
        {
            Retest();
        }
        else
        {
            StartBackTestNew();
            //StartCoroutine(BackTesting_Guide(4, 8, 5, 15, 10, 28, 5));
        }
    }

    #region 지표 계산
    private void CalculateRSI(List<CandlesParameters> parameters, int power, out Dictionary<CandlesParameters, float> rsiValues)
    {
        rsiValues = new Dictionary<CandlesParameters, float>();

        List<decimal> Us = new List<decimal>();
        List<decimal> Ds = new List<decimal>();

        decimal a = (decimal)1 / (1 + (power - 1));  // 지수 이동 평균의 정식 공식은 a = 2 / 1 + day 이지만 업비트에서 사용하는 수식은 a = 1 / (1 + (day - 1))


        // 기간내 상승/하락 계산
        for (int i = 1; i < parameters.Count; i++)
        {
            decimal change = (decimal)parameters[i - 1].trade_price - (decimal)parameters[i].trade_price;
            if (change > 0)
            {
                Us.Add(change);
                Ds.Add(0);
            }
            else
            {
                Us.Add(0);
                Ds.Add(-change);                
            }
        }

        // 첫 번째 RSI 값 계산
        for (int i = 0; i < parameters.Count - power; i++)
        {
            decimal sumU = 0, sumD = 0;
            decimal AU, AD;
            decimal RS;
            float RSI;

            for (int j = i + 199; j >= i; j--)
            {
                if(j >= Us.Count)
                {
                    continue;
                }

                sumU = (Us[j] * a) + (sumU * (1 - a));
                sumD = (Ds[j] * a) + (sumD * (1 - a));
            }

            AU = sumU / power;
            AD = sumD / power;

            if (AD <= 0)
            {
                RSI = 100.0f;
            }
            else
            {
                RS = AU / AD;
                RSI = (float)(100 - (100 / (1 + RS))); //((RS / (1 + RS)) * (decimal)100.0);
            }

            rsiValues.Add(parameters[i], RSI);
        }
    }

    private void CalculateStochasticSlow(List<CandlesParameters> parameters, int k, int d, int power, out Dictionary<CandlesParameters, float> kValues, out Dictionary<CandlesParameters, float> dValues)
    {
        kValues = new Dictionary<CandlesParameters, float>();
        dValues = new Dictionary<CandlesParameters, float>();

        List<float> kValueList = new List<float>();
        List<float> dValueList = new List<float>();

        decimal a = (decimal)1 / (1 + (power - 1));

        //%K 값 구하기
        for (int i = 0; i < parameters.Count - power; i++)
        {
            //기간내 최저, 최고값 산출
            decimal low = decimal.MaxValue, high = decimal.MinValue;
            for (int j = i; j < power + i; j++)
            {
                if ((decimal)parameters[j].low_price < low)
                    low = (decimal)parameters[j].low_price;
                if ((decimal)parameters[j].high_price > high)
                    high = (decimal)parameters[j].high_price;
            }

            float kk = 100f;
            if ((high - low) > 0)
            {
                kk = (float)(((decimal)parameters[i].trade_price - low) / (high - low)) * 100f;
            }                
            kValueList.Add(kk);
        }

        List<float> slowKValueList = new List<float>();
        for (int i = 0; i < kValueList.Count - k; i++)
        {
            float tt = 0;
            for (int j = i; j < i + k; j++)
            {
                tt += kValueList[j];
            }
                        
            slowKValueList.Add(tt/k);
        }

        // %D 값 계산        
        for (int i = 0; i < slowKValueList.Count - d; i++)
        {
            float sumSlowK = 0;

            for (int j = i; j < i + d; j++)
            {
                sumSlowK += slowKValueList[j];
            }

            dValueList.Add(sumSlowK / d);
        }   

        for (int i = 0; i < dValueList.Count; i++)
        {
            kValues.Add(parameters[i], slowKValueList[i]);
            dValues.Add(parameters[i], dValueList[i]);

            //Debug.Log($"Stochastic = {parameters[i].candle_date_time_kst} - k: {kValueList[i]} / d: {dValueList[i]} / {slowKValueList[i]}");
        }
    }

    private void CalculateMACD(List<CandlesParameters> parameters, int shortPeriod, int longPeriod, int signalPeriod, 
        out Dictionary<CandlesParameters, double> macdLines, out Dictionary<CandlesParameters, double> signalLines)
    {
        macdLines = new Dictionary<CandlesParameters, double>();
        signalLines = new Dictionary<CandlesParameters, double>();

        List<double> macdLineList = new List<double>();  

        List<double> shortEMA = CalculateEMA(parameters, shortPeriod);
        List<double> longEMA = CalculateEMA(parameters, longPeriod);

        // MACD 라인 계산
        for (int i = 0; i < parameters.Count; i++)
        {
            macdLineList.Add(shortEMA[i] - longEMA[i]);
        }

        //MACD값 저장
        for (int i = 0; i < macdLineList.Count; i++)
        {
            macdLines.Add(parameters[i], macdLineList[i]);
        }

        // 시그널 라인 계산        
        List<double> signalLineList = CalculateEMA(macdLineList, signalPeriod);

        for (int i = 0; i < signalLineList.Count; i++)
        {
            signalLines.Add(parameters[i], signalLineList[i]);            
        }        
    }

    private List<double> CalculateEMA(List<CandlesParameters> parameters, int period)
    {
        List<double> emaValues = new List<double>();
        decimal multiplier = (decimal)2 / (1 + period);

        decimal ema = (decimal)parameters[parameters.Count - 1].trade_price;
        emaValues.Add((double)ema);

        for (int i = parameters.Count - 2; i >= 0; i--)
        {
            ema = ((decimal)parameters[i].trade_price * multiplier) + (ema * (1 - multiplier));
            emaValues.Add((double)ema);
        }

        emaValues.Reverse();
        return emaValues;
    }

    private List<double> CalculateEMA(List<double> prices, int period)
    {
        List<double> emaValues = new List<double>();
        //decimal multiplier = (decimal)1 / (1 + (period - 1));
        decimal multiplier = (decimal)2 / (1 + period);

        decimal ema = (decimal)prices[prices.Count - 1];
        emaValues.Add((double)ema);

        for (int i = prices.Count - 2; i >= 0; i--)
        {
            ema = ((decimal)prices[i] * multiplier) + (ema * (1 - multiplier));
            emaValues.Add((double)ema);
        }
        emaValues.Reverse();
        return emaValues;
    }

    private List<float> CalculateEMA(List<float> prices, int period)
    {
        List<float> emaValues = new List<float>();
        decimal multiplier = (decimal)2 / (1 + period);

        decimal ema = (decimal)prices[prices.Count - 1];
        emaValues.Add((float)ema);

        for (int i = prices.Count - 2; i >= 0; i--)
        {
            ema = ((decimal)prices[i] * multiplier) + (ema * (1 - multiplier));
            emaValues.Add((float)ema);
        }
        emaValues.Reverse();
        return emaValues;
    }
    #endregion
}

public struct CalcTrade : IJob
{
    public int RsiPeriod; // RSI 기간
    public int StochasticK; // 스토캐스틱 기간
    public int StochasticD; // 스토캐스틱 기간
    public int StochasticPeriod; // 스토캐스틱 기간
    public int MacdShortPeriod; // MACD 단기 이동평균 기간
    public int MacdLongPeriod; // MACD 장기 이동평균 기간
    public int MacdSignalPeriod; // MACD 시그널 라인 기간    

    public float OverBuyPeriod;
    public float OverSellPeriod;

    public float GuideRsiPeriod;

    public double ProfitRate;

    double money;
    double beforeMoney;

    float tradeCount;
    int failCount;

    double buyPrice;
    double buyUnitCount;

    double lossCutLine;
    double profitCutLine;

    DateTime buyDateTime;
    DateTime sellDateTime;

    public void Execute()
    {
        throw new NotImplementedException();
    }
}