#version 400

// Waveform pass: draws the current audio window as a line strip across the
// screen center. Each vertex carries a single amplitude value; the x
// coordinate derives from gl_VertexID. The 1023 divisor must match the
// audio Buffer count minus one.

layout(location = 0) in float amp;

void main()
{
  float x = 2.0 * float(gl_VertexID) / 1023.0 - 1.0;
  float y = clamp(amp, -1.0, 1.0);
  gl_Position = vec4(x, y, 0.0, 1.0);
}
