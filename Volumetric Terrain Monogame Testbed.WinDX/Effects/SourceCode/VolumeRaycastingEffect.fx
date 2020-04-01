#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif


// Strukturen für den Input der Vertex-Shader.
struct VS_Input_BoundingBox
{
	float4 Position : POSITION0;
};


// Strukturen für den Output der Vertex-Shader bzw. den Input der Pixel-Shader.
struct VS_Output_BoundingBox
{
	// Vorsicht: SV_POSITION gibt die ScreenSpace Koordinaten in Pixel an (z.B. [0;1920[ fürden x-Wert).
	float4 Position : SV_POSITION;

	// Zusätzlich soll die Position im World Space transportiert werden.
	// Dafür wird eine Semantic des Typs float4 benötigt.
	float4 PositionWorld : COLOR;
};

// Die aktuelle Position der Camera im WorldSpace.
extern uniform float4 CameraPositionWorldSpace;

// Vertex-Shader Funktionen
VS_Output VS_ForwardPosition(in VS_Input_BoundingBox input)
{
	// TODO: Bounding Box muss transformiert werden per WorldViewProjection MAxtrix.
	VS_Output output;
	output.Position = input.Position;
	output.PositionScreen = input.Position;

	return output;
}

// Pixel-Shader Funktionen
float4 PS_OutputColor(in VS_Output input) : COLOR
{
	// Die übergebene ScreenSpace-Position in den WorldSpace transformieren.
	float4 positionWorldSpace = mul(input.PositionScreen, InvertedViewProjectionMatrix);
	positionWorldSpace = positionWorldSpace / positionWorldSpace.w;

	// Den Richtungsvektor von der Kamera-Position zur berechneten WorldSpace-Position.
	float4 rayDirection = normalize(positionWorldSpace - CameraPositionWorldSpace);

	float4 outputColor = 0;

	float distanceToSurface = GetShortestDistanceToSurface(CameraPositionWorldSpace, rayDirection);

	DirectionalLight sun;
	sun.Direction = normalize(float3(-0.8, 0.4, -0.3));
	sun.Direction = normalize(float3(0.2, 0.75, -1.0));
	sun.Intensity = 1.25;
	sun.Color = float3(1.0, 0.8, 0.6);

	if (distanceToSurface >= MaximumDistance)
	{
		// sky
		float3 col = float3(0.2, 0.5, 0.85)*1.1 - rayDirection.y*rayDirection.y*0.5;
		col = lerp(col, 0.85*float3(0.7, 0.75, 0.85), pow(1.0 - max(rayDirection.y, 0.0), 4.0));

		// sun
		float3 light1 = sun.Direction; // Lichtrichtung? Kann eigentlich nicht sein. Eher die Richtung in der die Sonne sich befindet.
		float sundot = clamp(dot((float3)rayDirection, light1), 0.0, 1.0);
		col += 0.25*float3(1.0, 0.7, 0.4)*pow(sundot, 5.0);
		col += 0.25*float3(1.0, 0.8, 0.6)*pow(sundot, 64.0);  // Sundot <= 1, daher nimmt die Farbintensität mit steigendem Exponenten ab und wirkt lokal begrenzter.
		col += 0.2*float3(1.0, 0.8, 0.6)*pow(sundot, 512.0);

		return float4(col.x, col.y, col.z, 1.0);
	}
	if (distanceToSurface == MaximumDistance)
	{
		// Keine Oberfläche in Sichtweite.
		outputColor = float4(0.0, 0.5333333, 1.0, 1.0);
		return float4(0.0, 0.5333333, 1.0, 1.0);
	}
	if (distanceToSurface == MaximumDistance * 2)
	{
		// Maximale Anzahl Schritte aufgebraucht.
		// Für den Moment rot zurückgeben.
		outputColor = float4(1.0, 0.0, 0.0, 1.0);
		return float4(1.0, 0.0, 0.0, 1.0);
	}
	if (distanceToSurface == MaximumDistance * 3)
	{
		// Maximale Anzahl Schritte aufgebraucht.
		// Für den Moment rot zurückgeben.
		outputColor = float4(1.0, 0.0, 0.0, 1.0);
		return float4(0.0, 1.0, 0.0, 1.0);
	}

	// Oberfläche gefunden.
	// Den Kontaktpunkt mit der Oberfläche bestimmen.
	float3 pointOfContactWorldSpace = (float3)(CameraPositionWorldSpace + distanceToSurface * rayDirection);
	float3 pointOfContactNormalWorldSpace = estimateNormal(CameraPositionWorldSpace, pointOfContactWorldSpace);

	AmbientLight ambient;
	ambient.Color = float3(1.0, 1.0, 1.0);
	ambient.Intensity = 0.0;

	DirectionalLight sky;
	sky.Color = float3(0.2, 0.5, 0.85);
	sky.Direction = float3(0.0, 1.0, 0.0);
	sky.Intensity = 0.2;

	DirectionalLight globalIllumination;
	globalIllumination.Color = sun.Color;
	globalIllumination.Direction = float3(-sun.Direction.x, 0.0, -sun.Direction.z);
	globalIllumination.Intensity = 0.3;

	float3 colorWithIllumination = Illuminate(pointOfContactWorldSpace, pointOfContactNormalWorldSpace, ambient, sun, sky, globalIllumination);

	return float4(colorWithIllumination.x, colorWithIllumination.y, colorWithIllumination.z, 1.0);
}


// Techniken
technique BoundingBox
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL VS_ForwardPosition();
		PixelShader = compile PS_SHADERMODEL PS_OutputColor();
	}
};