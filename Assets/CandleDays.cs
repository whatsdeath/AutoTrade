using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

public struct CandlesDayParameters
{
    public string market;                   //마켓명
    public string candle_date_time_utc;     //캔들 기준 시각(UTC 기준) 포맷: yyyy-MM-dd'T'HH:mm:ss
    public string candle_date_time_kst;     //캔들 기준 시각(KST 기준) 포맷: yyyy-MM-dd'T'HH:mm:ss
    public double opening_price;            //시가
    public double high_price;               //고가
    public double low_price;                //저가
    public double trade_price;              //종가
    public long timestamp;                  //마지막 틱이 저장된 시각
    public double candle_acc_trade_price;   //누적 거래 금액
    public double candle_acc_trade_volume;  //누적 거래량
    public double prev_closing_price;       //전일 종가(UTC 0시 기준)
    public double change_price;             //전일 종가 대비 변화 금액
    public double change_rate;              //전일 종가 대비 변화량
    public double converted_trade_price;    //종가 환산 화폐 단위로 환산된 가격(convertingPriceUnit 파라미터 없을 시 해당 필드 포함되지 않음.)
}

public class CandleDays : BaseCandleSearch
{
    protected override string additionalUrl { get => "v1/candles/days"; }
    protected override string apiEndPoint { get => $"market={market}&count={count}&convertingPriceUnit={convertingPriceUnit}"; }

    protected string convertingPriceUnit { get => "KRW"; }
    protected int count { get => 20; }
}
