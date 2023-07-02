Shader "Custom/DisplayShader" {
	SubShader{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 200
		Cull off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert alpha:fade
		#pragma target 3.0

		struct Input {
			float4 vertColor;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertColor = v.color;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.vertColor.rgb;
			o.Alpha = IN.vertColor.a;
		}
		ENDCG
	}
		FallBack "Diffuse"
}
