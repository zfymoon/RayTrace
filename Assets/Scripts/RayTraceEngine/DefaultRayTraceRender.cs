using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefaultRayTraceRender : RayTraceRender
{

    private static int MAX_PATH_LENGTH = 5;

    private int mCurrentPathCount;

    private float mAverageTime = 0.0f;
    private long mLastTimeStamp;
    private int mRenderCount = 0;
    private long mStartTime;
    private long mEndTime;

    private Dictionary<Vector3, Vector3> mNormalList = new Dictionary<Vector3, Vector3>();

    public bool GetNormal(Vector3 position,out Vector3 val)
    {
        return  mNormalList.TryGetValue(position, out val);
    }
    public void PutNormal(Vector3 position, Vector3 val)
    {
        if(mNormalList.Keys.Count > 100000)
        {
            mNormalList.Clear();
        }

        mNormalList.Add(position, val);

    }

    private Dictionary<string, SurfaceInfo> mObjectSurfaceInfo = new Dictionary<string, SurfaceInfo>();

    public override SurfaceInfo GetSurfaceInfo(Vector2 screenPosition, RaycastHit hit)
    {
        SurfaceInfo surfaceInfo = null;
        if(hit.collider != null)
        {
            GameObject gameObject = hit.collider.gameObject;
            if(gameObject != null)
            {
                surfaceInfo = new SurfaceInfo();
                surfaceInfo.Init();
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
                    MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                    MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                    if (meshFilter)
                    {
                        Mesh mesh = meshFilter.sharedMesh;
                        Vector3 n;
                        if (GetNormal(hit.point, out n))
                        {
                            surfaceInfo.normal = n;
                        }
                        else
                        {
                            GetFixedNormal(mesh, ref surfaceInfo.normal, hit.triangleIndex, hit.collider.transform, hit.barycentricCoordinate);
                            PutNormal(hit.point, surfaceInfo.normal);
                        }

                    }
                    if (gameObject.name.Contains("neonsi"))
                    {
                        surfaceInfo.light = new Vector3(1.0f, 0.5f, 0.5f);
                    }


                }
                else
                {

                    if (gameObject.name.Contains("neonsi"))
                    {
                        surfaceInfo.light = new Vector3(0.7f, 0.8f, 0.5f);
                    }
                    Texture2D texture = null;
                    Material material = null;
                    SkinnedMeshRenderer skinnedMeshRenderer = null;
                    SearchTexture(gameObject.transform, ref texture,ref material,ref skinnedMeshRenderer);
                    surfaceInfo.alpha = 0.0f;
                    surfaceInfo.albedo = 0.0f;
                    if (texture && material)
                    {
                        surfaceInfo.emission = Util.ColorToVector3(texture.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));
                        surfaceInfo.alpha = RayUtil.SmoothnessToAlpha(material.GetFloat("_Glossiness"));
                        surfaceInfo.albedo = material.GetFloat("_Metallic");

                    }
                    if (skinnedMeshRenderer)
                    {
                        Mesh mesh = skinnedMeshRenderer.sharedMesh;
                        Vector3 n;
                        if (GetNormal(hit.point, out n))
                        {
                            surfaceInfo.normal = n;
                        }
                        else
                        {

                            GetFixedNormal(mesh, ref surfaceInfo.normal, hit.triangleIndex, hit.collider.transform, hit.barycentricCoordinate);
                            PutNormal(hit.point, surfaceInfo.normal);
                        }
                      
                    }
                  


                }


            }
        }

        return surfaceInfo;

    }

    public void SearchTexture(Transform transform,ref Texture2D textureResult,ref Material materialResult,ref SkinnedMeshRenderer skinnedMeshRenderer)
    {
        if (transform)
        {
            GameObject gameObject = transform.gameObject;
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer)
            {
                Material[] materialList = renderer.materials;
                foreach(Material material in materialList) {
                    if (material)
                    {
                        Texture texture = material.mainTexture;
                        if (texture)
                        {

                            textureResult = (Texture2D)texture;
                            materialResult = material;
                            skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                            return;
                        }
                    }
                }
            }
        }

        int childCound = transform.childCount;
        for(int i = 0; i < childCound; i++)
        {
            SearchTexture(transform.GetChild(i),ref textureResult,ref materialResult,ref skinnedMeshRenderer);
            if (textureResult && materialResult)
            {
                return;
            }
        }
    }

    public override void PostTrace()
    {
        mCurrentPathCount = 1;
        long currentTimeStamp = Util.GetTimeStamp();
        if(Mathf.Abs(mAverageTime - 0.0f) < 0.0001f)
        {
            mAverageTime = currentTimeStamp - mLastTimeStamp;

        }
        float total = mAverageTime * (mRenderCount-1);
        total += (currentTimeStamp - mLastTimeStamp);
        mAverageTime = total / (mRenderCount + 0.0f);
       
    }

    public override void PreTrace()
    {
        mLastTimeStamp = Util.GetTimeStamp();
        mCurrentPathCount = 1;
        mRenderCount++;
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

    public override void OnFinish()
    {
        mEndTime = Util.GetTimeStamp();
        Debug.Log("Total render count : "+mRenderCount+" Total Trace count :"+mTraceCount +" cost "+(mEndTime - mStartTime));
       
    }

    public void GetFixedNormal(Mesh mesh,ref Vector3 normal,int hIndex,Transform transform,Vector3 barycentricCoordinate)
    {
       
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;

        int trianglesLength = triangles.Length;
        int normalLength = normals.Length;

        int tIndex0 = hIndex * 3 + 0;
        int tIndex1 = hIndex * 3 + 1;
        int tIndex2 = hIndex * 3 + 2;


        if (tIndex0 >= 0 && tIndex0 < trianglesLength && tIndex1 < trianglesLength && tIndex2 < trianglesLength)
        {
            int vIndex0 = triangles[tIndex0];
            int vIndex1 = triangles[tIndex1];
            int vIndex2 = triangles[tIndex2];
            // Extract local space normals of the triangle we hit

            if (vIndex0 < normalLength && vIndex1 < normalLength && vIndex2 < normalLength)
            {
                Vector3 n0 = normals[vIndex0];
                Vector3 n1 = normals[vIndex1];
                Vector3 n2 = normals[vIndex2];

                // interpolate using the barycentric coordinate of the hitpoint
                Vector3 baryCenter = barycentricCoordinate;

                // Use barycentric coordinate to interpolate normal
                Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
                // normalize the interpolated normal
                interpolatedNormal = interpolatedNormal.normalized;

                // Transform local space normals to world space
                Transform hitTransform = transform;
                interpolatedNormal = hitTransform.TransformDirection(interpolatedNormal);

                normal = interpolatedNormal;
            }
           
        }
       
    }

    public override void OnStart()
    {

        mStartTime = Util.GetTimeStamp();

    }
}
