using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine.Networking;

public struct OrderParameters
{
    public string uuid; //주문의 고유 아이디
    public string side; //주문 종류
    public string ord_type; //주문 방식
    public double price; //주문 당시 화폐 가격
    public string state; //주문 상태
    public string market; //마켓의 유일키
    public string created_at; //주문 생성 시간
    public double volume; //사용자가 입력한 주문 양
    public double remaining_volume; //체결 후 남은 주문 양
    public double reserved_fee; //수수료로 예약된 비용
    public double remaining_fee; //남은 수수료
    public double paid_fee; //사용된 수수료
    public double locked; //거래에 사용중인 비용
    public double executed_volume; //체결된 양  
    public int trades_count; //해당 주문에 걸린 체결 수
    public string time_in_force; //IOC, FOK 설정
}

public class Order : BaseWebRequest<OrderParameters>
{
    protected override string additionalUrl { get => $"v1/orders"; }

    Dictionary<string, object> orderParameter = new Dictionary<string, object>();

    protected override string SetParamsKey(object data)
    {
        return ((OrderParameters)data).uuid;
    }

    override protected void Init()
    {
        payloadParameter.Add("query_hash_alg", "SHA512");
        payloadParameter.Add("query_hash", string.Empty);
    }

    public void BuyOrder(MarketList market, double price)
    {
        orderParameter.Clear();

        orderParameter.Add("market", "KRW-" + market.ToString());
        orderParameter.Add("side", "bid");
        orderParameter.Add("volume", null);
        orderParameter.Add("price", price.ToString());
        orderParameter.Add("ord_type", "price");

        StartCoroutine(Access());
    }


    public void SellOrder(MarketList market, double volume)
    {
        orderParameter.Clear();

        orderParameter.Add("market", "KRW-" + market.ToString());
        orderParameter.Add("side", "ask");
        orderParameter.Add("volume", volume.ToString());
        orderParameter.Add("price", null);
        orderParameter.Add("ord_type", "market");

        StartCoroutine(Access());
    }

    protected override UnityWebRequest ConnectionRequest(string url)
    {
        UnityWebRequest request;

        var jsonParams = JsonConvert.SerializeObject(orderParameter);

        request = UnityWebRequest.Post(url, jsonParams, "application/json");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", CreateToken());

        return request;
    }

    protected override void PreparationCreateToken()
    {
        var queryElements = new List<string>();

        foreach (var entity in orderParameter)
        {
            queryElements.Add($"{entity.Key}={entity.Value}");
        }

        string queryString = string.Join("&", queryElements);

        var hashBytes = Encoding.UTF8.GetBytes(queryString);

        using (SHA512 shaM = new SHA512Managed())
        {
            var hash = shaM.ComputeHash(hashBytes);
            string queryHash = string.Concat(hash.Select(b => b.ToString("x2"))).PadLeft(128, '0');

            payloadParameter["query_hash"] = queryHash;
        }                  
    }

    protected override void AfterProcess()
    {
        List<string> keyList = new List<string>(parameters.Keys);

        for (int i = 0;  i < keyList.Count; i++) 
        {
            AppManager.Instance.TelegramMassage($"uuid : {parameters[keyList[i]].uuid} / side : {parameters[keyList[i]].side} / market : {parameters[keyList[i]].market}", TelegramBotType.Trade);

            MarketList market = Enum.Parse<MarketList>(parameters[keyList[i]].market.Remove(0, 4)); //"KRW-" 제거

            //구매하거나 판매가 확정되면 데이터 저장
            switch(parameters[keyList[i]].side)
            {
                case "bid": //구매 시
                    TradeManager.Instance.SaveDataByAfterBuy(market);
                    break;

                case "ask": //판매 시
                    TradeManager.Instance.SaveDataByAfterSell(market);
                    break;
            }

        }
    }
}
