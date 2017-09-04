Shader "Dan W./Robot"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump"{}
		_NoiseTex("Noise", 2D) = "white"{}
		_Color("Tint", Color) = (1, 1, 1, 1)
	}

		SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Cull Back
		Blend One OneMinusSrcColor

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform fixed4 _MainTex_ST;
			uniform sampler2D _NormalMap;
			uniform fixed4 _NormalMap_ST;
			uniform sampler2D _NoiseTex;
			uniform fixed4 _NoiseTex_ST;
			uniform fixed4 _Color;

			struct vertexInput
			{
				fixed4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2 : TEXCOORD1;
				half3 norm : NORMAL;
				fixed4 tan : TANGENT;
			};

			struct fragmentInput
			{
				fixed4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2 : TEXCOORD1;
				half3 norm : NORMAL;
				fixed3 tan : TANGENT;
				fixed3 bitTan : FLOAT;
			};

			fragmentInput vert(vertexInput input)
			{
				fragmentInput output;
				output.pos = UnityObjectToClipPos(input.pos);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				output.norm = UnityObjectToWorldNormal(input.norm);
				output.tan = normalize(mul(unity_ObjectToWorld, fixed4(input.tan.xyz, 0.0)).xyz);
				output.bitTan = normalize(cross(output.norm, output.tan) * input.tan.w);
				output.uv2 = TRANSFORM_TEX(input.uv2, _NoiseTex);

				return output;
			}

			fixed4 frag(fragmentInput input) : COLOR
			{
				fixed4 mainColor = tex2D(_MainTex, input.uv);
				fixed3 normalColor = UnpackNormal(tex2D(_NormalMap, input.uv));
				fixed3x3 tangentTransform = fixed3x3(input.tan, input.bitTan, normalize(input.norm));
				fixed3 normalDirection = normalize(mul(normalColor, tangentTransform));
				fixed3 normData = normalize(fixed3(1, 1, 1));
				fixed normDot = dot(normalDirection, normData);
				fixed3 NDotLWrap = max(0.0, normDot);
				mainColor.rgb *= NDotLWrap;

				half2 noiseUV = input.uv2;
				noiseUV.y += _Time / frac(sin(dot(3, 17) * 930.0));
				fixed4 noise = tex2D(_NoiseTex, noiseUV);

				mainColor += noise.g;
				mainColor.rgb *= _Color.rgb;

				return mainColor;
			}
			
			ENDCG
		}
	}
}