using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MultiDecompress
{
    private List<CompressConfig> configs = new List<CompressConfig>();
    private List<DecompressNotMono> workingTask = new List<DecompressNotMono>();
    private List<DecompressNotMono> finishTask = new List<DecompressNotMono>();
    private CompressCallback callback = null;
    private bool working = false;
    /// <summary>
    /// 系统的核数
    /// </summary>
    private int processorCount = 1;
    public CompressState Status
    {
        get
        {
            if (!working)
            {
                return CompressState.None;
            }
            foreach (DecompressNotMono com in workingTask)
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
            foreach (DecompressNotMono com in finishTask)
            {
                finishSize += com.FinishSize;
            }
            foreach (DecompressNotMono com in workingTask)
            {
                finishSize += com.FinishSize;
            }
            return finishSize;
        }
    }

    private long totalSize = 0;
    public long TotalSize
    {
        get
        {
            return totalSize;
        }
    }

    public MultiDecompress()
    {
        processorCount = SystemInfo.processorCount;
        callback = null;
        working = false;
        totalSize = 0;
    }

    /// <summary>
    /// 设置回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetCallback(CompressCallback callback)
    {
        this.callback = callback;
    }

    public void AddCompressConfig(CompressConfig config)
    {
        if (config.inFileSize == 0
            && File.Exists(config.inFile))
        {
            FileStream input = new FileStream(config.inFile, FileMode.Open);
            byte[] properties = new byte[5];
            input.Read(properties, 0, 5);

            byte[] fileLengthBytes = new byte[8];
            input.Read(fileLengthBytes, 0, 8);

            config.inFileSize = BitConverter.ToInt64(fileLengthBytes, 0);
            input.Close();
            input.Dispose();

            totalSize += config.inFileSize;
        }

        if (processorCount > workingTask.Count)
        {
            DecompressNotMono com = new DecompressNotMono(config, () =>
            {
                Refresh();
            });
            if (working)
            {
                com.StartCompress();
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
                DecompressNotMono com = new DecompressNotMono(configs[0], () =>
                {
                    Refresh();
                });
                com.StartCompress();
                workingTask.Add(com);
                configs.RemoveAt(0);
            }
        }
    }

    public void StartCompress()
    {
        if (working)
        {
            return;
        }
        working = true;
        foreach (DecompressNotMono com in workingTask)
        {
            com.StartCompress();
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