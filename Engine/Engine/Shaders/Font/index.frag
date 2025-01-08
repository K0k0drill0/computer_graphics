#version 330 core

out vec4 FragColor;

in vec2 texCoord;

uniform sampler2D tex;
uniform vec3 color;

void main()
{
    vec4 depthValue = texture(tex, texCoord);

    if (depthValue.a < 0.1) discard;

    FragColor = vec4(color, 1);
}
