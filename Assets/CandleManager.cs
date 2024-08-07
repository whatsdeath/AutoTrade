using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum MarketList
{
    WAVES, FLOW, ARB, SUI, MATIC, ARK, LINK, STX, SHIB, SEI,
    CHZ, DOGE, KNC, BLUR, PYTH, POLYX, ONG, NEAR, GLM, SOL, MLK, ELF, SNT, 
    GAS, MTL, AVAX, AKT, LSK, STRAX, META, UPP, TAIKO, HIFI, ZRO, HPO, BLAST, STMX, AUCTION, TT, MaxCount, Test
}

public class CandleManager : BaseManager<CandleManager>
{
    private MarketAll marketAll;
    private CandleMinutes candle;

    private Order order;

    private Dictionary<MarketList, CandleMinutes> candleSearchByMarket = new Dictionary<MarketList, CandleMinutes>();

    public Dictionary<MarketList, List<CandlesParameters>> candleListDic = new Dictionary<MarketList, List<CandlesParameters>>();

    IEnumerator firstSearch;
    IEnumerator autoSearch;

    protected override void Init()
    {
        for (int i = 0; i < (int)MarketList.MaxCount; i++)
        {
            candleSearchByMarket.Add((MarketList)i, CreateComponentObjectInChildrenAndReturn<CandleMinutes>(true));
            candleSearchByMarket[(MarketList)i].SetSearchMarket((MarketList)i);
            candleListDic.Add((MarketList)i, new List<CandlesParameters>());
        }

        order = CreateComponentObjectInChildrenAndReturn<Order>();
    }

    public void FirstSearchAll()
    {
        if (firstSearch != null)
        {
            StopCoroutine(firstSearch);
            firstSearch = null;
        }

        firstSearch = FirstSearch();
        StartCoroutine(firstSearch);
    }

    public void StartAutoSearchTradeMode()
    {
        if(autoSearch != null) 
        {
            StopCoroutine(autoSearch);
            autoSearch = null;
        }

        autoSearch = AutoSearchTradeMode();
        StartCoroutine(autoSearch);
    }

    public void StopAutoSearchTradeMode()
    {
        if (autoSearch != null)
        {
            StopCoroutine(autoSearch);
            autoSearch = null;
        }
    }


    public void CandleSearchByMarket(MarketList market)
    {
        candleSearchByMarket[market].SearchCandle();
    }

    public void SetCandleData(MarketList market, List<CandlesParameters> candles)
    {
        if (!candleListDic.ContainsKey(market))
        {
            candleListDic.Add(market, new List<CandlesParameters>());
        }

        candleListDic[market] = candles.ToList();
    }

    public bool ChkCandleDataByMarket(MarketList market)
    {
        return !candleListDic[market].Count.Equals(0);
    }


    public List<CandlesParameters> GetCandleData(MarketList market)
    {
        return candleListDic[market];
    }

    IEnumerator AutoSearchTradeMode()
    {
        while (true)
        {
            for (int i = 0; i < (int)MarketList.MaxCount; i++)
            {
                //트레이드 모드에만 검색.
                if (TimeManager.Instance.processSequence.Equals(ProcessSequence.TradePhase))
                {
                    MarketList curruntMarket = (MarketList)i;

                    if (!TradeManager.Instance.ChkTradeCondition(curruntMarket))
                    {
                        continue;
                    }

                    CandleSearchByMarket(curruntMarket);
                }

                yield return new WaitForSeconds(GlobalValue.ACCESS_INTERVAL);
            }
        }
    }

    IEnumerator FirstSearch()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < (int)MarketList.MaxCount; i++)
        {
            MarketList curruntMarket = (MarketList)i;

            if (!TradeManager.Instance.ChkTradeCondition(curruntMarket))
            {
                continue;
            }

            CandleSearchByMarket(curruntMarket);

            yield return new WaitForSeconds(GlobalValue.ACCESS_INTERVAL);
        }

        TradeManager.Instance.SetTradeReady(true);
        DebugByPlatform.Debug.Log("준비완료");
    }
}
