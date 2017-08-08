using System.IO;

public class MultiCompress : MultiCompressBase
{
    public MultiCompress()
        : base()
    {
    }

    public override void AddCompressConfig(CompressConfig config)
    {
        if (config.inFileSize == 0
            && File.Exists(config.inFile))
        {
            FileStream input = new FileStream(config.inFile, FileMode.Open);
            config.inFileSize = input.Length;
            input.Close();
            input.Dispose();
            totalSize += config.inFileSize;
        }

        if (processorCount > workingTask.Count)
        {
            CompressNotMono com = new CompressNotMono(config, () =>
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
    protected override void Refresh()
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
                CompressNotMono com = new CompressNotMono(configs[0], () =>
                {
                    Refresh();
                });
                com.Start();
                workingTask.Add(com);
                configs.RemoveAt(0);
            }
        }
    }
}