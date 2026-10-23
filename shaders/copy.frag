#version 400

// Copy pass: blits the freshly warped frame into the feedback texture so the
// next frame's warp pass can sample it. Avoids sampling and rendering the
// same texture within one pass.

uniform sampler2D tex;
in vec2 texCoord;
out vec4 fragColor;

void main()
{
  fragColor = texture(tex, texCoord);
}
