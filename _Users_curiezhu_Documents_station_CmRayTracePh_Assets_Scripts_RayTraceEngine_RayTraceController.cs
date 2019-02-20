using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RayTraceManager : MonoBehaviour
{

    struct SurfaceInfo
    {
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    }
    struct RayNode
    {
        Ray ray;
        RaycastHit hit;
        SurfaceInfo surfaceInfo;
        Ray[] newRayList;
    }


    public void Awake()
    {

    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
