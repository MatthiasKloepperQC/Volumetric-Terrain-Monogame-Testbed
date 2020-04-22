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
struct PositionColor
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};


// Strukturen für den Output der Vertex-Shader bzw. den Input der Pixel-Shader.
struct SV_PositionColor
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

// Die globalen Variablen können aus der Anwendung heraus gesetzt werden.

// Die kombinierten World-, View- und Projection-Matritzen zur Positionierung der Zeichnung an der richtigen Stelle auf dem Bildschirm.
matrix WorldViewProjectionMatrix;

// Überführt die Positionen eines Eckpunkts anhand der WorldViewProjectionMatrix aus dem Model-Space in den View-Space.
float4 TransformPositionToScreenSpace(in float4 position)
{
	return mul(position, WorldViewProjectionMatrix);
}


// Vertex-Shader Funktionen
SV_PositionColor VS_TransformPosition_TransportColor(in PositionColor input)
{
	SV_PositionColor output = (PositionColor)0;

	output.Position = TransformPositionToScreenSpace(input.Position);
	output.Color = input.Color;
	return output;
}


// Pixel-Shader Funktionen
float4 PS_OutputVertexColor(SV_PositionColor input) : COLOR
{
	float4 output = (float4)0;
	output = input.Color;

	return output;
}


// Techniken
technique VertexColors
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_TransformPosition_TransportColor();
		PixelShader = compile PS_SHADERMODEL PS_OutputVertexColor();
	}
};