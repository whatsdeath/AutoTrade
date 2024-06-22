using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CandleChartUI : MonoBehaviour
{
    RectTransform rectTransform;

    public float chartHeight { get => rectTransform.rect.height; }

    public float minPrice { get; private set; }
    public float maxPrice { get; private set; }
    public float centerPrice { get => (minPrice + maxPrice) * 0.5f; }


    private CandleBarUI candleBar; 

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        candleBar = GetComponentInChildren<CandleBarUI>();
    }

    private void Start()
    {
        UIManager.Instance.SetCandleChartUI(this);
    }


    public void VisualizationCandleChart(List<CandlesParameters> parameters)
    {
        minPrice = float.MaxValue;
        maxPrice = 0.0f;

        for (int i = 0; i < parameters.Count; i++) 
        { 
            if (parameters[i].high_price > maxPrice)
            {
                maxPrice = (float)parameters[i].high_price;
            }

            if (parameters[i].low_price < minPrice)
            {
                minPrice = (float)parameters[i].low_price;
            }
        }

        candleBar.VisualizationCandle(parameters[0], maxPrice, minPrice, chartHeight);
    }

}
