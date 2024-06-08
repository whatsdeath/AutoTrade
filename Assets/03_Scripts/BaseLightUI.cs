using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseLightUI : MonoBehaviour
{
    [SerializeField] protected Image lamp;
    protected Color onTradeMode = Color.green;
    protected Color offTradeMode = Color.gray;

    bool isOn;

    protected void Awake()
    {
        lamp = GetComponentInChildren<Image>();
        lamp.color = offTradeMode;

        Init();
    }

    virtual protected void Init() { }

    protected void VisualizationSwitchLight(bool value)
    {
        if (value.Equals(isOn))
        {
            return;
        }

        if (value)
        {
            lamp.color = onTradeMode;
        }
        else
        {
            lamp.color = offTradeMode;
        }

        isOn = value;
    }
}
