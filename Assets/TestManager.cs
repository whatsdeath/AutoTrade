using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Unity.Jobs;
using UnityEngine.SocialPlatforms.Impl;

public class TestManager : BaseManager<TestManager>
{
    private MarketAll marketAll;
    private CandleDataDownload dataDownload;
    private MarketDataSave dataSave;

    public List<string> marketList { get => marketAll.marketList; }

    public Dictionary<string, List<CandleData>> datasByMarket = new Dictionary<string, List<CandleData>>();


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
            //BacktestStochastic(true, 3, 7, 5, 11, 80, 25, 50, 12, 30, 5, 1.6f);

            //{ k} / { d} / { stPower} 
            // {rsiPower} 
            // {macdShort} / {macdLong} / {macdSignal} 
            // {overBuy} / {overSell} / {guideRSI} 
            // {profita}", TelegramBotType.BackTest);
            //StartBackTestPhase2(5, 5, 8, 11);

            StartCoroutine(DataSetting(true, true, false, true, false));
            //StartCoroutine(BackTesting_Guide(3, 10, 7, 18, 12, 21, 6));

            //StartCoroutine(BackTesting_ProfitRate(3, 10, 7, 18, 12, 21, 6, 72, 20, 48));
            //StartCoroutine(BackTesting_ProfitRate(3, 10, 7, 18, 12, 21, 6, 72, 20, 49));

        }
        //BackTest.BackTestingCoverRSI(prices);
        //BackTest.BackTestingCoverRSI(prices, 34.7f, 66.6f, 7);
    }
    string tmarket { get => "POLYX"; }
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

    public int RsiPeriod = 14; // isRSI 기간
    public int StochasticK = 3; // 스토캐스틱 기간
    public int StochasticD = 3; // 스토캐스틱 기간
    public int StochasticPeriod = 14; // 스토캐스틱 기간
    public int MacdShortPeriod = 12; // isMACD 단기 이동평균 기간
    public int MacdLongPeriod = 26; // isMACD 장기 이동평균 기간
    public int MacdSignalPeriod = 9; // isMACD 시그널 라인 기간    
    public int TradePricePeriod = 20;

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


    DateTime buyDateTime = new DateTime();
    DateTime sellDateTime = new DateTime();


    public void StartBackTestNew()
    {
        StartCoroutine(BackTesting_Stochastic());
    }

    IEnumerator BackTesting_Stochastic()
    {
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


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        int powera = 0;
        int ka = 0;
        int da = 0;
        //int macdSignalPerioda = 0;

        for (int k = minStochasticK; k <= maxStochasticK; k++)
        {
            for (int d = minStochasticD; d <= maxStochasticD; d++)
            {
                for (int power = minStochasticPower; power <= maxStochasticPower; power++)
                {
                    count++;
                    float winRate = 0.0f;
                    var aa = BacktestStochastic(out winRate, k, d, power);

                    if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
                    {
                        winRatea = winRate;
                        maxMoney = aa;
                        powera = power;
                        ka = k;
                        da = d;
                        //macdSignalPerioda = macdSignal;
                    }
                    Debug.Log($"{count} / {counta} ::: {aa} / {winRate} / {k} / {d} / {power}");
                    yield return null;


                }
            }
        }

        AppManager.Instance.TelegramMassage($"[{testMarket}] Stochastic [{count}/{counta}] {maxMoney} / {winRatea} ::: {ka} / {da} / {powera}", TelegramBotType.BackTest);

        StartCoroutine(BackTesting_RSI(ka, da, powera));
    }

    private double BacktestStochastic(out float winRate, int stochasticK = 3, int stochasticD = 3, int stochasticPeriod = 14) // 2% 손절

    {
        //변수 세팅
        StochasticK = stochasticK; // 스토캐스틱 기간
        StochasticD = stochasticD; // 스토캐스틱 기간
        StochasticPeriod = stochasticPeriod; // 스토캐스틱 기간

        //자금계산을 위한 변수모음
        money = 3000000;
        beforeMoney = 0;

        tradeCount = 0;
        failCount = 0;

        buyPrice = 0;
        buyUnitCount = 0;

        #region tresh
        /*
        if (!stochasticKValues.ContainsKey(stochasticK))
        {
            Dictionary<CandlesParameters, float> kValueDic, dValueDic;
            CalculateStochasticSlow(parameters, stochasticK, stochasticD, stochasticPeriod, out kValueDic, out dValueDic);

            stochasticKValues.Add(stochasticK, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticKValues[stochasticK].Add(stochasticD, new Dictionary<int, Dictionary<CandlesParameters, float>>());
            stochasticKValues[stochasticK][stochasticD].Add(stochasticPeriod, new Dictionary<CandlesParameters, float>(kValueDic));

            stochasticDValues.Add(stochasticK, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticDValues[stochasticK].Add(stochasticD, new Dictionary<int, Dictionary<CandlesParameters, float>>());
            stochasticDValues[stochasticK][stochasticD].Add(stochasticPeriod, new Dictionary<CandlesParameters, float>(dValueDic));

            Dictionary<CandlesParameters, float> rsiValueDic;
            CalculateRSI(parameters, rsiPeriod, out rsiValueDic);

            tradePriceValues.Add(rsiPeriod, new Dictionary<CandlesParameters, float>(rsiValueDic));


            Dictionary<CandlesParameters, double> macdLineDic, signalLineDic;
            CalculateMACD(parameters, macdShortPeriod, macdLongPeriod, macdSignalPeriod, out macdLineDic, out signalLineDic);

            macdMACDValues.Add(macdShortPeriod, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdMACDValues[macdShortPeriod].Add(macdLongPeriod, new Dictionary<int, Dictionary<CandlesParameters, double>>());
            macdMACDValues[macdShortPeriod][macdLongPeriod].Add(macdSignalPeriod, new Dictionary<CandlesParameters, double>(macdLineDic));

            macdSignalValues.Add(macdShortPeriod, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdSignalValues[macdShortPeriod].Add(macdLongPeriod, new Dictionary<int, Dictionary<CandlesParameters, double>>());
            macdSignalValues[macdShortPeriod][macdLongPeriod].Add(macdSignalPeriod, new Dictionary<CandlesParameters, double>(signalLineDic));
        }*/

        //Debug.Log("오디ㄲㅈ ㅇㄴㅂㅈ");
        #endregion

        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            //구매한게 있으면 매도관련 판단
            if (buyUnitCount > 0)
            {
                if (ChkSellConditionStochastic(parameters,
                    stochasticKValues[StochasticK][StochasticD][StochasticPeriod],
                    stochasticDValues[StochasticK][StochasticD][StochasticPeriod], i))
                {
                    money += (parameters[i].trade_price * buyUnitCount) * penalty;
                    sellDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);

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
            else
            //구매한게 없으면 매수관련 판단
            {
                if (ChkBuyConditionStochastic(parameters,
                    stochasticKValues[StochasticK][StochasticD][StochasticPeriod],
                    stochasticDValues[StochasticK][StochasticD][StochasticPeriod], i))
                {
                    buyPrice = parameters[i].trade_price;
                    buyUnitCount = (money / buyPrice) * penalty;
                    beforeMoney = money;
                    money = 0;
                    buyDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);
                    tradeCount++;
                }
            }
        }

        Debug.Log($"최종액수 {money} 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}");


        winRate = 1 - (failCount / tradeCount);
        return money + (buyPrice * buyUnitCount);
    }

    private bool ChkSellConditionStochastic(List<CandlesParameters> parameters,
        Dictionary<CandlesParameters, float> kValues,
        Dictionary<CandlesParameters, float> dValues, int index)
    {
        if (!kValues.ContainsKey(parameters[index]) || !dValues.ContainsKey(parameters[index]))
        {
            return false;
        }

        if (kValues[parameters[index]] >= 80.0f && (kValues[parameters[index]] > dValues[parameters[index]]))
        {
            return true;
        }

        return false;
    }


    private bool ChkBuyConditionStochastic(List<CandlesParameters> parameters,
        Dictionary<CandlesParameters, float> kValues,
        Dictionary<CandlesParameters, float> dValues,
        int index)
    {
        if (!kValues.ContainsKey(parameters[index]) || !dValues.ContainsKey(parameters[index]))
        {
            return false;
        }

        if ((kValues[parameters[index]] <= 20.0f && dValues[parameters[index]] <= 20.0f) && (kValues[parameters[index]] > dValues[parameters[index]]))
        {
            return true;
        }

        return false;
    }



    IEnumerator BackTesting_RSI(int k, int d, int stPower)
    {
        int counta = 0;

        for (int rsiLength = minRSIPower; rsiLength <= maxRSIPower; rsiLength++)
        {
            counta++;
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        int rsiLengtha = 0;

        for (int rsiLength = minRSIPower; rsiLength <= maxRSIPower; rsiLength++)
        {
            count++;
            float winRate;
            var aa = BacktestRSI(out winRate, rsiLength);

            if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
            {
                winRatea = winRate;
                maxMoney = aa;
                rsiLengtha = rsiLength;
            }
            Debug.Log($"{count} / {counta} ::: {aa} / {winRate} // {rsiLength}");
            yield return null;
        }

        AppManager.Instance.TelegramMassage($"[{testMarket}] RSI [{count}/{counta}] {maxMoney} / {winRatea} ::: {rsiLengtha}", TelegramBotType.BackTest);

        StartCoroutine(BackTesting_Final(k, d, stPower, rsiLengtha));
    }


    private double BacktestRSI(out float winRate, int rsiPeriod = 14)
    {
        RsiPeriod = rsiPeriod; // isRSI 기간

        //자금계산을 위한 변수모음
        money = 3000000;
        beforeMoney = 0;

        tradeCount = 0;
        failCount = 0;

        buyPrice = 0;
        buyUnitCount = 0;

        #region tresh
        /*
        if (!stochasticKValues.ContainsKey(stochasticK))
        {
            Dictionary<CandlesParameters, float> kValueDic, dValueDic;
            CalculateStochasticSlow(parameters, stochasticK, stochasticD, stochasticPeriod, out kValueDic, out dValueDic);

            stochasticKValues.Add(stochasticK, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticKValues[stochasticK].Add(stochasticD, new Dictionary<int, Dictionary<CandlesParameters, float>>());
            stochasticKValues[stochasticK][stochasticD].Add(stochasticPeriod, new Dictionary<CandlesParameters, float>(kValueDic));

            stochasticDValues.Add(stochasticK, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticDValues[stochasticK].Add(stochasticD, new Dictionary<int, Dictionary<CandlesParameters, float>>());
            stochasticDValues[stochasticK][stochasticD].Add(stochasticPeriod, new Dictionary<CandlesParameters, float>(dValueDic));

            Dictionary<CandlesParameters, float> rsiValueDic;
            CalculateRSI(parameters, rsiPeriod, out rsiValueDic);

            tradePriceValues.Add(rsiPeriod, new Dictionary<CandlesParameters, float>(rsiValueDic));


            Dictionary<CandlesParameters, double> macdLineDic, signalLineDic;
            CalculateMACD(parameters, macdShortPeriod, macdLongPeriod, macdSignalPeriod, out macdLineDic, out signalLineDic);

            macdMACDValues.Add(macdShortPeriod, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdMACDValues[macdShortPeriod].Add(macdLongPeriod, new Dictionary<int, Dictionary<CandlesParameters, double>>());
            macdMACDValues[macdShortPeriod][macdLongPeriod].Add(macdSignalPeriod, new Dictionary<CandlesParameters, double>(macdLineDic));

            macdSignalValues.Add(macdShortPeriod, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdSignalValues[macdShortPeriod].Add(macdLongPeriod, new Dictionary<int, Dictionary<CandlesParameters, double>>());
            macdSignalValues[macdShortPeriod][macdLongPeriod].Add(macdSignalPeriod, new Dictionary<CandlesParameters, double>(signalLineDic));
        }*/

        //Debug.Log("오디ㄲㅈ ㅇㄴㅂㅈ");
        #endregion

        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            //구매한게 있으면 매도관련 판단
            if (buyUnitCount > 0)
            {
                if (ChkSellConditionRSI(parameters, rsiValues[RsiPeriod], i))
                {
                    money += (parameters[i].trade_price * buyUnitCount) * penalty;
                    sellDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);

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
            else
            //구매한게 없으면 매수관련 판단
            {
                if (ChkBuyConditionRSI(parameters, rsiValues[RsiPeriod], i))
                {
                    buyPrice = parameters[i].trade_price;
                    buyUnitCount = (money / buyPrice) * penalty;
                    beforeMoney = money;
                    money = 0;
                    buyDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);
                    tradeCount++;
                }
            }
        }

        Debug.Log($"최종액수 {money} 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}");


        winRate = 1 - (failCount / tradeCount);
        return money + (buyPrice * buyUnitCount);
    }


    private bool ChkSellConditionRSI(List<CandlesParameters> parameters,
        Dictionary<CandlesParameters, float> rsiValues, int index)
    {
        if (!rsiValues.ContainsKey(parameters[index]))
        {
            return false;
        }

        if (rsiValues[parameters[index]] >= 70.0f)
        {
            return true;
        }

        return false;
    }


    private bool ChkBuyConditionRSI(List<CandlesParameters> parameters,
        Dictionary<CandlesParameters, float> rsiValues, int index)
    {
        if (!rsiValues.ContainsKey(parameters[index]))
        {
            return false;
        }

        if (rsiValues[parameters[index]] <= 30.0f)
        {
            return true;
        }

        return false;
    }



    IEnumerator BackTesting_Final(int k, int d, int stPower, int rsiLength)
    {
        int counta = 0;

        for (int tradePriceLength = minTradePrice; tradePriceLength <= maxTradePrice; tradePriceLength++)
        {
            for (float multi = 2.0f; multi <= 4.0f; multi = multi + 0.1f)
            {
                counta++;
            }
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        int tradePriceLengtha = int.MinValue;
        float multia = float.MinValue;

        for (int tradePriceLength = minTradePrice; tradePriceLength <= maxTradePrice; tradePriceLength++)
        {
            for (float multi = 2.0f; multi <= 4.0f; multi = multi + 0.1f)
            {
                count++;
                float winRate;
                var aa = BacktestFinal(out winRate, k, d, stPower, rsiLength, tradePriceLength, multi);

                if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
                {
                    winRatea = winRate;
                    maxMoney = aa;
                    tradePriceLengtha = tradePriceLength;
                    multia = multi;
                }
                Debug.Log($"{count} / {counta} ::: {aa} / {winRate} // {rsiLength}");
                yield return null;
            }
        }

        AppManager.Instance.TelegramMassage($"[{testMarket}] Final [{count}/{counta}] {maxMoney} / {winRatea} ::: {tradePriceLengtha} / {multia}", TelegramBotType.BackTest);

        //StartCoroutine(BackTesting_Guide(k, d, stPower, rsiPower, macdShorta, macdLonga, macdSignala));
    }


    private double BacktestFinal(out float winRate, int stochasticK = 3, int stochasticD = 3, int stochasticPeriod = 14, int rsiPeriod = 14, int tradePriceLength = 20, float multi = 3.0f)
    {
        //변수 세팅
        StochasticK = stochasticK; // 스토캐스틱 기간
        StochasticD = stochasticD; // 스토캐스틱 기간
        StochasticPeriod = stochasticPeriod; // 스토캐스틱 기간

        RsiPeriod = rsiPeriod; // isRSI 기간

        TradePricePeriod = tradePriceLength;

        //자금계산을 위한 변수모음
        money = 3000000;
        beforeMoney = 0;

        tradeCount = 0;
        failCount = 0;

        buyPrice = 0;
        buyUnitCount = 0;



        #region tresh
        /*
        if (!stochasticKValues.ContainsKey(stochasticK))
        {
            Dictionary<CandlesParameters, float> kValueDic, dValueDic;
            CalculateStochasticSlow(parameters, stochasticK, stochasticD, stochasticPeriod, out kValueDic, out dValueDic);

            stochasticKValues.Add(stochasticK, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticKValues[stochasticK].Add(stochasticD, new Dictionary<int, Dictionary<CandlesParameters, float>>());
            stochasticKValues[stochasticK][stochasticD].Add(stochasticPeriod, new Dictionary<CandlesParameters, float>(kValueDic));

            stochasticDValues.Add(stochasticK, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>());
            stochasticDValues[stochasticK].Add(stochasticD, new Dictionary<int, Dictionary<CandlesParameters, float>>());
            stochasticDValues[stochasticK][stochasticD].Add(stochasticPeriod, new Dictionary<CandlesParameters, float>(dValueDic));

            Dictionary<CandlesParameters, float> rsiValueDic;
            CalculateRSI(parameters, rsiPeriod, out rsiValueDic);

            tradePriceValues.Add(rsiPeriod, new Dictionary<CandlesParameters, float>(rsiValueDic));


            Dictionary<CandlesParameters, double> macdLineDic, signalLineDic;
            CalculateMACD(parameters, macdShortPeriod, macdLongPeriod, macdSignalPeriod, out macdLineDic, out signalLineDic);

            macdMACDValues.Add(macdShortPeriod, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdMACDValues[macdShortPeriod].Add(macdLongPeriod, new Dictionary<int, Dictionary<CandlesParameters, double>>());
            macdMACDValues[macdShortPeriod][macdLongPeriod].Add(macdSignalPeriod, new Dictionary<CandlesParameters, double>(macdLineDic));

            macdSignalValues.Add(macdShortPeriod, new Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>());
            macdSignalValues[macdShortPeriod].Add(macdLongPeriod, new Dictionary<int, Dictionary<CandlesParameters, double>>());
            macdSignalValues[macdShortPeriod][macdLongPeriod].Add(macdSignalPeriod, new Dictionary<CandlesParameters, double>(signalLineDic));
        }*/

        //Debug.Log("오디ㄲㅈ ㅇㄴㅂㅈ");
        #endregion

        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            int score = 0;

            //구매한게 있으면 매도관련 판단
            if (buyUnitCount > 0)
            {
                if (ChkSellConditionStochastic(parameters,
                    stochasticKValues[StochasticK][StochasticD][StochasticPeriod],
                    stochasticDValues[StochasticK][StochasticD][StochasticPeriod], i))
                {
                    score++;
                }


                if (ChkSellConditionRSI(parameters, rsiValues[RsiPeriod], i))
                {
                    score++;
                }

                if (ChkSellConditionFinal(parameters, tradePriceAvg[tradePriceLength], multi, i))
                {
                    score++;
                }

                if (score >= 2)
                {
                    money += (parameters[i].trade_price * buyUnitCount) * penalty;
                    sellDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);

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
            else
            //구매한게 없으면 매수관련 판단
            {
                if (ChkBuyConditionStochastic(parameters,
                    stochasticKValues[StochasticK][StochasticD][StochasticPeriod],
                    stochasticDValues[StochasticK][StochasticD][StochasticPeriod], i))
                {
                    score++;
                }

                if (ChkBuyConditionRSI(parameters, rsiValues[RsiPeriod], i))
                {
                    score++;
                }

                if (ChkBuyConditionFinal(parameters, tradePriceAvg[tradePriceLength], multi, i))
                {
                    score++;
                }

                if (score >= 2)
                {
                    buyPrice = parameters[i].trade_price;
                    buyUnitCount = (money / buyPrice) * penalty;
                    beforeMoney = money;
                    money = 0;
                    buyDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);
                    tradeCount++;
                }
            }
        }

        Debug.Log($"최종액수 {money} 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}");


        winRate = 1 - (failCount / tradeCount);
        return money + (buyPrice * buyUnitCount);
    }


    private bool ChkSellConditionFinal(List<CandlesParameters> parameters,
        Dictionary<CandlesParameters, double> tradePriceValues, float multi, int index)
    {
        if (!tradePriceValues.ContainsKey(parameters[index]))
        {
            return false;
        }

        if (parameters[index].candle_acc_trade_price >= tradePriceValues[parameters[index]] * multi)
        {
            return true;
        }

        return false;
    }


    private bool ChkBuyConditionFinal(List<CandlesParameters> parameters,
        Dictionary<CandlesParameters, double> tradePriceValues, float multi, int index)
    {
        if (!tradePriceValues.ContainsKey(parameters[index]))
        {
            return false;
        }

        if (parameters[index].candle_acc_trade_price >= tradePriceValues[parameters[index]] * multi)
        {
            return true;
        }

        return false;
    }






    //Stochastic
    int minStochasticK = 3, maxStochasticK = 10;
    int minStochasticD = 3, maxStochasticD = 10;
    int minStochasticPower = 5, maxStochasticPower = 20;

    //isRSI
    int minRSIPower = 10, maxRSIPower = 20;

    //isMACD
    int minMacdShort = 10, maxMacdShort = 20;
    int minMacdLong = 20, maxMacdLong = 30;
    int minMacdSignal = 5, maxMacdSignal = 15;

    //TradePrice
    int minTradePrice = 20, maxTradePrice = 40;


    Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>>
        stochasticKValues = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>>(),
        stochasticDValues = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, float>>>>();

    Dictionary<int, Dictionary<CandlesParameters, float>>
        rsiValues = new Dictionary<int, Dictionary<CandlesParameters, float>>();


    Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>>
        macdMACDValues = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>>(),
        macdSignalValues = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<CandlesParameters, double>>>>();


    Dictionary<int, Dictionary<CandlesParameters, double>>
        tradePriceAvg = new Dictionary<int, Dictionary<CandlesParameters, double>>();


    IEnumerator DataSetting(bool isStochastic, bool isRSI, bool isMACD, bool isTradePriceAvg, bool retest)
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
        if (isStochastic)
        {
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
        }
        #endregion

        #region RSI
        if (isRSI)
        {
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
        }
        #endregion

        #region MACD
        if (isMACD)
        {
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
        }
        #endregion

        #region VolumeAvg
        if (isTradePriceAvg)
        {
            tradePriceAvg.Clear();

            for (int tradePrice = minTradePrice; tradePrice <= maxTradePrice; tradePrice++)
            {
                Dictionary<CandlesParameters, double> tradePriceAvgDic;

                CalculateEMATradePriceAvg(parameters, tradePrice, out tradePriceAvgDic);

                tradePriceAvg.Add(tradePrice, new Dictionary<CandlesParameters, double>(tradePriceAvgDic));
            }

        }

        #endregion



        if (retest)
        {
            //Retest();
        }
        else
        {
            StartBackTestNew();
            //StartCoroutine(BackTesting_Guide(4, 8, 5, 15, 10, 28, 5));
            //StartCoroutine(BackTesting_Guide(3, 10, 7, 18, 12, 21, 6));
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

        // 첫 번째 isRSI 값 계산
        for (int i = 0; i < parameters.Count - power; i++)
        {
            decimal sumU = 0, sumD = 0;
            decimal AU, AD;
            decimal RS;
            float RSI;

            for (int j = i + 199; j >= i; j--)
            {
                if (j >= Us.Count)
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

            slowKValueList.Add(tt / k);
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

        List<double> shortEMA = CalculateEMAPrice(parameters, shortPeriod);
        List<double> longEMA = CalculateEMAPrice(parameters, longPeriod);

        // isMACD 라인 계산
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

    private List<double> CalculateEMATradePriceAvg(List<CandlesParameters> parameters, int period, out Dictionary<CandlesParameters, double> tradePrices)
    {
        tradePrices = new Dictionary<CandlesParameters, double>();

        List<double> emaValues = new List<double>();
        decimal multiplier = (decimal)2 / (1 + period);

        decimal ema = (decimal)parameters[parameters.Count - 1].candle_acc_trade_price;
        emaValues.Add((double)ema);

        for (int i = parameters.Count - 2; i >= 0; i--)
        {
            ema = ((decimal)parameters[i].candle_acc_trade_price * multiplier) + (ema * (1 - multiplier));
            emaValues.Add((double)ema);
        }

        emaValues.Reverse();

        for (int i = 0; i < emaValues.Count; i++)
        {
            tradePrices.Add(parameters[i], emaValues[i]);
        }
        return emaValues;
    }

    private List<double> CalculateEMAPrice(List<CandlesParameters> parameters, int period)
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
    public int RsiPeriod; // isRSI 기간
    public int StochasticK; // 스토캐스틱 기간
    public int StochasticD; // 스토캐스틱 기간
    public int StochasticPeriod; // 스토캐스틱 기간
    public int MacdShortPeriod; // isMACD 단기 이동평균 기간
    public int MacdLongPeriod; // isMACD 장기 이동평균 기간
    public int MacdSignalPeriod; // isMACD 시그널 라인 기간    

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