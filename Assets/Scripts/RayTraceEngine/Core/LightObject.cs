using System;
using System.Collections.Generic;
using UnityEngine;
/*
 * 处理发光物的丁达尔效应
 */
public class LightObject : MonoBehaviour
{
    public BoundingSphere boundSphere;
    public static float MAX_LIGHT_DISTANCE = 2.0f;
    //物体最靠近的颜色
    public static float MIN_DISTANCE = 1.0E-3f;

    public Vector3 lightColor;

    public static int MAX_SAMPLE_COUNT = 10;
    
    public void Init(IEnumerable<Vector3> aPoints)
    {
         Vector3 xmin, xmax, ymin, ymax, zmin, zmax;
         xmin = ymin = zmin = Vector3.one * float.PositiveInfinity;
         xmax = ymax = zmax = Vector3.one * float.NegativeInfinity;
         foreach(var p in aPoints)
         {
             if(p.x < xmin.x) xmin = p;
             if(p.x > xmax.x) xmax = p;
             if(p.y < ymin.y) ymin = p;
             if(p.y > ymax.y) ymax = p;
             if(p.z < zmin.z) zmin = p;
             if(p.z > zmax.z) zmax = p;
         }
         var xSpan = (xmax - xmin).sqrMagnitude;
         var ySpan = (ymax - ymin).sqrMagnitude;
         var zSpan = (zmax - zmin).sqrMagnitude;
         var dia1 = xmin;
         var dia2 = xmax;
         var maxSpan = xSpan;
         if (ySpan > maxSpan)
         {
             maxSpan = ySpan;
             dia1 = ymin; dia2 = ymax;
         }
         if (zSpan > maxSpan)
         {
             dia1 = zmin; dia2 = zmax;
         }
         var center = (dia1 + dia2) * 0.5f;
         var sqRad = (dia2 - center).sqrMagnitude;
         var radius = Mathf.Sqrt(sqRad);
         foreach (var p in aPoints)
         {
             float d = (p - center).sqrMagnitude;
             if(d > sqRad)
             {
                 var r = Mathf.Sqrt(d);
                 radius = (radius + r) * 0.5f;
                 sqRad = radius * radius;
                 var offset = r - radius;
                 center = (radius * center + offset * p) / r;
             }
         }

        boundSphere = new BoundingSphere();
        boundSphere.position = center;
        boundSphere.radius = radius;

    }

    public void ExtendBound()
    {
        boundSphere.radius += MAX_LIGHT_DISTANCE;
    }

    public bool IntersectTest(Vector3 position,Vector3 direction)
    {

     
        return false;

    }
    /*
     * 光线步进的方式获取光照颜色   
     */   
    public Vector3 RayMarch(int maxDistance)
    {
        Vector3 ori = new Vector3();
        Vector3 direction = new Vector3();
        float t = 0.0f;
        Vector3 result = new Vector3();
        for(int i = 0; i < MAX_SAMPLE_COUNT; i++)
        {
            if (InShadow())
            {
                break;
            }
            else
            {
                Vector3 currentPosition = ori + direction * t;
                float distance = Vector3.Distance(currentPosition,boundSphere.position);
                result += GetSampleColor(distance);
            }
        }
        result /= (MAX_SAMPLE_COUNT + 0.0f);
        return result;
    }
    public bool InShadow()
    {
        return false;
    }

    public Vector3 GetSampleColor(float distance)
    {
        return lightColor * (MIN_DISTANCE / (distance * distance + 0.0f));
    }

}

