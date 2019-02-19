using UnityEngine;
using System.Collections;
//光线处理工具
public class RayUtil
{

    /*--------------------------------------局部光照模型------------------------------------
     * 单一光源，特定BRDF下的推导。需要光线方向，光照强度，视线方向
     */

    /*
     * 反射光线在视线上的投影
     * Phone模型   ks*ls*(v dot r)^n 高光系数*光照强度*(反射光线 点乘 视线 )^高光指数
     */
    public static Vector3 PhoneLight(Vector3 lightDirection, Vector3 lightColor, float specular, float lightStrength, Vector3 normal, Vector3 viewDirection, float alpha)
    {


        return lightColor * specular *  lightStrength * 
        Mathf.Pow(
            (Vector3.Dot(Vector3.Normalize(Vector3.Reflect(lightDirection, normal)), Vector3.Normalize(viewDirection))), 
            alpha);
    }

    /*
     * 漫反射，光照射到粗糙的表面的时候，均匀向四周反射，漫反射的光强与入射方向与法线的夹角余弦成正比，因此此模型不涉及视线
     * 
     * Lambert模型 kd*ld*(n dot l) 漫反射属性*入射光强度*(入射单位法向量 dot 入射点指向光源的单位向量)
     */
    public static Vector3 LambertLight(Vector3 lightDirection, Vector3 lightColor, float albedo, Vector3 normal,float ligthStrength)
    {

        return lightColor * albedo  *  Mathf.Max((Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(lightDirection)))*0.5f+0.5f,0.0f);

    }

    public static Vector3 BlinnPhong(Vector3 lightDirection, Vector3 lightColor, float specular, float lightStrength, Vector3 normal, Vector3 viewDirection, float alpha)
    {

        return lightColor * specular * lightStrength * Mathf.Pow(Mathf.Max(Vector3.Dot(Vector3.Normalize(lightDirection + viewDirection ), Vector3.Normalize(normal)),0.0f), alpha);

    }

    public static float SmoothnessToAlpha(float s)
    {
        return Mathf.Pow(1000.0f, s * s);
    }


}