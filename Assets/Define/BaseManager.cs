using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager<TManager> : MonoBehaviour where TManager : MonoBehaviour
{
    #region Field
    protected static bool _isQuit;
    #endregion

    #region Method - Singleton
    private static TManager instance;

    public static TManager Instance
    {
        get
        {
            if(_isQuit)
                return null;

            if (instance == null)
            {
                GameObject obj;
                obj = GameObject.Find($"@{typeof(TManager).Name}");
                if (obj == null)
                {
                    obj = new GameObject($"@{typeof(TManager).Name}");
                    instance = obj.AddComponent<TManager>();
                }
                else
                {
                    instance = obj.GetComponent<TManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    #region Unity Event
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = gameObject.GetComponent<TManager>();

        Init();
    }

    protected virtual void Init() {}

    public void Reset()
    {
        instance = null;
    }

    private void OnEnable()
    {
        instance = null;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void OnApplicationQuit()
    {
        _isQuit = true;
    }

    public void Call() { }
    public void Call(Transform parent) 
    { 
        transform.parent = parent;
    }

    protected void CreateComponentObjectInChildren<TComponent>() where TComponent : Component
    {
        if (GetComponentInChildren<TComponent>() == null)
        {
            GameObject newGameObject = new GameObject($"#{typeof(TComponent)}");
            newGameObject.GetComponent<Transform>().parent = transform;
            newGameObject.AddComponent<TComponent>();
        }       
    }

    protected TComponent CreateComponentObjectInChildrenAndReturn<TComponent>(bool multiple = false) where TComponent : Component
    {
        if (multiple || GetComponentInChildren<TComponent>() == null)
        {
            GameObject newGameObject = new GameObject($"#{typeof(TComponent)}");
            newGameObject.GetComponent<Transform>().parent = transform;
            return newGameObject.AddComponent<TComponent>();
        }

        return GetComponentInChildren<TComponent>();
    }
    #endregion
}
