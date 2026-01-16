#ifndef WORLD_SPACE_COLORS_HLSL
#define WORLD_SPACE_COLORS_HLSL

float wsc_easeInOut(float x, float p, float i) {
  return  x <= 0.0 ? 0.0 :
          x >= 1.0 ? 1.0 :
          x <= i ? 1.0 / pow(i, p - 1.0) * pow(x, p) :
          1.0 - 1.0 / pow(1.0 - i, p - 1.0) * pow(1.0 - x, p);
}

float wsc_smoothstep(float edge0, float edge1, float x, float p, float i) {
  float t = saturate((x - edge0) / (edge1 - edge0));
  return wsc_easeInOut(t, p, i);
}

float wsc_computeWeight(
  float3 world_pos,
  float3 spot_location,
  float3 spot_props
) {
  float range_start = spot_props.x;
  float range_end = spot_props.y;
  float intensity = spot_props.z;
  float inflection = 0.5;
  float d = distance(world_pos, spot_location);
  return wsc_smoothstep(range_end, range_start, d, 2.0, inflection) * intensity;
}

void WorldSpaceColors_float(
  float4 default_color,
  float3 world_pos,
  
  float4 spot1_color,
  float3 spot1_location,
  float4 spot1_props,

  float4 spot2_color,
  float3 spot2_location,
  float4 spot2_props,

  float4 spot3_color,
  float3 spot3_location,
  float4 spot3_props,

  float4 spot4_color,
  float3 spot4_location,
  float4 spot4_props,

  out float4 color_out
) {

  float cum_weight = 0.0;
  float4 cum_color = float4(0.0, 0.0, 0.0, 0.0);

  float w1 = wsc_computeWeight(world_pos, spot1_location, spot1_props);
  cum_color += spot1_color * w1;
  cum_weight += w1;

  float w2 = wsc_computeWeight(world_pos, spot2_location, spot2_props);
  cum_color += spot2_color * w2;
  cum_weight += w2;

  float w3 = wsc_computeWeight(world_pos, spot3_location, spot3_props);
  cum_color += spot3_color * w3;
  cum_weight += w3;

  float w4 = wsc_computeWeight(world_pos, spot4_location, spot4_props);
  cum_color += spot4_color * w4;
  cum_weight += w4;

  if (cum_weight < 1.0) {
    cum_color += default_color * (1.0 - cum_weight);
  } else {
    cum_color /= cum_weight;
  }
  color_out = cum_color;
}

#endif