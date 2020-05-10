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


// Strukturen für den Output der Vertex-Shader bzw. den Input der Pixel-Shader.
struct SV_PositionScreenPosition
{
	// Warning: SV_POSITION returns screen space coordinates scaled to viewport resolution in pixels (e.g. [0;1920[ for the x-value).
	float4 Position : SV_POSITION;

	// But we are looking for the coordinates in classic screenspace [0,1] range.
	// This has to be realized by an unused semantic that transport the transformed coordinates unmodified.
	float4 ScreenPosition : TEXCOORD7;
};

struct SV_PositionCameraRayDirection
{
	// Warning: SV_POSITION returns screen space coordinates scaled to viewport resolution in pixels (e.g. [0;1920[ for the x-value).
	float4 Position : SV_POSITION;

	/// Direction of the ray originating at the camera.
	float4 CameraRayDirection : TEXCOORD7;
};


// Die globalen Variablen können aus der Anwendung heraus gesetzt werden.
// The position of the camera in world space.
uniform float4 CameraPositionWorldSpace;

// The inverse of the ViewProjection matrix to recreate world space from screen space.
uniform matrix InverseViewProjectionMatrix;

// The combined world-, view- and projection-matrices to position the vertices at the correct point on screen.
// TODO: Currently unused.
uniform matrix WorldViewProjectionMatrix;


// Vertex-Shader Funktionen
/// <summary>
/// Transforms the supplied screen space coordinates into the direction of a ray that originates at the camera and passes through the given screen space coordinates.
/// </summary>
/// <param name="position">Vertex positions as normalized device coordinates (NDC) already in screen space.</param>
/// <returns>Struct containing the SV_Position (screen space scaled to pixels) and the ray direction.</returns>
SV_PositionCameraRayDirection VS_RayDirectionFromPreTransformedScreenSpaceCoordinates(in float4 position : POSITION0)
{
	SV_PositionCameraRayDirection output;

	// Semantic: SV_POSITION -> Scales normalized device coordinates to pixel coordinates.
	// The pixel shader needs this information. Passing is mandatory.
	// This vertex shader shader expects vertices as normalized device coordinates [-1.0f, 1.0f] already in screen space.
	// No need to transform anything. Just pass to pixel shader unmodified.
	output.Position = position;

	// Any other semantic does not scale but transports coordinates unmodified.
	// Restore posible world space position from screen space position.
	float4 worldSpacePosition = mul(position, InverseViewProjectionMatrix);
	worldSpacePosition = worldSpacePosition / worldSpacePosition.w;

	// Calculate dirction of ray from camera position to the restored world space position.
	output.CameraRayDirection = normalize(worldSpacePosition - CameraPositionWorldSpace);

	// World space restoration and ray direction determination are calculated per vertex and than interpolated per pixel.
	// This should definitely run faster (calculated for 6 vertices + some linear interpolation) compared to per pixel.
	// Direct comparison with per pixel calculation shows some small differences. Might be necessary to move to the pixel shader
	// again if precision is not high enough.
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
	// Restore world space position from screen space position.
	float4 worldSpacePos = mul(input.ScreenPosition, InverseViewProjectionMatrix);
	worldSpacePos = worldSpacePos / worldSpacePos.w;

	// Calculate ray direction from camera to restored world space position.
	float4 cameraRayDirection = worldSpacePos - CameraPositionWorldSpace;
	cameraRayDirection = normalize(cameraRayDirection);

	// Scale direction from [-1.0, 1.0f] to [0.0f, 1.0f] to use full range of colors.
	float4 output = cameraRayDirection / 2.0f + 0.5f;
	output.w = 1.0f;

	return output;
}

/// <summary>
/// Transforms the supplied ray direction into a pixel color.
/// </summary>
/// <param name="input">Struct containing the SV_Position (screen space scaled to pixels) and the ray direction.</param>
/// <returns>The color of the pixel.</returns>
float4 PS_ColorFromVSCameraRayDirection(SV_PositionCameraRayDirection input) : COLOR
{
	// Scale direction from [-1.0, 1.0f] to [0.0f, 1.0f] to use full range of colors.
	float4 output = input.CameraRayDirection / 2.0f + 0.5f;
	output.w = 1.0f;

	return output;
}

float4 PS_ColorFromScreenSpacePosition(SV_PositionScreenPosition input) : COLOR
{
	// Vertex shader passes unmodified normalized device coordinates in input.ScreenPosition [-1.0f, 1.0f].
	// Scale to [0.0f, 1.0f] to use the full range of colors.
	input.ScreenPosition = input.ScreenPosition / 2.0f + 0.5f;
	float4 output = float4(input.ScreenPosition.x, input.ScreenPosition.y, 0.0f, 1.0f);

	return output;
}

/// <summary>
/// Raymarches into a simple function.
/// </summary>
/// <param name="input">Struct containing the SV_Position (screen space scaled to pixels) and the ray direction.</param>
/// <returns>The color of the pixel.</returns>
float4 PS_RaymarchFunction(SV_PositionCameraRayDirection input) : COLOR
{
	float accumulatedDensity = 0.0;
	float stepSize = 1.0;
	int stepsTaken = 0;
	int maxSteps = 15;
	float4 rayPosition = CameraPositionWorldSpace;
	float4 output;
	float4 sphereTranslation = float4(0.0, 0.0, -3.0, 0.0);

	while ((stepsTaken <= maxSteps) && (accumulatedDensity < 1.0))
	{
		rayPosition += stepSize * input.CameraRayDirection;
		rayPosition = rayPosition / rayPosition.w;
		if (distance(rayPosition - sphereTranslation, CameraPositionWorldSpace) <= 1.0) accumulatedDensity += 1;
		stepsTaken++;
	}

	if (stepsTaken > maxSteps) output = float4(1.0, 0.0, 0.0, 1.0);
	else output = float4(stepsTaken / (float)maxSteps, stepsTaken / (float)maxSteps, stepsTaken / (float)maxSteps, 1.0);
	//if (CameraPositionWorldSpace.z == 0.0) output = float4(1.0, 1.0, 0.0, 1.0);

	return output;
}


// Techniken
technique FullVolumeRaycasting
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_RayDirectionFromPreTransformedScreenSpaceCoordinates();
		//PixelShader = compile PS_SHADERMODEL PS_ColorFromVSCameraRayDirection();
		PixelShader = compile PS_SHADERMODEL PS_RaymarchFunction();
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