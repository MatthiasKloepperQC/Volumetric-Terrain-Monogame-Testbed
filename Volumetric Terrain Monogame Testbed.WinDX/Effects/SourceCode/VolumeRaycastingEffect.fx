#if OPENGL
	//#define SV_POSITION POSITION // Scheint eigentlich doch eher unnötig zu sein. Wird in BasicEffect.fx nicht mehr verwendet.
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

struct SV_PositionScreenPosition
{
	// Warning: SV_POSITION returns screen space coordinates scaled to viewport resolution in pixels (e.g. [0;1920[ for the x-value).
	float4 Position : SV_POSITION;

	// But we are looking for the coordinates in classic screenspace [0,1] range.
	// This has to be realized by an unused semantic that transport the transformed coordinates unmodified.
	float4 ScreenPosition : TEXCOORD7;
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
SV_PositionScreenPosition VS_RaymarchFullscreen(in float4 position : POSITION0)
{
	// Semantic: SV_POSITION
	SV_PositionScreenPosition output;
	output.Position = TransformPositionToScreenSpace(position);

	// 
	// Transform gives screen space in homogeneous coordinates in the range [-1.0f,1.0f]. Modify to euclidian coordinates in the range [0.0f,1.0f].
	output.ScreenPosition = output.Position / output.Position.w / 2.0f + 0.5f;

	return output;
}

SV_PositionColor VS_TransformPosition_TransportColor(in PositionColor input)
{
	SV_PositionColor output = (PositionColor)0;

	output.Position = TransformPositionToScreenSpace(input.Position);
	output.Color = input.Color;
	return output;
}


// Pixel-Shader Funktionen
float4 PS_OutputScreenposColor(SV_PositionScreenPosition input) : COLOR
{
	float4 output = float4(input.ScreenPosition.x, input.ScreenPosition.y, 0.0f, 0.0f);
	float xline = 0.5f;
	float xspread = 0.002f;
	float xmin = xline - xspread;
	float xmax = xline + xspread;
	float yline = 0.5f;
	float yspread = xspread;
	float ymin = yline - yspread;
	float ymax = yline + yspread;

	if ((input.ScreenPosition.x >= xmin) && (input.ScreenPosition.x <= xmax)) output = float4(1.0f, 1.0f, 1.0f, 0.0f);
	if ((input.ScreenPosition.y >= ymin) && (input.ScreenPosition.y <= ymax)) output = float4(1.0f, 1.0f, 1.0f, 0.0f);

	return output;
}

float4 PS_OutputVertexColor(SV_PositionColor input) : COLOR
{
	float4 output = (float4)0;
	output = input.Color;

	return output;
}


// Techniken
technique RaymarchFullScreen
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_RaymarchFullscreen();
		PixelShader = compile PS_SHADERMODEL PS_OutputScreenposColor();
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