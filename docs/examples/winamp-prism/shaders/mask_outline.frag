#version 400

uniform sampler2D mask;
uniform float beat;

in vec2 texCoord;
out vec4 fragColor;

const vec3 backgroundColor = vec3(0.0);
const float edgeGain = 3.0;
const vec3 edgeColor = vec3(0.4, 0.9, 1.0);

void main()
{
  vec2 uv = texCoord;
  float m = texture(mask, uv).r;

  float inside = smoothstep(0.35, 0.65, m);

  // boundary strength from the mask gradient, sampled one texel either side
  vec2 texel = 1.0 / vec2(textureSize(mask, 0));
  float ml = texture(mask, uv - vec2(texel.x, 0.0)).r;
  float mr = texture(mask, uv + vec2(texel.x, 0.0)).r;
  float md = texture(mask, uv - vec2(0.0, texel.y)).r;
  float mu = texture(mask, uv + vec2(0.0, texel.y)).r;
  float edge = clamp(length(vec2(mr - ml, mu - md)) * edgeGain, 0.0, 1.0);

  // opaque outside the body, transparent inside, so the preset shows
  // only through the silhouette
  vec3 color = mix(backgroundColor, edgeColor * (1.0 + beat), edge);
  float alpha = max(1.0 - inside, edge);

  fragColor = vec4(color, alpha);
}
