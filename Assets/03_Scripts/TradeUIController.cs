using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class TradeUIController : MonoBehaviour
{
    private Dictionary<MarketList, MarketInfoUI> marketInfoUIs = new Dictionary<MarketList, MarketInfoUI>();

    private void Awake()
    {
        var uis = GetComponentsInChildren<MarketInfoUI>();

        for (int i = 0; i < uis.Length; i++)
        {
            if(i < (int)MarketList.MaxCount)
            {
                MarketList market = (MarketList)i;
                marketInfoUIs.Add(market, uis[i]);
                marketInfoUIs[market].VisualizationMarketName(market);
            }
        }
    }

    private void OnEnable()
    {
        UIManager.Instance.delegate_VisualizationRSIByMarket_By_Market_RSI_Condition_IsSell.AddDelegate(VisualizationRSIByMarket);
        UIManager.Instance.delegate_VisualizationRSIByMarket_By_Market_RSI_Condition_HexColor_IsSell.AddDelegate(VisualizationRSIByMarket);
        UIManager.Instance.delegate_VisualizationPriceByMarket_By_Market_Price_HexColor.AddDelegate(VisualizationPriceByMarket);
        UIManager.Instance.delegate_VisualizationNERSIByMarket_By_Market_NERSI.AddDelegate(VisualizationNERSIByMarket);
        UIManager.Instance.delegate_VisualizationNERSIByMarket_By_Market_NERSI_HexColor.AddDelegate(VisualizationNERSIByMarket);
        UIManager.Instance.delegate_VisualizationBalanceByMarket_By_Market_Balance.AddDelegate(VisualizationBalanceByMarket);
        UIManager.Instance.delegate_VisualizationLastTradeTimeByMarket_By_Market_LastTime.AddDelegate(VisualizationLastTradeTimeByMarket);
    }

    private void OnDisable()
    {
        if(UIManager.Instance != null)
        {
            UIManager.Instance.delegate_VisualizationRSIByMarket_By_Market_RSI_Condition_IsSell.RemoveDelegate(VisualizationRSIByMarket);
            UIManager.Instance.delegate_VisualizationRSIByMarket_By_Market_RSI_Condition_HexColor_IsSell.RemoveDelegate(VisualizationRSIByMarket);
            UIManager.Instance.delegate_VisualizationPriceByMarket_By_Market_Price_HexColor.RemoveDelegate(VisualizationPriceByMarket);
            UIManager.Instance.delegate_VisualizationNERSIByMarket_By_Market_NERSI.RemoveDelegate(VisualizationNERSIByMarket);
            UIManager.Instance.delegate_VisualizationNERSIByMarket_By_Market_NERSI_HexColor.RemoveDelegate(VisualizationNERSIByMarket);
            UIManager.Instance.delegate_VisualizationBalanceByMarket_By_Market_Balance.RemoveDelegate(VisualizationBalanceByMarket);
            UIManager.Instance.delegate_VisualizationLastTradeTimeByMarket_By_Market_LastTime.RemoveDelegate(VisualizationLastTradeTimeByMarket);
        }
    }

        

    public void VisualizationRSIByMarket(MarketList market, float RSI, float condition, bool isSell)
    {
        marketInfoUIs[market].VisualizationRSI(RSI, condition, isSell);
    }
    public void VisualizationRSIByMarket(MarketList market, float RSI, float condition, string hexColor, bool isSell)
    {
        marketInfoUIs[market].VisualizationRSI(RSI, condition, hexColor, isSell);
    }

    public void VisualizationPriceByMarket(MarketList market, double curruntPrice, string hexColor)
    {
        marketInfoUIs[market].VisualizationPrice(curruntPrice, hexColor);
    }

    public void VisualizationNERSIByMarket(MarketList market, float NERSI)
    {
        marketInfoUIs[market].VisualizationNERSI(NERSI);
    }

    public void VisualizationNERSIByMarket(MarketList market, float NERSI, string hexColor)
    {
        marketInfoUIs[market].VisualizationNERSI(NERSI, hexColor);
    }

    public void VisualizationBalanceByMarket(MarketList market, double balance)
    {
        marketInfoUIs[market].VisualizationBalance(balance);
    }

    public void VisualizationLastTradeTimeByMarket(MarketList market, DateTime lastTime)
    {
        marketInfoUIs[market].VisualizationLastTradeTime(lastTime);
    }
}
