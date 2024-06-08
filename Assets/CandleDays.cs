using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

public struct CandlesDayParameters
{
    public string market;                   //���ϸ�
    public string candle_date_time_utc;     //ĵ�� ���� �ð�(UTC ����) ����: yyyy-MM-dd'T'HH:mm:ss
    public string candle_date_time_kst;     //ĵ�� ���� �ð�(KST ����) ����: yyyy-MM-dd'T'HH:mm:ss
    public double opening_price;            //�ð�
    public double high_price;               //��
    public double low_price;                //����
    public double trade_price;              //����
    public long timestamp;                  //������ ƽ�� ����� �ð�
    public double candle_acc_trade_price;   //���� �ŷ� �ݾ�
    public double candle_acc_trade_volume;  //���� �ŷ���
    public double prev_closing_price;       //���� ����(UTC 0�� ����)
    public double change_price;             //���� ���� ��� ��ȭ �ݾ�
    public double change_rate;              //���� ���� ��� ��ȭ��
    public double converted_trade_price;    //���� ȯ�� ȭ�� ������ ȯ��� ����(convertingPriceUnit �Ķ���� ���� �� �ش� �ʵ� ���Ե��� ����.)
}

public class CandleDays : BaseCandleSearch
{
    protected override string additionalUrl { get => "v1/candles/days"; }
    protected override string apiEndPoint { get => $"market={market}&count={count}&convertingPriceUnit={convertingPriceUnit}"; }

    protected string convertingPriceUnit { get => "KRW"; }
    protected int count { get => 20; }
}
