// Shader created with Shader Forge v1.21 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.21;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1098,x:33556,y:32686,varname:node_1098,prsc:2|diff-5710-OUT,spec-5456-OUT,gloss-2904-OUT,normal-9421-OUT,amdfl-7849-OUT,amspl-2245-OUT;n:type:ShaderForge.SFN_Lerp,id:5710,x:33690,y:32135,varname:node_5710,prsc:2|A-6729-RGB,B-9349-RGB,T-2984-OUT;n:type:ShaderForge.SFN_Tex2d,id:6729,x:32051,y:31975,ptovrint:False,ptlb:Albedo 1,ptin:_Albedo1,varname:node_6729,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:1abbdc4bf23e178488d25d076ee950ab,ntxv:2,isnm:False|UVIN-9919-OUT;n:type:ShaderForge.SFN_Tex2d,id:9349,x:32051,y:32158,ptovrint:False,ptlb:Albedo 2,ptin:_Albedo2,varname:node_9349,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:9b8099742b7ee8b4fa601cd8484f73c0,ntxv:0,isnm:False|UVIN-9919-OUT;n:type:ShaderForge.SFN_Step,id:6755,x:33044,y:32265,varname:node_6755,prsc:2|A-3244-R,B-8500-RGB;n:type:ShaderForge.SFN_Tex2d,id:9335,x:32051,y:32557,ptovrint:False,ptlb:Normal 1,ptin:_Normal1,varname:_ColorMap3,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c0aad5555e442464a84e6d8e67b1c025,ntxv:3,isnm:True|UVIN-9919-OUT;n:type:ShaderForge.SFN_Tex2d,id:6903,x:32101,y:33032,ptovrint:False,ptlb:Normal 2,ptin:_Normal2,varname:_NormalMap2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c0aad5555e442464a84e6d8e67b1c025,ntxv:3,isnm:True|UVIN-9919-OUT;n:type:ShaderForge.SFN_Step,id:2549,x:33060,y:32881,varname:node_2549,prsc:2|A-3244-R,B-8500-RGB;n:type:ShaderForge.SFN_Lerp,id:9421,x:33267,y:32767,varname:node_9421,prsc:2|A-8860-OUT,B-9663-OUT,T-2549-OUT;n:type:ShaderForge.SFN_Tex2d,id:8500,x:32051,y:32356,ptovrint:False,ptlb:Height Map,ptin:_HeightMap,varname:node_8500,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c8a72295548df094ebd1e5a181d0062d,ntxv:0,isnm:False|UVIN-7086-OUT;n:type:ShaderForge.SFN_TexCoord,id:5961,x:31408,y:31883,varname:node_5961,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:6256,x:31129,y:31990,ptovrint:False,ptlb:Main Tiling Factor,ptin:_MainTilingFactor,varname:node_6256,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:5;n:type:ShaderForge.SFN_Multiply,id:9919,x:31622,y:32112,varname:node_9919,prsc:2|A-5961-UVOUT,B-6256-OUT;n:type:ShaderForge.SFN_Tex2d,id:3244,x:32830,y:32405,ptovrint:False,ptlb:Splat Map,ptin:_SplatMap,varname:node_3244,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:f3cf4ccc50148bb4f9d8b991280d0014,ntxv:0,isnm:False|UVIN-2205-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:2205,x:32655,y:32405,varname:node_2205,prsc:2,uv:0;n:type:ShaderForge.SFN_TexCoord,id:4828,x:31478,y:32302,varname:node_4828,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:8232,x:31199,y:32409,ptovrint:False,ptlb:Height Tile Factor,ptin:_HeightTileFactor,varname:_TilingFactor_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:5;n:type:ShaderForge.SFN_Multiply,id:7086,x:31694,y:32531,varname:node_7086,prsc:2|A-4828-UVOUT,B-8232-OUT;n:type:ShaderForge.SFN_Tex2d,id:3154,x:32782,y:33193,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:node_3154,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e958c6041cfe445e987c73751e8d4082,ntxv:0,isnm:False|UVIN-1371-OUT;n:type:ShaderForge.SFN_TexCoord,id:2704,x:31919,y:33419,varname:node_2704,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:1371,x:32152,y:33490,varname:node_1371,prsc:2|A-2704-UVOUT,B-1849-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1849,x:31919,y:33586,ptovrint:False,ptlb:Metallic Tile Factor,ptin:_MetallicTileFactor,varname:node_1849,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:50;n:type:ShaderForge.SFN_Slider,id:4715,x:32671,y:33373,ptovrint:False,ptlb:Metallic Strength,ptin:_MetallicStrength,varname:node_4715,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.4414188,max:1;n:type:ShaderForge.SFN_Multiply,id:5456,x:33034,y:33244,varname:node_5456,prsc:2|A-3154-G,B-4715-OUT;n:type:ShaderForge.SFN_Tex2d,id:3978,x:32051,y:32747,ptovrint:False,ptlb:Detail 1,ptin:_Detail1,varname:node_3978,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e08c295755c0885479ad19f518286ff2,ntxv:3,isnm:True|UVIN-2096-OUT;n:type:ShaderForge.SFN_Tex2d,id:9035,x:32101,y:33214,ptovrint:False,ptlb:Detail 2,ptin:_Detail2,varname:node_9035,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e08c295755c0885479ad19f518286ff2,ntxv:3,isnm:True|UVIN-8400-OUT;n:type:ShaderForge.SFN_TexCoord,id:101,x:31618,y:32774,varname:node_101,prsc:2,uv:0;n:type:ShaderForge.SFN_TexCoord,id:824,x:31486,y:33117,varname:node_824,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:8400,x:31858,y:33162,varname:node_8400,prsc:2|A-824-UVOUT,B-3025-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3025,x:31486,y:33288,ptovrint:False,ptlb:Detail 2 Scale,ptin:_Detail2Scale,varname:node_3025,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:100;n:type:ShaderForge.SFN_ValueProperty,id:1740,x:31603,y:32945,ptovrint:False,ptlb:Detail 1 Scale,ptin:_Detail1Scale,varname:node_1740,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:100;n:type:ShaderForge.SFN_Multiply,id:2096,x:31809,y:32811,varname:node_2096,prsc:2|A-101-UVOUT,B-1740-OUT;n:type:ShaderForge.SFN_NormalBlend,id:8860,x:32328,y:32623,varname:node_8860,prsc:2|BSE-9335-RGB,DTL-3978-RGB;n:type:ShaderForge.SFN_NormalBlend,id:9663,x:32384,y:33154,varname:node_9663,prsc:2|BSE-6903-RGB,DTL-9035-RGB;n:type:ShaderForge.SFN_Slider,id:2904,x:33235,y:33459,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:node_2904,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3504274,max:1;n:type:ShaderForge.SFN_Subtract,id:348,x:33044,y:32405,varname:node_348,prsc:2|A-3244-R,B-6755-OUT;n:type:ShaderForge.SFN_Vector1,id:9000,x:33173,y:32419,varname:node_9000,prsc:2,v1:1;n:type:ShaderForge.SFN_Subtract,id:9897,x:33346,y:32419,varname:node_9897,prsc:2|A-9000-OUT,B-348-OUT;n:type:ShaderForge.SFN_Clamp01,id:2984,x:33346,y:32280,varname:node_2984,prsc:2|IN-9897-OUT;n:type:ShaderForge.SFN_Cubemap,id:7185,x:33431,y:33602,ptovrint:False,ptlb:Skybox,ptin:_Skybox,varname:node_7185,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,cube:2193c36bfedeb12449e6a7f103a67f2f,pvfc:0;n:type:ShaderForge.SFN_Multiply,id:7849,x:33591,y:33602,varname:node_7849,prsc:2|A-7185-RGB,B-8983-OUT;n:type:ShaderForge.SFN_Slider,id:8983,x:33274,y:33771,ptovrint:False,ptlb:Skybox Intensity,ptin:_SkyboxIntensity,varname:node_8983,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.6561018,max:1;n:type:ShaderForge.SFN_Multiply,id:2245,x:33787,y:33602,varname:node_2245,prsc:2|A-7849-OUT,B-4468-OUT;n:type:ShaderForge.SFN_Vector1,id:4468,x:33787,y:33734,varname:node_4468,prsc:2,v1:0.3;proporder:6729-9349-9335-6903-3154-4715-1849-8500-3244-6256-8232-3978-1740-9035-3025-2904-7185-8983;pass:END;sub:END;*/

Shader "Yogscart/GenericSplatShaderWorkAround" {
    Properties {
        _Albedo1 ("Albedo 1", 2D) = "black" {}
        _Albedo2 ("Albedo 2", 2D) = "white" {}
        _Normal1 ("Normal 1", 2D) = "bump" {}
        _Normal2 ("Normal 2", 2D) = "bump" {}
        _Metallic ("Metallic", 2D) = "white" {}
        _MetallicStrength ("Metallic Strength", Range(0, 1)) = 0.4414188
        _MetallicTileFactor ("Metallic Tile Factor", Float ) = 50
        _HeightMap ("Height Map", 2D) = "white" {}
        _SplatMap ("Splat Map", 2D) = "white" {}
        _MainTilingFactor ("Main Tiling Factor", Float ) = 5
        _HeightTileFactor ("Height Tile Factor", Float ) = 5
        _Detail1 ("Detail 1", 2D) = "bump" {}
        _Detail1Scale ("Detail 1 Scale", Float ) = 100
        _Detail2 ("Detail 2", 2D) = "bump" {}
        _Detail2Scale ("Detail 2 Scale", Float ) = 100
        _Gloss ("Gloss", Range(0, 1)) = 0.3504274
        _Skybox ("Skybox", Cube) = "_Skybox" {}
        _SkyboxIntensity ("Skybox Intensity", Range(0, 1)) = 0.6561018
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
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Albedo1; uniform float4 _Albedo1_ST;
            uniform sampler2D _Albedo2; uniform float4 _Albedo2_ST;
            uniform sampler2D _Normal1; uniform float4 _Normal1_ST;
            uniform sampler2D _Normal2; uniform float4 _Normal2_ST;
            uniform sampler2D _HeightMap; uniform float4 _HeightMap_ST;
            uniform float _MainTilingFactor;
            uniform sampler2D _SplatMap; uniform float4 _SplatMap_ST;
            uniform float _HeightTileFactor;
            uniform sampler2D _Metallic; uniform float4 _Metallic_ST;
            uniform float _MetallicTileFactor;
            uniform float _MetallicStrength;
            uniform sampler2D _Detail1; uniform float4 _Detail1_ST;
            uniform sampler2D _Detail2; uniform float4 _Detail2_ST;
            uniform float _Detail2Scale;
            uniform float _Detail1Scale;
            uniform float _Gloss;
            uniform samplerCUBE _Skybox;
            uniform float _SkyboxIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float2 node_9919 = (i.uv0*_MainTilingFactor);
                float3 _Normal1_var = UnpackNormal(tex2D(_Normal1,TRANSFORM_TEX(node_9919, _Normal1)));
                float2 node_2096 = (i.uv0*_Detail1Scale);
                float3 _Detail1_var = UnpackNormal(tex2D(_Detail1,TRANSFORM_TEX(node_2096, _Detail1)));
                float3 node_8860_nrm_base = _Normal1_var.rgb + float3(0,0,1);
                float3 node_8860_nrm_detail = _Detail1_var.rgb * float3(-1,-1,1);
                float3 node_8860_nrm_combined = node_8860_nrm_base*dot(node_8860_nrm_base, node_8860_nrm_detail)/node_8860_nrm_base.z - node_8860_nrm_detail;
                float3 node_8860 = node_8860_nrm_combined;
                float3 _Normal2_var = UnpackNormal(tex2D(_Normal2,TRANSFORM_TEX(node_9919, _Normal2)));
                float2 node_8400 = (i.uv0*_Detail2Scale);
                float3 _Detail2_var = UnpackNormal(tex2D(_Detail2,TRANSFORM_TEX(node_8400, _Detail2)));
                float3 node_9663_nrm_base = _Normal2_var.rgb + float3(0,0,1);
                float3 node_9663_nrm_detail = _Detail2_var.rgb * float3(-1,-1,1);
                float3 node_9663_nrm_combined = node_9663_nrm_base*dot(node_9663_nrm_base, node_9663_nrm_detail)/node_9663_nrm_base.z - node_9663_nrm_detail;
                float3 node_9663 = node_9663_nrm_combined;
                float4 _SplatMap_var = tex2D(_SplatMap,TRANSFORM_TEX(i.uv0, _SplatMap));
                float2 node_7086 = (i.uv0*_HeightTileFactor);
                float4 _HeightMap_var = tex2D(_HeightMap,TRANSFORM_TEX(node_7086, _HeightMap));
                float3 normalLocal = lerp(node_8860,node_9663,step(_SplatMap_var.r,_HeightMap_var.rgb));
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float specPow = exp2( gloss * 10.0+1.0);
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                d.boxMax[0] = unity_SpecCube0_BoxMax;
                d.boxMin[0] = unity_SpecCube0_BoxMin;
                d.probePosition[0] = unity_SpecCube0_ProbePosition;
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.boxMax[1] = unity_SpecCube1_BoxMax;
                d.boxMin[1] = unity_SpecCube1_BoxMin;
                d.probePosition[1] = unity_SpecCube1_ProbePosition;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float3 node_7849 = (texCUBE(_Skybox,viewReflectDirection).rgb*_SkyboxIntensity);
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float4 _Albedo1_var = tex2D(_Albedo1,TRANSFORM_TEX(node_9919, _Albedo1));
                float4 _Albedo2_var = tex2D(_Albedo2,TRANSFORM_TEX(node_9919, _Albedo2));
                float3 diffuseColor = lerp(_Albedo1_var.rgb,_Albedo2_var.rgb,saturate((1.0-(_SplatMap_var.r-step(_SplatMap_var.r,_HeightMap_var.rgb))))); // Need this for specular when using metallic
                float specularMonochrome;
                float3 specularColor;
                float2 node_1371 = (i.uv0*_MetallicTileFactor);
                float4 _Metallic_var = tex2D(_Metallic,TRANSFORM_TEX(node_1371, _Metallic));
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, (_Metallic_var.g*_MetallicStrength), specularColor, specularMonochrome );
                specularMonochrome = 1-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
                float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
                float3 directSpecular = 1 * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular + (node_7849*0.3));
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += node_7849; // Diffuse Ambient Light
                indirectDiffuse += gi.indirect.diffuse;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Albedo1; uniform float4 _Albedo1_ST;
            uniform sampler2D _Albedo2; uniform float4 _Albedo2_ST;
            uniform sampler2D _Normal1; uniform float4 _Normal1_ST;
            uniform sampler2D _Normal2; uniform float4 _Normal2_ST;
            uniform sampler2D _HeightMap; uniform float4 _HeightMap_ST;
            uniform float _MainTilingFactor;
            uniform sampler2D _SplatMap; uniform float4 _SplatMap_ST;
            uniform float _HeightTileFactor;
            uniform sampler2D _Metallic; uniform float4 _Metallic_ST;
            uniform float _MetallicTileFactor;
            uniform float _MetallicStrength;
            uniform sampler2D _Detail1; uniform float4 _Detail1_ST;
            uniform sampler2D _Detail2; uniform float4 _Detail2_ST;
            uniform float _Detail2Scale;
            uniform float _Detail1Scale;
            uniform float _Gloss;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float2 node_9919 = (i.uv0*_MainTilingFactor);
                float3 _Normal1_var = UnpackNormal(tex2D(_Normal1,TRANSFORM_TEX(node_9919, _Normal1)));
                float2 node_2096 = (i.uv0*_Detail1Scale);
                float3 _Detail1_var = UnpackNormal(tex2D(_Detail1,TRANSFORM_TEX(node_2096, _Detail1)));
                float3 node_8860_nrm_base = _Normal1_var.rgb + float3(0,0,1);
                float3 node_8860_nrm_detail = _Detail1_var.rgb * float3(-1,-1,1);
                float3 node_8860_nrm_combined = node_8860_nrm_base*dot(node_8860_nrm_base, node_8860_nrm_detail)/node_8860_nrm_base.z - node_8860_nrm_detail;
                float3 node_8860 = node_8860_nrm_combined;
                float3 _Normal2_var = UnpackNormal(tex2D(_Normal2,TRANSFORM_TEX(node_9919, _Normal2)));
                float2 node_8400 = (i.uv0*_Detail2Scale);
                float3 _Detail2_var = UnpackNormal(tex2D(_Detail2,TRANSFORM_TEX(node_8400, _Detail2)));
                float3 node_9663_nrm_base = _Normal2_var.rgb + float3(0,0,1);
                float3 node_9663_nrm_detail = _Detail2_var.rgb * float3(-1,-1,1);
                float3 node_9663_nrm_combined = node_9663_nrm_base*dot(node_9663_nrm_base, node_9663_nrm_detail)/node_9663_nrm_base.z - node_9663_nrm_detail;
                float3 node_9663 = node_9663_nrm_combined;
                float4 _SplatMap_var = tex2D(_SplatMap,TRANSFORM_TEX(i.uv0, _SplatMap));
                float2 node_7086 = (i.uv0*_HeightTileFactor);
                float4 _HeightMap_var = tex2D(_HeightMap,TRANSFORM_TEX(node_7086, _HeightMap));
                float3 normalLocal = lerp(node_8860,node_9663,step(_SplatMap_var.r,_HeightMap_var.rgb));
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float4 _Albedo1_var = tex2D(_Albedo1,TRANSFORM_TEX(node_9919, _Albedo1));
                float4 _Albedo2_var = tex2D(_Albedo2,TRANSFORM_TEX(node_9919, _Albedo2));
                float3 diffuseColor = lerp(_Albedo1_var.rgb,_Albedo2_var.rgb,saturate((1.0-(_SplatMap_var.r-step(_SplatMap_var.r,_HeightMap_var.rgb))))); // Need this for specular when using metallic
                float specularMonochrome;
                float3 specularColor;
                float2 node_1371 = (i.uv0*_MetallicTileFactor);
                float4 _Metallic_var = tex2D(_Metallic,TRANSFORM_TEX(node_1371, _Metallic));
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, (_Metallic_var.g*_MetallicStrength), specularColor, specularMonochrome );
                specularMonochrome = 1-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
                float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Albedo1; uniform float4 _Albedo1_ST;
            uniform sampler2D _Albedo2; uniform float4 _Albedo2_ST;
            uniform sampler2D _HeightMap; uniform float4 _HeightMap_ST;
            uniform float _MainTilingFactor;
            uniform sampler2D _SplatMap; uniform float4 _SplatMap_ST;
            uniform float _HeightTileFactor;
            uniform sampler2D _Metallic; uniform float4 _Metallic_ST;
            uniform float _MetallicTileFactor;
            uniform float _MetallicStrength;
            uniform float _Gloss;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.posWorld = mul(_Object2World, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
                float2 node_9919 = (i.uv0*_MainTilingFactor);
                float4 _Albedo1_var = tex2D(_Albedo1,TRANSFORM_TEX(node_9919, _Albedo1));
                float4 _Albedo2_var = tex2D(_Albedo2,TRANSFORM_TEX(node_9919, _Albedo2));
                float4 _SplatMap_var = tex2D(_SplatMap,TRANSFORM_TEX(i.uv0, _SplatMap));
                float2 node_7086 = (i.uv0*_HeightTileFactor);
                float4 _HeightMap_var = tex2D(_HeightMap,TRANSFORM_TEX(node_7086, _HeightMap));
                float3 diffColor = lerp(_Albedo1_var.rgb,_Albedo2_var.rgb,saturate((1.0-(_SplatMap_var.r-step(_SplatMap_var.r,_HeightMap_var.rgb)))));
                float specularMonochrome;
                float3 specColor;
                float2 node_1371 = (i.uv0*_MetallicTileFactor);
                float4 _Metallic_var = tex2D(_Metallic,TRANSFORM_TEX(node_1371, _Metallic));
                diffColor = DiffuseAndSpecularFromMetallic( diffColor, (_Metallic_var.g*_MetallicStrength), specColor, specularMonochrome );
                float roughness = 1.0 - _Gloss;
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
