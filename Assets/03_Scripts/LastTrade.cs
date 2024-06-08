using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


public class LastTrade : BaseWebRequest<OrderParameters>
{
    protected override string additionalUrl { get => $"v1/orders"; }

    Dictionary<string, object> orderParameter = new Dictionary<string, object>();

    override protected void Init()
    {
        payloadParameter.Add("query_hash_alg", "SHA512");
        payloadParameter.Add("query_hash", string.Empty);
    }

    protected override string SetParamsKey(object data)
    {
        return ((OrderParameters)data).uuid;
    }

    public void ChkLastOrder(MarketList market)
    {
        orderParameter.Clear();

        //orderParameter.Add("name", "KRW-" + name.ToString());
        orderParameter.Add("state", "done");
        //orderParameter.Add("order_by", "desc");

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
        Debug.Log(queryString);

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

    }
}
