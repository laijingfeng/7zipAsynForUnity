using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MultiDecompress : MultiCompressBase
{
    public MultiDecompress()
        :base()
    {
    }

    public override void AddCompressConfig(CompressConfig config)
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
                DecompressNotMono com = new DecompressNotMono(configs[0], () =>
                {
                    Refresh();
                });
                com.Start();
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