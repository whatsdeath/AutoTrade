using System;
using Unity.VisualScripting;

public enum ProcessSequence
{
    None, TradePhase, BackTestPhase
}

public class TimeManager : BaseManager<TimeManager>
{
    ServerTimeSyncer timeSyncer;

    private DateTime _nowTime;
    public DateTime nowTime { get => _nowTime; }

    public TimeSpan timeCorrectionValue { get; private set; }
    public bool isCorrection { get; private set; }

    public ProcessSequence processSequence { get; private set; }

    protected override void Init()
    {
        timeSyncer = CreateComponentObjectInChildrenAndReturn<ServerTimeSyncer>();
    }

    private void Start()
    {
        timeSyncer.TimeSyncWithServer();
    }

    private void FixedUpdate()
    {
        if (!isCorrection)
        {
            _nowTime = DateTime.Now;
            return;
        }

        _nowTime = DateTime.Now + timeCorrectionValue;

        UIManager.Instance.VisualizationNowTime();

        if(ChkTradeTime())
        {
            SetProcessSequence(ProcessSequence.TradePhase);
        }
        else
        {
            SetProcessSequence(ProcessSequence.BackTestPhase);
        }
    }

    private void SetProcessSequence(ProcessSequence sequence)
    {
        if (!sequence.Equals(processSequence) && !sequence.Equals(ProcessSequence.None))
        {
            string massege = $"{AppManager.Instance.machineName}({AppManager.Instance.ip})\n[{nowTime}]\n";

            if (!processSequence.Equals(ProcessSequence.None))
            {
                massege += $"{processSequence}를 종료합니다. \n=> ";
            }

            processSequence = sequence;
            massege += $"<b>{processSequence}를 실행합니다.</b>";

            AppManager.Instance.TelegramMassage(massege, TelegramBotType.DebugLog);
        }
    }

    public void TimeCorrection(DateTime time)
    {
        timeCorrectionValue = time - DateTime.Now;
        isCorrection = true;
    }

    public bool ChkTradeTime()
    {
        int remainder = _nowTime.Minute % GlobalValue.CAMDLE_MINUTE_UNIT;

        if (remainder.Equals(0) || (remainder.Equals(GlobalValue.CAMDLE_MINUTE_UNIT - 1) && _nowTime.Second >= 30))
        {
            return true;
        }

        return false;
    }
}
