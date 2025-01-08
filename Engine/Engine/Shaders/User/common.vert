#version 400 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform vec3 color;
uniform vec3 material;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 FragPos;
out vec4 FragPos2;
out vec3 uColor;

out float objAmbient;
out float objDiffuse;
out float objSpecular;

out vec2 texCoord;

void main(void) 
{
	FragPos = vec3(vec4(aPos, 1.0) * model);
	FragPos2 = vec4(aPos, 1.0) * model;
	Normal = aNormal * mat3(transpose(inverse(model)));
	uColor = color;
	objAmbient = material.x;
	objDiffuse = material.y;
	objSpecular = material.z;
	texCoord = aTexCoord;

	gl_Position = vec4(aPos, 1.0) * model * view * projection;
}