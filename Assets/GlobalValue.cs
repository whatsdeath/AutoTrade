using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class GlobalValue
{
    public const string ACCESS_KEY = "HeSpT67EQwOF0K4xiZHHSmvZS5S6MeIDlW4QE53f";
    public const string SECRET_KEY = "SPTQ3GZn7QcT7AGrSLS3KxPda7yDzJvgr7z6AfJ0";

    public const string SERVER_URL = "https://api.upbit.com";

    static public readonly string[] CHECK_PRICE_UNIT = new string[]
    {
        "KRW"
    };

    static public readonly string[] IGNORE_CURRENCYS = new string[]
    {   
        "VTHO", "APENFT", "ICZ"
    };

    public const float ACCESS_INTERVAL = 0.2f;

    public const int SAVE_DATA_MAX_COUNT = 10000; //4500;//150000;// 450000;
    public const int SAVE_10MIN_DATA_MAX_COUNT = 225000;
    public const int SAVE_15MIN_DATA_MAX_COUNT = 150000;

    public const int CAMDLE_MINUTE_UNIT = (int)MinuteUnit.Minutes_5;

    public const int RSI_MAX_STRENGTH = 20;
}
