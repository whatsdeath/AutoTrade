using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public abstract class BaseWebRequest<Tstruct> : MonoBehaviour where Tstruct : struct
{
    public readonly char[] trimList = new char[] { '[', ']' };

    protected Dictionary<string, object> payloadParameter = new Dictionary<string, object>();
    Dictionary<string, object> headers = new Dictionary<string, object>();

    [SerializeField] private Dictionary<string, Tstruct> _parameters = new Dictionary<string, Tstruct>();
    virtual public Dictionary<string, Tstruct> parameters { get => _parameters; }

    abstract protected string additionalUrl { get; }
    virtual protected string apiEndPoint { get => string.Empty; }

    private void Awake()
    {
        payloadParameter.Add("access_key", GlobalValue.ACCESS_KEY);
        payloadParameter.Add("nonce", string.Empty);

        Init();
    }

    virtual protected void Init() { }

    protected IEnumerator AccessLoop(string apiEndPoint = "")
    {
        float nextAccessTime = Time.realtimeSinceStartup + GlobalValue.ACCESS_INTERVAL;

        while (true)
        {
            UnityWebRequest request;

            if (!string.IsNullOrEmpty(apiEndPoint))
            {
                request = ConnectionRequest($"{GlobalValue.SERVER_URL}/{additionalUrl}?{apiEndPoint}");
            }
            else
            {
                request = ConnectionRequest();
            }

            yield return request.SendWebRequest();

            if (TryDataProcessing(request.downloadHandler))
            {
                AfterProcess();
            }
            else
            {
                yield break;
            }

            if (nextAccessTime > Time.realtimeSinceStartup)
            {
                yield return new WaitForSeconds(nextAccessTime - Time.realtimeSinceStartup);

            }

            nextAccessTime = Time.realtimeSinceStartup + GlobalValue.ACCESS_INTERVAL;
        }
    }

    public IEnumerator Access(string apiEndPoint = "")
    {
        UnityWebRequest request;

        if (!string.IsNullOrEmpty(apiEndPoint))
        {
            request = ConnectionRequest($"{GlobalValue.SERVER_URL}/{additionalUrl}?{apiEndPoint}");
        }
        else
        {
            request = ConnectionRequest();
        }        

        yield return request.SendWebRequest();

        if (TryDataProcessing(request.downloadHandler))
        {
            AfterProcess();
        }        
    }


    private UnityWebRequest ConnectionRequest()
    {
        if (string.IsNullOrEmpty(apiEndPoint))
        {
            return ConnectionRequest($"{GlobalValue.SERVER_URL}/{additionalUrl}");
        }
        else
        {
            return ConnectionRequest($"{GlobalValue.SERVER_URL}/{additionalUrl}?{apiEndPoint}");
        }
    }

    virtual protected UnityWebRequest ConnectionRequest(string url)
    {
        UnityWebRequest request;
        
        request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", CreateToken());

        return request;
    }

    private bool TryDataProcessing(DownloadHandler handler)
    {
        string resultReplace = ReplaceProccess(handler);

        if(string.IsNullOrEmpty(resultReplace))
        {
            DebugByPlatform.Debug.Log($"{typeof(Tstruct)} : 값을 반환받지 못했습니다.");
            ErrorProcess();
            return false;
        }        
        string[] splitResults = resultReplace.Split(';');


        //Debug.Log(resultReplace);

        _parameters.Clear();

        for (int i = 0; i < splitResults.Length; i++)
        {
            Tstruct data = JsonUtility.FromJson<Tstruct>(splitResults[i]);

            //예외조건이 설정되어 있고, 그에 적용될 경우 continue
            if (ChkIgnoreCondition(data))
            {
                continue;
            }


            string paramKey = SetParamsKey(data);

            if(string.IsNullOrEmpty(paramKey))
            {
                DebugByPlatform.Debug.Log(resultReplace);
                continue;
            }

            if (_parameters.ContainsKey(paramKey))
            {
                DebugByPlatform.Debug.Log($"{typeof(Tstruct)} : 중복된 Key가 발견되었습니다.");
            }

            _parameters.Add(paramKey, data);
        }

        return true;
    }

    virtual protected void AfterProcess() { }
    virtual protected void ErrorProcess() { }

    protected string CreateToken()
    {
        PreparationCreateToken();

        payloadParameter["nonce"] = Guid.NewGuid().ToString();
        
        var jwtToken = JWT.JsonWebToken.Encode(payloadParameter, GlobalValue.SECRET_KEY, JWT.JwtHashAlgorithm.HS256);

        return $"Bearer {jwtToken}";        
    }

    virtual protected void PreparationCreateToken() {}

    private string ReplaceProccess(DownloadHandler handler)
    {
        return ReplaceProccess(handler.text);
    }

    private string ReplaceProccess(string text)
    {
        string replacePreparation = text.Replace("},{", "};{");
        return replacePreparation.Trim(trimList);
    }

    virtual protected bool ChkIgnoreCondition(object data)
    {
        return false;
    }

    abstract protected string SetParamsKey(object data);
}
