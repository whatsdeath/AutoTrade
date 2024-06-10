using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CandleMinutes : BaseCandleSearch
{
    protected override string additionalUrl { get => $"v1/candles/minutes/{timeUnit}"; }
    protected override string apiEndPoint { get => $"market=KRW-{market}&count={count}"; }

    protected int timeUnit { get => (int)MinuteUnit.Minutes_5; }
    protected int count { get => 200; }

    public void SetSearchMarket(MarketList market)
    {
        this.market = market;
    }

    public void SearchCandle()
    {
        CandleSearch();
    }
}
