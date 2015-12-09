// Shader created with Shader Forge v1.21 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.21;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:4013,x:32719,y:32712,varname:node_4013,prsc:2|emission-436-OUT;n:type:ShaderForge.SFN_Tex2d,id:2405,x:31944,y:32786,ptovrint:False,ptlb:Zeh Color,ptin:_ZehColor,varname:node_2405,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:11ac0fa1120dce14cb33dbd357fe551c,ntxv:0,isnm:False|UVIN-4875-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:9293,x:31944,y:32593,ptovrint:False,ptlb:Zeh Mask,ptin:_ZehMask,varname:node_9293,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:d4616beade482f9468242a29c7796c5f,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2507,x:32297,y:32624,varname:node_2507,prsc:2|A-9293-RGB,B-2405-RGB;n:type:ShaderForge.SFN_Panner,id:4875,x:31734,y:32786,varname:node_4875,prsc:2,spu:-0.8,spv:0|DIST-1826-OUT;n:type:ShaderForge.SFN_Slider,id:4473,x:32060,y:33064,ptovrint:False,ptlb:Intensity,ptin:_Intensity,varname:node_4473,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3652015,max:1;n:type:ShaderForge.SFN_Add,id:9570,x:32128,y:32786,varname:node_9570,prsc:2|A-2405-RGB,B-4473-OUT;n:type:ShaderForge.SFN_Multiply,id:436,x:32420,y:32955,varname:node_436,prsc:2|A-9570-OUT,B-9293-RGB;n:type:ShaderForge.SFN_Time,id:7546,x:31330,y:32752,varname:node_7546,prsc:2;n:type:ShaderForge.SFN_Divide,id:1826,x:31547,y:32715,varname:node_1826,prsc:2|A-7546-T,B-5234-OUT;n:type:ShaderForge.SFN_Vector1,id:5234,x:31486,y:32834,varname:node_5234,prsc:2,v1:2;proporder:2405-9293-4473;pass:END;sub:END;*/

Shader "Yogscart/Zeh Shader" {
    Properties {
        _ZehColor ("Zeh Color", 2D) = "white" {}
        _ZehMask ("Zeh Mask", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0.3652015
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _ZehColor; uniform float4 _ZehColor_ST;
            uniform sampler2D _ZehMask; uniform float4 _ZehMask_ST;
            uniform float _Intensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
/////// Vectors:
////// Lighting:
////// Emissive:
                float4 node_7546 = _Time + _TimeEditor;
                float2 node_4875 = (i.uv0+(node_7546.g/2.0)*float2(-0.8,0));
                float4 _ZehColor_var = tex2D(_ZehColor,TRANSFORM_TEX(node_4875, _ZehColor));
                float4 _ZehMask_var = tex2D(_ZehMask,TRANSFORM_TEX(i.uv0, _ZehMask));
                float3 emissive = ((_ZehColor_var.rgb+_Intensity)*_ZehMask_var.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
