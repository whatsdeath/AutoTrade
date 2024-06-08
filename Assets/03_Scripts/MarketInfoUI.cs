using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarketInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI market;
    [SerializeField] private TextMeshProUGUI curruntRSI;
    [SerializeField] private TextMeshProUGUI curruntPrice;
    
    [SerializeField] private TextMeshProUGUI neRSI;
    [SerializeField] private TextMeshProUGUI lastTradeTime;
    [SerializeField] private TextMeshProUGUI quantity;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        var texts = gameObject.GetComponentsInChildren<TextMeshProUGUI>();

        for (int i = 0; i < texts.Length; i++)
        {
            switch (texts[i].gameObject.name)
            {
                case "Market":
                    market = texts[i];
                    break;

                case "NowPrice":
                    curruntPrice = texts[i];
                    break;

                case "RSI":
                    curruntRSI = texts[i];
                    break;

                case "NextExpectedRSI":
                    neRSI = texts[i];
                    break;

                case "LastTradeTime":
                    lastTradeTime = texts[i];
                    break;

                case "Quantity":
                    quantity = texts[i];
                    break;
            }
        }
    }

    public void VisualizationMarketName(MarketList market)
    {
        if (this.market == null)
        {
            Init();
        }
        this.market.text = market.ToString();
    }

    public void VisualizationRSI(float RSI, float condition, bool isSell)
    {
        if (this.curruntRSI == null)
        {
            Init();
        }
        curruntRSI.text = $"RSI : {RSI.ToString("#0.0")} {(isSell ? "<color=#55ff55>S" : "<color=#ff5555>B")}({condition.ToString("#0.0#")})</color>";
    }

    public void VisualizationRSI(float RSI, float condition, string hexColor, bool isSell)
    {
        if (curruntRSI == null)
        {
            Init();
        }
        curruntRSI.text = $"RSI : <color=#{hexColor}>{RSI.ToString("#0.0#")}</color> {(isSell ? "<color=#55ff55>S" : "<color=#ff5555>B")}({condition.ToString("#0.0#")})</color>";
    }

    public void VisualizationPrice(double curruntPrice, string hexColor)
    {
        if (this.curruntPrice == null)
        {
            Init();
        }
        this.curruntPrice.text = $"<color=#{hexColor}>({curruntPrice.ToString("#0.0####")})</color>";
    }

    public void VisualizationNERSI(float NERSI)
    {
        if (neRSI == null)
        {
            Init();
        }
        neRSI.text = $"NERSI : {NERSI.ToString("#0.0")}";
    }

    public void VisualizationNERSI(float NERSI, string hexColor)
    {
        if (neRSI == null)
        {
            Init();
        }
        neRSI.text = $"NERSI : <color=#{hexColor}>{NERSI.ToString("#0.0")}</color>";
    }

    public void VisualizationLastTradeTime(DateTime lastTime)
    {
        if (lastTradeTime == null)
        {
            Init();
        }
        lastTradeTime.text = $"LT : {lastTime.ToString("yy.MM.dd HH:mm:ss")}";
    }

    public void VisualizationBalance(double quantity)
    {
        if (this.quantity == null)
        {
            Init();
        }

        if (quantity <= 0.0f)
        {
            this.quantity.text = "";
        }
        else
        {
            this.quantity.text = $"/  {quantity.ToString("#0.0")}";
        }
    }
}
