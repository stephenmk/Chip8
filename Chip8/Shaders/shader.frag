#version 330 core
out vec4 FragColor;
in vec2 TexCoord;
uniform sampler2D screenTexture;

void main()
{
    float pixel = texture(screenTexture, TexCoord).r;
    FragColor = vec4(vec3(pixel), 1.0);
}
