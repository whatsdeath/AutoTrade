using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct MarketAllParam
{
    public string market;   //����Ʈ���� �������� ���� ����
    public string korean_name; //�ŷ� ��� ������ �ڻ� �ѱ۸�
    public string english_name;//�ŷ� ��� ������ �ڻ� ������
    //public bool market_event.warning; //����Ʈ ����溸 > �������� ���� ����
    //public object market_event.caution;  //����Ʈ ����溸 > �������� ���� ����
    //- PRICE_FLUCTUATIONS: ���� �޵�� �溸 �߷� ����
    //- TRADING_VOLUME_SOARING: �ŷ��� �޵� �溸 �߷� ����
    //- DEPOSIT_AMOUNT_SOARING: �Աݷ� �޵� �溸 �߷� ����
    //- GLOBAL_PRICE_DIFFERENCES: ���� ���� �溸 �߷� ����
    //- CONCENTRATION_OF_SMALL_ACCOUNTS: �Ҽ� ���� ���� �溸 �߷� ����
}

public class MarketAll : BaseWebRequest<MarketAllParam>
{
    private enum MarketParam
    {
        PriceUnit, Currency
    }

    protected override string additionalUrl { get => "v1/market/all"; }
    protected override string apiEndPoint { get => $"isDetails={isDetail}"; }

    private bool isDetail { get => false; }

    private List<string> _marketList = new List<string>();
    public List<string> marketList { get => _marketList; }


    override protected void Init()
    {
        StartCoroutine(Access());
    }

    override protected bool ChkIgnoreCondition(object data)
    {
        string[] marketParam = ((MarketAllParam)data).market.Split('-');

        if (!GlobalValue.CHECK_PRICE_UNIT.Contains(marketParam[(int)MarketParam.PriceUnit]))
        {
            return true;
        }

        if (GlobalValue.IGNORE_CURRENCYS.Contains(marketParam[(int)MarketParam.Currency]))
        {
            return true;
        }

        return false;
    }

    override protected string SetParamsKey(object data)
    {
        return ((MarketAllParam)data).market;
    }

    protected override void AfterProcess()
    {
        _marketList = parameters.Keys.ToList();
    }
}
