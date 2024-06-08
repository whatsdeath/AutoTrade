using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class FireStore : MonoBehaviour
{

    private static readonly HttpClient httpClient = new HttpClient();
    private string projectId = "pacific-legend-425715-h1";
    private string databaseName = "ksj-auto-trade";
    private string firebaseToken;

    void Start()
    {
        // Initialize Firestore
        AuthenticateWithGoogle();
    }

    private async void AuthenticateWithGoogle()
    {
        string jsonPath = Application.dataPath + "/GoogleCloud.Json";
        string[] scopes = { "https://www.googleapis.com/auth/datastore" };

        // Load the service account key file
        var googleCredential = GoogleCredential.FromFile(jsonPath).CreateScoped(scopes);

        // Request an access token
        var tokenResponse = await googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        firebaseToken = tokenResponse;

        Debug.Log("Authenticated with Google, token: " + firebaseToken);
        /*
        AppManager.Instance.SaveData(MarketList.STX, new TradingParameters
        {
            name = MarketList.STX.ToString(),

            stochasticK = 5,
            stochasticD = 10,
            stochasticStrength = 5,

            rsiStrength = 17,

            macdShort = 12,
            macdLong = 25,
            macdSignal = 13,

            overBuy = 70.0f,
            overSell = 23.0f,
            guideRsi = 50.0f,
        
            profitRate = 1.05f,

            lossCut = 0,
            profitCut = 0
    });*/

        TradeManager.Instance.SetConditionByMarket(GetTradingParameters());
    }


    public void AddOrUpdateTradingParameters(Dictionary<MarketList, TradingParameters> parametersDictionary)
    {
        foreach (var kvp in parametersDictionary)
        {
            AddOrUpdateTradingParameter(kvp.Key, kvp.Value);
        }
    }


    public void AddOrUpdateTradingParameter(MarketList market, TradingParameters parameters)
    {
        try
        {
            string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/{databaseName}/documents/users/{market.ToString()}";
            var data = new
            {
                fields = new Dictionary<string, object>
                {
                    { "name", new { stringValue = parameters.name } },
                    { "stochasticK", new { integerValue = parameters.stochasticK } },
                    { "stochasticD", new { integerValue = parameters.stochasticD } },
                    { "stochasticStrength", new { integerValue = parameters.stochasticStrength } },
                    { "rsiStrength", new { integerValue = parameters.rsiStrength } },
                    { "macdShort", new { integerValue = parameters.macdShort } },
                    { "macdLong", new { integerValue = parameters.macdLong } },
                    { "macdSignal", new { integerValue = parameters.macdSignal } },
                    { "overBuy", new { doubleValue = parameters.overBuy } },
                    { "overSell", new { doubleValue = parameters.overSell } },
                    { "guideRsi", new { doubleValue = parameters.guideRsi } },
                    { "profitRate", new { doubleValue = parameters.profitRate } },
                    { "lossCut", new { doubleValue = parameters.lossCut } },
                    { "profitCut", new { doubleValue = parameters.profitCut } }
                }
            };

            string json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add the authorization token to the request
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", firebaseToken);

            // PATCH 요청을 사용하여 문서 업데이트
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = content
            };

            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                Debug.Log($"Document {market} successfully written: " + responseBody);
                AppManager.Instance.TelegramMassage($"[KRW-{market}] 마켓의 정보가 성공적으로 저장되었습니다.", TelegramBotType.Trade);
            }
            else
            {
                Debug.LogError($"Error writing document {market}: " + responseBody);
                AppManager.Instance.TelegramMassage($"[KRW-{market}] 마켓의 정보 저장이 실패하였습니다. : {responseBody}", TelegramBotType.Trade);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception writing document {market}: {e.Message}");
            AppManager.Instance.TelegramMassage($"[KRW-{market}] 마켓의 정보 저장이 실패하였습니다. : {e.Message}", TelegramBotType.Trade);
        }
    }


    public Dictionary<MarketList, TradingParameters> GetTradingParameters()
    {
        var result = new Dictionary<MarketList, TradingParameters>();

        try
        {
            string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/{databaseName}/documents/users";

            // Add the authorization token to the request
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", firebaseToken);

            HttpResponseMessage response = httpClient.GetAsync(url).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Documents data: " + responseBody);

                // JSON 문자열을 명시적으로 파싱
                var documents = JsonConvert.DeserializeObject<FirestoreDocuments>(responseBody);

                if (documents.documents != null)
                {
                    foreach (var document in documents.documents)
                    {
                        var fields = document.fields;

                        TradingParameters parameters = new TradingParameters
                        {
                            name = fields.name.stringValue,
                            stochasticK = fields.stochasticK.integerValue,
                            stochasticD = fields.stochasticD.integerValue,
                            stochasticStrength = fields.stochasticStrength.integerValue,
                            rsiStrength = fields.rsiStrength.integerValue,
                            macdShort = fields.macdShort.integerValue,
                            macdLong = fields.macdLong.integerValue,
                            macdSignal = fields.macdSignal.integerValue,
                            overBuy = fields.overBuy.doubleValue,
                            overSell = fields.overSell.doubleValue,
                            guideRsi = fields.guideRsi.doubleValue,
                            profitRate = fields.profitRate.doubleValue,
                            lossCut = fields.lossCut.doubleValue,
                            profitCut = fields.profitCut.doubleValue
                        };

                        string documentName = document.name;
                        string[] nameParts = documentName.Split('/');
                        string key = nameParts[nameParts.Length - 1];

                        result.Add(Enum.Parse<MarketList>(key), parameters);
                    }
                }
                AppManager.Instance.TelegramMassage("모든 마켓의 데이터가 로드되었습니다.", TelegramBotType.Trade);
            }
            else
            {
                Debug.LogError("Error getting documents: " + responseBody);
                AppManager.Instance.TelegramMassage($"데이터가 로드가 실패하였습니다. : {responseBody}", TelegramBotType.Trade);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.Message}");
            AppManager.Instance.TelegramMassage($"데이터가 로드가 실패하였습니다. : {e.Message}", TelegramBotType.Trade);
        }

        return result;
    }
}

public class FirestoreDocuments
{
    public List<FirestoreDocument> documents { get; set; }
}

public class FirestoreDocument
{
    public string name { get; set; }
    public FirestoreFields fields { get; set; }
}

public class FirestoreFields
{
    public FirestoreValue name { get; set; }
    public FirestoreValue stochasticK { get; set; }
    public FirestoreValue stochasticD { get; set; }
    public FirestoreValue stochasticStrength { get; set; }
    public FirestoreValue rsiStrength { get; set; }
    public FirestoreValue macdShort { get; set; }
    public FirestoreValue macdLong { get; set; }
    public FirestoreValue macdSignal { get; set; }
    public FirestoreValue overBuy { get; set; }
    public FirestoreValue overSell { get; set; }
    public FirestoreValue guideRsi { get; set; }
    public FirestoreValue profitRate { get; set; }
    public FirestoreValue lossCut { get; set; }
    public FirestoreValue profitCut { get; set; }
}

public class FirestoreValue
{
    public string stringValue { get; set; }
    public int integerValue { get; set; }
    public float doubleValue { get; set; }
}