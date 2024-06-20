using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Google.Rpc.Context.AttributeContext.Types;

public class TestManager : BaseManager<TestManager>
{
    private MarketAll marketAll;
    private CandleDataDownload dataDownload;
    private MarketDataSave dataSave;

    public List<string> marketList { get => marketAll.marketList; }

    public List<CandlesParameters> parameters = new List<CandlesParameters>();

    private MarketList currentTestMarket = MarketList.MATIC;

    bool isCurrentTestEnd = true;
    bool isTestMode { get => TimeManager.Instance.processSequence.Equals(ProcessSequence.BackTestPhase)
            && TradeManager.Instance.isReady && TradeManager.Instance.tradeMode; }


    TradingParameters testParameter = new TradingParameters();

    //Stochastic

    int minStochasticK { get => TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticK - 3 >= 3 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticK - 3 : 3; }
    int maxStochasticK { get => TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticK + 3 <= 15 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticK + 3 : 15; }
    int minStochasticD { get => TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticD - 3 >= 3 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticD - 3 : 3; }
    int maxStochasticD { get => TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticD + 3 <= 15 ?
            TradeManager.Instance.conditionByMarket [currentTestMarket].stochasticD + 3 : 15; }

    int minStochasticPower { get => TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticStrength - 5 >= 5 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticStrength - 5 : 5; }
    int maxStochasticPower { get => TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticStrength + 5 <= 20 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].stochasticStrength + 5 : 20; }
    /*

    int minStochasticK = 3, maxStochasticK = 15;
    int minStochasticD = 5, maxStochasticD = 15;
    int minStochasticPower = 5, maxStochasticPower = 20;*/

    //isRSI
    int minRSIPower { get => TradeManager.Instance.conditionByMarket[currentTestMarket].rsiStrength - 10 >= 10 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].rsiStrength - 5 : 10; }
    int maxRSIPower { get => TradeManager.Instance.conditionByMarket[currentTestMarket].rsiStrength + 10 <= 20 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].rsiStrength + 5 : 20; }

    int minSellRSIPower
    {
        get => TradeManager.Instance.conditionByMarket[currentTestMarket].rsiStrength - 10 >= 10 ?
        TradeManager.Instance.conditionByMarket[currentTestMarket].rsiStrength - 5 : 10;
    }
    int maxSellRSIPower
    {
        get => TradeManager.Instance.conditionByMarket[currentTestMarket].rsiStrength + 10 <= 20 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].rsiStrength + 5 : 20;
    }

    //int minRSIPower = 10, maxRSIPower = 30;

    //isMACD    
    int minMacdShort = 10, maxMacdShort = 20;
    int minMacdLong = 20, maxMacdLong = 30;
    int minMacdSignal = 5, maxMacdSignal = 15;

    //TradePrice
    int minTradePrice { get => TradeManager.Instance.conditionByMarket[currentTestMarket].tradePriceEMALength - 10 >= 20 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].tradePriceEMALength - 10 : 20; }
    int maxTradePrice { get => TradeManager.Instance.conditionByMarket[currentTestMarket].tradePriceEMALength + 10 <= 50 ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].tradePriceEMALength + 10 : 50; }

    float minTradeMultiple { get => TradeManager.Instance.conditionByMarket[currentTestMarket].tradePriceConditionMul - 1.0f >= 1.0f ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].tradePriceConditionMul - 1.0f : 1.0f; }
    float maxTradeMultiple { get => TradeManager.Instance.conditionByMarket[currentTestMarket].tradePriceConditionMul + 1.0f <= 3.0f ?
            TradeManager.Instance.conditionByMarket[currentTestMarket].tradePriceConditionMul + 1.0f : 3.0f; }
    /*
    int minTradePrice = 20, maxTradePrice = 50;
    float minTradeMultiple = 1.5f, maxTradeMultiple = 4.0f*/

    protected override void Init()
    {
        marketAll = CreateComponentObjectInChildrenAndReturn<MarketAll>();
        dataDownload = CreateComponentObjectInChildrenAndReturn<CandleDataDownload>();
        dataSave = CreateComponentObjectInChildrenAndReturn<MarketDataSave>();
    }

    string currentTestMarketName { get => currentTestMarket.ToString(); }
    string currentTestMarketFullname { get => $"KRW-{currentTestMarketName}"; }

    float penalty { get => 0.998f; }

    #region data  
    public void DataDownload()
    {
        if (isTestMode && isCurrentTestEnd)
        {
            string massege = $"{AppManager.Instance.machineName}({AppManager.Instance.ip})\n[{TimeManager.Instance.nowTime}]\n";

            System.GC.Collect();

            massege += $"<b>{currentTestMarketName}의 테스트를 시작합니다.. </b>";
            AppManager.Instance.TelegramMassage(massege, TelegramBotType.BackTest);

            isCurrentTestEnd = false;
            dataDownload.StartCandleDataDownload(currentTestMarketFullname);
        }
    }

    public void DataSaveAndTestStart(string market, List<CandlesParameters> candleDatas)
    {
        dataSave.CandleSave(market, candleDatas);

        parameters = candleDatas.ToList();
        parameters.Sort((p1, p2) => p2.timestamp.CompareTo(p1.timestamp));

        Debug.Log(parameters.Count);

        StartCoroutine(DataSetting(true, true, false, true, false));
    }

    bool isRetest;
    public void TestComplete()
    {
        string massege = $"{AppManager.Instance.machineName}({AppManager.Instance.ip})\n[{TimeManager.Instance.nowTime}]\n";

        massege += $"<b>{currentTestMarketName}의 테스트가 완료되었습니다.</b>";
        AppManager.Instance.TelegramMassage(massege, TelegramBotType.BackTest);
        isCurrentTestEnd = true;

        stochasticKValues.Clear();
        stochasticDValues.Clear();
        rsiValues.Clear();
        macdMACDValues.Clear();
        macdSignalValues.Clear();
        tradePriceAvg.Clear();        

        if (!isRetest)
        {
            currentTestMarket++;
        }

        isRetest = false;

        if(currentTestMarket.Equals(MarketList.MaxCount))
        {
            currentTestMarket = 0;
        }
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

    double lossCutLine = 0.0f;
    double profitCutLine = 0.0f;

    DateTime buyDateTime = new DateTime();
    DateTime sellDateTime = new DateTime();


    public void StartBackTestNew()
    {
        testParameter = new TradingParameters();

        testParameter.name = currentTestMarketName;

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
                    while (!isTestMode)
                    {
                        yield return null;
                    }

                    count++;
                    float winRate = 0.0f;
                    var aa = BacktestStochastic(out winRate, k, d, power);

                    //if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
                    if (maxMoney < aa)
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

        AppManager.Instance.TelegramMassage($"[{currentTestMarketFullname}] Stochastic [{count}/{counta}] {maxMoney} / {winRatea} ::: {ka} / {da} / {powera}", TelegramBotType.BackTest);

        if (count.Equals(0))
        {
            testParameter.amountStochastic = 0;
            testParameter.winRateStochastic = 0;

            testParameter.stochasticK = 15;
            testParameter.stochasticD = 15;
            testParameter.stochasticStrength = 20;

            isRetest = true;
        }
        else
        {
            testParameter.amountStochastic = maxMoney;
            testParameter.winRateStochastic = winRatea;

            testParameter.stochasticK = ka;
            testParameter.stochasticD = da;
            testParameter.stochasticStrength = powera;
        }

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
            for (int rsiSellLength = minSellRSIPower; rsiSellLength <= maxSellRSIPower; rsiSellLength++)
            {
                counta++;
            }
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        int rsiLengtha = 0;

        for (int rsiLength = minRSIPower; rsiLength <= maxRSIPower; rsiLength++)
        {
            for (int rsiSellLength = minSellRSIPower; rsiSellLength <= maxSellRSIPower; rsiSellLength++)
            {
                while (!isTestMode)
                {
                    yield return null;
                }

                count++;
                float winRate;
                var aa = BacktestRSI(out winRate, rsiLength);

                //if (winRatea < winRate || (winRatea == winRate && maxMoney < aa))
                if (maxMoney < aa)
                {
                    winRatea = winRate;
                    maxMoney = aa;
                    rsiLengtha = rsiLength;
                }
                Debug.Log($"{count} / {counta} ::: {aa} / {winRate} // {rsiLength}");
                yield return null;
            }
        }

        AppManager.Instance.TelegramMassage($"[{currentTestMarketFullname}] RSI [{count}/{counta}] {maxMoney} / {winRatea} ::: {rsiLengtha}", TelegramBotType.BackTest);

        if (count.Equals(0))
        {
            testParameter.amountRSI = 0;
            testParameter.winRateRSI = 0;

            testParameter.rsiStrength = 20;

            isRetest = true;
        }
        else
        {
            testParameter.amountRSI = maxMoney;
            testParameter.winRateRSI = winRatea;

            testParameter.rsiStrength = rsiLengtha;
        }

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
            for (float multi = minTradeMultiple; multi <= maxTradeMultiple; multi = multi + 0.1f)
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
            for (float multi = minTradeMultiple; multi <= maxTradeMultiple; multi = multi + 0.1f)
            {
                while (!isTestMode)
                {
                    yield return null;
                }

                count++;
                float winRate;
                var aa = BacktestFinal(out winRate, k, d, stPower, rsiLength, tradePriceLength, multi);

                if (maxMoney < aa)
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

        AppManager.Instance.TelegramMassage($"[{currentTestMarketFullname}] Final [{count}/{counta}] {maxMoney} / {winRatea} ::: {tradePriceLengtha} / {multia}", TelegramBotType.BackTest);

        if (count.Equals(0))
        {
            testParameter.amountStoRsiTrade = 0;
            testParameter.winRateStoRsiTrade = 0;

            testParameter.tradePriceEMALength = 30;
            testParameter.tradePriceConditionMul = 2;

            isRetest = true;
        }
        else
        {
            testParameter.amountStoRsiTrade = maxMoney;
            testParameter.winRateStoRsiTrade = winRatea;

            testParameter.tradePriceEMALength = tradePriceLengtha;
            testParameter.tradePriceConditionMul = multia;
        }

        TradingParameters tradingParameters = new TradingParameters(testParameter);

        TradeManager.Instance.SetConditionByMarket(currentTestMarket, tradingParameters);    
        AppManager.Instance.SaveData(currentTestMarketName, tradingParameters);

        TestComplete();

        //StartCoroutine(BackTesting_Final2(k, d, stPower, rsiLength, tradePriceLengtha, multia));
    }


    double maxPrice;
    bool isDefence;

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

        maxPrice = 0;
        isDefence = false;

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



    IEnumerator BackTesting_Final2(int k, int d, int stPower, int rsiLength, int tradePriceLength, float multi)
    {
        int counta = 0;

        for (float lossCut = 0.5f; lossCut <= 2.0f; lossCut += 0.1f)
        {
            for (float profitCut = 0.5f; profitCut <= 2.0f; profitCut += 0.1f)
            {
                counta++;
            }
        }


        int count = 0;

        double maxMoney = double.MinValue;
        float winRatea = float.MinValue;
        float lossCuta = float.MinValue;
        float profitCuta = float.MinValue;

        for (float lossCut = 0.5f; lossCut <= 5.0f; lossCut += 0.1f)
        {
            for (float profitCut = 0.5f; profitCut <= 0.5f; profitCut += 0.1f)
            {
                count++;
                float winRate;
                var aa = BacktestFinal2(out winRate, k, d, stPower, rsiLength, tradePriceLength, multi, lossCut, profitCut);

                if (maxMoney < aa)
                {
                    winRatea = winRate;
                    maxMoney = aa;
                    lossCuta = lossCut;
                    profitCuta = profitCut;
                }
                Debug.Log($"{count} / {counta} ::: {aa} / {winRate} // {rsiLength}");
                yield return null;
            }
        }

        AppManager.Instance.TelegramMassage($"[{currentTestMarketFullname}] Final [{count}/{counta}] {maxMoney} / {winRatea} ::: {tradePriceLength} / {multi} ::: {lossCuta} / {profitCuta}", TelegramBotType.BackTest);
        /*
        AppManager.Instance.SaveData(currentTestMarketName, new TradingParameters
        {
            name = currentTestMarketName,

            stochasticK = k,
            stochasticD = d,
            stochasticStrength = stPower,

            rsiStrength = rsiLength,

            tradePriceEMALength = tradePriceLengtha,
            tradePriceConditionMul = multia
        });*/
        //StartCoroutine(BackTesting_Guide(k, d, stPower, rsiPower, macdShorta, macdLonga, macdSignala));
    }

    float lossCut;
    float profitCut;

    private double BacktestFinal2(out float winRate, int stochasticK = 3, int stochasticD = 3, int stochasticPeriod = 14, int rsiPeriod = 14, int tradePriceLength = 20, float multi = 3.0f, float losscut = 0.5f, float profitcut = 1.0f)
    {
        //변수 세팅
        StochasticK = stochasticK; // 스토캐스틱 기간
        StochasticD = stochasticD; // 스토캐스틱 기간
        StochasticPeriod = stochasticPeriod; // 스토캐스틱 기간

        RsiPeriod = rsiPeriod; // isRSI 기간

        TradePricePeriod = tradePriceLength;

        lossCut = losscut;
        profitCut = profitcut;

        //자금계산을 위한 변수모음
        money = 3000000;
        beforeMoney = 0;

        tradeCount = 0;
        failCount = 0;

        buyPrice = 0;
        buyUnitCount = 0;

        maxPrice = 0;
        isDefence = false;
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

                bool isManyTrade = false;
                if (ChkSellConditionFinal(parameters, tradePriceAvg[tradePriceLength], multi, i))
                {
                    score++;
                    isManyTrade = true;
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

                //디펜스 모드가 아닐경우 
                if (!isDefence)
                {/*
                    //반익절 구간 지나면 디펜스모드로 전환.
                    if (parameters[i].trade_price >= buyPrice * (1 + (profitCut * 0.01f)))
                    {
                        money += (parameters[i].trade_price * buyUnitCount * 0.5f) * penalty;
                        buyUnitCount *= 0.5f;

                        buyPrice = parameters[i].trade_price; //구매금액은 이제 의미 없으니 고점으로 사용. 
                        isDefence = true;
                    }
                    //내려갔지만 거래량이 더 터졌을 경우 보류. 반익절라인에 가더라도 이득이 아니겠지만 거기까지 올라가는게 더힘들거라 판단함.
                    //그냥 정리하고 밑에서 구매했다 생각하자.
                    else */
                    if (isManyTrade && parameters[i].trade_price < buyPrice)
                    {
                        buyPrice = parameters[i].trade_price;
                    }
                    //거래량이 안터지고 손절구간이 지나면 손절하고 거래량 터질때 다시 줍자.
                    else if (parameters[i].trade_price <= buyPrice * (1 - (lossCut * 0.01f)))
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

                //디펜스 모드이면 
                if (isDefence)
                {
                    //새로운 고점이 등장하면 고점 갱신
                    if (parameters[i].trade_price > buyPrice)
                    {
                        buyPrice = parameters[i].trade_price;
                    }

                    //고점대비 손절구간이 지나면 죄다 정리.
                    if (parameters[i].trade_price <= buyPrice * (1 - (lossCut * 0.01f)))
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
                        while (!isTestMode)
                        {
                            yield return null;
                        }

                        if (stochasticKValues[k][d].ContainsKey(power) && stochasticDValues[k][d].ContainsKey(power))
                        {
                            continue;
                        }

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
                while (!isTestMode)
                {
                    yield return null;
                }

                if (rsiValues.ContainsKey(rsiPower))
                {
                    continue;
                }

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
                        while(!isTestMode)
                        {
                            yield return null;
                        }

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
                while (!isTestMode)
                {
                    yield return null;
                }

                if (tradePriceAvg.ContainsKey(tradePrice))
                {
                    continue;
                }

                Dictionary<CandlesParameters, double> tradePriceAvgDic;

                CalculateEMATradePriceAvg(parameters, tradePrice, out tradePriceAvgDic);

                tradePriceAvg.Add(tradePrice, new Dictionary<CandlesParameters, double>(tradePriceAvgDic));

                yield return null;
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
