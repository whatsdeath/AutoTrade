using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDTotalKRW : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI totalKRWText;

    private void Awake()
    {
        totalKRWText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UIManager.Instance.delegate_VisualizationTotalKRW_By_TotalKRW.AddDelegate(VisualizationTotalKRW);
    }

    private void OnDisable()
    {
        if(UIManager.Instance != null)
        {
            UIManager.Instance.delegate_VisualizationTotalKRW_By_TotalKRW.RemoveDelegate(VisualizationTotalKRW);
        }
    }

    private void VisualizationTotalKRW(double balance)
    {
        totalKRWText.text = "\\ " + balance.ToString("#,###");
    }
}
