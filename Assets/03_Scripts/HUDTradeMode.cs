using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDTradeMode : BaseLightUI
{
    private void OnEnable()
    {
        UIManager.Instance.delegate_VisualizationTradeMode_By_value.AddDelegate(VisualizationSwitchLight);
    }

    private void OnDisable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.delegate_VisualizationTradeMode_By_value.RemoveDelegate(VisualizationSwitchLight);
        }
    }
}
