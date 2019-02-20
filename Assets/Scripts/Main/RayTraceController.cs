using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controller

public class RayTraceController : MonoBehaviour
{

    public int MAX_RAY_CAST_DISTANCE = 10000;

    public bool mEnableRealTime = false;
    public Light mLight;
    public uint mSampleCount = 2;

    private Texture2D mTexture;
    private Texture2D mScreenTexture;

    private Material mPostMaterial;
    private Light[] mLightList;

    private float mCurrentSample = 1;

    private RayTraceRender mRayTraceRender;

    private bool mIsRendered = false;



    private int mScreenWidth;
    private int mScreenHeight;

    struct SurfaceInfo
    {
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    }

    public void Awake()
    {
        Color white = new Color(1.0f, 1.0f, 1.0f);
        RenderSettings.ambientSkyColor = white;
        RenderSettings.ambientGroundColor = white;
        RenderSettings.ambientEquatorColor = white;
        RenderSettings.ambientLight = white;
        RenderSettings.ambientIntensity = 1.0f;

        mScreenWidth = Screen.width;
        mScreenHeight = Screen.height;
        mTexture = new Texture2D(mScreenWidth, mScreenHeight);
        mLightList = FindObjectsOfType(typeof(Light)) as Light[];

        int lightLength = mLightList.Length;
        for (int i = 0; i < lightLength; i++)
        {
            mLightList[i].enabled = false;
        }

        mRayTraceRender = new DefaultRayTraceRender();

    }

    public void Start()
    {
        if (mLight != null)
        {
            mLight.enabled = false;
        }
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mPostMaterial == null)
        {
            mPostMaterial = new Material(Shader.Find("Custom/PostHandle"));

        }
       

        

        if (!mIsRendered)
        {
            mPostMaterial.SetFloat("_Sample", mCurrentSample);
            mScreenTexture = Util.RenderTextureToTexture2D(source, mScreenWidth, mScreenHeight);
            mRayTraceRender.Init(ref mTexture, mScreenWidth, mScreenHeight, ref mLightList,mScreenTexture);
            mRayTraceRender.Render();
            mTexture.Apply();
            mIsRendered = true;
            // Render();
        }


        Graphics.Blit(mTexture, destination,mPostMaterial);
        mCurrentSample++;

    }

    private void Render()
    {
        if (mTexture != null)
        {
            Vector2 screenPosition = new Vector2(0, 0);
            for (int y = mScreenHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < mScreenWidth; x++)
                {
                    //每个点采样多次
                    Color color = new Color(0f, 0f, 0f);
                    for (int i = 0; i < 1; i++)
                    {
                        screenPosition.x = x + Random.value;
                        screenPosition.y = y + Random.value;
                        RaycastHit hit;
                        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
                        color += RayTracing(ray, x, y, out hit, 1);

                    }
                    color /= (mSampleCount + 0.0f);
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b));
                    mTexture.SetPixel(x, y, color);


                }
            }
            mTexture.Apply();
        }

    }

    private float SmoothnessToPhongAlpha(float s)
    {

        return Mathf.Pow(1000.0f, s * s);

    }

    private Vector3 SampleHemisphere(Vector3 normal, float alpha)
    {

        float cosTheta = Mathf.Pow(Random.value, 1.0f / (alpha + 1.0f));
        float sinTheta = Mathf.Sqrt(1.0f - cosTheta * cosTheta);
        float phi = 2 * Mathf.PI * Random.value;
        Vector3 tangentSpaceDir = new Vector3(Mathf.Cos(phi) * sinTheta, Mathf.Sin(phi) * sinTheta, cosTheta);
        return normal + tangentSpaceDir;

    }


    private Color RayTracing(Ray ray, int x, int y, out RaycastHit hit, int depth)
    {

        if (Physics.Raycast(ray, out hit, MAX_RAY_CAST_DISTANCE))
        {
            Ray newRay;
            SurfaceInfo surfaceInfo = getSurfaceInfo(hit, x, y);


            if (depth < 100 && HandleLight(ray, hit, out newRay, ref surfaceInfo))
            {
                Vector3 r = surfaceInfo.emission = MulColor(surfaceInfo.emission, RayTracing(newRay, x, y, out hit, depth + 1));
                return new Color(r.x, r.y, r.z);
            }
            else
            {
                return new Color(0, 0, 0);
            }

        }

        hit = new RaycastHit();

        return mScreenTexture.GetPixel(x, y);


        //return new Color(1, 1, 1);



    }
    float sdot(Vector3 x, Vector3 y, float f = 1.0f)
    {
        float val = Vector3.Dot(x, y) * f;
        return val < 0 ? 0 : (val > 1 ? 1 : val);
    }
    float energy(Vector3 vec)
    {
        return Vector3.Dot(vec, new Vector3(1 / 3.0f, 1 / 3.0f, 1 / 3.0f));
    }
    public Vector3 Min(Vector3 first, Vector3 second)
    {
        return (first.x < second.x || first.y < second.y || first.z < second.z) ? first : second;
    }
    public Vector3 Mul(Vector3 first, Vector3 second)
    {
        first.x *= second.x;
        first.y *= second.y;
        first.z *= second.z;
        return first;

    }
    public Vector3 MulColor(Vector3 first, Color second)
    {
        first.x *= second.r;
        first.y *= second.g;
        first.z *= second.b;
        return first;

    }
    private bool HandleLight(Ray ray, RaycastHit hit, out Ray newRay, ref SurfaceInfo surfaceInfo)
    {


        surfaceInfo.albedo = Min(new Vector3(1.0f, 1.0f, 1.0f) - surfaceInfo.specular, surfaceInfo.albedo);
        float specChance = energy(surfaceInfo.specular);
        float diffChance = energy(surfaceInfo.albedo);
        float roulette = Random.value;
        newRay = new Ray();
        if (roulette < specChance)
        {
            newRay.origin = hit.point + hit.normal * 0.001f;
            float alpha = SmoothnessToPhongAlpha(surfaceInfo.smoothness);
            newRay.direction = SampleHemisphere(Vector3.Reflect(ray.direction, hit.normal), alpha);
            float f = (alpha + 2) / (alpha + 1);
            surfaceInfo.emission = Mul(surfaceInfo.emission, (1.0f / specChance) * surfaceInfo.specular * sdot(hit.normal, newRay.direction, f));
        }
        else if (diffChance > 0 && roulette < specChance + diffChance)
        {
            newRay.origin = hit.point + hit.normal * 0.001f;
            newRay.direction = SampleHemisphere(hit.normal, 1.0f);
            surfaceInfo.emission = Mul(surfaceInfo.emission, (1.0f / diffChance) * surfaceInfo.albedo);
        }
        else
        {
            surfaceInfo.emission = new Vector3(0, 0, 0);
        }

        return Vector3.Dot(newRay.direction, hit.normal) > 0;

    }

    private Color getLightColor()
    {

        Color light = RenderSettings.ambientLight;
        Color result = new Color(0, 0, 0);
        result += result;
        return new Color();
    }


    private SurfaceInfo getSurfaceInfo(RaycastHit hit, int x, int y)
    {
        SurfaceInfo surfaceInfo = new SurfaceInfo();

        MeshFilter filter = hit.transform.GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;

        int limit = hit.triangleIndex * 3;
        int submesh;
        for (submesh = 0; submesh < mesh.subMeshCount; submesh++)
        {
            int numIndices = mesh.GetTriangles(submesh).Length;
            if (numIndices > limit)
                break;

            limit -= numIndices;
        }



        if (hit.transform.GetComponent<Renderer>() != null)
        {

            Material material = hit.transform.GetComponent<Renderer>().materials[submesh];

            float metallic = material.GetFloat("_Metallic");


            if (material.mainTexture != null)
            {
                Texture2D texture = material.mainTexture as Texture2D;
                surfaceInfo.emission = Util.ColorToVector3(texture.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));
            }
            else
            {
                surfaceInfo.emission = Util.ColorToVector3(material.color);
            }

        }
        else
        {
            surfaceInfo.emission = Util.ColorToVector3(mScreenTexture.GetPixel(x, y));
        }

        GameObject gameObject = hit.collider.gameObject;
       

        return surfaceInfo;

    }
}
