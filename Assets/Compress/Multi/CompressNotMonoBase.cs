using System;
using System.IO;
using System.Threading;
using UnityEngine;

public abstract class CompressNotMonoBase
{
    protected CompressConfig config;
    protected Thread thread = null;
    protected Action finishCallback = null;

    protected CompressState status = CompressState.None;
    public CompressState Status
    {
        get
        {
            return status;
        }
    }

    public virtual long FinishSize
    {
        get
        {
            return 0;
        }
    }

    public virtual long TotalSize
    {
        get
        {
            return 0;
        }
    }

    public CompressNotMonoBase()
    {
    }

    public virtual void SetConfig(CompressConfig config, Action callback = null)
    {
        this.config = config;
        this.finishCallback = callback;
    }

    public void Start()
    {
        status = CompressState.Working;
        thread = new Thread(new ThreadStart(Work));
        thread.IsBackground = true;
        thread.Start();
    }

    protected FileStream input = null;
    protected FileStream output = null;
    private void Work()
    {
        try
        {
            if (!File.Exists(this.config.inFile))
            {
                status = CompressState.Error;
                if (finishCallback != null)
                {
                    finishCallback();
                }
                return;
            }
            input = new FileStream(this.config.inFile, FileMode.Open);
            output = new FileStream(this.config.outFile, FileMode.OpenOrCreate);

            DoWork();

            output.Flush();
            output.Close();
            output.Dispose();
            input.Close();
            input.Dispose();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        if (input != null)
        {
            input.Close();
            input.Dispose();
        }

        if (output != null)
        {
            output.Close();
            output.Dispose();
        }

        status = CompressState.Finish;
        if (finishCallback != null)
        {
            finishCallback();
        }
    }

    protected abstract void DoWork();
}