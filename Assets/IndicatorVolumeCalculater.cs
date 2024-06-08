using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IndicatorVolumeCalculater
{
    public static float CalcRSI(List<CandlesParameters> parameters, int power)
    {
        List<decimal> Us = new List<decimal>();
        List<decimal> Ds = new List<decimal>();

        decimal a = (decimal)1 / (1 + (power - 1));

        // 기간내 상승/하락 계산
        for (int i = 1; i < Mathf.Max(parameters.Count, 200); i++)
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

        decimal AU = 0, AD = 0, RS;
        float RSI;

        for (int i = Us.Count - 1; i >= 0; i--)
        {
            AU = (Us[i] * a) + (AU * (1 - a));
            AD = (Ds[i] * a) + (AD * (1 - a));
        }

        if (AD <= 0)
        {
            RSI = 100.0f;
        }
        else
        {
            RS = AU / AD;
            RSI = (float)(100 - (100 / (1 + RS))); //((RS / (1 + RS)) * (decimal)100.0);
        }

        return RSI;
    }

    public static void CalculateStochasticSlow(List<CandlesParameters> parameters, int k, int d, int power,
    out Dictionary<CandlesParameters, float> kValues, out Dictionary<CandlesParameters, float> dValues)
    {
        kValues = new Dictionary<CandlesParameters, float>();
        dValues = new Dictionary<CandlesParameters, float>();

        List<float> kValueList = new List<float>();
        List<float> slowDValueList = new List<float>();

        //%K 값 구하기
        for (int i = 0; i < Mathf.Max(parameters.Count, 200) - power; i++)
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

            float kValue = 100.0f;
            if ((high - low) > 0)
            {
                kValue = (float)(((decimal)parameters[i].trade_price - low) / (high - low)) * 100f;
            }
            kValueList.Add(kValue);
        }

        List<float> slowKValueList = new List<float>();
        for (int i = 0; i < kValueList.Count - k; i++)
        {
            float slowK = 0;
            for (int j = i; j < i + k; j++)
            {
                slowK += kValueList[j];
            }

            slowKValueList.Add(slowK / k);
        }

        // %D 값 계산
        for (int i = 0; i < slowKValueList.Count - d; i++)
        {
            float sumSlowK = 0;

            for (int j = i; j < i + d; j++)
            {
                sumSlowK += slowKValueList[j];
            }

            slowDValueList.Add(sumSlowK / d);
        }

        for (int i = 0; i < slowDValueList.Count; i++)
        {
            kValues.Add(parameters[i], slowKValueList[i]);
            dValues.Add(parameters[i], slowDValueList[i]);
        }
    }

    public static void CalculateMACD(List<CandlesParameters> parameters, int shortPeriod, int longPeriod, int signalPeriod,
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

    public static List<float> CalculateEMA(List<float> prices, int period)
    {
        List<float> emaValues = new List<float>();
        decimal multiplier = (decimal)1 / (1 + (period - 1));

        decimal ema = (decimal)prices[prices.Count - 1];

        for (int i = prices.Count - 2; i >= 0; i--)
        {
            ema = ((decimal)prices[i] * multiplier) + (ema * (1 - multiplier));
            emaValues.Add((float)ema);
        }
        emaValues.Reverse();
        return emaValues.ToList();
    }

    private static List<double> CalculateEMA(List<double> prices, int period)
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

    private static List<double> CalculateEMA(List<CandlesParameters> parameters, int period)
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
}
