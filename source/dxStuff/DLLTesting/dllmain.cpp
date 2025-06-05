// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "../packages/Microsoft.DXSDK.D3DX.9.29.952.8/build/native/include/d3dx9.h"
#include "../packages/Microsoft.DXSDK.D3DX.9.29.952.8/build/native/include/d3dx9effect.h"
#include "../packages/Microsoft.DXSDK.D3DX.9.29.952.8/build/native/include/D3dx9tex.h"
#include <dinput.h>

struct Shader
{
    BYTE pad_0x0[0x54];
    LPD3DXEFFECT pEffect_0x54;
    BYTE pad_0x58[16];
    D3DXHANDLE hTex0_0x68;
    D3DXHANDLE hTex1_0x6c;
    D3DXHANDLE hTex2_0x70;
    D3DXHANDLE hTex3_0x74;
    D3DXHANDLE mCub_0x78;
    D3DXHANDLE dFac_0x7c;
    D3DXHANDLE vDiff_0x80;
};

typedef void (*TaskModalMessageBoxPtr)(LPCSTR lpText);

#define TMMessage ((TaskModalMessageBoxPtr)0x00640260)

// App_008da71c.d3dDevice_0x6c
#define GetDevice() (*(LPDIRECT3DDEVICE9*)(0x008DA71C + 0x6C))

// This took me a while to figure out
// First you need 'extern', then you need __declspec(dllexport)
// but that's not enough because it also needs to be in an extern "C" block
extern "C" 
{
    __declspec(dllexport) extern void __stdcall RecompileShader(Shader* pShader, char* assembly, UINT assemblyLen)
    {
        LPD3DXEFFECT newEffect;
        LPD3DXBUFFER compilationErrors;
        HRESULT hr = D3DXCreateEffect(GetDevice(), assembly, assemblyLen, NULL, NULL, D3DXSHADER_USE_LEGACY_D3DX9_31_DLL, NULL, &newEffect, &compilationErrors);

        if (compilationErrors)
            TMMessage((char*)compilationErrors->GetBufferPointer());
        else if (FAILED(hr))
            TMMessage("No errors, but the HR indicates failure");
        else
        {
            LPDIRECT3DBASETEXTURE9 tex0, tex1, tex2, tex3;
            pShader->pEffect_0x54->GetTexture(pShader->hTex0_0x68, &tex0);
            pShader->pEffect_0x54->GetTexture(pShader->hTex1_0x6c, &tex1);
            pShader->pEffect_0x54->GetTexture(pShader->hTex2_0x70, &tex2);
            pShader->pEffect_0x54->GetTexture(pShader->hTex3_0x74, &tex3);

            pShader->pEffect_0x54->Release();
            pShader->pEffect_0x54 = newEffect;

            pShader->hTex0_0x68 = newEffect->GetParameterByName(NULL, "Tex0");
            pShader->hTex1_0x6c = newEffect->GetParameterByName(NULL, "Tex1");
            pShader->hTex2_0x70 = newEffect->GetParameterByName(NULL, "Tex2");
            pShader->hTex3_0x74 = newEffect->GetParameterByName(NULL, "Tex3");
            pShader->mCub_0x78 = newEffect->GetParameterByName(NULL, "mCub");
            pShader->dFac_0x7c = newEffect->GetParameterByName(NULL, "dFac");
            pShader->vDiff_0x80 = newEffect->GetParameterByName(NULL, "vDiff");

            newEffect->SetTexture(pShader->hTex0_0x68, tex0);
            newEffect->SetTexture(pShader->hTex1_0x6c, tex1);
            newEffect->SetTexture(pShader->hTex2_0x70, tex2);
            newEffect->SetTexture(pShader->hTex3_0x74, tex3);
        }
    }
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

