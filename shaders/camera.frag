#version 400

// Draws a texture with a horizontal ripple. The workflow sets both uniforms:
//   time    seconds since the workflow started (drives the ripple along)
//   amount  ripple strength, 0.0 for none, around 0.1 for a lot
uniform sampler2D tex;
uniform float time = 0.0;
uniform float amount = 0.0;

in vec2 texCoord;
out vec4 fragColor;

void main()
{
    vec2 uv = texCoord;
    uv.x += sin(uv.y * 20.0 + time * 3.0) * amount;
    fragColor = texture(tex, uv);
}
