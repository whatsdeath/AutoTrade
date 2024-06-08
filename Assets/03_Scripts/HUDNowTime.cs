using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class HUDNowTime : BaseLightUI
{
    [SerializeField] TextMeshProUGUI timeText;

    protected override void Init()
    {
        timeText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UIManager.Instance.delegate_VisualizationNowTime_By_NowTime_IsCorrection.AddDelegate(VisualizationNowTime);
    }

    private void OnDisable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.delegate_VisualizationNowTime_By_NowTime_IsCorrection.RemoveDelegate(VisualizationNowTime);
        }
    }

    private void VisualizationNowTime(DateTime time, bool isCorrection)
    {
        timeText.text = time.ToString("yyyy.MM.dd HH:mm:ss");
        VisualizationSwitchLight(isCorrection);
    }
}
