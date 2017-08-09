using System.Collections.Generic;
using UnityEngine;

public abstract class MultiCompressBase<T> where T : CompressNotMonoBase, new()
{
    protected List<CompressConfig> configs = new List<CompressConfig>();
    protected List<T> workingTask = new List<T>();
    protected List<T> finishTask = new List<T>();
    protected CompressCallback callback = null;
    protected bool working = false;
    /// <summary>
    /// 系统的核数
    /// </summary>
    protected int processorCount = 1;

    protected long totalSize = 0;
    public long TotalSize
    {
        get
        {
            return totalSize;
        }
    }

    public long FinishSize
    {
        get
        {
            long finishSize = 0;
            foreach (T com in finishTask)
            {
                finishSize += com.FinishSize;
            }
            foreach (T com in workingTask)
            {
                finishSize += com.FinishSize;
            }
            return finishSize;
        }
    }

    public CompressState Status
    {
        get
        {
            if (!working)
            {
                return CompressState.None;
            }
            foreach (T com in workingTask)
            {
                if (com.Status == CompressState.Working)
                {
                    return CompressState.Working;
                }
            }
            if (configs.Count > 0)
            {
                return CompressState.Working;
            }
            return CompressState.Finish;
        }
    }

    public MultiCompressBase()
    {
        processorCount = SystemInfo.processorCount;
        callback = null;
        working = false;
        totalSize = 0;
    }

    public abstract CompressConfig CalInFileSize(CompressConfig config);

    public void AddCompressConfig(CompressConfig config)
    {
        config = CalInFileSize(config);
        totalSize += config.inFileSize;

        if (processorCount > workingTask.Count)
        {
            T com = new T();
            com.SetConfig(config, () =>
            {
                Refresh();
            });
            if (working)
            {
                com.Start();
            }
            workingTask.Add(com);
        }
        else
        {
            configs.Add(config);
        }
    }

    object lockd = new object();
    private void Refresh()
    {
        if (!working)
        {
            return;
        }
        lock (lockd)
        {
            for (int i = 0; i < workingTask.Count; i++)
            {
                if (workingTask[i].Status == CompressState.Finish
                    || workingTask[i].Status == CompressState.Error)
                {
                    finishTask.Add(workingTask[i]);
                    workingTask.RemoveAt(i);
                    i--;
                }
            }
            while (true)
            {
                if (configs.Count <= 0)
                {
                    break;
                }
                if (workingTask.Count >= processorCount)
                {
                    break;
                }
                T com = new T();
                com.SetConfig(configs[0], () =>
                {
                    Refresh();
                });
                com.Start();
                workingTask.Add(com);
                configs.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// 设置回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetCallback(CompressCallback callback)
    {
        this.callback = callback;
    }

    public void Start()
    {
        if (working)
        {
            return;
        }
        working = true;
        foreach (T com in workingTask)
        {
            com.Start();
        }
    }

    public void UpdateCallback()
    {
        if (callback != null)
        {
            callback(FinishSize, TotalSize, Status);
        }
    }
}