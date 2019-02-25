using System;
using UnityEngine;

public class ComputeController
{
    private static string TAG = "ComputeShader";

    private DebugUtil.DebugInfo mDebugInput;
    private DebugUtil.DebugInfo mDebugOutput;

    public ComputeShader mComputeShader;
    public ComputeController(ComputeShader shader)
    {
        mComputeShader = shader;
        InitDebugInfo();
    }

    public void InitDebugInfo()
    {
        mDebugInput = new DebugUtil.DebugInfo();
        mDebugOutput = new DebugUtil.DebugInfo();

        mDebugInput.tag = DebugUtil.GetASCIIFromString(TAG);
        mDebugInput.level = DebugUtil.GetASCIIFromString(DebugUtil.LEVEL_DEBUG);
        mDebugInput.info = new float[DebugUtil.INFO_LENGTH];

        mDebugOutput.tag = DebugUtil.GetASCIIFromString(TAG);
        mDebugOutput.level = DebugUtil.GetASCIIFromString(DebugUtil.LEVEL_DEBUG);
        mDebugOutput.info = new float[DebugUtil.INFO_LENGTH];
    }

    public void RunShader()
    {
        ComputeBuffer buffer = new ComputeBuffer(1,mDebugInput.info.Length + mDebugInput.level.Length+mDebugInput.level.Length);
        DebugUtil.DebugInfo[] inList =  { mDebugInput};
        DebugUtil.DebugInfo[] outList = { mDebugOutput };
        RenderTexture tex = new RenderTexture(256, 256, 24);
        tex.enableRandomWrite = true;
        tex.Create();
        int kernel = mComputeShader.FindKernel("CSMain");
        mComputeShader.SetTexture(kernel,"Result",tex);
        //mComputeShader.SetBuffer(kernel, "dataBuffer", buffer);
        mComputeShader.Dispatch(kernel, 256/8, 256/8, 1);
        //buffer.GetData(outList);
        //DebugUtil.PrintLog(outList[0]);

        buffer.Release();
    }

}

