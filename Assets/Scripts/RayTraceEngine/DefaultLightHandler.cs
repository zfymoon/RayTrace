using UnityEngine;
using System.Collections;

public class DefaultLightHandler : LightHandler
{
    public override Vector3 OnHandle(Light light, SurfaceInfo surfaceInfo,RaycastHit hit,Ray ray)
    {
        if(surfaceInfo == null)
        {
            return new Vector3(0.0f,0.0f,0.0f);
        }
            
        Vector3 lightResult = new Vector3(0.0f,0.0f,0.0f);
        Vector3 ligthDirection = light.transform.forward;
      
      
        Vector3 normal = hit.normal;
        Vector3 viewDirection = ray.direction * -1.0f ;
        Vector3 lightNormal = new Vector3(0.0f, 0.0f, 0.0f);
        //处理环境光

        //计算计算点是不是在阴影里面

        switch (light.type)
        {
            case LightType.Directional:
                ligthDirection = Vector3.Normalize(-light.transform.forward);
                if (Vector3.Dot(ligthDirection, hit.normal) > 0)
                {
                    //在圆内随机采样

                    Vector3 shadowRayDirection = ligthDirection + Random.onUnitSphere * 0.009f;
                    //Vector3 shadowRayDirection = ligthDirection;
                    if (Physics.Raycast(hit.point, shadowRayDirection, 500))
                    {
                        return new Vector3(0.0f, 0.0f, 0.0f);
                    }
                    //计算高光反射
                    //lightResult += RayUtil.PhoneLight(ligthDirection, Util.ColorToVector3(light.color), surfaceInfo.specular, light.intensity, normal, viewDirection, surfaceInfo.alpha);
                    lightResult += RayUtil.BlinnPhong(ligthDirection, Util.ColorToVector3(light.color), surfaceInfo.specular, light.intensity, normal, viewDirection, surfaceInfo.alpha);

                    //计算漫反射
                    lightResult += RayUtil.LambertLight( light.transform.position - hit.point, Util.ColorToVector3(light.color), surfaceInfo.albedo, normal, light.intensity);

                }
                break;
            case LightType.Point:
                ligthDirection = Vector3.Normalize(hit.point - light.transform.position);
                break;
            case LightType.Spot:
                //聚光灯

                break;
        }
                
        return lightResult;
    }
}
