using System;

public class TimeManager : BaseManager<TimeManager>
{
    ServerTimeSyncer timeSyncer;

    private DateTime _nowTime;
    public DateTime nowTime { get => _nowTime; }

    public TimeSpan timeCorrectionValue { get; private set; }
    public bool isCorrection { get; private set; }

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
        if(isCorrection)
        {
            _nowTime = DateTime.Now + timeCorrectionValue;
        }
        else
        {
            _nowTime = DateTime.Now;
        }        

        UIManager.Instance.VisualizationNowTime();        
    }

    public void TimeCorrection(DateTime time)
    {
        timeCorrectionValue = time - DateTime.Now;
        isCorrection = true;
    }
}
