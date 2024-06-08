using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDBalanceKRW : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI balanceKRWText;

    private void Awake()
    {
        balanceKRWText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UIManager.Instance.delegate_VisualizationKRWBalance_By_KRWBalance.AddDelegate(VisualizationKRWBalance);
    }

    private void OnDisable()
    {
        if(UIManager.Instance != null)
        {
            UIManager.Instance.delegate_VisualizationKRWBalance_By_KRWBalance.RemoveDelegate(VisualizationKRWBalance);
        }
    }

    private void VisualizationKRWBalance(double balance)
    {
        balanceKRWText.text = "\\ " + balance.ToString("#,###");
    }
}
