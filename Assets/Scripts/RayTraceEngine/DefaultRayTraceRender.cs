using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefaultRayTraceRender : RayTraceRender
{

    private static int MAX_PATH_LENGTH = 5;

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
                    surfaceInfo.normal = hit.normal;
                }
                else
                {

                    Material material = gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[1];
                    if (material.mainTexture)
                    {
                        Texture2D texture = material.mainTexture as Texture2D;
                        surfaceInfo.emission = Util.ColorToVector3(texture.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));
                    }
                    surfaceInfo.alpha = RayUtil.SmoothnessToAlpha(material.GetFloat("_Glossiness"));
                    surfaceInfo.albedo = material.GetFloat("_Metallic");
                    Mesh mesh = gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                    Vector3[] normals = mesh.normals;
                    int[] triangles = mesh.triangles;

                    // Extract local space normals of the triangle we hit
                    Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
                    Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
                    Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];

                    // interpolate using the barycentric coordinate of the hitpoint
                    Vector3 baryCenter = hit.barycentricCoordinate;

                    // Use barycentric coordinate to interpolate normal
                    Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
                    // normalize the interpolated normal
                    interpolatedNormal = interpolatedNormal.normalized;

                    // Transform local space normals to world space
                    Transform hitTransform = hit.collider.transform;
                    interpolatedNormal = hitTransform.TransformDirection(interpolatedNormal);

                    surfaceInfo.normal = interpolatedNormal;

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
