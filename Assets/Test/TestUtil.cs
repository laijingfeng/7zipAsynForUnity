using System.Collections.Generic;
using UnityEngine;

public class TestUtil
{
    public static List<string> TestFiles
    {
        get
        {
            return new List<string>()
            {
                "general_assets_bundle",
                "pet005_bundle",
            };
        }

    }

    public static string TestDir
    {
        get
        {
            return Application.dataPath + "/../TestFile/";
        }
    }
}