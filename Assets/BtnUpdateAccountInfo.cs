using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnUpdateAccountInfo : MonoBehaviour
{
    public void OnClick()
    {
        TradeManager.Instance.UpdateAccountInfo();
    }
}
