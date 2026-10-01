#version 400

// Draws the camera image with a ripple and a rainbow that both grow with loudness.
// The workflow sets two uniforms:
//   time    seconds since the workflow started
//   amount  how loud it is: 0.0 when quiet, about 0.3 when very loud
uniform sampler2D tex;
uniform float time = 0.0;
uniform float amount = 0.0;

in vec2 texCoord;
out vec4 fragColor;

void main()
{
    // 1. Ripple: slide each row of pixels sideways by a wave.
    vec2 uv = texCoord;
    uv.x += sin(uv.y * 20.0 + time * 3.0) * amount;

    // 2. Read the camera colour at the rippled position.
    vec3 color = texture(tex, uv).rgb;

    // 3. Rainbow: turn the pixel's brightness into a colour. Louder shifts the colours.
    float brightness = (color.r + color.g + color.b) / 3.0;
    vec3 rainbow = 0.5 + 0.5 * cos(6.28 * (brightness + amount * 3.0 + vec3(0.0, 0.33, 0.67)));

    // 4. Blend: the rainbow fades in between amount 0.1 and 0.3, and never covers more
    //    than 60% of the camera.
    float blend = smoothstep(0.1, 0.3, amount) * 0.6;
    color = mix(color, rainbow, blend);
    fragColor = vec4(color, 1.0);
}
