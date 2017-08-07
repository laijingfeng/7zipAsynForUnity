using UnityEditor;
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
        Compress.Inst.CompressFile(Application.dataPath + "/../" + fileName, null, (n, t) =>
        {
            Debug.LogWarning(string.Format("{0}/{1}", n, t));
        });
    }

    [ContextMenu("UnDo")]
    private void UnDo()
    {
        Compress.Inst.DecompressFileLZMA(Application.dataPath + "/../" + fileName, null, (n, t) =>
        {
            Debug.LogWarning(string.Format("{0}/{1}", n, t));
        });
    }

    private void DoDO()
    {
        
    }
}