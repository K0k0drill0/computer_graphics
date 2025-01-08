#version 330 core

out vec4 FragColor;

in vec2 texCoord;

uniform sampler2D shadow;

void main()
{
    vec4 depthValue = texture(shadow, texCoord);

    if (texture(shadow, texCoord).a < 0.1) discard;

    FragColor = vec4(vec3(depthValue.r), 1);
}
