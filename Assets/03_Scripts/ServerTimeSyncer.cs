using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public struct ServerTimeParam
{
    public string market;
    public string trade_date_utc;
    public string trade_time_utc;
    public long timestamp;         //ȭ�� �ǹ��ϴ� ���� �빮�� �ڵ�
}

public class ServerTimeSyncer : BaseWebRequest<ServerTimeParam>
{
    private string market { get => "KRW-BTC"; }

    protected override string additionalUrl { get => $"v1/trades/ticks?market={market}&count=1"; }

    public void TimeSyncWithServer()
    {
        StartCoroutine(Access());
    }

    protected override string SetParamsKey(object data)
    {
        return ((ServerTimeParam)data).market;
    }

    protected override void AfterProcess()
    {
        var time = $"{parameters[market].trade_date_utc} {parameters[market].trade_time_utc}";
        DateTime serverTime = DateTime.Parse(time).AddHours(9);

        TimeManager.Instance.TimeCorrection(serverTime);
    }
}