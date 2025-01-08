#version 400 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

uniform vec3 color;
uniform vec3 material;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 FragPos;
out vec3 Normal;
out vec3 uColor;

out float objAmbient;
out float objDiffuse;
out float objSpecular;

void main(void) 
{
	gl_Position = vec4(aPos, 1.0) * model * view * projection;

	FragPos = vec3(vec4(aPos, 1.0) * model);
	Normal = aNormal * mat3(transpose(inverse(model)));
	
	uColor = color;
	objAmbient = material[0];
	objDiffuse = material[1];
	objSpecular = material[2];
}