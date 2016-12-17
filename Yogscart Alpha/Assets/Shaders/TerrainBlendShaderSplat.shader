// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.13 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.13;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,nrsp:0,limd:3,spmd:0,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,rprd:False,enco:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,bsrc:0,bdst:1,culm:0,dpts:2,wrdp:True,dith:0,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1098,x:33556,y:32686,varname:node_1098,prsc:2|diff-5710-OUT,spec-495-OUT,gloss-3256-OUT,normal-9421-OUT,emission-966-OUT;n:type:ShaderForge.SFN_Lerp,id:5710,x:33300,y:32158,varname:node_5710,prsc:2|A-6729-RGB,B-9349-RGB,T-6755-OUT;n:type:ShaderForge.SFN_Tex2d,id:6729,x:32051,y:31975,ptovrint:False,ptlb:Color Map 1,ptin:_ColorMap1,varname:node_6729,prsc:2,ntxv:0,isnm:False|UVIN-9919-OUT;n:type:ShaderForge.SFN_Tex2d,id:9349,x:32051,y:32158,ptovrint:False,ptlb:Color Map 2,ptin:_ColorMap2,varname:node_9349,prsc:2,ntxv:0,isnm:False|UVIN-9919-OUT;n:type:ShaderForge.SFN_Step,id:6755,x:33093,y:32217,varname:node_6755,prsc:2|A-3244-R,B-8500-RGB;n:type:ShaderForge.SFN_Tex2d,id:9335,x:32051,y:32557,ptovrint:False,ptlb:Normal Map 1,ptin:_NormalMap1,varname:_ColorMap3,prsc:2,ntxv:3,isnm:True|UVIN-9919-OUT;n:type:ShaderForge.SFN_Tex2d,id:6903,x:32063,y:33163,ptovrint:False,ptlb:Normal Map 2,ptin:_NormalMap2,varname:_NormalMap2,prsc:2,ntxv:3,isnm:True|UVIN-9919-OUT;n:type:ShaderForge.SFN_Step,id:2549,x:33136,y:32886,varname:node_2549,prsc:2|A-3244-R,B-8500-RGB;n:type:ShaderForge.SFN_Lerp,id:9421,x:33354,y:32795,varname:node_9421,prsc:2|A-9335-RGB,B-6903-RGB,T-2549-OUT;n:type:ShaderForge.SFN_Tex2d,id:8500,x:32051,y:32356,ptovrint:False,ptlb:Height Map,ptin:_HeightMap,varname:node_8500,prsc:2,ntxv:0,isnm:False|UVIN-7086-OUT;n:type:ShaderForge.SFN_TexCoord,id:5961,x:31408,y:31883,varname:node_5961,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:6256,x:31129,y:31990,ptovrint:False,ptlb:Tiling Factor,ptin:_TilingFactor,varname:node_6256,prsc:2,glob:False,v1:2;n:type:ShaderForge.SFN_Multiply,id:9919,x:31624,y:32112,varname:node_9919,prsc:2|A-5961-UVOUT,B-6256-OUT;n:type:ShaderForge.SFN_Slider,id:2763,x:32073,y:32782,ptovrint:False,ptlb:Specular,ptin:_Specular,varname:node_2763,prsc:2,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:272,x:33626,y:32258,varname:node_272,prsc:2|A-5710-OUT,B-8407-OUT;n:type:ShaderForge.SFN_Slider,id:8407,x:32996,y:32452,ptovrint:False,ptlb:Emit,ptin:_Emit,varname:node_8407,prsc:2,min:-1,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:2208,x:32058,y:32883,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:node_2208,prsc:2,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Fresnel,id:7223,x:32946,y:33169,varname:node_7223,prsc:2|EXP-484-OUT;n:type:ShaderForge.SFN_Add,id:966,x:33314,y:32972,varname:node_966,prsc:2|A-272-OUT,B-926-OUT;n:type:ShaderForge.SFN_Slider,id:484,x:32624,y:33235,ptovrint:False,ptlb:Fresnel Falloff,ptin:_FresnelFalloff,varname:node_484,prsc:2,min:2,cur:5.048257,max:10;n:type:ShaderForge.SFN_Slider,id:7470,x:32874,y:33460,ptovrint:False,ptlb:Fresnel Strength,ptin:_FresnelStrength,varname:_node_484_copy,prsc:2,min:0,cur:0.6719882,max:1;n:type:ShaderForge.SFN_Multiply,id:926,x:33129,y:33219,varname:node_926,prsc:2|A-7223-OUT,B-7470-OUT;n:type:ShaderForge.SFN_OneMinus,id:495,x:32470,y:32784,varname:node_495,prsc:2|IN-2763-OUT;n:type:ShaderForge.SFN_OneMinus,id:3256,x:32470,y:32940,varname:node_3256,prsc:2|IN-2208-OUT;n:type:ShaderForge.SFN_Tex2d,id:3244,x:32788,y:32405,ptovrint:False,ptlb:Splat Map,ptin:_SplatMap,varname:node_3244,prsc:2,ntxv:0,isnm:False|UVIN-2205-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:2205,x:32607,y:32405,varname:node_2205,prsc:2,uv:0;n:type:ShaderForge.SFN_TexCoord,id:4828,x:31478,y:32302,varname:node_4828,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:8232,x:31199,y:32409,ptovrint:False,ptlb:Height Tile Factor,ptin:_HeightTileFactor,varname:_TilingFactor_copy,prsc:2,glob:False,v1:2;n:type:ShaderForge.SFN_Multiply,id:7086,x:31694,y:32531,varname:node_7086,prsc:2|A-4828-UVOUT,B-8232-OUT;proporder:6729-9349-9335-6903-8500-6256-2763-2208-8407-484-7470-3244-8232;pass:END;sub:END;*/

Shader "Yogscart/BasicEnvironmentSplatBlend" {
    Properties {
        _ColorMap1 ("Color Map 1", 2D) = "white" {}
        _ColorMap2 ("Color Map 2", 2D) = "white" {}
        _NormalMap1 ("Normal Map 1", 2D) = "bump" {}
        _NormalMap2 ("Normal Map 2", 2D) = "bump" {}
        _HeightMap ("Height Map", 2D) = "white" {}
        _TilingFactor ("Tiling Factor", Float ) = 2
        _Specular ("Specular", Range(0, 1)) = 0
        _Gloss ("Gloss", Range(0, 1)) = 0
        _Emit ("Emit", Range(-1, 1)) = 0
        _FresnelFalloff ("Fresnel Falloff", Range(2, 10)) = 5.048257
        _FresnelStrength ("Fresnel Strength", Range(0, 1)) = 0.6719882
        _SplatMap ("Splat Map", 2D) = "white" {}
        _HeightTileFactor ("Height Tile Factor", Float ) = 2
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
            uniform sampler2D _ColorMap1; uniform float4 _ColorMap1_ST;
            uniform sampler2D _ColorMap2; uniform float4 _ColorMap2_ST;
            uniform sampler2D _NormalMap1; uniform float4 _NormalMap1_ST;
            uniform sampler2D _NormalMap2; uniform float4 _NormalMap2_ST;
            uniform sampler2D _HeightMap; uniform float4 _HeightMap_ST;
            uniform float _TilingFactor;
            uniform float _Specular;
            uniform float _Emit;
            uniform float _Gloss;
            uniform float _FresnelFalloff;
            uniform float _FresnelStrength;
            uniform sampler2D _SplatMap; uniform float4 _SplatMap_ST;
            uniform float _HeightTileFactor;
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
            o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
            o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
            float3 lightColor = _LightColor0.rgb;
            o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
            UNITY_TRANSFER_FOG(o,o.pos);
            TRANSFER_VERTEX_TO_FRAGMENT(o)
            return o;
        }
        float4 frag(VertexOutput i) : COLOR {
            i.normalDir = normalize(i.normalDir);
            float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/// Vectors:
            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
            float2 node_9919 = (i.uv0*_TilingFactor);
            float3 _NormalMap1_var = UnpackNormal(tex2D(_NormalMap1,TRANSFORM_TEX(node_9919, _NormalMap1)));
            float3 _NormalMap2_var = UnpackNormal(tex2D(_NormalMap2,TRANSFORM_TEX(node_9919, _NormalMap2)));
            float4 _SplatMap_var = tex2D(_SplatMap,TRANSFORM_TEX(i.uv0, _SplatMap));
            float2 node_7086 = (i.uv0*_HeightTileFactor);
            float4 _HeightMap_var = tex2D(_HeightMap,TRANSFORM_TEX(node_7086, _HeightMap));
            float3 normalLocal = lerp(_NormalMap1_var.rgb,_NormalMap2_var.rgb,step(_SplatMap_var.r,_HeightMap_var.rgb));
            float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
            float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
            float3 lightColor = _LightColor0.rgb;
            float3 halfDirection = normalize(viewDirection+lightDirection);
// Lighting:
            float attenuation = LIGHT_ATTENUATION(i);
            float3 attenColor = attenuation * _LightColor0.xyz;
            float Pi = 3.141592654;
            float InvPi = 0.31830988618;
///// Gloss:
            float gloss = (1.0 - _Gloss);
            float specPow = exp2( gloss * 10.0+1.0);
/// GI Data:
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
            UnityGI gi = UnityGlobalIllumination (d, 1, gloss, normalDirection);
            lightDirection = gi.light.dir;
            lightColor = gi.light.color;
// Specular:
            float NdotL = max(0, dot( normalDirection, lightDirection ));
            float LdotH = max(0.0,dot(lightDirection, halfDirection));
            float node_495 = (1.0 - _Specular);
            float3 specularColor = float3(node_495,node_495,node_495);
            float specularMonochrome = max( max(specularColor.r, specularColor.g), specularColor.b);
            float NdotV = max(0.0,dot( normalDirection, viewDirection ));
            float NdotH = max(0.0,dot( normalDirection, halfDirection ));
            float VdotH = max(0.0,dot( viewDirection, halfDirection ));
            float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
            float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
            float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
            float3 directSpecular = 1 * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
            float3 specular = directSpecular;
/// Diffuse:
            NdotL = max(0.0,dot( normalDirection, lightDirection ));
            half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
            float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
            float3 indirectDiffuse = float3(0,0,0);
            indirectDiffuse += gi.indirect.diffuse;
            float4 _ColorMap1_var = tex2D(_ColorMap1,TRANSFORM_TEX(node_9919, _ColorMap1));
            float4 _ColorMap2_var = tex2D(_ColorMap2,TRANSFORM_TEX(node_9919, _ColorMap2));
            float3 node_5710 = lerp(_ColorMap1_var.rgb,_ColorMap2_var.rgb,step(_SplatMap_var.r,_HeightMap_var.rgb));
            float3 diffuseColor = node_5710;
            diffuseColor *= 1-specularMonochrome;
            float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
// Emissive:
            float3 emissive = ((node_5710*_Emit)+(pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelFalloff)*_FresnelStrength));
// Final Color:
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
        #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
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
        uniform sampler2D _ColorMap1; uniform float4 _ColorMap1_ST;
        uniform sampler2D _ColorMap2; uniform float4 _ColorMap2_ST;
        uniform sampler2D _NormalMap1; uniform float4 _NormalMap1_ST;
        uniform sampler2D _NormalMap2; uniform float4 _NormalMap2_ST;
        uniform sampler2D _HeightMap; uniform float4 _HeightMap_ST;
        uniform float _TilingFactor;
        uniform float _Specular;
        uniform float _Emit;
        uniform float _Gloss;
        uniform float _FresnelFalloff;
        uniform float _FresnelStrength;
        uniform sampler2D _SplatMap; uniform float4 _SplatMap_ST;
        uniform float _HeightTileFactor;
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
        };
        VertexOutput vert (VertexInput v) {
            VertexOutput o = (VertexOutput)0;
            o.uv0 = v.texcoord0;
            o.uv1 = v.texcoord1;
            o.uv2 = v.texcoord2;
            o.normalDir = UnityObjectToWorldNormal(v.normal);
            o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
            o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
            float3 lightColor = _LightColor0.rgb;
            o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
            TRANSFER_VERTEX_TO_FRAGMENT(o)
            return o;
        }
        float4 frag(VertexOutput i) : COLOR {
            i.normalDir = normalize(i.normalDir);
            float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/// Vectors:
            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
            float2 node_9919 = (i.uv0*_TilingFactor);
            float3 _NormalMap1_var = UnpackNormal(tex2D(_NormalMap1,TRANSFORM_TEX(node_9919, _NormalMap1)));
            float3 _NormalMap2_var = UnpackNormal(tex2D(_NormalMap2,TRANSFORM_TEX(node_9919, _NormalMap2)));
            float4 _SplatMap_var = tex2D(_SplatMap,TRANSFORM_TEX(i.uv0, _SplatMap));
            float2 node_7086 = (i.uv0*_HeightTileFactor);
            float4 _HeightMap_var = tex2D(_HeightMap,TRANSFORM_TEX(node_7086, _HeightMap));
            float3 normalLocal = lerp(_NormalMap1_var.rgb,_NormalMap2_var.rgb,step(_SplatMap_var.r,_HeightMap_var.rgb));
            float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
            float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
            float3 lightColor = _LightColor0.rgb;
            float3 halfDirection = normalize(viewDirection+lightDirection);
// Lighting:
            float attenuation = LIGHT_ATTENUATION(i);
            float3 attenColor = attenuation * _LightColor0.xyz;
            float Pi = 3.141592654;
            float InvPi = 0.31830988618;
///// Gloss:
            float gloss = (1.0 - _Gloss);
            float specPow = exp2( gloss * 10.0+1.0);
// Specular:
            float NdotL = max(0, dot( normalDirection, lightDirection ));
            float LdotH = max(0.0,dot(lightDirection, halfDirection));
            float node_495 = (1.0 - _Specular);
            float3 specularColor = float3(node_495,node_495,node_495);
            float specularMonochrome = max( max(specularColor.r, specularColor.g), specularColor.b);
            float NdotV = max(0.0,dot( normalDirection, viewDirection ));
            float NdotH = max(0.0,dot( normalDirection, halfDirection ));
            float VdotH = max(0.0,dot( viewDirection, halfDirection ));
            float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
            float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
            float specularPBL = max(0, (NdotL*visTerm*normTerm) * unity_LightGammaCorrectionConsts_PIDiv4 );
            float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
            float3 specular = directSpecular;
/// Diffuse:
            NdotL = max(0.0,dot( normalDirection, lightDirection ));
            half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
            float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
            float4 _ColorMap1_var = tex2D(_ColorMap1,TRANSFORM_TEX(node_9919, _ColorMap1));
            float4 _ColorMap2_var = tex2D(_ColorMap2,TRANSFORM_TEX(node_9919, _ColorMap2));
            float3 node_5710 = lerp(_ColorMap1_var.rgb,_ColorMap2_var.rgb,step(_SplatMap_var.r,_HeightMap_var.rgb));
            float3 diffuseColor = node_5710;
            diffuseColor *= 1-specularMonochrome;
            float3 diffuse = directDiffuse * diffuseColor;
// Final Color:
            float3 finalColor = diffuse + specular;
            return fixed4(finalColor * 1,0);
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
        uniform sampler2D _ColorMap1; uniform float4 _ColorMap1_ST;
        uniform sampler2D _ColorMap2; uniform float4 _ColorMap2_ST;
        uniform sampler2D _HeightMap; uniform float4 _HeightMap_ST;
        uniform float _TilingFactor;
        uniform float _Specular;
        uniform float _Emit;
        uniform float _Gloss;
        uniform float _FresnelFalloff;
        uniform float _FresnelStrength;
        uniform sampler2D _SplatMap; uniform float4 _SplatMap_ST;
        uniform float _HeightTileFactor;
        struct VertexInput {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
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
        };
        VertexOutput vert (VertexInput v) {
            VertexOutput o = (VertexOutput)0;
            o.uv0 = v.texcoord0;
            o.uv1 = v.texcoord1;
            o.uv2 = v.texcoord2;
            o.normalDir = UnityObjectToWorldNormal(v.normal);
            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
            o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
            return o;
        }
        float4 frag(VertexOutput i) : SV_Target {
            i.normalDir = normalize(i.normalDir);
/// Vectors:
            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
            float3 normalDirection = i.normalDir;
            UnityMetaInput o;
            UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
            
            float2 node_9919 = (i.uv0*_TilingFactor);
            float4 _ColorMap1_var = tex2D(_ColorMap1,TRANSFORM_TEX(node_9919, _ColorMap1));
            float4 _ColorMap2_var = tex2D(_ColorMap2,TRANSFORM_TEX(node_9919, _ColorMap2));
            float4 _SplatMap_var = tex2D(_SplatMap,TRANSFORM_TEX(i.uv0, _SplatMap));
            float2 node_7086 = (i.uv0*_HeightTileFactor);
            float4 _HeightMap_var = tex2D(_HeightMap,TRANSFORM_TEX(node_7086, _HeightMap));
            float3 node_5710 = lerp(_ColorMap1_var.rgb,_ColorMap2_var.rgb,step(_SplatMap_var.r,_HeightMap_var.rgb));
            o.Emission = ((node_5710*_Emit)+(pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelFalloff)*_FresnelStrength));
            
            float3 diffColor = node_5710;
            float node_495 = (1.0 - _Specular);
            float3 specColor = float3(node_495,node_495,node_495);
            float specularMonochrome = max(max(specColor.r, specColor.g),specColor.b);
            diffColor *= (1.0-specularMonochrome);
            float roughness = 1.0 - (1.0 - _Gloss);
            o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
            
            return UnityMetaFragment( o );
        }
        ENDCG
    }
}
FallBack "Diffuse"
CustomEditor "ShaderForgeMaterialInspector"
}
