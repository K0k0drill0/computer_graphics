#version 400 core

#define NR_POINT_LIGHTS 20
#define NR_SPOT_LIGHTS 20
#define NR_DIRECTION_LIGHTS 20
#define SHININESS 32.0
#define BIAS 0.005

struct DirectionLight 
{
	vec3 direction;
	vec3 color;
	mat4 view;
	mat4 projection;
	sampler2D shadowMap; 
};

uniform int nrDL;
uniform DirectionLight directionLights[NR_DIRECTION_LIGHTS];

struct PointLight 
{
	vec3 position;
	vec3 color;

	float constant;
	float linear;
	float quadratic;
};

uniform int nrPL;
uniform PointLight pointLights[NR_POINT_LIGHTS];


struct SpotLight 
{
	vec3 position;
	vec3 color;

	vec3 direction;
	float cutOff;
	float outerCutOff;

	float constant;
	float linear;
	float quadratic;
};

uniform int nrSL;
uniform SpotLight spotLights[NR_SPOT_LIGHTS];


out vec4 FragColor;

uniform sampler2D texture0;
uniform bool texIsAdded;
uniform vec3 viewPos;

in vec2 texCoord;
in vec3 Normal;
in vec3 FragPos;
in vec4 FragPos2;

in vec3 uColor;
in float objAmbient;
in float objDiffuse;
in float objSpecular;

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcDirectionLight(DirectionLight light, vec3 normal, vec3 viewDir);

void main() 
{
	vec3 norm = normalize(Normal);
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 pl_summary = vec3(0.0), sl_summary = vec3(0.0), dl_summary = vec3(0.0);
	
	if (nrPL > 0) 
	{
		pl_summary = CalcPointLight(pointLights[0], norm, FragPos, viewDir);

		for(int i = 1; i < NR_POINT_LIGHTS && i < nrPL; i++)
			pl_summary += CalcPointLight(pointLights[i], norm, FragPos, viewDir);
	}

	if (nrSL > 0) 
	{
		sl_summary = CalcSpotLight(spotLights[0], norm, FragPos, viewDir);

		for(int i = 1; i < NR_SPOT_LIGHTS && i < nrSL; i++)
			sl_summary += CalcSpotLight(spotLights[i], norm, FragPos, viewDir);
	}

	if (nrDL > 0) 
	{
		dl_summary = CalcDirectionLight(directionLights[0], norm, viewDir);

		for(int i = 1; i < NR_DIRECTION_LIGHTS && i < nrDL; i++)
			dl_summary += CalcDirectionLight(directionLights[i], norm, viewDir);
	}

	vec3 res = uColor;
	vec3 LightSummary = vec3(0.0);

	if (nrPL == 0 && nrSL == 0 && nrDL == 0)
		res *= 0.2;
	else 
	{
		if (nrPL > 0) LightSummary += pl_summary;
		if (nrSL > 0) LightSummary += sl_summary;
		if (nrDL > 0) LightSummary += dl_summary;
	}
	
		if (!texIsAdded)
			FragColor = vec4(res * LightSummary, 1.0);
		else 
			FragColor = texture(texture0, texCoord) * vec4(LightSummary, 1.0);
}

vec3 CalcDirectionLight(DirectionLight light, vec3 normal, vec3 viewDir)
{
	vec3 ambient  = light.color * objAmbient;

	vec4 fragPosLightSpace = FragPos2 * light.view * light.projection;
    fragPosLightSpace.xyz /= fragPosLightSpace.w;

	float shadowMapCoordX = clamp(fragPosLightSpace.x * 0.5 + 0.5, 0.0, 1.0);
	float shadowMapCoordY = clamp(fragPosLightSpace.y * 0.5 + 0.5, 0.0, 1.0);
    float shadowMapCoordZ = fragPosLightSpace.z * 0.5 + 0.5;

    float closestDepth = texture(light.shadowMap, vec2(shadowMapCoordX, shadowMapCoordY)).r;

	vec3  lightDir   = normalize(-light.direction);
    float diff       = max(dot(normal, lightDir), 0.0);
	vec3  reflectDir = reflect(-lightDir, normal);
    float spec       = pow(max(dot(viewDir, reflectDir), 0.0), SHININESS);
    
    vec3 diffuse  = light.color * objDiffuse  * diff;
    vec3 specular = light.color * objSpecular * spec;

	if (closestDepth < shadowMapCoordZ - BIAS) 
		return min((ambient + light.color * objDiffuse * 0.2), (ambient + diffuse + specular));

    return (ambient + diffuse + specular);
} 

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
	vec3  lightDir   = normalize(light.position - fragPos);
    float diff       = max(dot(normal, lightDir), 0.0);
	vec3  reflectDir = reflect(-lightDir, normal);
    float spec       = pow(max(dot(viewDir, reflectDir), 0.0), SHININESS);
    
	float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
	vec3 ambient  = light.color * objAmbient  * attenuation;
    vec3 diffuse  = light.color * objDiffuse  * diff * attenuation;
    vec3 specular = light.color * objSpecular * spec * attenuation;

    return (ambient + diffuse + specular);
} 

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3  lightDir   = normalize(light.position - fragPos);
    float diff       = max(dot(normal, lightDir), 0.0);
	vec3  reflectDir = reflect(-lightDir, normal);
    float spec       = pow(max(dot(viewDir, reflectDir), 0.0), SHININESS);
    
	float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
	float theta     = dot(lightDir, normalize(-light.direction));
	float epsilon   = light.cutOff - light.outerCutOff;
	float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

	vec3 ambient  = light.color * objAmbient  * attenuation;
    vec3 diffuse  = light.color * objDiffuse  * diff * attenuation * intensity;
    vec3 specular = light.color * objSpecular * spec * attenuation * intensity;

    return (ambient + diffuse + specular);
} 