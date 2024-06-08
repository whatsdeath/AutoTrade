using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ChkEmpty
{
    public bool ChkEmpty();
    public void Clear();
}

public struct Delegate : ChkEmpty
{
    Action list;
    public void AddDelegate(Action func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Action func)
    {
        list -= func;
    }
    public void Play()
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty delegate");
            return;
        }
        list.Invoke();
    }

    public bool ChkEmpty()
    {
        return list == null;
    }
    public void Clear()
    {
        list = null;
    }
}
public struct Delegate<T> : ChkEmpty
{
    Action<T> list;
    public void AddDelegate(Action<T> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Action<T> func)
    {
        list -= func;
    }
    public void Play(T value) 
    { 
        if(list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty delegate<T>");
            return;
        }
        list.Invoke(value);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }
    public void Clear()
    {
        list = null;
    }
}
public struct Delegate<T1, T2> : ChkEmpty
{
    Action<T1, T2> list;
    public void AddDelegate(Action<T1, T2> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Action<T1, T2> func)
    {
        list -= func;
    }
    public void Play(T1 value1, T2 value2)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty delegate<T1, T2>");
            return;
        }
        list.Invoke(value1, value2);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }
    public void Clear()
    {
        list = null;
    }
}
public struct Delegate<T1, T2, T3> : ChkEmpty
{
    Action<T1, T2, T3> list;
    public void AddDelegate(Action<T1, T2, T3> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Action<T1, T2, T3> func)
    {
        list -= func;
    }
    public void Play(T1 value1, T2 value2, T3 value3)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty delegate<T1, T2, T3>");
            return;
        }
        list.Invoke(value1, value2, value3);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }
    public void Clear()
    {
        list = null;
    }
}
public struct Delegate<T1, T2, T3, T4> : ChkEmpty
{
    Action<T1, T2, T3, T4> list;
    public void AddDelegate(Action<T1, T2, T3, T4> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Action<T1, T2, T3, T4> func)
    {
        list -= func;
    }
    public void Play(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty delegate<T1, T2, T3, T4>");
            return;
        }
        list.Invoke(value1, value2, value3, value4);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }

    public void Clear()
    {
        list = null;
    }
}
public struct Delegate<T1, T2, T3, T4, T5>  : ChkEmpty
{
    Action<T1, T2, T3, T4, T5> list;
    public void AddDelegate(Action<T1, T2, T3, T4, T5> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Action<T1, T2, T3, T4, T5> func)
    {
        list -= func;
    }
    public void Play(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty delegate<T1, T2, T3, T4, T5>");
            return;
        }
        list.Invoke(value1, value2, value3, value4, value5);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }
    public void Clear()
    {
        list = null;
    }
}
public struct Delegate<T1, T2, T3, T4, T5, T6> : ChkEmpty
{
    Action<T1, T2, T3, T4, T5, T6> list;
    public void AddDelegate(Action<T1, T2, T3, T4, T5, T6> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Action<T1, T2, T3, T4, T5, T6> func)
    {
        list -= func;
    }
    public void Play(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty delegate<T1, T2, T3, T4, T5, T6>");
            return;
        }
        list.Invoke(value1, value2, value3, value4, value5, value6);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }
    public void Clear()
    {
        list = null;
    }
}

public struct Function<T1, T2> : ChkEmpty
{
    Func<T1, T2> list;
    public void AddDelegate(Func<T1, T2> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Func<T1, T2> func)
    {
        list -= func;
    }
    public T2 Play(T1 value1)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty function<T1, T2>");
            return default(T2);
        }
        return list.Invoke(value1);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }
    public void Clear()
    {
        list = null;
    }
}
public struct Function<T1, T2, T3> : ChkEmpty
{
    Func<T1, T2, T3> list;
    public void AddDelegate(Func<T1, T2, T3> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Func<T1, T2, T3> func)
    {
        list -= func;
    }
    public T3 Play(T1 value1, T2 value2)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty function<T1, T2, T3>");
            return default(T3);
        }
        return list.Invoke(value1, value2);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }

    public void Clear()
    {
        list = null;
    }
}
public struct Function<T1, T2, T3, T4> : ChkEmpty
{
    Func<T1, T2, T3, T4> list;
    public void AddDelegate(Func<T1, T2, T3, T4> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Func<T1, T2, T3, T4> func)
    {
        list -= func;
    }
    public T4 Play(T1 value1, T2 value2, T3 value3)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty function<T1, T2, T3, T4>");
            return default(T4);
        }
        return list.Invoke(value1, value2, value3);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }

    public void Clear()
    {
        list = null;
    }
}
public struct Function<T1, T2, T3, T4, T5> : ChkEmpty
{
    Func<T1, T2, T3, T4, T5> list;
    public void AddDelegate(Func<T1, T2, T3, T4, T5> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Func<T1, T2, T3, T4, T5> func)
    {
        list -= func;
    }
    public T5 Play(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty function<T1, T2, T3, T4, T5>");
            return default(T5);
        }
        return list.Invoke(value1, value2, value3, value4);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }

    public void Clear()
    {
        list = null;
    }
}
public struct Function<T1, T2, T3, T4, T5, T6> : ChkEmpty
{
    Func<T1, T2, T3, T4, T5, T6> list;
    public void AddDelegate(Func<T1, T2, T3, T4, T5, T6> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Func<T1, T2, T3, T4, T5, T6> func)
    {
        list -= func;
    }
    public T6 Play(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty function<T1, T2, T3, T4, T5, T6>");
            return default(T6);
        }
        return list.Invoke(value1, value2, value3, value4, value5);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }

    public void Clear()
    {
        list = null;
    }
}
public struct Function<T1, T2, T3, T4, T5, T6, T7> : ChkEmpty
{
    Func<T1, T2, T3, T4, T5, T6, T7> list;
    public void AddDelegate(Func<T1, T2, T3, T4, T5, T6, T7> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Func<T1, T2, T3, T4, T5, T6, T7> func)
    {
        list -= func;
    }
    public T7 Play(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty function<T1, T2, T3, T4, T5, T6, T7>");
            return default(T7);
        }
        return list.Invoke(value1, value2, value3, value4, value5, value6);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }

    public void Clear()
    {
        list = null;
    }
}
public struct Function<T1, T2, T3, T4, T5, T6, T7, T8> : ChkEmpty
{
    Func<T1, T2, T3, T4, T5, T6, T7, T8> list;
    public void AddDelegate(Func<T1, T2, T3, T4, T5, T6, T7, T8> func)
    {
        list -= func;
        list += func;
    }
    public void RemoveDelegate(Func<T1, T2, T3, T4, T5, T6, T7, T8> func)
    {
        list -= func;
    }
    public T8 Play(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
    {
        if (list == null)
        {
            DebugByPlatform.Debug.LogOnlyEditer("Empty function<T1, T2, T3, T4, T5, T6, T7, T8>");
            return default(T8);
        }
        return list.Invoke(value1, value2, value3, value4, value5, value6, value7);
    }
    public bool ChkEmpty()
    {
        return list == null;
    }

    public void Clear()
    {
        list = null;
    }
}


public static class DictionaryFunction
{
    public static void ClearValueOnly<TKey, TValue>(Dictionary<TKey, List<TValue>> listDic) where TValue : class
    {
        List<TKey> keysList = new List<TKey>(listDic.Keys);

        for (int i = 0; i < keysList.Count; i++)
        {
            listDic[keysList[i]].Clear();
        }
    }

    public static void ClearValueOnly<TKey, TValue>(Dictionary<TKey, TValue> dic) where TValue : class
    {
        List<TKey> keysList = new List<TKey>(dic.Keys);

        for (int i = 0; i < keysList.Count; i++)
        {
            dic[keysList[i]] = null;
        }
    }

    public static void ClearValueOnly<TKey>(Dictionary<TKey, bool> dic)
    {
        List<TKey> keysList = new List<TKey>(dic.Keys);

        for (int i = 0; i < keysList.Count; i++)
        {
            dic[keysList[i]] = false;
        }
    }

    public static void ClearValueOnly<TKey>(Dictionary<TKey, int> dic)
    {
        List<TKey> keysList = new List<TKey>(dic.Keys);

        for (int i = 0; i < keysList.Count; i++)
        {
            dic[keysList[i]] = 0;
        }
    }

    public static void ClearValueOnly<TKey>(Dictionary<TKey, float> dic)
    {
        List<TKey> keysList = new List<TKey>(dic.Keys);

        for (int i = 0; i < keysList.Count; i++)
        {
            dic[keysList[i]] = 0.0f;
        }
    }

    public static TKey FindFirstKey<TKey, TValue>(Dictionary<TKey, TValue> dic, TValue value)
    {
        List<TKey> keyList = new List<TKey>(dic.Keys);

        for (int i = 0; i < keyList.Count; i++)
        {
            if(dic[keyList[i]] != null && dic[keyList[i]].Equals(value))
            {
                return keyList[i];
            }
        }

        return default;
    }

    public static List<TValue> GetValueList<TKey, TValue>(Dictionary<TKey, TValue> dic)
    {
        List<TValue> valueList = new List<TValue>(dic.Values);

        return valueList;
    }
}


