#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 model;

void main(void) 
{
	gl_Position = vec4(aPos, 0.0, 1.0) * model;

	texCoord = aTexCoord;
}