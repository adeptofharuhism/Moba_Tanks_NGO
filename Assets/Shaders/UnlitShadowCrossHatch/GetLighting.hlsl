void GetLight_float(float3 worldPosition, out float3 direction, out float3 color, out float attenuation) {
#if defined(SHADERGRAPH_PREVIEW)
	direction = half3(0.5, 0.5, 0);
	color = 1;
	attenuation = 1;
#else
	half4 shadowCoord = TransformWorldToShadowCoord(worldPosition);

	Light mainLight = GetMainLight(shadowCoord);
	direction = mainLight.direction;
	color = mainLight.color;
	attenuation = mainLight.shadowAttenuation;
#endif
}