// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UMI3D/default_UMI3D_shader"
{
	Properties
	{
		[Toggle]_ActivateClip("Activate Clip", Float) = 1
		_Color("Main Color", Color) = (0.1640964,1,0,0)
		_MainTex("Main Texture", 2D) = "white" {}
		[HDR]_EmissiveColor("Emissive Color", Color) = (0,0,0,0)
		[Toggle]_Enable_cubemap("Enable_cubemap", Float) = 0
		_Cubemap("Cubemap", CUBE) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		[Normal]_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalMapScale("Normal Map Scale", Range( 0 , 1)) = 0
		[Toggle]_EnableChannelMap("Enable Channel Map", Float) = 0
		_ChannelMap("Channel Map", 2D) = "white" {}
		_Tiling("Tiling", Vector) = (1,1,0,0)
		_Offset("Offset", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		AlphaToMask On
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 2.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float3 uv_texcoord;
			float3 worldPos;
			float3 worldRefl;
			INTERNAL_DATA
		};

		uniform sampler2D _NormalMap;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform float _NormalMapScale;
		uniform sampler2D _MainTex;
		uniform float4 _Color;
		uniform float _ActivateClip;
		uniform float4 _PlaneClipNormals;
		uniform float _Enable_cubemap;
		uniform samplerCUBE _Cubemap;
		uniform float4 _EmissiveColor;
		uniform float _EnableChannelMap;
		uniform float _Metallic;
		uniform float _Smoothness;
		uniform sampler2D _ChannelMap;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uvs_TexCoord83 = i.uv_texcoord;
			uvs_TexCoord83.xy = i.uv_texcoord.xy * _Tiling + _Offset;
			float2 texture_coord87 = uvs_TexCoord83.xy;
			float3 Normal50 = UnpackScaleNormal( tex2D( _NormalMap, texture_coord87 ), _NormalMapScale );
			float3 temp_output_51_0 = Normal50;
			o.Normal = temp_output_51_0;
			float4 Albedo48 = ( tex2D( _MainTex, texture_coord87 ) * _Color );
			float3 ase_worldPos = i.worldPos;
			float dotResult126 = dot( (_PlaneClipNormals).xyz , ase_worldPos );
			clip( (( _ActivateClip )?( -( dotResult126 + _PlaneClipNormals.w ) ):( 0.0 )) );
			float4 Albedo_Cut133 = Albedo48;
			o.Albedo = Albedo_Cut133.rgb;
			float4 temp_cast_1 = (1.0).xxxx;
			float4 CubemapNormal154 = (( _Enable_cubemap )?( texCUBE( _Cubemap, WorldReflectionVector( i , Normal50 ) ) ):( temp_cast_1 ));
			float4 appendResult62 = (float4(_Metallic , 1.0 , _Smoothness , 1.0));
			float4 break26 = tex2D( _ChannelMap, texture_coord87 );
			float R_metal54 = break26.r;
			float G_occlusion55 = break26.g;
			float A_smoothness57 = break26.a;
			float B_emission56 = break26.b;
			float4 appendResult67 = (float4(R_metal54 , G_occlusion55 , A_smoothness57 , B_emission56));
			float4 break71 = (( _EnableChannelMap )?( appendResult67 ):( appendResult62 ));
			o.Emission = ( ( CubemapNormal154 * _EmissiveColor ) * break71.w ).rgb;
			o.Metallic = break71;
			o.Smoothness = break71.z;
			o.Occlusion = break71.y;
			float Opacity76 = _Color.a;
			o.Alpha = Opacity76;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			AlphaToMask Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xyz = customInputData.uv_texcoord;
				o.customPack1.xyz = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xyz;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldRefl = -worldViewDir;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
1536;538;1937;687;5344.106;4287.98;5.002018;True;False
Node;AmplifyShaderEditor.CommentaryNode;96;-53.93892,-3135.547;Inherit;False;839.0342;363.0854;;4;87;85;84;83;TEXTURE COORDINATES;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;84;0.06101632,-3085.547;Inherit;False;Property;_Tiling;Tiling;13;0;Create;False;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;85;-3.938972,-2946.547;Inherit;False;Property;_Offset;Offset;14;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;83;311.0609,-3000.547;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;87;557.0607,-3004.547;Inherit;False;texture_coord;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;53;-1031.124,-3139.192;Inherit;False;915.1437;368.2449;;4;14;7;50;89;NORMAL;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-992.0122,-3059.129;Inherit;False;87;texture_coord;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-999.0986,-2974.753;Inherit;False;Property;_NormalMapScale;Normal Map Scale;9;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;135;-2126.1,-2063.467;Inherit;False;1539.076;753.327;from shader PlaneClip;2;129;121;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;121;-2077.841,-1775.682;Inherit;False;1206.166;434.1638;Check on which side of the plane this fragment is;7;128;127;126;125;124;123;122;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;7;-659.0652,-3067.947;Inherit;True;Property;_NormalMap;Normal Map;8;1;[Normal];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;58;-1039.553,-2669.859;Inherit;False;1078.752;443.1189;;7;88;57;56;54;55;26;11;CHANNEL MAP;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;52;-1034.548,-3678.51;Inherit;False;1176.516;452.7788;;6;90;2;1;76;48;47;ALBEDO;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;122;-2029.285,-1557.211;Inherit;False;Global;_PlaneClipNormals;_PlaneClipNormals;13;0;Create;False;0;0;0;False;0;False;0,0,0,0;-0.7160075,-6.109477E-07,0.6980926,-4.990572;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;50;-339.9802,-3068.241;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;88;-1013.203,-2502.107;Inherit;False;87;texture_coord;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-1010.409,-3600.126;Inherit;False;87;texture_coord;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;123;-1629.24,-1610.527;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SwizzleNode;124;-1613.958,-1723.672;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;11;-787.5468,-2526.025;Inherit;True;Property;_ChannelMap;Channel Map;12;0;Create;False;0;0;0;False;0;False;-1;None;f296dd9a0eab37c459e37c5c027db4b9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;51;2488.152,-2740.48;Inherit;False;50;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldReflectionVector;151;2695.7,-3029.864;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;1;-696.4949,-3420.891;Inherit;False;Property;_Color;Main Color;1;0;Create;False;0;0;0;False;0;False;0.1640964,1,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;125;-1323.306,-1423.183;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-780.0706,-3624.51;Inherit;True;Property;_MainTex;Main Texture;2;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;26;-452.7738,-2520.932;Inherit;True;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DotProductOpNode;126;-1342.027,-1717.681;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;57;-188.1504,-2361.77;Inherit;False;A_smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;129;-1172.208,-2013.467;Inherit;False;538.6697;195.436;To deactivate clip simply place 0 on its inputs;2;131;130;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;158;3038.802,-3212.539;Inherit;False;Constant;_Float1;Float 1;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-185.341,-2535.497;Inherit;False;G_occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;152;2912.42,-3059.883;Inherit;True;Property;_Cubemap;Cubemap;5;0;Create;True;0;0;0;False;0;False;-1;None;784d8a7d322a43baa027462fbd81f226;True;0;False;white;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-187.4204,-2446.012;Inherit;False;B_emission;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-292.675,-3620.353;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;127;-1188.473,-1650.681;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;54;-184.8014,-2619.859;Inherit;False;R_metal;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;1275.934,-2390.393;Inherit;False;55;G_occlusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;136;-416.4057,-2026.631;Inherit;False;815.9672;214.1059;Comment;3;49;132;133;ALBEDO CUT;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;3;1110.276,-2678.579;Inherit;False;Property;_Metallic;Metallic;6;0;Create;False;0;0;0;False;0;False;0;0.865;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;1111.443,-2601.342;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;False;0;0;0;False;0;False;1;0.629;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;128;-1035.673,-1661.08;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;1265.401,-2315.041;Inherit;False;57;A_smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;1282.233,-2238.756;Inherit;False;56;B_emission;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;1287.389,-2467.64;Inherit;False;54;R_metal;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-1128.208,-1950.467;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;157;3224.887,-3147.501;Inherit;False;Property;_Enable_cubemap;Enable_cubemap;4;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;-86.03155,-3625.21;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;62;1494.626,-2673.653;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ToggleSwitchNode;131;-868.2087,-1949.467;Inherit;False;Property;_ActivateClip;Activate Clip;0;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;154;3486.896,-3147.521;Inherit;False;CubemapNormal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;67;1552.934,-2504.392;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-366.4059,-1976.264;Inherit;False;48;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;23;1829.565,-2678.195;Inherit;False;Property;_EnableChannelMap;Enable Channel Map;10;0;Create;False;0;0;0;False;0;False;0;True;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ClipNode;132;-39.80089,-1971.525;Inherit;False;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;149;1812.786,-3008.633;Inherit;False;Property;_EmissiveColor;Emissive Color;3;1;[HDR];Create;False;0;0;0;False;0;False;0,0,0,0;0.08490568,0.08490568,0.08490568,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;156;1808.885,-3086.644;Inherit;False;154;CubemapNormal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;175.5612,-1976.631;Inherit;False;Albedo_Cut;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;155;2109.555,-3080.271;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;71;2101.695,-2672.348;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode;76;-394.9749,-3326.796;Inherit;False;Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;150;2276.013,-2706.986;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;2673.001,-2764.414;Inherit;False;133;Albedo_Cut;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;143;2701.737,-2550.475;Inherit;False;76;Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2922.54,-2760.833;Float;False;True;-1;0;ASEMaterialInspector;0;0;Standard;UMI3D/default_UMI3D_shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;True;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;11;-1;-1;-1;0;True;0;0;False;14;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;83;0;84;0
WireConnection;83;1;85;0
WireConnection;87;0;83;0
WireConnection;7;1;89;0
WireConnection;7;5;14;0
WireConnection;50;0;7;0
WireConnection;124;0;122;0
WireConnection;11;1;88;0
WireConnection;151;0;51;0
WireConnection;125;0;122;4
WireConnection;2;1;90;0
WireConnection;26;0;11;0
WireConnection;126;0;124;0
WireConnection;126;1;123;0
WireConnection;57;0;26;3
WireConnection;55;0;26;1
WireConnection;152;1;151;0
WireConnection;56;0;26;2
WireConnection;47;0;2;0
WireConnection;47;1;1;0
WireConnection;127;0;126;0
WireConnection;127;1;125;0
WireConnection;54;0;26;0
WireConnection;128;0;127;0
WireConnection;157;0;158;0
WireConnection;157;1;152;0
WireConnection;48;0;47;0
WireConnection;62;0;3;0
WireConnection;62;2;4;0
WireConnection;131;0;130;0
WireConnection;131;1;128;0
WireConnection;154;0;157;0
WireConnection;67;0;63;0
WireConnection;67;1;64;0
WireConnection;67;2;66;0
WireConnection;67;3;65;0
WireConnection;23;0;62;0
WireConnection;23;1;67;0
WireConnection;132;0;49;0
WireConnection;132;1;131;0
WireConnection;133;0;132;0
WireConnection;155;0;156;0
WireConnection;155;1;149;0
WireConnection;71;0;23;0
WireConnection;76;0;1;4
WireConnection;150;0;155;0
WireConnection;150;1;71;3
WireConnection;0;0;134;0
WireConnection;0;1;51;0
WireConnection;0;2;150;0
WireConnection;0;3;71;0
WireConnection;0;4;71;2
WireConnection;0;5;71;1
WireConnection;0;9;143;0
ASEEND*/
//CHKSM=D817B7E8D1F2375A99C0BE874A4AD8947876265E