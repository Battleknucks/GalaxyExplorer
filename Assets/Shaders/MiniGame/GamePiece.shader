// GamePiece.shader
// Written by Dan W.
//

Shader "Dan W./GamePiece"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white"{}
		_Color("Color", Color) = (1, 1, 1, 1)
		_Pusle("Pulse", float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}

		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform fixed4 _Color;
			uniform fixed _Pulse;

			struct vertexInput
			{
				fixed4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};

			struct fragmentInput
			{
				fixed4 pos : POSITION;
				half2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			fragmentInput vert(vertexInput input)
			{
				fragmentInput output;
				output.pos = UnityObjectToClipPos(input.pos);
				output.uv = input.uv;
				output.color = lerp(_Color * 1.1, _Color * 0.9, abs(fmod(_Time.a * 1.2, 2.0) - 1.0));

				return output;
			}

			fixed4 frag(fragmentInput input) : COLOR
			{
				fixed4 color = tex2D(_MainTex, input.uv) * _Color;

				if (_Pulse == 1.0)
				{
					color *= input.color;
				}

				return color;
			}

			ENDCG
		}
	}
}