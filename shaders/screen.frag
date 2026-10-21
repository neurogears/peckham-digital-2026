#version 400

// Composite pass: output shaping applied outside the feedback loop so the
// effects do not accumulate across frames. Layers a mirrored echo of the
// frame, flashes brightness with bass, drifts hue over time, lifts gamma,
// and applies a soft vignette.

uniform sampler2D tex;
uniform float time;
uniform float bass, bass_att;
in vec2 texCoord;
out vec4 fragColor;

vec3 hueRotate(vec3 color, float angle)
{
  const vec3 k = vec3(0.57735);
  float c = cos(angle);
  float s = sin(angle);
  return color * c + cross(k, color) * s + k * dot(k, color) * (1.0 - c);
}

void main()
{
  vec3 color = texture(tex, texCoord).rgb;

  // echo: a magnified, mirrored copy of the frame layered on top
  vec2 euv = (texCoord - 0.5) * 0.5 + 0.5;
  euv.x = 1.0 - euv.x;
  vec3 echo = texture(tex, euv).rgb;
  color = max(color, echo * 0.4);

  float pulse = clamp(bass / max(bass_att, 1e-4), 0.0, 3.0);
  color *= clamp(1.0 + 0.25 * (pulse - 1.0), 0.5, 1.8);

  color = hueRotate(color, 0.2 * time);

  color = pow(color, vec3(0.8));

  float r = length(texCoord - 0.5);
  color *= 1.0 - 0.5 * smoothstep(0.5, 0.9, r);

  fragColor = vec4(color, 1.0);
}
