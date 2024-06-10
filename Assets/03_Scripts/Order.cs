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
    public string uuid; //�ֹ��� ���� ���̵�
    public string side; //�ֹ� ����
    public string ord_type; //�ֹ� ���
    public double price; //�ֹ� ��� ȭ�� ����
    public string state; //�ֹ� ����
    public string market; //������ ����Ű
    public string created_at; //�ֹ� ���� �ð�
    public double volume; //����ڰ� �Է��� �ֹ� ��
    public double remaining_volume; //ü�� �� ���� �ֹ� ��
    public double reserved_fee; //������� ����� ���
    public double remaining_fee; //���� ������
    public double paid_fee; //���� ������
    public double locked; //�ŷ��� ������� ���
    public double executed_volume; //ü��� ��  
    public int trades_count; //�ش� �ֹ��� �ɸ� ü�� ��
    public string time_in_force; //IOC, FOK ����
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

            MarketList market = Enum.Parse<MarketList>(parameters[keyList[i]].market.Remove(0, 4)); //"KRW-" ����

            //�����ϰų� �ǸŰ� Ȯ���Ǹ� ������ ����
            switch(parameters[keyList[i]].side)
            {
                case "bid": //���� ��
                    TradeManager.Instance.SaveDataByAfterBuy(market);
                    break;

                case "ask": //�Ǹ� ��
                    TradeManager.Instance.SaveDataByAfterSell(market);
                    break;
            }

        }
    }
}
