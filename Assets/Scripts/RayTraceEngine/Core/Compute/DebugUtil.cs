using System;
using UnityEngine;

//处理图像
public class DebugUtil
{
    public static string LEVEL_INFO = "INFO";
    public static string LEVEL_DEBUG = "DEBUG";
    public static string LEVEL_ERROR = "ERROR";
    public static int INFO_LENGTH = 100;
    public struct DebugInfo
    {
        public float[] tag;
        public float[] level;
        public float[] info;
    }

    public static void PrintLog(DebugInfo info)
    {

    }

    public static string GetStringFromASCII(float[] codeList)
    {
        string result = "";
        if(codeList == null)
        {
            return result;
        }
        foreach (float val in codeList)
        {
            char c = (char)val;
            result += c;

        }
        return result;
    }
    public static float[] GetASCIIFromString(string str)
    {
        if(str == null)
        {
            return new float[] { 0 };
        }
        int length = INFO_LENGTH;
       
        float[] result = new float[length];
        for(int i = 0; i < length; i++)
        {
            if (i >= str.Length)
            {
                result[i] = 0;
            }
            else
            {
                result[i] = str[i];
            }
        }
        return result;
    }
}

