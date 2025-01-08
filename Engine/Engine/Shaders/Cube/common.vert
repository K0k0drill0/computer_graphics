#version 400 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

layout (location = 3) in vec4 aModelRow1;
layout (location = 4) in vec4 aModelRow2;
layout (location = 5) in vec4 aModelRow3;
layout (location = 6) in vec4 aModelRow4;

layout (location = 7) in vec3 aColor;
layout (location = 8) in vec3 aMaterial;

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
	mat4 model = mat4(aModelRow1, aModelRow2, aModelRow3, aModelRow4);

	FragPos = vec3(vec4(aPos, 1.0) * model);
	FragPos2 = vec4(aPos, 1.0) * model;

	Normal = aNormal * mat3(transpose(inverse(model)));
	uColor = aColor;
	objAmbient = aMaterial.x;
	objDiffuse = aMaterial.y;
	objSpecular = aMaterial.z;
	texCoord = aTexCoord;

	gl_Position = vec4(aPos, 1.0) * model * view * projection;
}