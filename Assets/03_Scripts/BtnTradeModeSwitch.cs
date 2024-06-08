using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BtnTradeModeSwitch : MonoBehaviour
{
    [SerializeField] private TMP_InputField dogeInputField;
    [SerializeField] private TMP_InputField shibInputField;


    public void OnClick()
    {
        TradeManager.Instance.SetTradeMode(true);
    }
}
