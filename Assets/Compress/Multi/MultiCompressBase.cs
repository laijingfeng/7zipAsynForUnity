using System.Collections.Generic;
using UnityEngine;

public abstract class MultiCompressBase
{
    protected List<CompressConfig> configs = new List<CompressConfig>();
    protected List<CompressNotMonoBase> workingTask = new List<CompressNotMonoBase>();
    protected List<CompressNotMonoBase> finishTask = new List<CompressNotMonoBase>();
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

    public MultiCompressBase()
    {
        processorCount = SystemInfo.processorCount;
        callback = null;
        working = false;
        totalSize = 0;
    }

    public CompressState Status
    {
        get
        {
            if (!working)
            {
                return CompressState.None;
            }
            foreach (CompressNotMonoBase com in workingTask)
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

    public long FinishSize
    {
        get
        {
            long finishSize = 0;
            foreach (CompressNotMonoBase com in finishTask)
            {
                finishSize += com.FinishSize;
            }
            foreach (CompressNotMonoBase com in workingTask)
            {
                finishSize += com.FinishSize;
            }
            return finishSize;
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

    public abstract void AddCompressConfig(CompressConfig config);
    protected abstract void Refresh();

    public void Start()
    {
        if (working)
        {
            return;
        }
        working = true;
        foreach (CompressNotMonoBase com in workingTask)
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