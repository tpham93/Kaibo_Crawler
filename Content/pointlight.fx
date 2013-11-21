Texture2D diffuseTexture;		// Do not change this name: Model.cs is using it
SamplerState diffuseSampler;


struct VS_IN
{
	float3 pos : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
};

struct PointLight {
	float3 pos;
	float att1;				// -1 / (outer_range-inner_range)
	float3 color;
	float att2;				// 1 + inner_range / (outer_range-inner_range)
	// The inner_range is the distance at which the light starts to fade out.
	// The outer_range is the distance where the light gets 0.
};

cbuffer Transforms
{
	row_major float4x4 worldViewProj;
	row_major float4x4 world;
	row_major float4x4 worldInvTranspose;
	float3 cameraPos;
}

cbuffer Lights
{
	float3 lightDir;		// Global direction light
	float specularPower;	// Blinn Phong specular exponent (Material)
	float3 dirLightColor;
	uint numPointLights;
	PointLight lights[8];
}

float smootherstep(float x) { return (x*x*x)*(x*(x*6.0f - 15.0f) + 10.0f); }

// This could be any BRDF
float3 lightning( float3 _vN, float3 _vL, float3 _vV, float3 lightColor, float3 materialColor )
{
	// Gamma-corrected two-sided N dot L light
	float fNdotL = dot( _vN, _vL );

	float fDiffuse = abs( fNdotL * 0.7 + 0.3);
	fDiffuse = pow(fDiffuse, 1.5);

	// Normalized Blinn-Phong specular
	float3 vH = normalize(_vV + _vL);
  	float fSpecular = pow(saturate(dot(vH, _vN)), specularPower);
	float fNormalization = 0.159154943 * specularPower + 0.318309886;

	return lightColor * materialColor * fDiffuse + lightColor * (fSpecular * fNormalization);
}

PS_IN VS( VS_IN input )
{
	PS_IN output;
	output.pos = mul(float4(input.pos,1), worldViewProj);
	output.normal = mul(input.normal, (float3x3)worldInvTranspose);
	output.worldPos =  mul(float4(input.pos, 1), world).xyz;
	output.uv = input.uv;
	
	return output;
}

float4 PS( PS_IN input ) : SV_TARGET
{
	float3 N = normalize(input.normal);
	float3 V = normalize(cameraPos - input.worldPos);

	float3 diffuseTex = diffuseTexture.Sample(diffuseSampler, input.uv).xyz;
	
	// Global direction light
	float3 color = lightning(N, lightDir, V, dirLightColor, diffuseTex);
	
	// All point lights
	for( uint i=0; i<numPointLights; ++i )
	{
		float3 vLightDir = lights[i].pos - input.worldPos;
		float distance = length(vLightDir);
		float attentuation = smootherstep( saturate(lights[i].att1 * distance + lights[i].att2) );

		if( attentuation > 0.0f)
		{
			vLightDir /= distance;
			color += lightning( N, vLightDir, V, lights[i].color, diffuseTex ) * attentuation;
		}
	}

	return float4(color, 1);
}

technique basic
{
	pass p0
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
}
