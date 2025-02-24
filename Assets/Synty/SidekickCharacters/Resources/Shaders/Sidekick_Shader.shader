// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Sidekick_Shader_URP"
{
	Properties
	{
		[NoScaleOffset][SingleLineTexture]_ColorMap("ColorMap", 2D) = "white" {}
		[HideInInspector][NoScaleOffset][SingleLineTexture]_EmissionMap("EmissionMap", 2D) = "white" {}
		[HideInInspector][NoScaleOffset][SingleLineTexture]_MetallicMap("MetallicMap", 2D) = "white" {}
		[HideInInspector][NoScaleOffset][SingleLineTexture]_SmoothnessMap("SmoothnessMap", 2D) = "white" {}
		[NoScaleOffset][SingleLineTexture]_DarkMaskTexture("Dark Mask Texture", 2D) = "white" {}
		[HideInInspector]_DarkColor("Dark Color", Color) = (0,0,0,0)
		[NoScaleOffset][SingleLineTexture]_DirtMaskTexture("Dirt Mask Texture", 2D) = "white" {}
		_DirtColor("Dirt Color", Color) = (0.4433962,0.2989818,0.161742,0)
		_DirtAmount("Dirt Amount", Range( 0 , 1)) = 0.5
		[NoScaleOffset][SingleLineTexture]_CutsMaskTexture("Cuts Mask Texture", 2D) = "white" {}
		_CutsColor("Cuts Color", Color) = (0.3333333,0.3333333,0.3333333,0)
		_CutsAmount("Cuts Amount", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _CutsColor;
		uniform float4 _DirtColor;
		uniform float4 _DarkColor;
		uniform sampler2D _ColorMap;
		uniform sampler2D _DarkMaskTexture;
		uniform sampler2D _DirtMaskTexture;
		uniform float _DirtAmount;
		uniform sampler2D _CutsMaskTexture;
		uniform float _CutsAmount;
		uniform sampler2D _EmissionMap;
		uniform sampler2D _MetallicMap;
		uniform sampler2D _SmoothnessMap;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_ColorMap1 = i.uv_texcoord;
			float4 color21 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float2 uv_DarkMaskTexture19 = i.uv_texcoord;
			float4 lerpResult14 = lerp( color21 , tex2D( _DarkMaskTexture, uv_DarkMaskTexture19 ) , 0.5);
			float4 lerpResult22 = lerp( _DarkColor , tex2D( _ColorMap, uv_ColorMap1 ) , lerpResult14);
			float2 uv_DirtMaskTexture17 = i.uv_texcoord;
			float4 lerpResult15 = lerp( color21 , tex2D( _DirtMaskTexture, uv_DirtMaskTexture17 ) , _DirtAmount);
			float4 lerpResult23 = lerp( _DirtColor , lerpResult22 , lerpResult15);
			float2 uv_CutsMaskTexture45 = i.uv_texcoord;
			float4 lerpResult48 = lerp( color21 , tex2D( _CutsMaskTexture, uv_CutsMaskTexture45 ) , _CutsAmount);
			float4 lerpResult46 = lerp( _CutsColor , lerpResult23 , lerpResult48);
			o.Albedo = lerpResult46.rgb;
			float2 uv_EmissionMap2 = i.uv_texcoord;
			float4 color56 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 lerpResult52 = lerp( tex2D( _EmissionMap, uv_EmissionMap2 ) , color56 , float4( 1,1,1,0 ));
			o.Emission = lerpResult52.rgb;
			float2 uv_MetallicMap3 = i.uv_texcoord;
			float4 color53 = IsGammaSpace() ? float4(0.5974842,0.5974842,0.5974842,0) : float4(0.3156182,0.3156182,0.3156182,0);
			float4 lerpResult54 = lerp( tex2D( _MetallicMap, uv_MetallicMap3 ) , color53 , float4( 1,1,1,0 ));
			o.Metallic = lerpResult54.r;
			float2 uv_SmoothnessMap4 = i.uv_texcoord;
			float4 lerpResult55 = lerp( tex2D( _SmoothnessMap, uv_SmoothnessMap4 ) , color53 , float4( 1,1,1,0 ));
			o.Smoothness = lerpResult55.r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19603
Node;AmplifyShaderEditor.CommentaryNode;27;-448,-512;Inherit;False;761.6338;375.9998;Dark;5;22;14;12;13;19;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;26;-448,-128;Inherit;False;767.0411;374.9999;Dirt;5;15;9;8;17;23;;0.6132076,0.2980871,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;28;416,-512;Inherit;False;301.4;1207.067;Textures;6;5;4;3;6;2;1;;0,0.6262839,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;21;-368,-704;Inherit;False;Constant;_White;White;11;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.4433962,0.2989818,0.161742,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;19;-432,-464;Inherit;True;Property;_DarkMaskTexture;Dark Mask Texture;7;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;08475d8495d3e8c45b04efee61a5f57e;08475d8495d3e8c45b04efee61a5f57e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;13;-432,-208;Inherit;False;Constant;_DarkAmount;Dark Amount;9;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;43;-448,256;Inherit;False;767.0411;374.9999;Cuts;5;48;47;46;45;44;;0.6132076,0.2980871,0,1;0;0
Node;AmplifyShaderEditor.LerpOp;14;-80,-256;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;17;-432,-64;Inherit;True;Property;_DirtMaskTexture;Dirt Mask Texture;9;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;e7c56d4419b4d144ca3f5af3ee0c81cb;e7c56d4419b4d144ca3f5af3ee0c81cb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;8;-432,160;Inherit;False;Property;_DirtAmount;Dirt Amount;11;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-80,-464;Inherit;False;Property;_DarkColor;Dark Color;8;1;[HideInInspector];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;1;432,-464;Inherit;True;Property;_ColorMap;ColorMap;0;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;2a028b3e0aa947648add582ca16a29b1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;22;176,-464;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;9;-80,-80;Inherit;False;Property;_DirtColor;Dirt Color;10;0;Create;True;0;0;0;False;0;False;0.4433962,0.2989818,0.161742,0;0.4433962,0.2989818,0.161742,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;15;-80,112;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;45;-432,320;Inherit;True;Property;_CutsMaskTexture;Cuts Mask Texture;12;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;75ccb45ed310e81409b3f9d13dce709d;75ccb45ed310e81409b3f9d13dce709d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;44;-432,544;Inherit;False;Property;_CutsAmount;Cuts Amount;14;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;25;-448,800;Inherit;False;766.0084;371;Blood (Coming Soon);5;11;18;16;10;24;;0.6320754,0.01685535,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;31;-832,-512;Inherit;False;349;981.3334;Temp;1;20;;1,0.9010862,0,1;0;0
Node;AmplifyShaderEditor.LerpOp;23;176,-80;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;47;-80,304;Inherit;False;Property;_CutsColor;Cuts Color;13;0;Create;True;0;0;0;False;0;False;0.3333333,0.3333333,0.3333333,0;0.4433962,0.2989818,0.161742,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;48;-80,496;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;4;432,304;Inherit;True;Property;_SmoothnessMap;SmoothnessMap;4;3;[HideInInspector];[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;a9afe302bb810e741a33702651691a01;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;3;432,112;Inherit;True;Property;_MetallicMap;MetallicMap;3;3;[HideInInspector];[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;9355e2c6fbbf433459fd984f6ab2710d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;2;432,-272;Inherit;True;Property;_EmissionMap;EmissionMap;2;3;[HideInInspector];[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;9b81938a0fb0eed49998fe071f3542ce;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;56;528,-896;Inherit;False;Constant;_Black1;Black;11;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.4433962,0.2989818,0.161742,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;53;528,-704;Inherit;False;Constant;_Grey;Grey;11;0;Create;True;0;0;0;False;0;False;0.5974842,0.5974842,0.5974842,0;0.4842767,0.4842767,0.4842767,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;20;-800,-208;Inherit;True;Property;_DecalMap;DecalMap;1;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;18;-432,832;Inherit;True;Property;_BloodMaskTexture;Blood Mask Texture;15;3;[HideInInspector];[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;24;160,832;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;10;-80,832;Inherit;False;Property;_BloodColor;Blood Color;16;0;Create;True;0;0;0;False;0;False;0.4339623,0.1473742,0.1419247,0;0.4339623,0.1473742,0.1419246,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;16;-80,1024;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;46;176,304;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-416,1088;Inherit;False;Property;_BloodAmount;Blood Amount;17;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;52;768,-272;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;54;752,112;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;55;752,304;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;5;432,496;Inherit;True;Property;_OpacityMap;OpacityMap;5;3;[HideInInspector];[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;6;432,-80;Inherit;True;Property;_ReflectionMap;ReflectionMap;6;3;[HideInInspector];[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;b1b7b6d2944530e4289fb60e3e8bb392;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;57;1280,-480;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Sidekick_Shader_URP;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;21;0
WireConnection;14;1;19;0
WireConnection;14;2;13;0
WireConnection;22;0;12;0
WireConnection;22;1;1;0
WireConnection;22;2;14;0
WireConnection;15;0;21;0
WireConnection;15;1;17;0
WireConnection;15;2;8;0
WireConnection;23;0;9;0
WireConnection;23;1;22;0
WireConnection;23;2;15;0
WireConnection;48;0;21;0
WireConnection;48;1;45;0
WireConnection;48;2;44;0
WireConnection;24;0;10;0
WireConnection;24;2;16;0
WireConnection;16;0;18;0
WireConnection;16;1;21;0
WireConnection;16;2;11;0
WireConnection;46;0;47;0
WireConnection;46;1;23;0
WireConnection;46;2;48;0
WireConnection;52;0;2;0
WireConnection;52;1;56;0
WireConnection;54;0;3;0
WireConnection;54;1;53;0
WireConnection;55;0;4;0
WireConnection;55;1;53;0
WireConnection;57;0;46;0
WireConnection;57;2;52;0
WireConnection;57;3;54;0
WireConnection;57;4;55;0
ASEEND*/
//CHKSM=7D472E206FA279711B585EFB84AD1BE8E1C424A2