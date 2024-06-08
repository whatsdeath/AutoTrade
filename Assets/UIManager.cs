using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class UIManager : BaseManager<UIManager>
{
    public Delegate<bool> delegate_VisualizationTradeMode_By_value;
    public Delegate<DateTime, bool> delegate_VisualizationNowTime_By_NowTime_IsCorrection;
    public Delegate<double> delegate_VisualizationKRWBalance_By_KRWBalance;
    public Delegate<double> delegate_VisualizationTotalKRW_By_TotalKRW;

    public Delegate<MarketList, float, float, bool> delegate_VisualizationRSIByMarket_By_Market_RSI_Condition_IsSell;
    public Delegate<MarketList, float, float, string, bool> delegate_VisualizationRSIByMarket_By_Market_RSI_Condition_HexColor_IsSell;
    public Delegate<MarketList, double, string> delegate_VisualizationPriceByMarket_By_Market_Price_HexColor;
    public Delegate<MarketList, float> delegate_VisualizationNERSIByMarket_By_Market_NERSI;
    public Delegate<MarketList, float, string> delegate_VisualizationNERSIByMarket_By_Market_NERSI_HexColor;
    public Delegate<MarketList, double> delegate_VisualizationBalanceByMarket_By_Market_Balance;
    public Delegate<MarketList, DateTime> delegate_VisualizationLastTradeTimeByMarket_By_Market_LastTime;

    CandleChartUI candleChart;

    public void SetCandleChartUI(CandleChartUI candleChartUI)
    {
        candleChart = candleChartUI;
    }

    public void VisualizationCandleChart(List<CandlesParameters> parameters)
    {
        if(candleChart == null)
        {
            return;
        }

        candleChart.VisualizationCandleChart(parameters);
    }

    public void VisualizationTradeMode(bool value)
    {
        if (delegate_VisualizationTradeMode_By_value.ChkEmpty())
        {
            DebugByPlatform.Debug.LogOnlyEditer($"VisualizationTradeMode 가 등록되지 않았습니다.");
            return;
        }

        delegate_VisualizationTradeMode_By_value.Play(value);
    }

    public void VisualizationNowTime()
    {
        delegate_VisualizationNowTime_By_NowTime_IsCorrection.Play(TimeManager.Instance.nowTime, TimeManager.Instance.isCorrection);
    }
    
    public void VisualizationKRWBalance(double balance)
    {
        delegate_VisualizationKRWBalance_By_KRWBalance.Play(TradeManager.Instance.accountParam.balanceKRW);
    }

    public void VisualizationTotalKRW(double total)
    {
        delegate_VisualizationTotalKRW_By_TotalKRW.Play(TradeManager.Instance.accountParam.myProperty);
    }


    public void VisualizationRSIByMarket(MarketList market, float RSI, float condition, bool isSell)
    {
        delegate_VisualizationRSIByMarket_By_Market_RSI_Condition_IsSell.Play(market, RSI, condition, isSell);
    }

    public void VisualizationRSIByMarket(MarketList market, float RSI, float condition, string hexColor, bool isSell)
    {
        delegate_VisualizationRSIByMarket_By_Market_RSI_Condition_HexColor_IsSell.Play(market, RSI, condition, hexColor, isSell);
    }

    public void VisualizationPriceByMarket(MarketList market, double price, string hexColor)
    {
        delegate_VisualizationPriceByMarket_By_Market_Price_HexColor.Play(market, price, hexColor);
    }

    public void VisualizationNERSIByMarket(MarketList market, float NERSI)
    {
        delegate_VisualizationNERSIByMarket_By_Market_NERSI.Play(market, NERSI);
    }

    public void VisualizationNERSIByMarket(MarketList market, float NERSI, string hexColor)
    {
        delegate_VisualizationNERSIByMarket_By_Market_NERSI_HexColor.Play(market, NERSI, hexColor);
    }

    public void VisualizationBalanceByMarket(MarketList market, double balance)
    {
        delegate_VisualizationBalanceByMarket_By_Market_Balance.Play(market, balance);
    }

    public void VisualizationLastTradeTimeByMarket(MarketList market, DateTime lastTime)
    {
        delegate_VisualizationLastTradeTimeByMarket_By_Market_LastTime.Play(market, lastTime);
    }
}
