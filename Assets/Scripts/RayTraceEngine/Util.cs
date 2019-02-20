using UnityEngine;
using System.Collections;


public class Util : MonoBehaviour
{
    public static void Log(string tag, params string[] message)
    {
        if (tag != null)
        {
            string result = "";
            if (message != null && message.Length > 0)
            {
                foreach (string tmp in message)
                {
                    result += tmp;
                }
            }
            Debug.Log("[" + tag + "][" + result + "]");
        }
    }
    public static Texture2D RenderTextureToTexture2D(RenderTexture renderTexture,int width,int height)
    {
        Texture2D texture = new Texture2D(width,height);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0,0,width,height),0,0);
        texture.Apply();
        return texture;
    }

    public static Color Vector3ToColor(Vector3 vec)
    {
        return new Color(vec.x,vec.y,vec.z);
    }
    public static Vector3 ColorToVector3(Color color)
    {
        return new Vector3(color.r,color.g,color.b);
    }
    public static Vector3 Vector3MulNumber(Vector3 vector,float num)
    {
        return new Vector3(vector.x*num,vector.y*num,vector.z*num);
    }
    public static Vector3 Vector3MulVector3(Vector3 first,Vector3 second)
    {
        return new Vector3(first.x*second.x,first.y*second.y,first.z*second.z);
    }

    /*
     * 求三角形面积
     * 
     */
    public static float ComputeTriangleSpace(Vector3 a,Vector3 b,Vector3 c)
    {

        return Vector3.Dot(a, b) / 2.0f;

    }

   
}
