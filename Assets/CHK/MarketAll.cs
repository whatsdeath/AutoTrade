using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct MarketAllParam
{
    public string market;   //업비트에서 제공중인 시장 정보
    public string korean_name; //거래 대상 디지털 자산 한글명
    public string english_name;//거래 대상 디지털 자산 영문명
    //public bool market_event.warning; //업비트 시장경보 > 유의종목 지정 여부
    //public object market_event.caution;  //업비트 시장경보 > 주의종목 지정 여부
    //- PRICE_FLUCTUATIONS: 가격 급등락 경보 발령 여부
    //- TRADING_VOLUME_SOARING: 거래량 급등 경보 발령 여부
    //- DEPOSIT_AMOUNT_SOARING: 입금량 급등 경보 발령 여부
    //- GLOBAL_PRICE_DIFFERENCES: 가격 차이 경보 발령 여부
    //- CONCENTRATION_OF_SMALL_ACCOUNTS: 소수 계정 집중 경보 발령 여부
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
