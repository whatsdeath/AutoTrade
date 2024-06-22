using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public enum TelegramBotType
{
    Trade, BackTest, DebugLog, MaxCount
}

public class TelegramLogger : MonoBehaviour
{
    public readonly string[] botTokens = new string[(int)TelegramBotType.MaxCount]
    {
        "7312145875:AAFMNEDtQ1RZnwtkOBz0V4fF4j5IQFsXqzM", //Trade
        "7495142060:AAEZaAU8_QPEeYnEbGspjQd0-2zu6zxqgsc", //BackTest
        "7127935712:AAE3JI1ryK_k0B_l1jNHouoNRE79LfdvD1s" //DebugLog
    };

    private static readonly HttpClient client = new HttpClient();
    private const string ChatId = "7205994029"; // 얻은 개인 채팅 ID

    public async Task SendMessageToTelegram(string message, TelegramBotType type)
    {
        try
        {
            string url = $"https://api.telegram.org/bot{botTokens[(int)type]}/sendMessage?chat_id={ChatId}&text={Uri.EscapeDataString(message)}&parse_mode=html";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            DebugByPlatform.Debug.LogOnlyEditer("Message sent to Telegram successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send message to Telegram: {ex.Message}");
        }
    }
}
