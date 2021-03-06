﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//光线追踪渲染
abstract public class RayTraceRender 
{

    public Texture2D mScreenTexture;
    private int mScreenWidth;
    private int mScreenHeight;
    private int MAX_CAST_DISTANCE = 100;
    private Light[] mLightList;
    private List<LightHandler> mLightHandlerList;
    public static int SAMPLE_COUNT = 4;
    private Texture2D mMainTexture;
    public int mTraceCount = 0;
    public Dictionary<string, LightObject> mLightObjectList = new Dictionary<string, LightObject>();
    public Vector3 ambientColor;

    public void Init(ref Texture2D screenTexture,int width,int height,ref Light[] lightList,Texture2D mainTexture)
    {
        mScreenTexture = screenTexture;
        mScreenWidth = width;
        mScreenHeight = height;
        mLightList = lightList;
        LoadLightHandler();
        mMainTexture = mainTexture;
        ambientColor = new Vector3(0.48f, 0.48f, 0.48f);
    }

    public void Render()
    {
        OnStart();
        for(int y= mScreenHeight - 1; y >= 0; y--)
        {
            for(int x = 0; x < mScreenWidth; x++)
            {
                Vector3 color = new Vector3();
                for (int i = 0; i < SAMPLE_COUNT; i++)
                {
                    float offsetX = 0.0f + Random.value*1.0f;
                    float offsetY = 0.0f + Random.value*1.0f;
                    Vector2 position = new Vector2(x+offsetX, y+offsetY);
               
                    Ray ray = Camera.main.ScreenPointToRay(position);
                    PreTrace();
                    color += Trace(ray, position);
                   
                  
                    foreach (Light light in mLightList)
                    {

                        if (light != null && (light.type == LightType.Spot || light.type == LightType.Point))
                        {

                            LightObject lightObj;
                            if(!mLightObjectList.TryGetValue(light.name,out lightObj))
                            {
                                LightObject tmpLightObj = new LightObject();
                                lightObj = tmpLightObj;
                                mLightObjectList.Add(light.name, tmpLightObj);
                                lightObj.Init(Util.ColorToVector3(light.color),light.transform.position,1.0f);
                            }
                         
                            if(lightObj != null)
                            {
                                color += lightObj.RayMarch(ray,0f,100f);

                            }
                        }
                    }
                    PostTrace();
                }
                color /= (SAMPLE_COUNT + 0.0f);
               
                mScreenTexture.SetPixel(x, y, Util.Vector3ToColor(color));
            }
        }
        OnFinish();

    }
    private Vector3 Trace(Ray ray, Vector2 screenPosition)
    {
        RaycastHit hit;

        long startTime = Util.GetTimeStamp();
        if(Physics.Raycast(ray,out hit, MAX_CAST_DISTANCE))
        {
          
            if (hit.distance > 0.0f)
            {
                return Shade(hit, ray, screenPosition);
            }
            else
            {
                return new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
        else
        {
            return new Vector3(0.0f, 0.0f, 0.0f);
        }

    }
    private Vector3 Shade(RaycastHit hit,Ray ray,Vector2 screenPosition)
    {
       
        Vector3 result = new Vector3(0.0f, 0.0f, 0.0f);
        List<Light> lightList = new List<Light>(mLightList);
        SurfaceInfo surfaceInfo = GetSurfaceInfo(screenPosition, hit);
        hit.normal = surfaceInfo.normal;
        if(surfaceInfo.light.x > 0.0f)
        {

            float degree = Vector3.Dot(Vector3.Normalize(hit.normal), Vector3.Normalize(ray.direction * -1.0f));
            return surfaceInfo.emission * degree*(10.0f/hit.distance)*0.8f;  


        }
       
            if (!TerminalTrace(screenPosition, hit, ray))
            {
                //能反射的时候才反射
                if (surfaceInfo.albedo > 0.0f)
                {
                    mTraceCount++;
                    //一定有反射，先不考虑折射
                    Ray reflectRay = new Ray();
                    reflectRay.origin = hit.point + hit.normal * 0.0001f;
                    reflectRay.direction = Vector3.Reflect(ray.direction, hit.normal);
                    //反射方向随机采样
                    //reflectRay.direction += Random.onUnitSphere;
                    result += (Trace(reflectRay, screenPosition) * surfaceInfo.albedo);
            }

            }
            foreach (Light light in lightList)
            {
                foreach (LightHandler handler in mLightHandlerList)
                {
                    if (handler != null)
                    {
                        result += handler.OnHandle(light, surfaceInfo, hit, ray);
                    }
                }

            }
        
        result += ambientColor;

        if (surfaceInfo != null)
        {
            result = Util.Vector3MulVector3(result, surfaceInfo.emission);
        }

      

        return result;

    }

    public void LoadLightHandler()
    {
        DefaultLightHandler defaultLightHandler = new DefaultLightHandler();
        if(mLightHandlerList == null)
        {
            mLightHandlerList = new List<LightHandler>();
        }
        mLightHandlerList.Add(defaultLightHandler);
    }

    abstract public SurfaceInfo GetSurfaceInfo(Vector2 screenPosition,RaycastHit hit);
    abstract public bool TerminalTrace(Vector2 screenPosition, RaycastHit hit,Ray ray);
    abstract public void PreTrace();
    abstract public void PostTrace();
    abstract public void OnFinish();
    abstract public void OnStart();




}
