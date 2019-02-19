using UnityEngine;
using System.Collections;

abstract public class LightHandler 
{
    abstract public Vector3 OnHandle(Light light, SurfaceInfo surfaceInfo, RaycastHit hit,Ray ray);
}
