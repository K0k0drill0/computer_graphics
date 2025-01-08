#include <GL/glew.h>
#include <GL/gl.h>
#include <GL/glu.h>
#include <SFML/Window.hpp>
#include <SFML/OpenGL.hpp>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>
#include <iostream>
#include <vector>

#define SCREEN_WIDTH 800
#define SCREEN_HEIGHT 600

// Вершинный шейдер
const char* vertexShaderSource = R"(
#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;

out vec3 FragPos;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    FragPos = vec3(model * vec4(aPos, 1.0));
    Normal = mat3(transpose(inverse(model))) * aNormal;
    gl_Position = projection * view * vec4(FragPos, 1.0);
}
)";

// Фрагментный шейдер
const char* fragmentShaderSource = R"(
#version 330 core

struct DirectionalLight {
    vec3 direction;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct PointLight {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float constant;
    float linear;
    float quadratic;
};

in vec3 FragPos;
in vec3 Normal;
out vec4 FragColor;

uniform vec3 viewPos;
uniform DirectionalLight dirLight;
uniform PointLight pointLight;
uniform bool useDirLight;
uniform bool usePointLight;

void main()
{
    vec3 result = vec3(0.0);

    // Нормали
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    if (useDirLight)
    {
        // Направленный свет
        vec3 lightDir = normalize(-dirLight.direction);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);

        vec3 ambient = dirLight.ambient;
        vec3 diffuse = dirLight.diffuse * diff;
        vec3 specular = dirLight.specular * spec;

        result += ambient + diffuse + specular;
    }

    if (usePointLight)
    {
        // Точечный свет
        vec3 lightDir = normalize(pointLight.position - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);

        float distance = length(pointLight.position - FragPos);
        float attenuation = 1.0 / (pointLight.constant + pointLight.linear * distance + pointLight.quadratic * (distance * distance));

        vec3 ambient = pointLight.ambient * attenuation;
        vec3 diffuse = pointLight.diffuse * diff * attenuation;
        vec3 specular = pointLight.specular * spec * attenuation;

        result += ambient + diffuse + specular;
    }

    FragColor = vec4(result, 1.0);
}
)";

bool useDirLight = true;
bool usePointLight = true;

// Сфера
void generateSphere(std::vector<float>& vertices, std::vector<unsigned int>& indices, float radius, unsigned int sectorCount, unsigned int stackCount)
{
    float x, y, z, xy;
    float nx, ny, nz, lengthInv = 1.0f / radius;
    float sectorStep = 2 * M_PI / sectorCount;
    float stackStep = M_PI / stackCount;
    float sectorAngle, stackAngle;

    for (unsigned int i = 0; i <= stackCount; ++i)
    {
        stackAngle = M_PI / 2 - i * stackStep;
        xy = radius * cosf(stackAngle);
        z = radius * sinf(stackAngle);

        for (unsigned int j = 0; j <= sectorCount; ++j)
        {
            sectorAngle = j * sectorStep;

            x = xy * cosf(sectorAngle);
            y = xy * sinf(sectorAngle);

            vertices.push_back(x);
            vertices.push_back(y);
            vertices.push_back(z);

            nx = x * lengthInv;
            ny = y * lengthInv;
            nz = z * lengthInv;
            vertices.push_back(nx);
            vertices.push_back(ny);
            vertices.push_back(nz);
        }
    }

    unsigned int k1, k2;
    for (unsigned int i = 0; i < stackCount; ++i)
    {
        k1 = i * (sectorCount + 1);
        k2 = k1 + sectorCount + 1;

        for (unsigned int j = 0; j < sectorCount; ++j, ++k1, ++k2)
        {
            if (i != 0)
            {
                indices.push_back(k1);
                indices.push_back(k2);
                indices.push_back(k1 + 1);
            }

            if (i != (stackCount - 1))
            {
                indices.push_back(k1 + 1);
                indices.push_back(k2);
                indices.push_back(k2 + 1);
            }
        }
    }
}

// Пирамида
float pyramidVertices[] = {
    // Основание
	-0.5f, 0.0f, -0.5f,		0.0f, -1.0f, 0.0f,
	0.5f, 0.0f, -0.5f,		0.0f, -1.0f, 0.0f,
	0.5f, 0.0f, 0.5f,		0.0f, -1.0f, 0.0f,
	0.5f, 0.0f, 0.5f,		0.0f, -1.0f, 0.0f,
	-0.5f, 0.0f, 0.5f,		0.0f, -1.0f, 0.0f,
	-0.5f, 0.0f, -0.5f,		0.0f, -1.0f, 0.0f,

	//боковые грани
	-0.5f, 0.0f, -0.5f,		-0.8944f, 0.4472f, 0.0f,
	0.0f, 1.0f, 0.0f,		-0.8944f, 0.4472f, 0.0f,
	-0.5f, 0.0f, 0.5f,		-0.8944f, 0.4472f, 0.0f,

	0.5f, 0.0f, -0.5f,		0.0f, 0.4472f, -0.8944f,
	0.0f, 1.0f, 0.0f,		0.0f, 0.4472f, -0.8944f,
	-0.5f, 0.0f, -0.5f,		0.0f, 0.4472f, -0.8944f,

	0.5f, 0.0f, 0.5f,		0.8944f, 0.4472f, 0.0f,
	0.0f, 1.0f, 0.0f,		0.8944f, 0.4472f, 0.0f,
	0.5f, 0.0f, -0.5f,		0.8944f, 0.4472f, 0.0f,

	-0.5f, 0.0f, 0.5f,		0.0f, 0.4472f, 0.8944f,
	0.0f, 1.0f, 0.0f,		0.0f, 0.4472f, 0.8944f,
	0.5f, 0.0f, 0.5f,		0.0f, 0.4472f, 0.8944f,
};

int main() {
	sf::Window window(sf::VideoMode(SCREEN_WIDTH, SCREEN_HEIGHT), "SFML OpenGL Lighting", sf::Style::Default, sf::ContextSettings(24));
	window.setVerticalSyncEnabled(true);
	window.setMouseCursorGrabbed(true);
	window.setMouseCursorVisible(false);

	glewExperimental = GL_TRUE;
	glewInit();

	glEnable(GL_DEPTH_TEST);

	GLuint vertexShader = glCreateShader(GL_VERTEX_SHADER);
	GLuint fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
	GLuint shaderProgram = glCreateProgram();

	glShaderSource(vertexShader, 1, & vertexShaderSource, NULL);
	glCompileShader(vertexShader);

	GLint success;
	glGetShaderiv(vertexShader, GL_COMPILE_STATUS, & success);
	if(!success) {
		char infoLog[512];
		glGetShaderInfoLog(vertexShader, 512, NULL, infoLog);
		std::cout << "Vertex Shader Compilation Failed\n" << infoLog << std::endl;
	}

	glShaderSource(fragmentShader, 1, & fragmentShaderSource, NULL);
	glCompileShader(fragmentShader);
	glGetShaderiv(fragmentShader, GL_COMPILE_STATUS, & success);
	if(!success) {
		char infoLog[512];
		glGetShaderInfoLog(fragmentShader, 512, NULL, infoLog);
		std::cout << "Fragment Shader Compilation Failed\n" << infoLog << std::endl;
	}
    
	glAttachShader(shaderProgram, vertexShader);
	glAttachShader(shaderProgram, fragmentShader);
	glLinkProgram(shaderProgram);
	glGetProgramiv(shaderProgram, GL_LINK_STATUS, & success);
	if(!success) {
		char infoLog[512];
		glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
		std::cout << "Shader Program Linking Failed\n" << infoLog << std::endl;
	}
	glDeleteShader(vertexShader);
	glDeleteShader(fragmentShader);
	//куб
	float cubeVertices[] = {
		// точка
        -0.5f, -0.5f, -0.5f,	0.0f, 0.0f, -1.0f,
        0.5f, 0.5f, -0.5f,		0.0f, 0.0f, -1.0f,
        0.5f, -0.5f, -0.5f,		0.0f, 0.0f, -1.0f,
        0.5f, 0.5f, -0.5f,		0.0f, 0.0f, -1.0f,
        -0.5f, -0.5f, -0.5f,	0.0f, 0.0f, -1.0f,
        -0.5f, 0.5f, -0.5f,		0.0f, 0.0f, -1.0f,

		-0.5f, -0.5f, 0.5f,		0.0f, 0.0f, 1.0f,
		0.5f, -0.5f, 0.5f,		0.0f, 0.0f, 1.0f,
		0.5f, 0.5f, 0.5f,		0.0f, 0.0f, 1.0f,
		0.5f, 0.5f, 0.5f,		0.0f, 0.0f, 1.0f,
		-0.5f, 0.5f, 0.5f,		0.0f, 0.0f, 1.0f,
		-0.5f, -0.5f, 0.5f,		0.0f, 0.0f, 1.0f,

		-0.5f, 0.5f, 0.5f, 		-1.0f, 0.0f, 0.0f,
		-0.5f, 0.5f, -0.5f, 	-1.0f, 0.0f, 0.0f,
		-0.5f, -0.5f, -0.5f, 	-1.0f, 0.0f, 0.0f,
		-0.5f, -0.5f, -0.5f, 	-1.0f, 0.0f, 0.0f,
		-0.5f, -0.5f, 0.5f, 	-1.0f, 0.0f, 0.0f,
		-0.5f, 0.5f, 0.5f, 		-1.0f, 0.0f, 0.0f,

		0.5f, 0.5f, 0.5f, 		1.0f, 0.0f, 0.0f,
		0.5f, -0.5f, -0.5f, 	1.0f, 0.0f, 0.0f,
		0.5f, 0.5f, -0.5f, 		1.0f, 0.0f, 0.0f,
		0.5f, -0.5f, -0.5f, 	1.0f, 0.0f, 0.0f,
		0.5f, 0.5f, 0.5f, 		1.0f, 0.0f, 0.0f,
		0.5f, -0.5f, 0.5f, 		1.0f, 0.0f, 0.0f,

		-0.5f, -0.5f, -0.5f,	0.0f, -1.0f, 0.0f,
		0.5f, -0.5f, -0.5f,		0.0f, -1.0f, 0.0f,
		0.5f, -0.5f, 0.5f,		0.0f, -1.0f, 0.0f,
		0.5f, -0.5f, 0.5f,		0.0f, -1.0f, 0.0f,
		-0.5f, -0.5f, 0.5f,		0.0f, -1.0f, 0.0f,
		-0.5f, -0.5f, -0.5f,	0.0f, -1.0f, 0.0f,

		-0.5f, 0.5f, -0.5f,		0.0f, 1.0f, 0.0f,
		0.5f, 0.5f, 0.5f,		0.0f, 1.0f, 0.0f,
		0.5f, 0.5f, -0.5f,		0.0f, 1.0f, 0.0f,
		0.5f, 0.5f, 0.5f,		0.0f, 1.0f, 0.0f,
		-0.5f, 0.5f, 0.5f,		0.0f, 1.0f, 0.0f,
		-0.5f, 0.5f, -0.5f,		0.0f, 1.0f, 0.0f,
	};
	GLuint cubeVBO, cubeVAO;
	glGenVertexArrays(1, & cubeVAO);
	glGenBuffers(1, & cubeVBO);

	glBindVertexArray(cubeVAO);

	glBindBuffer(GL_ARRAY_BUFFER, cubeVBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(cubeVertices), cubeVertices, GL_STATIC_DRAW);

	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void * ) 0);
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float),
		(void * )(3 * sizeof(float)));
	glEnableVertexAttribArray(1);
	glBindVertexArray(0);

	GLuint pyramidVBO, pyramidVAO;

	glGenVertexArrays(1, & pyramidVAO);
	glGenBuffers(1, & pyramidVBO);
	glBindVertexArray(pyramidVAO);

	glBindBuffer(GL_ARRAY_BUFFER, pyramidVBO);

	glBufferData(GL_ARRAY_BUFFER, sizeof(pyramidVertices), pyramidVertices, GL_STATIC_DRAW);
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void * ) 0);
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float),
		(void * )(3 * sizeof(float)));
	glEnableVertexAttribArray(1);
	glBindVertexArray(0);

	std::vector<float> sphereVertices;
	std::vector<unsigned int> sphereIndices;
	generateSphere(sphereVertices, sphereIndices, 0.5f, 36, 18);

	GLuint sphereVBO, sphereVAO, sphereEBO;
	glGenVertexArrays(1, & sphereVAO);
	glGenBuffers(1, & sphereVBO);
	glGenBuffers(1, & sphereEBO);

	glBindVertexArray(sphereVAO);

	glBindBuffer(GL_ARRAY_BUFFER, sphereVBO);
	glBufferData(GL_ARRAY_BUFFER, sphereVertices.size() * sizeof(float), & sphereVertices[0], GL_STATIC_DRAW);


	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, sphereEBO);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, sphereIndices.size() * sizeof(unsigned int), & sphereIndices[0], GL_STATIC_DRAW);

	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void * ) 0);
	glEnableVertexAttribArray(0);

	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float),
		(void * )(3 * sizeof(float)));
	glEnableVertexAttribArray(1);
	glBindVertexArray(0);

	//направленный
	glm::vec3 dirLightDirection(-0.2f, -1.0f, -0.3f);
	glm::vec3 dirLightAmbient(0.05f, 0.05f, 0.05f);
	glm::vec3 dirLightDiffuse(0.4f, 0.4f, 0.4f);
	glm::vec3 dirLightSpecular(0.5f, 0.5f, 0.5f);

	//точечный
	glm::vec3 pointLightPosition(1.2f, 1.0f, 2.0f);
	glm::vec3 pointLightAmbient(0.05f, 0.05f, 0.05f);
	glm::vec3 pointLightDiffuse(0.8f, 0.8f, 0.8f);
	glm::vec3 pointLightSpecular(1.0f, 1.0f, 1.0f);
	float pointLightConstant = 1.0f;
	float pointLightLinear = 0.09f;
	float pointLightQuadratic = 0.032f;
	//камера
	glm::vec3 cameraPos = glm::vec3(0.0f, 0.0f, 5.0f);
	glm::vec3 cameraFront = glm::vec3(0.0f, 0.0f, -1.0f);
	glm::vec3 cameraUp = glm::vec3(0.0f, 1.0f, 0.0f);
	float yaw = -90.0f;
	float pitch = 0.0f;
	float lastX = SCREEN_WIDTH / 2.0f;
	float lastY = SCREEN_HEIGHT / 2.0f;
	float fov = 45.0f;
	bool firstMouse = true;
	float deltaTime = 0.0f;
	float lastFrame = 0.0f;
	sf::Clock clock;

	while(window.isOpen()) {
		float currentFrame = clock.getElapsedTime().asSeconds();
		deltaTime = currentFrame - lastFrame;
		lastFrame = currentFrame;
		sf::Event event;
		while(window.pollEvent(event)) {
			if(event.type == sf::Event::Closed || (event.type == sf::Event::KeyPressed && event.key.code == sf::Keyboard::Escape)) window.close();
			if(event.type == sf::Event::KeyPressed && event.key.code == sf::Keyboard::O) useDirLight = !useDirLight;
			if(event.type == sf::Event::KeyPressed && event.key.code == sf::Keyboard::P) usePointLight = !usePointLight;
			if(event.type == sf::Event::MouseMoved) {
				float xpos = static_cast < float > (event.mouseMove.x);
				float ypos = static_cast < float > (event.mouseMove.y);
				if(firstMouse) {
					lastX = xpos;
					lastY = ypos;
					firstMouse = false;
				}
				float xoffset = xpos - lastX;
				float yoffset = lastY - ypos;
				lastX = xpos;
				lastY = ypos;
				float sensitivity = 0.1f;
				xoffset *= sensitivity;
				yoffset *= sensitivity;
				yaw += xoffset;
				pitch += yoffset;
				if(pitch > 89.0f) pitch = 89.0f;
				if(pitch < -89.0f) pitch = -89.0f;
				glm::vec3 front;
				front.x = cos(glm::radians(yaw)) * cos(glm::radians(pitch));
				front.y = sin(glm::radians(pitch));
				front.z = sin(glm::radians(yaw)) * cos(glm::radians(pitch));
				cameraFront = glm::normalize(front);
			}
			if(event.type == sf::Event::MouseWheelScrolled) {
				if(event.mouseWheelScroll.delta > 0) fov -= 1.0f;
				else if(event.mouseWheelScroll.delta < 0) fov += 1.0f;
				if(fov < 1.0f) fov = 1.0f;
				if(fov > 45.0f) fov = 45.0f;
			}
		}
		float cameraSpeed = 2.5f * deltaTime;

		if(sf::Keyboard::isKeyPressed(sf::Keyboard::W)) cameraPos += cameraSpeed * cameraFront;
		if(sf::Keyboard::isKeyPressed(sf::Keyboard::S)) cameraPos -= cameraSpeed * cameraFront;
		if(sf::Keyboard::isKeyPressed(sf::Keyboard::A)) cameraPos -= glm::normalize(glm::cross(cameraFront, cameraUp)) * cameraSpeed;
		if(sf::Keyboard::isKeyPressed(sf::Keyboard::D)) cameraPos += glm::normalize(glm::cross(cameraFront, cameraUp)) * cameraSpeed;
		if(sf::Keyboard::isKeyPressed(sf::Keyboard::Space)) cameraPos += cameraSpeed * cameraUp;
		if(sf::Keyboard::isKeyPressed(sf::Keyboard::LShift)) cameraPos -= cameraSpeed * cameraUp;

		glClearColor(0.1f, 0.1f, 0.1f, 1.0f);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		glUseProgram(shaderProgram);

		GLint viewPosLoc = glGetUniformLocation(shaderProgram, "viewPos");
		glUniform3f(viewPosLoc, 0.0f, 0.0f, 3.0f);

		glUniform3fv(glGetUniformLocation(shaderProgram, "dirLight.direction"), 1, & dirLightDirection[0]);
		glUniform3fv(glGetUniformLocation(shaderProgram, "dirLight.ambient"), 1, & dirLightAmbient[0]);
		glUniform3fv(glGetUniformLocation(shaderProgram, "dirLight.diffuse"), 1, & dirLightDiffuse[0]);
		glUniform3fv(glGetUniformLocation(shaderProgram, "dirLight.specular"), 1, & dirLightSpecular[0]);
		glUniform3fv(glGetUniformLocation(shaderProgram, "pointLight.position"), 1, & pointLightPosition[0]);
		glUniform3fv(glGetUniformLocation(shaderProgram, "pointLight.ambient"), 1, & pointLightAmbient[0]);
		glUniform3fv(glGetUniformLocation(shaderProgram, "pointLight.diffuse"), 1, & pointLightDiffuse[0]);
		glUniform3fv(glGetUniformLocation(shaderProgram, "pointLight.specular"), 1, & pointLightSpecular[0]);
		glUniform1f(glGetUniformLocation(shaderProgram, "pointLight.constant"), pointLightConstant);
		glUniform1f(glGetUniformLocation(shaderProgram, "pointLight.linear"), pointLightLinear);
		glUniform1f(glGetUniformLocation(shaderProgram, "pointLight.quadratic"), pointLightQuadratic);
		glUniform1i(glGetUniformLocation(shaderProgram, "useDirLight"), useDirLight);
		glUniform1i(glGetUniformLocation(shaderProgram, "usePointLight"), usePointLight);

		glm::mat4 view = glm::lookAt(cameraPos, cameraPos + cameraFront, cameraUp);
		glm::mat4 projection = glm::perspective(glm::radians(fov),
			(float) SCREEN_WIDTH / (float) SCREEN_HEIGHT, 0.1f, 100.0f);

		GLuint viewLoc = glGetUniformLocation(shaderProgram, "view");
		GLuint projLoc = glGetUniformLocation(shaderProgram, "projection");

		glUniformMatrix4fv(viewLoc, 1, GL_FALSE, glm::value_ptr(view));
		glUniformMatrix4fv(projLoc, 1, GL_FALSE, glm::value_ptr(projection));

		GLuint modelLoc = glGetUniformLocation(shaderProgram, "model");
		glBindVertexArray(cubeVAO);

		glm::mat4 model = glm::mat4(1.0f);
		model = glm::translate(model, glm::vec3(-2.0f, 0.0f, 0.0f));
		model = glm::rotate(model, clock.getElapsedTime().asSeconds() * glm::radians(50.0f), glm::vec3(1.0f, 0.3f, 0.5f));

		glUniformMatrix4fv(modelLoc, 1, GL_FALSE, glm::value_ptr(model));
		glDrawArrays(GL_TRIANGLES, 0, 36);
		glBindVertexArray(sphereVAO);
		model = glm::mat4(1.0f);
		model = glm::translate(model, glm::vec3(0.0f, 0.0f, 0.0f));
		model = glm::rotate(model, clock.getElapsedTime().asSeconds() * glm::radians(30.0f), glm::vec3(0.0f, 1.0f, 0.0f));

		glUniformMatrix4fv(modelLoc, 1, GL_FALSE, glm::value_ptr(model));
		glDrawElements(GL_TRIANGLES, sphereIndices.size(), GL_UNSIGNED_INT, 0);
		glBindVertexArray(pyramidVAO);
		model = glm::mat4(1.0f);
		model = glm::translate(model, glm::vec3(2.0f, 0.0f, 0.0f));
		model = glm::rotate(model, clock.getElapsedTime().asSeconds() * glm::radians(40.0f), glm::vec3(0.0f, 1.0f, 0.0f));

		glUniformMatrix4fv(modelLoc, 1, GL_FALSE, glm::value_ptr(model));
		glDrawArrays(GL_TRIANGLES, 0, 18);
		glBindVertexArray(0);
		window.display();
	}
	
	glDeleteVertexArrays(1, & cubeVAO);
	glDeleteBuffers(1, & cubeVBO);
	glDeleteVertexArrays(1, & pyramidVAO);
	glDeleteBuffers(1, & pyramidVBO);
	glDeleteVertexArrays(1, & sphereVAO);
	glDeleteBuffers(1, & sphereVBO);
	glDeleteBuffers(1, & sphereEBO);
	return 0;
}