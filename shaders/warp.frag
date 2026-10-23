#version 400

// Warp pass: samples the previous frame (feedback texture) with a slowly
// breathing zoom/rotation about the center, applies decay so trails fade,
// and injects fresh energy from a moving colored blob and a spectrum bar
// strip along the bottom edge.

uniform sampler2D tex;
uniform sampler2D spectrum;
uniform float time;
uniform float bass, bass_att, mid, mid_att, treble, treble_att;
uniform float pulse_min = 0.0, 
              pulse_max = 3.0,
              pulse_offset = 1.0, 
              zoom_offset = 0.995, 
              zoom_scale = 0.02,
              twist_min = 0.0,
              twist_max = 3.0,
              twist_offset = 1.0,
              angle_offset = 0.015, 
              angle_scale = 0.004,
              prev_scale = 0.98,
              sparkle_min = 0.0,
              sparkle_max = 3.0,
              blob_offset = 0.5,
              blob_scale = 0.32,
              blob_x = 1.1,
              blob_y = 1.7,
              tint_offset = 0.5,
              tint_scale = 0.5,
              tint_x = 0,
              tint_y = 2.094,
              tint_z = 4.189,
              color_step = 0.06,
              energy_offset = 1.0,
              ring_offset = 0.31,
              ring_scale = 0.22,
              ring_a = 0.02;
in vec2 texCoord;
out vec4 fragColor;

const float eps = 1e-4;
const float twopi = 6.28318;

void main()
{
  vec2 uv = texCoord - 0.5;

  float pulse = clamp(bass / max(bass_att, eps), pulse_min, pulse_max);
  float zoom = zoom_offset + zoom_scale * (pulse -pulse_offset);

  float twist = clamp(mid / max(mid_att, eps), twist_min, twist_max);
  float angle = angle_offset + angle_scale * (twist - twist_offset);

  float s = sin(angle);
  float c = cos(angle);
  uv = mat2(c, -s, s, c) * uv / zoom;
  vec3 prev = texture(tex, uv + 0.5).rgb;

  prev *= prev_scale;

  float sparkle = clamp(treble / max(treble_att, 1e-4), sparkle_min, sparkle_max);

  vec2 blob = blob_offset + blob_scale * vec2(cos(time * blob_x), sin(time * blob_y));
  float d = length(texCoord - blob);
  vec3 tint = tint_offset + tint_scale * vec3(sin(time + tint_x), sin(time + tint_y), sin(time + tint_z));
  vec3 color = prev + tint * sparkle * smoothstep(color_step, 0.0, d);

  // spectrum bars: per-column frequency energy rising from the bottom edge,
  // softly normalized since raw magnitudes have arbitrary scale
  float ang = atan(uv.y, uv.x) / twopi + 0.5;
  float rad = length(uv);

  float energy = texture(spectrum, vec2(ang, 0)).r;
  float level = energy / (energy + energy_offset);
  float ring = step(0, rad) * step(rad, ring_offset + ring_scale * level);
  color += tint.bgr * ring_a * ring;

  fragColor = vec4(color, 1.0);
}
