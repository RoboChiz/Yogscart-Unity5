// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.13 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.13;sub:START;pass:START;ps:flbk:Standard (Specular setup),lico:1,lgpr:1,nrmq:1,nrsp:0,limd:3,spmd:0,trmd:0,grmd:1,uamb:True,mssp:True,bkdf:False,rprd:False,enco:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,bsrc:0,bdst:1,culm:0,dpts:2,wrdp:True,dith:0,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:8612,x:33224,y:32677,varname:node_8612,prsc:2|diff-7406-RGB,spec-3908-OUT,gloss-5002-OUT,normal-2888-OUT,emission-7873-RGB,transm-7675-RGB,lwrap-6815-OUT,amspl-3116-RGB;n:type:ShaderForge.SFN_Color,id:7406,x:33194,y:32275,ptovrint:False,ptlb:Albedo Color,ptin:_AlbedoColor,varname:node_7406,prsc:2,glob:False,c1:1,c2:0.8482758,c3:0,c4:1;n:type:ShaderForge.SFN_Fresnel,id:7703,x:32875,y:33304,varname:node_7703,prsc:2|EXP-9733-OUT;n:type:ShaderForge.SFN_Vector1,id:9733,x:32707,y:33388,varname:node_9733,prsc:2,v1:0.97;n:type:ShaderForge.SFN_Vector1,id:77,x:32875,y:33243,varname:node_77,prsc:2,v1:0.4;n:type:ShaderForge.SFN_Add,id:6815,x:33039,y:33281,varname:node_6815,prsc:2|A-77-OUT,B-7703-OUT;n:type:ShaderForge.SFN_Color,id:7675,x:33055,y:32275,ptovrint:False,ptlb:Transmission Color,ptin:_TransmissionColor,varname:_AlbedoColor_copy,prsc:2,glob:False,c1:1,c2:0.6982759,c3:0.375,c4:1;n:type:ShaderForge.SFN_Cubemap,id:3116,x:32396,y:32397,ptovrint:False,ptlb:Cubemap,ptin:_Cubemap,varname:node_3116,prsc:2,cube:2193c36bfedeb12449e6a7f103a67f2f,pvfc:0;n:type:ShaderForge.SFN_NormalVector,id:3631,x:31912,y:32667,prsc:2,pt:False;n:type:ShaderForge.SFN_Abs,id:7339,x:32156,y:32667,varname:node_7339,prsc:2|IN-3631-OUT;n:type:ShaderForge.SFN_Append,id:3871,x:32110,y:33003,varname:node_3871,prsc:2|A-7213-Z,B-7213-Y;n:type:ShaderForge.SFN_Append,id:7042,x:32110,y:32877,varname:node_7042,prsc:2|A-7213-X,B-7213-Z;n:type:ShaderForge.SFN_Append,id:8649,x:32110,y:33124,varname:node_8649,prsc:2|A-7213-X,B-7213-Y;n:type:ShaderForge.SFN_Multiply,id:5866,x:32360,y:32667,varname:node_5866,prsc:2|A-7339-OUT,B-7339-OUT;n:type:ShaderForge.SFN_ChannelBlend,id:2888,x:32721,y:32800,varname:node_2888,prsc:2,chbt:0|M-5866-OUT,R-4920-RGB,G-5301-RGB,B-2231-RGB,BTM-4920-RGB;n:type:ShaderForge.SFN_Tex2d,id:2231,x:32473,y:33259,varname:node_2231,prsc:2,tex:ec59b96044431a74e82ce58fde8a156e,ntxv:3,isnm:True|UVIN-2503-OUT,TEX-6518-TEX;n:type:ShaderForge.SFN_FragmentPosition,id:7213,x:31900,y:32924,varname:node_7213,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:4920,x:32473,y:33123,varname:node_4920,prsc:2,tex:ec59b96044431a74e82ce58fde8a156e,ntxv:3,isnm:True|UVIN-1659-OUT,TEX-6518-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:6518,x:32253,y:33388,ptovrint:False,ptlb:Honey AnimTex,ptin:_HoneyAnimTex,varname:node_6518,glob:False,tex:ec59b96044431a74e82ce58fde8a156e,ntxv:3,isnm:False;n:type:ShaderForge.SFN_Panner,id:1108,x:32253,y:33209,varname:node_1108,prsc:2,spu:0,spv:0.1|UVIN-8649-OUT;n:type:ShaderForge.SFN_Panner,id:5220,x:32265,y:33017,varname:node_5220,prsc:2,spu:0,spv:0.07|UVIN-3871-OUT;n:type:ShaderForge.SFN_Multiply,id:1659,x:32462,y:32973,varname:node_1659,prsc:2|A-5220-UVOUT,B-8228-OUT;n:type:ShaderForge.SFN_Multiply,id:2503,x:32426,y:33426,varname:node_2503,prsc:2|A-1108-UVOUT,B-8228-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8228,x:32110,y:33283,ptovrint:False,ptlb:Normal_Tiles,ptin:_Normal_Tiles,varname:node_8228,prsc:2,glob:False,v1:2;n:type:ShaderForge.SFN_ValueProperty,id:3908,x:32657,y:32425,ptovrint:False,ptlb:Spec Value,ptin:_SpecValue,varname:node_3908,prsc:2,glob:False,v1:0.15;n:type:ShaderForge.SFN_ValueProperty,id:5002,x:32538,y:32425,ptovrint:False,ptlb:Gloss Value,ptin:_GlossValue,varname:node_5002,prsc:2,glob:False,v1:0.5;n:type:ShaderForge.SFN_Tex2d,id:5301,x:32301,y:32841,ptovrint:False,ptlb:Top Project (Ignore),ptin:_TopProjectIgnore,varname:node_5301,prsc:2,ntxv:3,isnm:True|UVIN-7042-OUT;n:type:ShaderForge.SFN_Color,id:7873,x:32918,y:32275,ptovrint:False,ptlb:Emission Color,ptin:_EmissionColor,varname:node_7873,prsc:2,glob:False,c1:0.3161765,c2:0.189706,c3:0,c4:1;proporder:7406-7873-7675-6518-3116-8228-3908-5002-5301;pass:END;sub:END;*/

Shader "Yogscart/HoneyEmission" {
    Properties {
        _AlbedoColor ("Albedo Color", Color) = (1,0.8482758,0,1)
        _EmissionColor ("Emission Color", Color) = (0.3161765,0.189706,0,1)
        _TransmissionColor ("Transmission Color", Color) = (1,0.6982759,0.375,1)
        _HoneyAnimTex ("Honey AnimTex", 2D) = "bump" {}
        _Cubemap ("Cubemap", Cube) = "_Skybox" {}
        _Normal_Tiles ("Normal_Tiles", Float ) = 2
        _SpecValue ("Spec Value", Float ) = 0.15
        _GlossValue ("Gloss Value", Float ) = 0.5
        _TopProjectIgnore ("Top Project (Ignore)", 2D) = "bump" {}
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
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float4 _AlbedoColor;
            uniform float4 _TransmissionColor;
            uniform samplerCUBE _Cubemap;
            uniform sampler2D _HoneyAnimTex; uniform float4 _HoneyAnimTex_ST;
            uniform float _Normal_Tiles;
            uniform float _SpecValue;
            uniform float _GlossValue;
            uniform sampler2D _TopProjectIgnore; uniform float4 _TopProjectIgnore_ST;
            uniform float4 _EmissionColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float3 tangentDir : TEXCOORD2;
                float3 bitangentDir : TEXCOORD3;
                LIGHTING_COORDS(4,5)
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 node_7339 = abs(i.normalDir);
                float3 node_5866 = (node_7339*node_7339);
                float4 node_5292 = _Time + _TimeEditor;
                float2 node_1659 = ((float2(i.posWorld.b,i.posWorld.g)+node_5292.g*float2(0,0.07))*_Normal_Tiles);
                float4 node_4920 = tex2D(_HoneyAnimTex,TRANSFORM_TEX(node_1659, _HoneyAnimTex));
                float2 node_7042 = float2(i.posWorld.r,i.posWorld.b);
                float3 _TopProjectIgnore_var = UnpackNormal(tex2D(_TopProjectIgnore,TRANSFORM_TEX(node_7042, _TopProjectIgnore)));
                float2 node_2503 = ((float2(i.posWorld.r,i.posWorld.g)+node_5292.g*float2(0,0.1))*_Normal_Tiles);
                float4 node_2231 = tex2D(_HoneyAnimTex,TRANSFORM_TEX(node_2503, _HoneyAnimTex));
                float3 normalLocal = (node_5866.r*node_4920.rgb + node_5866.g*_TopProjectIgnore_var.rgb + node_5866.b*node_2231.rgb);
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
                float gloss = 1.0 - _GlossValue; // Convert roughness to gloss
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
                UnityGI gi = UnityGlobalIllumination (d, 1, gloss, normalDirection);
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float3 specularColor = float3(_SpecValue,_SpecValue,_SpecValue);
                float specularMonochrome = max( max(specularColor.r, specularColor.g), specularColor.b);
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
                float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
                float3 directSpecular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (0 + texCUBE(_Cubemap,viewReflectDirection).rgb);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = dot( normalDirection, lightDirection );
                float node_6815 = (0.4+pow(1.0-max(0,dot(normalDirection, viewDirection)),0.97));
                float3 w = float3(node_6815,node_6815,node_6815)*0.5; // Light wrapping
                float3 NdotLWrap = NdotL * ( 1.0 - w );
                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
                float3 backLight = max(float3(0.0,0.0,0.0), -NdotLWrap + w ) * _TransmissionColor.rgb;
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                NdotLWrap = max(float3(0,0,0), NdotLWrap);
                float3 directDiffuse = ((forwardLight+backLight) + ((1 +(fd90 - 1)*pow((1.00001-NdotLWrap), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL))*(0.5-max(w.r,max(w.g,w.b))*0.5) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 diffuseColor = _AlbedoColor.rgb;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float3 emissive = _EmissionColor.rgb;
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
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
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float4 _AlbedoColor;
            uniform float4 _TransmissionColor;
            uniform sampler2D _HoneyAnimTex; uniform float4 _HoneyAnimTex_ST;
            uniform float _Normal_Tiles;
            uniform float _SpecValue;
            uniform float _GlossValue;
            uniform sampler2D _TopProjectIgnore; uniform float4 _TopProjectIgnore_ST;
            uniform float4 _EmissionColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float3 tangentDir : TEXCOORD2;
                float3 bitangentDir : TEXCOORD3;
                LIGHTING_COORDS(4,5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 node_7339 = abs(i.normalDir);
                float3 node_5866 = (node_7339*node_7339);
                float4 node_6047 = _Time + _TimeEditor;
                float2 node_1659 = ((float2(i.posWorld.b,i.posWorld.g)+node_6047.g*float2(0,0.07))*_Normal_Tiles);
                float4 node_4920 = tex2D(_HoneyAnimTex,TRANSFORM_TEX(node_1659, _HoneyAnimTex));
                float2 node_7042 = float2(i.posWorld.r,i.posWorld.b);
                float3 _TopProjectIgnore_var = UnpackNormal(tex2D(_TopProjectIgnore,TRANSFORM_TEX(node_7042, _TopProjectIgnore)));
                float2 node_2503 = ((float2(i.posWorld.r,i.posWorld.g)+node_6047.g*float2(0,0.1))*_Normal_Tiles);
                float4 node_2231 = tex2D(_HoneyAnimTex,TRANSFORM_TEX(node_2503, _HoneyAnimTex));
                float3 normalLocal = (node_5866.r*node_4920.rgb + node_5866.g*_TopProjectIgnore_var.rgb + node_5866.b*node_2231.rgb);
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
                float gloss = 1.0 - _GlossValue; // Convert roughness to gloss
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float3 specularColor = float3(_SpecValue,_SpecValue,_SpecValue);
                float specularMonochrome = max( max(specularColor.r, specularColor.g), specularColor.b);
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
                float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = dot( normalDirection, lightDirection );
                float node_6815 = (0.4+pow(1.0-max(0,dot(normalDirection, viewDirection)),0.97));
                float3 w = float3(node_6815,node_6815,node_6815)*0.5; // Light wrapping
                float3 NdotLWrap = NdotL * ( 1.0 - w );
                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
                float3 backLight = max(float3(0.0,0.0,0.0), -NdotLWrap + w ) * _TransmissionColor.rgb;
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                NdotLWrap = max(float3(0,0,0), NdotLWrap);
                float3 directDiffuse = ((forwardLight+backLight) + ((1 +(fd90 - 1)*pow((1.00001-NdotLWrap), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL))*(0.5-max(w.r,max(w.g,w.b))*0.5) * attenColor;
                float3 diffuseColor = _AlbedoColor.rgb;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
    }
    FallBack "Standard (Specular setup)"
    CustomEditor "ShaderForgeMaterialInspector"
}
