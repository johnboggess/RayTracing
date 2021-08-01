-- Version
#version 430 core

-- Vertex
#include RenderShader.Version
in vec3 InPosition;
in vec2 InUV;

out vec2 FragUV;

void main()
{
    gl_Position = vec4(InPosition,1);
    FragUV = InUV;
}

-- Fragment
#include RenderShader.Version

in vec2 FragUV;
uniform sampler2D Texture;

out vec4 OutColor;

void main()
{
    OutColor = texture(Texture, FragUV);
}