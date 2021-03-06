﻿using System;
using System.Collections.Generic;
using UnityEngine;
/*
 * 处理发光物的丁达尔效应，默认为球体光源
 */
public class LightObject
{
    public BoundingSphere boundSphere;
    public static float MAX_LIGHT_DISTANCE = 2.0f;
    //物体最靠近的颜色
    public static float MIN_DISTANCE = 1.0E-2f;

    public Vector3 mLightColor;

    public static int MAX_SAMPLE_COUNT =180;

    public float mStep = 0.0f;

    public float mInnerDistance = MIN_DISTANCE;
    public Vector3 mPosition;


    public void Init(Vector3 color,Vector3 position,float length)
    {

        mPosition = position;
        mLightColor = color;
        mStep = 0.1f;
        mInnerDistance = length;

    }

    /*
     * 光线步进的方式获取光照颜色   
     */
    public Vector3 RayMarch(Ray ray,float minDistance,float maxDistance)
    {
        float t = minDistance;
        Vector3 result = new Vector3(0.0f,0.0f,0.0f);
        int realCount = 0;
        for(int i = 0; i < MAX_SAMPLE_COUNT && t >= minDistance && t <= maxDistance; i++)
        {
            Vector3 p = ray.GetPoint(t);
            if (!InShadow(p))
            {
                result += GetSampleColor(Vector3.Distance(p, GetPosition()));
                realCount++;
            }
            t += GetStep();
           // t += UnityEngine.Random.value;

        }
        result /= (realCount + 0.0f);
        return result;
    }
    public Vector3 GetSampleColor(float distance)
    {
        return mLightColor * (mInnerDistance / (distance  + 0.0f));
    }
    public Vector3 GetPosition()
    {
        return mPosition;
    }
    public float GetStep()
    {
        return mStep;
    }
    public Vector3 GetLightColor()
    {

        return mLightColor;

    }

    public bool InShadow(Vector3 point)
    {
     
        //return Vector3.Distance(point, mPosition) > 1.0f;

        Vector3 direction = (mPosition - point).normalized;
        RaycastHit hit;

       
        if (Physics.Raycast(point ,direction ,out hit,100f))
        {

            return hit.distance >= 0.0f;

        }
        else
        {
            return false;
        }

    }

}

