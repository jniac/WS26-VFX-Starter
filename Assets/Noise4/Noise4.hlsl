#ifndef NOISE4_HLSL
#define NOISE4_HLSL

#include "noiseSimplex.cginc"

// 4D Simplex Noise with Fractal Brownian Motion (fBM)
// Parameters:
//   p          - 3D position
//   t          - time or 4th dimension
//   octaves    - number of noise layers
//   persistence - amplitude multiplier per octave, typically 0.5
//   lacunarity  - frequency multiplier per octave, typically 2.0

void Noise4_float(
  float3 p,
  float t,

  int octaves,
  float persistence,
  float lacunarity,

  out float color_out
) {
  float4 p4 = float4(p, t);

  float amplitude = 1.0;
  float frequency = 1.0;
  float total = 0.0;
  float maxAmplitude = 0.0;

  for (int i = 0; i < octaves; i++)
  {
    total += snoise(p4 * frequency) * amplitude;
    maxAmplitude += amplitude;

    amplitude *= persistence;
    frequency *= lacunarity;
  }

  // Normalize to 0..1
  color_out = total / maxAmplitude * 0.5 + 0.5;
}

void Noise4_float(
  float3 p,
  float t,
  out float color_out
) {
  Noise4_float(
    p,
    t,
    5,      // octaves
    0.5,    // persistence
    2.0,    // lacunarity
    color_out
  );
}

void Noise4_half(
  half3 p,
  half t,

  int octaves,
  float persistence,
  float lacunarity,

  out half color_out
) {
  float color_f;
  Noise4_float(
    p,
    t,
    octaves,
    persistence,
    lacunarity,
    color_f
  );
  color_out = half(color_f);
}



// Octave 1

void Noise4_Octave1_float(
  float3 p,
  float t,
  float persistence,
  float lacunarity,
  out float color_out
) {
  Noise4_float(p, t, 1, persistence, lacunarity, color_out);
}

void Noise4_Octave1_half(
  half3 p,
  half t,
  float persistence,
  float lacunarity,
  out half color_out
) {
  float color_f;
  Noise4_float(p, t, 1, persistence, lacunarity, color_f);
  color_out = half(color_f);
}



// Octave 2

void Noise4_Octave2_float(
  float3 p,
  float t,
  float persistence,
  float lacunarity,
  out float color_out
) {
  Noise4_float(p, t, 2, persistence, lacunarity, color_out);
}

void Noise4_Octave2_half(
  half3 p,
  half t,
  float persistence,
  float lacunarity,
  out half color_out
) {
  float color_f;
  Noise4_float(p, t, 2, persistence, lacunarity, color_f);
  color_out = half(color_f);
}



// Octave 3

void Noise4_Octave3_float(
  float3 p,
  float t,
  float persistence,
  float lacunarity,
  out float color_out
) {
  Noise4_float(p, t, 3, persistence, lacunarity, color_out);
}

void Noise4_Octave3_half(
  half3 p,
  half t,
  float persistence,
  float lacunarity,
  out half color_out
) {
  float color_f;
  Noise4_float(p, t, 3, persistence, lacunarity, color_f);
  color_out = half(color_f);
}



// Octave 4

void Noise4_Octave4_float(
  float3 p,
  float t,
  float persistence,
  float lacunarity,
  out float color_out
) {
  Noise4_float(p, t, 4, persistence, lacunarity, color_out);
}

void Noise4_Octave4_half(
  half3 p,
  half t,
  float persistence,
  float lacunarity,
  out half color_out
) {
  float color_f;
  Noise4_float(p, t, 4, persistence, lacunarity, color_f);
  color_out = half(color_f);
}



// Octave 5

void Noise4_Octave5_float(
  float3 p,
  float t,
  float persistence,
  float lacunarity,
  out float color_out
) {
  Noise4_float(p, t, 5, persistence, lacunarity, color_out);
}

void Noise4_Octave5_half(
  half3 p,
  half t,
  float persistence,
  float lacunarity,
  out half color_out
) {
  float color_f;
  Noise4_float(p, t, 5, persistence, lacunarity, color_f);
  color_out = half(color_f);
}

#endif