using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class TelegramMessage
{
    public Result[] result;
}

[System.Serializable]
public class Result
{
    public int update_id;
    public Message message;
}

[System.Serializable]
public class Message
{
    public int message_id;
    public From from;
    public Chat chat;
    public int date;
    public string text;
    public Entity[] entities;
}

[System.Serializable]
public class From
{
    public int id;
    public bool is_bot;
    public string first_name;
    public string last_name;
    public string language_code;
}

[System.Serializable]
public class Chat
{
    public int id;
    public string first_name;
    public string last_name;
    public string type;
}

[System.Serializable]
public class Entity
{
    public int offset;
    public int length;
    public string type;
}


public class TelegramCommandListener : MonoBehaviour
{
    public readonly string[] botTokens = new string[(int)TelegramBotType.MaxCount]
    {
        "7312145875:AAFMNEDtQ1RZnwtkOBz0V4fF4j5IQFsXqzM", //Trade
        "7495142060:AAEZaAU8_QPEeYnEbGspjQd0-2zu6zxqgsc", //BackTest
        "7127935712:AAE3JI1ryK_k0B_l1jNHouoNRE79LfdvD1s" //DebugLog
    };

    private string chatId = "7205994029";

    private int[] lastUpdateId = new int[(int)TelegramBotType.MaxCount];

    void Start()
    {
        StartCoroutine(GetUpdates(TelegramBotType.Trade));
        /*StartCoroutine(GetUpdates(TelegramBotType.BackTest));*/
    }

    IEnumerator GetUpdates(TelegramBotType botType)
    {
        while (true)
        {
            UnityWebRequest first = UnityWebRequest.Get($"https://api.telegram.org/bot{botTokens[(int)botType]}/getUpdates");
            yield return first.SendWebRequest();

            if (first.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(first.error);
            }
            else
            {
                var updates = JsonUtility.FromJson<TelegramMessage>(first.downloadHandler.text);
                if (updates.result.Length > 0)
                {
                    lastUpdateId[(int)botType] = updates.result[updates.result.Length - 1].update_id;
                }
                yield return new WaitForSeconds(1);
                break;
            }
            yield return new WaitForSeconds(1);
        }

        while (true)
        {
            string url = $"https://api.telegram.org/bot{botTokens[(int)botType]}/getUpdates";
            if (lastUpdateId[(int)botType] != 0)
            {
                url += $"?offset={lastUpdateId[(int)botType] + 1}";
            }

            // 텔레그램 API의 getUpdates 메서드를 호출하여 봇에 대한 업데이트를 가져옵니다.
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                HandleUpdates(www.downloadHandler.text, botType);
            }

            yield return new WaitForSeconds(1);
        }
    }       

    void HandleUpdates(string json, TelegramBotType botType)
    {
        var updates = JsonUtility.FromJson<TelegramMessage>(json);

        foreach (var result in updates.result)
        {
            if (result.message != null)
            {
                lastUpdateId[(int)botType] = result.update_id; // 마지막 업데이트 ID 갱신
                Debug.Log($"botType = {botType} / lastUpdateId = {lastUpdateId[(int)botType]} / length = {result.message.text.Length} / result.message.text = {result.message.text}");

                ExecuteProgramFunction(result.message.text, botType);
            }
        }
    }

    void ExecuteProgramFunction(string text, TelegramBotType botType)
    {
        var splitText = text.Split(' ');

        if (splitText.Length > 0 && splitText[0][0].Equals('/')) 
        {
            switch (botType) 
            {
                case TelegramBotType.Trade:
                    ExecuteTradeCommand(splitText);
                    break;

                default:
                    break;            
            } 
        }
    }



    void ExecuteTradeCommand(string[] commandTexts)
    {
        switch (commandTexts[0].ToUpper())
        {
            case "/SETCONDITION":
                if (commandTexts.Length < 2)
                {
                    AppManager.Instance.TelegramMassage("양식에 맞지 않는 명령입니다. \n(/SetCondition {JsonText})", TelegramBotType.Trade);
                    break;
                }
                AppManager.Instance.AddData(commandTexts[1]);
                break;

            case "/LOADDATA":
                if (commandTexts.Length > 1)
                {
                    AppManager.Instance.TelegramMassage("양식에 맞지 않는 명령입니다. \n(/SetCondition {JsonText})", TelegramBotType.Trade);
                    break;
                }
                TradeManager.Instance.SetConditionByMarket(AppManager.Instance.LoadData());
                break;

            default:
                //AppManager.Instance.TelegramMassage("존재하지 않는 명령어입니다.", TelegramBotType.Trade);
                break;
        }
    }

    IEnumerator SendMessage(string message, TelegramBotType botType)
    {
        // �ڷ��׷� API�� sendMessage �޼��带 ȣ���Ͽ� �޽����� �����ϴ�.
        UnityWebRequest www = UnityWebRequest.PostWwwForm($"https://api.telegram.org/bot{botTokens[(int)botType]}/sendMessage?chat_id={chatId}&text={message}", "");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Message sent: " + message);
        }
    }
}

