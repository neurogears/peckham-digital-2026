#version 400

// Full-screen quad. Attribute locations match the TexturedQuad mesh in Bonsai.Shaders:
// position in normalized device coordinates, then texture coordinates.
layout(location = 0) in vec2 vp;
layout(location = 1) in vec2 vt;
out vec2 texCoord;

void main()
{
    texCoord = vt;
    gl_Position = vec4(vp, 0.0, 1.0);
}
