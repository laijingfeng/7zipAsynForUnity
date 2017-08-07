using UnityEditor;
using UnityEngine;
using System;
using System.Threading;

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
        Compress.CompressFile(Application.dataPath + "/../" + fileName);
        AssetDatabase.Refresh();
    }

    public string pp;

    [ContextMenu("UnDo")]
    private void UnDo()
    {
        pp = Application.dataPath + "/../" + fileName;
        Thread m_DecompressThread = null;
        m_DecompressThread = new Thread(new ThreadStart(DoDO));
        m_DecompressThread.Start();  
        
        //AssetDatabase.Refresh();
    }

    private void DoDO()
    {
        CodeProgress co = new CodeProgress(AA);
        Compress.DecompressFile(pp, null, co);
    }

    private void AA(Int64 a, Int64 b)
    {
        Debug.Log("xx " + a + " " + b);
    }
}