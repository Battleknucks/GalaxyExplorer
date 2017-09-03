Shader "Dan W./Map"
{
	Properties
	{
		_MainTex("Texture 1", 2D) = "white" {}
		_Texture2("Texture 1", 2D) = "white" {}
		_Fade("Fade", float) = 0.0
	}

		SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _Texture2;
			uniform fixed _Fade;

			struct vertexInput
			{
				fixed4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};

			struct fragmentInput
			{
				fixed4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};

			fragmentInput vert (vertexInput input)
			{
				fragmentInput output;
				output.pos = UnityObjectToClipPos(input.pos);
				output.uv = input.uv;

				return output;
			}
			
			fixed4 frag (fragmentInput input) : COLOR
			{
				fixed4 col1 = tex2D(_MainTex, input.uv);
				fixed4 col2 = tex2D(_Texture2, input.uv);

				return lerp(col1, col2, _Fade);
			}

			ENDCG
		}
	}
}