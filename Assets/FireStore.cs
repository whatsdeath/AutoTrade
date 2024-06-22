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

    float ReCertificationInterval { get => 1800.0f; }
    float NextCertificationTime;

    bool isCertification;

    void Start()
    {
        // Initialize Firestore
        AuthenticateWithGoogle();
    }
    
    private void FixedUpdate()
    {
        //ReCertificationInterval 시간마다 자동갱신
        if (Time.realtimeSinceStartup >= NextCertificationTime)
        {
            AuthenticateWithGoogle();
        }
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

        if (!isCertification)
        {
            OnCertification();
        }

        NextCertificationTime = Time.realtimeSinceStartup + ReCertificationInterval;
    }

    private void OnCertification()
    {
        isCertification = true;
        TradeManager.Instance.SetConditionByMarket(GetTradingParameters());
        /*
            AppManager.Instance.SaveData(MarketList.POLYX, new TradingParameters
            {
                name = (MarketList.POLYX).ToString(),

                stochasticK = 10,
                stochasticD = 10,
                stochasticStrength = 5,

                rsiStrength = 18,

                tradePriceEMALength = 36,
                tradePriceConditionMul = 4.0f
            });*/
    }

    public void ReloadConditionByMarkets()
    {
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
        AddOrUpdateTradingParameter(market.ToString(), parameters);
    }


    public void AddOrUpdateTradingParameter(string market, TradingParameters parameters)
    {
        try
        {
            string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/{databaseName}/documents/users/{market}";
            var data = new
            {
                fields = new Dictionary<string, object>
                {
                    { "name", new { stringValue = parameters.name } },

                    { "stochasticK", new { integerValue = parameters.stochasticK } },
                    { "stochasticD", new { integerValue = parameters.stochasticD } },
                    { "stochasticStrength", new { integerValue = parameters.stochasticStrength } },

                    { "stochasticSellK", new { integerValue = parameters.stochasticSellK } },
                    { "stochasticSellStrength", new { integerValue = parameters.stochasticSellStrength } },

                    { "rsiStrength", new { integerValue = parameters.rsiStrength } },
                    { "rsiSellStrength", new { integerValue = parameters.rsiSellStrength } },

                    { "tradePriceEMALength", new { integerValue = parameters.tradePriceEMALength } },
                    { "tradePriceConditionMul", new { doubleValue = parameters.tradePriceConditionMul } },
                    { "tradeSellPriceConditionMul", new { doubleValue = parameters.tradeSellPriceConditionMul } },

                    { "amountStochastic", new { doubleValue = parameters.amountStochastic } },
                    { "amountRSI", new { doubleValue = parameters.amountRSI } },
                    { "amountStoRsiTrade", new { doubleValue = parameters.amountStoRsiTrade } },

                    { "winRateStochastic", new { doubleValue = parameters.winRateStochastic } },
                    { "winRateRSI", new { doubleValue = parameters.winRateRSI } },
                    { "winRateStoRsiTrade", new { doubleValue = parameters.winRateStoRsiTrade } }
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
                DebugByPlatform.Debug.LogOnlyEditer($"Document {market} successfully written: " + responseBody);
                AppManager.Instance.TelegramMassage($"[KRW-{market}] 마켓의 정보가 성공적으로 저장되었습니다.", TelegramBotType.DebugLog);
            }
            else
            {
                Debug.LogError($"Error writing document {market}: " + responseBody);
                AppManager.Instance.TelegramMassage($"[KRW-{market}] 마켓의 정보 저장이 실패하였습니다. : {responseBody}", TelegramBotType.DebugLog);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception writing document {market}: {e.Message}");
            AppManager.Instance.TelegramMassage($"[KRW-{market}] 마켓의 정보 저장이 실패하였습니다. : {e.Message}", TelegramBotType.DebugLog);
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

                            stochasticSellK = fields.stochasticSellK != null ? fields.stochasticSellK.integerValue : fields.stochasticK.integerValue,
                            stochasticSellStrength = fields.stochasticSellStrength != null ? fields.stochasticSellStrength.integerValue : fields.stochasticStrength.integerValue,

                            rsiStrength = fields.rsiStrength.integerValue,
                            rsiSellStrength = fields.rsiSellStrength != null ? fields.rsiSellStrength.integerValue : fields.rsiStrength.integerValue,

                            tradePriceEMALength = fields.tradePriceEMALength.integerValue,
                            tradePriceConditionMul = fields.tradePriceConditionMul.doubleValue,
                            tradeSellPriceConditionMul = fields.tradeSellPriceConditionMul != null ? fields.tradeSellPriceConditionMul.doubleValue : fields.tradePriceConditionMul.doubleValue,

                            amountStochastic = fields.amountStochastic.doubleValue,
                            amountRSI = fields.amountRSI.doubleValue,
                            amountStoRsiTrade = fields.amountStoRsiTrade.doubleValue,

                            winRateStochastic = fields.winRateStochastic.doubleValue,
                            winRateRSI = fields.winRateRSI.doubleValue,
                            winRateStoRsiTrade = fields.winRateStoRsiTrade.doubleValue
                        };

                        string documentName = document.name;
                        string[] nameParts = documentName.Split('/');
                        string key = nameParts[nameParts.Length - 1];

                        result.Add(Enum.Parse<MarketList>(key), parameters);
                    }
                }
                AppManager.Instance.TelegramMassage("모든 마켓의 데이터가 로드되었습니다.", TelegramBotType.DebugLog);
            }
            else
            {
                Debug.LogError("Error getting documents: " + responseBody);
                AppManager.Instance.TelegramMassage($"데이터가 로드가 실패하였습니다. : {responseBody}", TelegramBotType.DebugLog);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.Message}");
            AppManager.Instance.TelegramMassage($"데이터가 로드가 실패하였습니다. : {e.Message}", TelegramBotType.DebugLog);
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

    public FirestoreValue stochasticSellK { get; set; }
    public FirestoreValue stochasticSellStrength { get; set; }

    public FirestoreValue rsiStrength { get; set; }
    public FirestoreValue rsiSellStrength { get; set; }

    public FirestoreValue tradePriceEMALength { get; set; }
    public FirestoreValue tradePriceConditionMul { get; set; }
    public FirestoreValue tradeSellPriceConditionMul { get; set; }

    public FirestoreValue amountStochastic { get; set; }
    public FirestoreValue amountRSI { get; set; }
    public FirestoreValue amountStoRsiTrade { get; set; }

    public FirestoreValue winRateStochastic { get; set; }
    public FirestoreValue winRateRSI { get; set; }
    public FirestoreValue winRateStoRsiTrade { get; set; }
}

public class FirestoreValue
{
    public string stringValue { get; set; }
    public int integerValue { get; set; }
    public float doubleValue { get; set; }
}