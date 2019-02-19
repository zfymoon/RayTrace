using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefaultRayTraceRender : RayTraceRender
{

    private static int MAX_PATH_LENGTH = 10;

    private int mCurrentPathCount;

    private Dictionary<string, SurfaceInfo> mObjectSurfaceInfo = new Dictionary<string, SurfaceInfo>();

    public override SurfaceInfo GetSurfaceInfo(Vector2 screenPosition, RaycastHit hit)
    {
        SurfaceInfo surfaceInfo = null;
        if(hit.collider != null)
        {
            GameObject gameObject = hit.collider.gameObject;
            if(gameObject != null)
            {
                surfaceInfo = gameObject.GetComponent<SurfaceInfo>();
                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer)
                {
                    Material material = renderer.material;
                    if (material.mainTexture)
                    {
                        Texture2D texture = material.mainTexture as Texture2D;
                        surfaceInfo.emission = Util.ColorToVector3(texture.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));
                    }
                    else
                    {
                        surfaceInfo.emission = Util.ColorToVector3(material.color);
                    }

                    surfaceInfo.alpha = RayUtil.SmoothnessToAlpha(material.GetFloat("_Glossiness"));
                    surfaceInfo.albedo = material.GetFloat("_Metallic");
                  

                }
                else
                {

                    Material material = gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[1];
                    if (material.mainTexture)
                    {
                        Texture2D texture = material.mainTexture as Texture2D;
                        surfaceInfo.emission = Util.ColorToVector3(texture.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));
                    }
                   
                }


            }
        }

        return surfaceInfo;

    }

    public override void PostTrace()
    {
        mCurrentPathCount = 1;
    }

    public override void PreTrace()
    {
        mCurrentPathCount = 1;
    }

    public override bool TerminalTrace(Vector2 screenPosition, RaycastHit hit, Ray ray)
    {
        mCurrentPathCount++;
        if(mCurrentPathCount > MAX_PATH_LENGTH)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
