using UnityEngine;
using System.Collections;

public class SurfaceInfo
{
    public float albedo;
    public float specular;
 
    public Vector3 emission;
    public float alpha;

    public Vector3 normal;

    public void Init()
    {
        Color color = Random.ColorHSV();
            float chance = Random.value;
            if (chance< 0.8f)
            {
                bool metal = chance < 0.4f;
                this.albedo = metal? 0.0f : color.r;
                this.specular = metal? 0.0f : 0.04f;
               
            
            }
            else
            {
                Color emission = Random.ColorHSV(0, 1, 0, 1, 3.0f, 8.0f);
                this.emission = new Vector3(emission.r, emission.g, emission.b);
            }
            this.albedo = 0.1f;
            this.specular = 0.7f;


            this.alpha = 8.0f;

            this.emission = new Vector3(1.0f, 0.8f, 0.8f);


    }


}
