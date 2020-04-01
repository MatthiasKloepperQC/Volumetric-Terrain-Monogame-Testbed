#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	//#define VS_SHADERMODEL vs_4_0_level_9_1
	//#define PS_SHADERMODEL ps_4_0_level_9_1
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif


// Strukturen für den Input der Vertex-Shader.
struct Input_PositionColor
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct Input_PositionSingleTextureCoordinates
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};


// Strukturen für den Output der Vertex-Shader bzw. den Input der Pixel-Shader.
struct VertexToPixel_PositionColor
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

struct VertexToPixel_PositionSingleTextureCoordinates
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinates : TEXCOORD0;
};


// Allgemeine Variablen
Texture2D FirstTexture;
sampler FirstTextureSampler = sampler_state
{
	MagFilter = ANISOTROPIC;
	MaxLOD = 0;
	MinFilter = ANISOTROPIC;
	//MinLOD = 0;
	MipFilter = ANISOTROPIC;
	MipLODBias = 0;
	AddressU = wrap;
	AddressV = mirror;
};
sampler TestTextureSampler : register(s0);
Texture2D texArray[16];

// Die können aus der Anwendung heraus gesetzt werden.
// Die WorldMatrix. Wird vor allem zur Transformation der Normalen benötigt.
matrix WorldMatrix;

// Die kombinierten World-, View- und Projection-Matritzen zur Positionierung der Zeichnung an der richtigen Stelle auf dem Bildschirm.
matrix WorldViewProjectionMatrix;


// Allgemeine Funktionen
// Transformiert eine Normale anhand der World-Matrix aus dem Model- in den World-Space.
// Da die Normalen als Richtungsvektor (.w 0 = 0) markiert sind sollte die Transformation
// die Transponierungs-Komponente der Matrix nicht verwenden.
float4 TransformNormalToWorldspace(in float4 normal)
{	
	return mul(normal, WorldMatrix);
}

// Überführt die Positionen eines Eckpunkts anhand der WorldViewProjectionMatrix aus dem Model-Space in den View-Space.
float4 TransformPositionToScreenSpace(in float4 position)
{
	return mul(position, WorldViewProjectionMatrix);
}


// Vertex-Shader Funktionen
VertexToPixel_PositionColor VS_TransformPosition_TransportColor(in Input_PositionColor input)
{
	VertexToPixel_PositionColor output = (VertexToPixel_PositionColor)0;

	output.Position = TransformPositionToScreenSpace(input.Position);
	output.Color = input.Color;
	return output;
}

VertexToPixel_PositionSingleTextureCoordinates VS_TransformPosition_TransportSingleTextureCoordinates(in Input_PositionSingleTextureCoordinates input)
{
	VertexToPixel_PositionSingleTextureCoordinates output = (VertexToPixel_PositionSingleTextureCoordinates)0;

	output.Position = TransformPositionToScreenSpace(input.Position);
	output.TextureCoordinates = input.TextureCoordinates;
	return output;
}


// Pixel-Shader Funktionen
float4 PS_OutputVertexColor(VertexToPixel_PositionColor input) : COLOR
{
	float4 output = (float4)0;
	output = input.Color;

	return output;
}

float4 PS_OutputSingleTextureColor(VertexToPixel_PositionSingleTextureCoordinates input) : COLOR
{
	float4 output = (float4)0;
	output = FirstTexture.Sample(FirstTextureSampler, input.TextureCoordinates);

	return output;
}


// Techniken
technique SingleTexture
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_TransformPosition_TransportSingleTextureCoordinates();
		PixelShader = compile PS_SHADERMODEL PS_OutputSingleTextureColor();
	}
};

technique VertexColors
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_TransformPosition_TransportColor();
		PixelShader = compile PS_SHADERMODEL PS_OutputVertexColor();
	}
};