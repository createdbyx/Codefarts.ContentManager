using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class Loom : MonoBehaviour
{
    public static int maxThreads = 8;
    static int numThreads;

    private static Loom _current;
    private int _count;
    public static Loom Current
    {
        get
        {
            Initialize();
            return _current;
        }
    }

    void Awake()
    {
        _current = this;
        initialized = true;
    }

    static bool initialized;

    static void Initialize()
    {
        if (!initialized)
        {
            if (!Application.isPlaying)
                return;
            initialized = true;
            var g = new GameObject("Loom");
            _current = g.AddComponent<Loom>();

            var values = Enum.GetValues(typeof(QueueType)) as QueueType[];
            foreach (var value in values)
            {
                Current._actions[value] = new List<QueueItem>();
                Current._delayed[value] = new List<DelayedQueueItem>();
                Current._currentDelayed[value] = new List<DelayedQueueItem>();
            }
        }
    }

    public struct QueueItem
    {
        public Action action;
        public Action continueWithAction;
    }

    private Dictionary<QueueType, List<QueueItem>> _actions = new Dictionary<QueueType, List<QueueItem>>();

    public struct DelayedQueueItem
    {
        public float time;
        public Action action;
    }

    Dictionary<QueueType, List<DelayedQueueItem>> _delayed = new Dictionary<QueueType, List<DelayedQueueItem>>();

    Dictionary<QueueType, List<DelayedQueueItem>> _currentDelayed = new Dictionary<QueueType, List<DelayedQueueItem>>();

    public enum QueueType
    {
        Update,
        PostRender,
        FixedUpdate,
        OnPreRender
    }

    public static void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(new QueueItem() { action = action }, 0f, QueueType.Update);
    }

    public static void QueueOnMainThread(Action action, float time)
    {
        QueueOnMainThread(new QueueItem() { action = action }, time, QueueType.Update);
    }
    public static void QueueOnMainThread(Action action, QueueType type)
    {
        QueueOnMainThread(new QueueItem() { action = action }, 0f, type);
    }

    public static void QueueOnMainThread(Action action, float time, QueueType type)
    {
        QueueOnMainThread(new QueueItem() { action = action }, time, type);
    }

    public static void QueueOnMainThread(QueueItem action)
    {
        QueueOnMainThread(action, 0f, QueueType.Update);
    }

    public static void QueueOnMainThread(QueueItem action, float time)
    {
        QueueOnMainThread(action, time, QueueType.Update);
    }

    public static void QueueOnMainThread(QueueItem action, QueueType type)
    {
        QueueOnMainThread(action, 0f, type);
    }

    public static void QueueOnMainThread(QueueItem action, float time, QueueType type)
    {
        if (time != 0)
        {
            lock (Current._delayed)
            {
                Current._delayed[type].Add(new DelayedQueueItem { time = Time.time + time, action = action.action });
            }
        }
        else
        {
            lock (Current._actions)
            {
                Current._actions[type].Add(action);
            }
        }
    }

    public static Thread RunAsync(Action a)
    {
        Initialize();
        while (numThreads >= maxThreads)
        {
            Thread.Sleep(1);
        }

        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }

    private static void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch
        {
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }
    }

    void OnDisable()
    {
        if (_current == this)
        {

            _current = null;
        }
    }

    List<QueueItem> _currentActions = new List<QueueItem>();

    public void OnPreRender()
    {
        this.DoCallbacks(QueueType.PostRender);
    }

    public void OnPostRender()
    {
        this.DoCallbacks(QueueType.PostRender);
    }

    public void FixedUpdate()
    {
        this.DoCallbacks(QueueType.FixedUpdate);
    }

    // Update is called once per frame
    void Update()
    {
        this.DoCallbacks(QueueType.Update);
    }

    private void DoCallbacks(QueueType type)
    {
        lock (_actions)
        {
            _currentActions.Clear();
            _currentActions.AddRange(_actions[type]);
            _actions[type].Clear();
        }

        foreach (var item in _currentActions)
        {
            item.action();
            if (item.continueWithAction != null)
            {
                _currentActions.Add(new QueueItem() { action = item.continueWithAction });
            }
        }

        var delayedQueueItems = _currentDelayed[type];
        lock (_delayed)
        {
            delayedQueueItems.Clear();
            var list = new DelayedQueueItem[1000];
            var count = 0;

            foreach (var d in _delayed[type])
            {
                if (d.time <= Time.time)
                {
                    list[count] = d;
                    count++;

                    if (count >= list.Length)
                    {
                        Array.Resize(ref list, (int)(list.Length * 1.25f));
                    }
                }
            }

            if (count > 0)
            {
                if (list.Length > count)
                {
                    Array.Resize(ref list, count);
                }

                delayedQueueItems.AddRange(list);
                //delayedQueueItems.AddRange(_delayed[type].Where(d => d.time <= Time.time));
                foreach (var item in delayedQueueItems)
                {
                    _delayed[type].Remove(item);
                }
            }
        }

        foreach (var delayed in delayedQueueItems)
        {
            delayed.action();
        }
    }
}

