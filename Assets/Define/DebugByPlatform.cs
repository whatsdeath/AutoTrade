using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DebugByPlatform
{
    public static class Debug
    {
        public static void Log(object message)
        {
#if UNITY_EDITOR
            LogByEditer(message);
#else
            if(AppManager.Instance != null)
            {
                AppManager.Instance.TelegramMassage(message.ToString(), TelegramBotType.DebugLog);
            }
#endif
        }

        public static void Log(object message, TelegramBotType logType)
        {
#if UNITY_EDITOR
            LogByEditer(message);
#else
            if(AppManager.Instance != null)
            {
                AppManager.Instance.TelegramMassage(message.ToString(), logType);
            }
#endif
        }

        public static void LogOnlyEditer(object message)
        {
#if UNITY_EDITOR
            LogByEditer(message);
#endif
        }

        private static void LogByEditer(object message)
        {
            UnityEngine.Debug.Log(message);
        }
    }
}

