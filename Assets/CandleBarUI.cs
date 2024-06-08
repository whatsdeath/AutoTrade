using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CandleBarUI : MonoBehaviour
{
    RectTransform bodyRectTr;
    RectTransform tailRectTr;

    private Image Body;
    private Image Tail;

    private void Awake()
    {
        bodyRectTr = GetComponent<RectTransform>();
        tailRectTr = GetComponentInChildren<RectTransform>();

        Body = GetComponent<Image>();
        Tail = GetComponentInChildren<Image>();
    }

    public void VisualizationCandle(CandlesParameters param, float maxPrice, float minPrice, float chartHeight)
    {
        float maxBodyPrice;
        float minBodyPrice;

        if (param.opening_price > param.trade_price)
        {
            maxBodyPrice = (float)param.opening_price;
            minBodyPrice = (float)param.trade_price;
        }
        else
        {
            maxBodyPrice = (float)param.trade_price;
            minBodyPrice = (float)param.opening_price;
        }

        float maxp = (maxBodyPrice - minPrice) / ((maxPrice - minPrice) / chartHeight);
        float minp = (minBodyPrice - minPrice) / ((maxPrice - minPrice) / chartHeight);

        Debug.Log(minp + ", " + maxp);

        //bodyRectTr.sizeDelta = new Vector2(bodyRectTr.sizeDelta.x, (float)(maxp - minp));

    }
}