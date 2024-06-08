using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnCandleSearch : MonoBehaviour
{
    public void OnClick()
    {
        //CandleManager.Instance.StartCandleSearch();
        TestManager.Instance.DataDownload();

        //TradeManager.Instance.SetTradeMode(true);
        //TestManager.Instance.DataLoad("KRW-DOGE");
    }
}
