#version 400

uniform sampler2D mask;
uniform sampler2D foreground;

in vec2 texCoord;
out vec4 fragColor;

void main()
{
  vec2 uv = texCoord;
  float inside = smoothstep(0.35, 0.65, texture(mask, uv).r);
  vec3 fg = texture(foreground, uv).rgb;

  fragColor = vec4(pow(fg, vec3(0.8)), inside);
}