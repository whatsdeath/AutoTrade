using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

public static class BackTest
{
    private static float buyCondition = 1.01f;
    private static float sellCondition = 0.95f;

    public static double BackTestingMARSIup(List<CandlesParameters> parameters, float buy, float sell, int RSIpower, float penalty, int power1, int power2)
    {
        buyCondition = buy;
        sellCondition = sell;

        return TestMARSIup(parameters, RSIpower, penalty, power1, power2);
    }


    public static double TestMARSIup(List<CandlesParameters> parameters, int RSIpower, float penalty, int power1, int power2)
    {
        double money = 3000000;

        double beforeMoney = 0;

        float tradeCount = 0;
        int failCount = 0;

        double buyPrice = 0;

        double buyUnitCount = 0;

        double MA1 = 0.0;
        double MA2 = 0.0;

        double beforeMA1;
        double beforeMA2;

        DateTime buyDateTime = new DateTime();
        DateTime sellDateTime = new DateTime();

        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            beforeMA1 = MA1;
            beforeMA2 = MA2;
            if (!CalcMA(parameters, i, power1, out MA1) || !CalcMA(parameters, i, power2, out MA2))
            {
                continue;
            }

            if (buyPrice != 0)
            {
                if (!ChkMABuyCondition(MA1, MA2) 
                    || (ChkMABuyCondition(MA1, MA2) && ChkBuyRSICondition(parameters, i, RSIpower, false)))
                {
                    money += (parameters[i].trade_price * buyUnitCount) * penalty;
                    sellDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);

                    if (beforeMoney < money)
                    {
                        //Debug.Log($"거래 성공? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")}");
                    }
                    else
                    {
                        //Debug.Log($"거래 실패? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")}");
                        failCount++;
                    }

                    buyPrice = 0;
                    buyUnitCount = 0;
                    continue;
                }

                continue;
            }

            if (ChkMABuyCondition(MA1, MA2) && ChkBuyRSICondition(parameters, i, RSIpower, true))
            {
                buyPrice = parameters[i].trade_price;
                buyUnitCount = (money / buyPrice) * penalty;
                beforeMoney = money;
                money = 0;
                buyDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);
                tradeCount++;
            }
        }
        Debug.Log($"MA1 = {power1}, MA2 = {power2} // ({buyCondition}/{sellCondition}/{RSIpower}) 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}");

        return money + (buyPrice * buyUnitCount);
    }

    public static double BackTestingMARSIall(List<CandlesParameters> parameters, int power1, int power2, float upbuy, float upsell, int upRSIpower, 
        float downbuy, float downsell, int downRSIpower ,float penalty)
    {
        return TestMARSIall(parameters, power1, power2, upbuy, upsell, upRSIpower, downbuy, downsell, downRSIpower, penalty);
    }


    public static double TestMARSIall(List<CandlesParameters> parameters, int power1, int power2, float upbuy, float upsell, int upRSIpower,
        float downbuy, float downsell, int downRSIpower ,float penalty)
    {
        double money = 3000000;

        double beforeMoney = 0;

        //float buyCondition = 1.01f;

        //float sellCondition = 0.95f;

        float tradeCount = 0;
        int failCount = 0;

        double buyPrice = 0;

        double buyUnitCount = 0;

        double MA1 = 0.0;
        double MA2 = 0.0;

        double beforeMA1;
        double beforeMA2;

        DateTime buyDateTime = new DateTime();
        DateTime sellDateTime = new DateTime();

        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            beforeMA1 = MA1;
            beforeMA2 = MA2;
            if (!CalcMA(parameters, i, power1, out MA1) || !CalcMA(parameters, i, power2, out MA2))
            {
                continue;
            }

            //상승이면 상승용 
            if (ChkMABuyCondition(MA1, MA2))
            {
                buyCondition = upbuy;
                sellCondition = upsell;
            }
            //하락이면 하락용
            else
            {
                buyCondition = downbuy;
                sellCondition = downsell;
            }

            if (buyPrice != 0)
            {
                if ((!ChkMABuyCondition(MA1, MA2) && ChkMABuyCondition(beforeMA1, beforeMA2)) //상승에서 하락 변환시 일단 판매
                    || (ChkMABuyCondition(MA1, MA2) && ChkBuyRSICondition(parameters, i, upRSIpower, false)) //상승장일때는 상승용
                    || (!ChkMABuyCondition(MA1, MA2) && ChkBuyRSICondition(parameters, i, downRSIpower, false))) //하락장일때는 하락용
                {
                    money += (parameters[i].trade_price * buyUnitCount) * penalty;
                    sellDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);

                    if (beforeMoney < money)
                    {
                        //Debug.Log($"거래 성공? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")}");
                    }
                    else
                    {
                        //Debug.Log($"거래 실패? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")}");
                        failCount++;
                    }

                    buyPrice = 0;
                    buyUnitCount = 0;
                    continue;
                }

                continue;
            }


            if ((ChkMABuyCondition(MA1, MA2) && ChkBuyRSICondition(parameters, i, upRSIpower, true)) //상승장일때는 상승용
                || (!ChkMABuyCondition(MA1, MA2) && ChkBuyRSICondition(parameters, i, downRSIpower, true))) //하락장일때는 하락용
            {
                buyPrice = parameters[i].trade_price;
                buyUnitCount = (money / buyPrice) * penalty;
                beforeMoney = money;
                money = 0;
                buyDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);
                tradeCount++;
            }
        }

        Debug.Log($"MA1 = {power1}, MA2 = {power2} // 상승({upbuy}/{upsell}/{upRSIpower}) // 하락({downbuy}/{downsell}/{downRSIpower}) 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}");

        return money + (buyPrice * buyUnitCount);
    }


    #region MA Test
    public static double BackTestingMA(List<CandlesParameters> parameters, float penalty, int power1, int power2)
    {
        return TestMA(parameters, penalty, power1, power2);
    }

    public static double TestMA(List<CandlesParameters> parameters, float penalty, int power1, int power2)
    {
        double money = 3000000;

        double beforeMoney = 0;

        float tradeCount = 0;
        int failCount = 0;

        double buyPrice = 0;

        double buyUnitCount = 0;

        double MA1;
        double MA2;

        DateTime buyDateTime = new DateTime();
        DateTime sellDateTime = new DateTime();

        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            if (!CalcMA(parameters, i, power1, out MA1) || !CalcMA(parameters, i, power2, out MA2))
            {
                continue;
            }

            if (buyPrice != 0)
            {
                if (!ChkMABuyCondition(MA1, MA2))
                {
                    money += (parameters[i].trade_price * buyUnitCount) * penalty;
                    sellDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);

                    if (beforeMoney < money)
                    {
                        //Debug.Log($"거래 성공? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")}");
                    }
                    else
                    {
                        //Debug.Log($"거래 실패? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")}");
                        failCount++;
                    }

                    buyPrice = 0;
                    buyUnitCount = 0;
                    continue;
                }

                continue;
            }

            if (ChkMABuyCondition(MA1, MA2))
            {
                buyPrice = parameters[i].trade_price;
                buyUnitCount = (money / buyPrice) * penalty;
                beforeMoney = money;
                money = 0;
                buyDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);
                tradeCount++;
            }
        }
        Debug.Log($"MA1 = {power1}, MA2 = {power2} // 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}");

        return money + (buyPrice * buyUnitCount);
    }

    public static bool CalcMA(List<CandlesParameters> parameters, int index, int power, out double MA)
    {
        if (parameters.Count - power <= index)
        {
            MA = 0;
            return false;
        }

        double tempTotalMove = 0.0;

        for (int j = 0; j < power; j++)
        {
            tempTotalMove += parameters[index + j].trade_price;
        }

        MA = tempTotalMove / power;
        return true;
    }

    private static bool ChkMABuyCondition(double MALowPower, double MAHHighPower)
    {
        return MALowPower > MAHHighPower;
    }
    #endregion

    #region RSI Test

    private static bool ChkBuyRSICondition(List<CandlesParameters> parameters, int index, int power, bool isbuy)
    {
        if (parameters.Count - power <= index)
        {
            return false;
        }

        double AU = 0.0;
        double AD = 0.0;

        for (int j = 0; j < power; j++)
        {
            if (parameters[index + (j + 1)].trade_price > parameters[index + j].trade_price)
            {
                AD += parameters[index + (j + 1)].trade_price - parameters[index + j].trade_price;
            }
            else
            {
                AU += parameters[index + j].trade_price - parameters[index + (j + 1)].trade_price;
            }
        }

        AU = AU / power;
        AD = AD / power;

        float RS = 0.0f;
        if(AD <= 0.0f)
        {
            RS = 99.9f;
        }
        else
        {
            RS = (float)(AU / AD);
        }        

        //RSI계산
        float RSIVolume = RS / (1 + RS) * 100.0f;

        if (isbuy)
            return RSIVolume <= buyCondition;
        else
            return RSIVolume >= sellCondition;
    }

    #endregion




    public static double BackTestingReMARSIall(List<CandlesParameters> parameters, int power1, int power2,
        float upbuy, int upbuypower, float upsell, int upsellpower,
        float downbuy, int downbuypower, float downsell, int downsellpower, float penalty)
    {
        return TestReMARSIall(parameters, power1, power2, upbuy, upbuypower, upsell, upsellpower, downbuy, downbuypower, downsell, downsellpower, penalty);
    }


    public static double TestReMARSIall(List<CandlesParameters> parameters, int power1, int power2, 
        float upbuy, int upbuypower, float upsell, int upsellpower,
        float downbuy, int downbuypower, float downsell, int downsellpower, float penalty)
    {
        double money = 3000000;

        double beforeMoney = 0;

        //float buyCondition = 1.01f;

        //float sellCondition = 0.95f;

        float tradeCount = 0;
        int failCount = 0;

        double buyPrice = 0;

        double buyUnitCount = 0;

        double MA1 = 0.0;
        double MA2 = 0.0;

        double beforeMA1;
        double beforeMA2;

        DateTime buyDateTime = new DateTime();
        DateTime sellDateTime = new DateTime();

        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            beforeMA1 = MA1;
            beforeMA2 = MA2;
            if (!CalcMA(parameters, i, power1, out MA1) || !CalcMA(parameters, i, power2, out MA2))
            {
                continue;
            }

            bool isBull = ChkMABuyCondition(MA1, MA2);

            if (buyPrice != 0)
            {
                float sellRSICondition = isBull ? upsell : downsell;
                int sellRSIpower = isBull ? upsellpower : downsellpower;
                float sellRSI;
                if (!TryCalcRSI(parameters, i, sellRSIpower, out sellRSI))
                {
                    continue;
                }

                bool deadCross = !ChkMABuyCondition(MA1, MA2) && ChkMABuyCondition(beforeMA1, beforeMA2);

                float downBuyRSI;
                if (!TryCalcRSI(parameters, i, downbuypower, out downBuyRSI))
                {
                    continue;
                }

                if ((deadCross && downbuy > downBuyRSI)
                    || (sellRSI >= sellRSICondition))
                {
                    money += (parameters[i].trade_price * buyUnitCount) * penalty;
                    sellDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);

                    if (beforeMoney < money)
                    {
                        //Debug.Log($"거래 성공? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")}");
                    }
                    else
                    {
                        //Debug.Log($"거래 실패? : ({buyDateTime}){beforeMoney.ToString("#,###")} -> ({sellDateTime}){money.ToString("#,###")}");
                        failCount++;
                    }

                    buyPrice = 0;
                    buyUnitCount = 0;
                    continue;
                }

                continue;
            }

            float buyRSICondition = isBull ? upbuy : downbuy;
            int buyRSIpower = isBull ? upbuypower : downbuypower;
            float buyRSI;
            if (!TryCalcRSI(parameters, i, buyRSIpower, out buyRSI))
            {
                continue;
            }

            if (buyRSI <= buyRSICondition) //하락장일때는 하락용
            {
                buyPrice = parameters[i].trade_price;
                buyUnitCount = (money / buyPrice) * penalty;
                beforeMoney = money;
                money = 0;
                buyDateTime = Convert.ToDateTime(parameters[i].candle_date_time_kst);
                tradeCount++;
            }
        }

        Debug.Log($"MA1 = {power1}, MA2 = {power2} // 상승({upbuy}/{upbuypower}//{upsell}/{upsellpower}) // 하락({downbuy}/{downbuypower}//{downsell}/{downsellpower}) 거래횟수 : {tradeCount}, 성공률 : {1 - (failCount / tradeCount)}, 금액 : {(money + (buyPrice * buyUnitCount)).ToString("#,###")}");

        return money + (buyPrice * buyUnitCount);
    }



    private static bool TryCalcRSI(List<CandlesParameters> parameters, int index, int power, out float RSI)
    {
        if (parameters.Count - power <= index)
        {
            RSI = 0;
            return false;
        }

        double AU = 0.0;
        double AD = 0.0;

        for (int j = 0; j < power; j++)
        {
            if (parameters[index + (j + 1)].trade_price > parameters[index + j].trade_price)
            {
                AD += parameters[index + (j + 1)].trade_price - parameters[index + j].trade_price;
            }
            else
            {
                AU += parameters[index + j].trade_price - parameters[index + (j + 1)].trade_price;
            }
        }

        AU = AU / power;
        AD = AD / power;

        float RS;
        if (AD <= 0.0f)
        {
            RSI = 100.0f;
            return true;
        }
        else
        {
            RS = (float)(AU / AD);
        }

        //RSI계산
        RSI = RS / (1 + RS) * 100.0f;
        return true;
    }

}
