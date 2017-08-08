using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
    }

    public string fileName;

    [ContextMenu("Do")]
    private void Do()
    {
        float tt = Time.realtimeSinceStartup;
        Debug.LogWarning("t=" + Time.realtimeSinceStartup);
        Compress.Inst.CompressFile(Application.dataPath + "/../" + fileName, null, (n, t) =>
        {
            Debug.LogWarning(string.Format("{0}/{1}", n, t));
        }, (finish) =>
        {
            Debug.LogWarning("time=" + Time.realtimeSinceStartup + " " + (Time.realtimeSinceStartup - tt));
        });
    }

    [ContextMenu("UnDo")]
    private void UnDo()
    {
        float tt = Time.realtimeSinceStartup;
        Debug.LogWarning("t=" + Time.realtimeSinceStartup);
        Compress.Inst.DecompressFileLZMA(Application.dataPath + "/../" + fileName, null, (n, t) =>
        {
            Debug.LogWarning(string.Format("{0}/{1}", n, t));
        }, (finish) =>
        {
            Debug.LogWarning("time=" + Time.realtimeSinceStartup + " " + (Time.realtimeSinceStartup - tt));
        });
    }

    #region MultiCompress

    private MultiCompress multiCompress = null;
    private float multiCompressStartTime = 0;

    [ContextMenu("MultiCompress")]
    private void MultiCompress()
    {
        string dir = Application.dataPath + "/../";
        List<string> files = new List<string>()
        {
            "general_assets_bundle",
            "pet005_bundle",
        };
        multiCompress = new MultiCompress();
        multiCompress.SetCallback((finish, total, status) =>
        {
            Debug.LogWarning(string.Format("{0:G}/{1:G} {2}% {3}", finish, total, total == 0 ? 0f : finish * 1.0f / total * 100f, status));
        });
        foreach (string file in files)
        {
            multiCompress.AddCompressConfig(new CompressConfig()
            {
                inFile = dir + file,
                outFile = dir + CompressUtil.GetCompressFileName(file),
            });
        }
        multiCompressStartTime = Time.realtimeSinceStartup;
        multiCompress.StartCompress();
        this.StartCoroutine("IE_UpdateMultiCompress");
    }

    private IEnumerator IE_UpdateMultiCompress()
    {
        while (multiCompress != null)
        {
            multiCompress.UpdateCallback();
            if (multiCompress.Status == CompressState.Finish)
            {
                multiCompress.UpdateCallback();
                break;
            }
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
        Debug.LogWarning("Finish " + (Time.realtimeSinceStartup - multiCompressStartTime));
    }

    #endregion MultiCompress
}