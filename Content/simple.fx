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

cbuffer Transforms
{
	row_major float4x4 worldViewProj;
	row_major float4x4 world;
	row_major float4x4 worldInvTranspose;
	float4 lightPos;
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
	float3 L = normalize(lightPos.xyz - input.worldPos);
	float3 N = normalize(input.normal);

	float3 diffuseTex = diffuseTexture.Sample(diffuseSampler, input.uv).xyz;

	float3 diffuse = (dot(N,L)*0.5 + 0.5) * diffuseTex;

	return float4(diffuse, 1);
}

technique basic
{
	pass p0
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
}
