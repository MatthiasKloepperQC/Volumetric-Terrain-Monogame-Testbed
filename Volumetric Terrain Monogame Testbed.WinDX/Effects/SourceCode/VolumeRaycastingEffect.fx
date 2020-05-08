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

// The position of the camera in world space.
uniform float4 CameraPositionWorldSpace;

// The inverse of the ViewProjection matrix to recreate world space from screen space.
uniform matrix InverseViewProjectionMatrix;

// The combined world-, view- and projection-matrices to position the vertices at the correct point on screen.
uniform matrix WorldViewProjectionMatrix;

// Transforms a vertex position from model space to screen space.
float4 TransformModelSpaceToScreenSpace(in float4 position)
{
	return mul(position, WorldViewProjectionMatrix);
}


// Vertex-Shader Funktionen
SV_PositionScreenPosition VS_RaymarchFullscreen(in float4 position : POSITION0)
{
	// Semantic: SV_POSITION -> Scales normalized device coordinates to pixel coordinates.
	SV_PositionScreenPosition output;
	// The vertex shader is fed with normalized device coordinates [-1.0f, 1.0f] already in screen space.
	// No need to transform any coordinates.
	output.Position = position;

	// Any other semantic does not scale but transports the normalized device coordinates unmodified.
	output.ScreenPosition = position / position.w;

	return output;
}

SV_PositionScreenPosition VS_ColorFromCameraRayDirection(in float4 position : POSITION0)
{
	// Semantic: SV_POSITION -> Scales normalized device coordinates to pixel coordinates.
	SV_PositionScreenPosition output;
	// The vertex shader is fed with normalized device coordinates [-1.0f, 1.0f] already in screen space.
	// No need to transform any coordinates.
	output.Position = position;

	// Any other semantic does not scale but transports the normalized device coordinates unmodified.
	output.ScreenPosition = position / position.w;

	return output;
}

SV_PositionScreenPosition VS_ColorFromScreenSpacePosition(in float4 position : POSITION0)
{
	// Semantic: SV_POSITION -> Scales normalized device coordinates to pixel coordinates.
	SV_PositionScreenPosition output;
	// The vertex shader is fed with normalized device coordinates [-1.0f, 1.0f] already in screen space.
	// No need to transform any coordinates.
	output.Position = position;

	// Any other semantic does not scale but transports the normalized device coordinates unmodified.
	output.ScreenPosition = position / position.w;

	return output;
}


// Pixel-Shader Funktionen
float4 PS_ColorFromCameraRayDirection(SV_PositionScreenPosition input) : COLOR
{
	float xline = 0.5f;
	float xspread = 0.001f;
	float xmin = xline - xspread;
	float xmax = xline + xspread;
	float yline = 0.5f;
	float yspread = xspread;
	float ymin = yline - yspread;
	float ymax = yline + yspread;

	/*
	// Vertex shader passes unmodified normalized device coordinates in input.ScreenPosition [-1.0f, 1.0f].
	// Scale to [0.0f, 1.0f] to use the full range of colors.
	input.ScreenPosition = input.ScreenPosition / 2.0f + 0.5f;
	float4 output = float4(input.ScreenPosition.x, input.ScreenPosition.y, 0.0f, 1.0f);

	// Draw a white cross through the center (x == 0.5f and y == 0.5f).
	float4 crossColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
	if ((input.ScreenPosition.x >= xmin) && (input.ScreenPosition.x <= xmax)) output = crossColor;
	if ((input.ScreenPosition.y >= ymin) && (input.ScreenPosition.y <= ymax)) output = crossColor;

	// Draw a white box around the screen (x == 0.0f and y == 0.0f and x == 1.0f and y == 1.0f).
	float4 borderColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
	if (input.ScreenPosition.x <= 0.0f + xspread) output = borderColor;
	if (input.ScreenPosition.x >= 1.0f - xspread) output = borderColor;
	if (input.ScreenPosition.y <= 0.0f + xspread) output = borderColor;
	if (input.ScreenPosition.y >= 1.0f - xspread) output = borderColor;
	*/

	// Restore world space position from screen space position.
	float4 worldSpacePos = mul(input.ScreenPosition, InverseViewProjectionMatrix);
	worldSpacePos = worldSpacePos / worldSpacePos.w;

	// Calculate ray direction from camera to restored world space position.
	float4 cameraRayDirection = worldSpacePos - CameraPositionWorldSpace;
	cameraRayDirection = normalize(cameraRayDirection);

	// Scale direction to [0.0f, 1.0f] to use full range of colors.
	float4 output = cameraRayDirection / 2.0f + 0.5f;
	output.w = 1.0f;

	return output;
}

float4 PS_ColorFromScreenSpacePosition(SV_PositionScreenPosition input) : COLOR
{
	float xline = 0.5f;
	float xspread = 0.001f;
	float xmin = xline - xspread;
	float xmax = xline + xspread;
	float yline = 0.5f;
	float yspread = xspread;
	float ymin = yline - yspread;
	float ymax = yline + yspread;

	// Vertex shader passes unmodified normalized device coordinates in input.ScreenPosition [-1.0f, 1.0f].
	// Scale to [0.0f, 1.0f] to use the full range of colors.
	input.ScreenPosition = input.ScreenPosition / 2.0f + 0.5f;
	float4 output = float4(input.ScreenPosition.x, input.ScreenPosition.y, 0.0f, 1.0f);

	// Draw a white cross through the center (x == 0.5f and y == 0.5f).
	float4 crossColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
	if ((input.ScreenPosition.x >= xmin) && (input.ScreenPosition.x <= xmax)) output = crossColor;
	if ((input.ScreenPosition.y >= ymin) && (input.ScreenPosition.y <= ymax)) output = crossColor;

	// Draw a white box around the screen (x == 0.0f and y == 0.0f and x == 1.0f and y == 1.0f).
	float4 borderColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
	if (input.ScreenPosition.x <= 0.0f + xspread) output = borderColor;
	if (input.ScreenPosition.x >= 1.0f - xspread) output = borderColor;
	if (input.ScreenPosition.y <= 0.0f + xspread) output = borderColor;
	if (input.ScreenPosition.y >= 1.0f - xspread) output = borderColor;

	return output;
}


// Techniken
technique RaymarchFullScreen
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_RaymarchFullscreen();
		PixelShader = compile PS_SHADERMODEL PS_ColorFromScreenSpacePosition();
	}
};

technique ColorFromCameraRayDirection
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_ColorFromCameraRayDirection();
		PixelShader = compile PS_SHADERMODEL PS_ColorFromCameraRayDirection();
	}
};

technique ColorFromScreenSpacePosition
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_ColorFromScreenSpacePosition();
		PixelShader = compile PS_SHADERMODEL PS_ColorFromScreenSpacePosition();
	}
};