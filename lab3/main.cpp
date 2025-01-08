#include <SFML/Window.hpp>
#include <SFML/OpenGL.hpp>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>
#include <chrono>
#include <iostream>
#include <cmath>

glm::vec3 cameraPos(0.0f, 0.0f, 5.0f);
glm::vec3 cameraUp(0.0f, 1.0f, 0.0f);

float radius = 5.0f;
float horizontalAngle = 0.0f;
float verticalAngle = 0.0f;

void drawCube() {
    glBegin(GL_QUADS);

    // Верхняя грань
    glColor3f(1.0f, 0.0f, 0.0f);
    glVertex3f(-0.5f, 0.5f, -0.5f);
    glVertex3f(0.5f, 0.5f, -0.5f);
    glVertex3f(0.5f, 0.5f, 0.5f);
    glVertex3f(-0.5f, 0.5f, 0.5f);

    // Нижняя грань
    glColor3f(0.0f, 1.0f, 0.0f);
    glVertex3f(-0.5f, -0.5f, -0.5f);
    glVertex3f(0.5f, -0.5f, -0.5f);
    glVertex3f(0.5f, -0.5f, 0.5f);
    glVertex3f(-0.5f, -0.5f, 0.5f);

    // Передняя грань
    glColor3f(0.0f, 0.0f, 1.0f);
    glVertex3f(-0.5f, -0.5f, 0.5f);
    glVertex3f(0.5f, -0.5f, 0.5f);
    glVertex3f(0.5f, 0.5f, 0.5f);
    glVertex3f(-0.5f, 0.5f, 0.5f);

    // Задняя грань
    glColor3f(1.0f, 1.0f, 0.0f);
    glVertex3f(-0.5f, -0.5f, -0.5f);
    glVertex3f(0.5f, -0.5f, -0.5f);
    glVertex3f(0.5f, 0.5f, -0.5f);
    glVertex3f(-0.5f, 0.5f, -0.5f);

    // Левая грань
    glColor3f(0.0f, 1.0f, 1.0f);
    glVertex3f(-0.5f, -0.5f, -0.5f);
    glVertex3f(-0.5f, -0.5f, 0.5f);
    glVertex3f(-0.5f, 0.5f, 0.5f);
    glVertex3f(-0.5f, 0.5f, -0.5f);

    // Правая грань
    glColor3f(1.0f, 0.0f, 1.0f);
    glVertex3f(0.5f, -0.5f, -0.5f);
    glVertex3f(0.5f, -0.5f, 0.5f);
    glVertex3f(0.5f, 0.5f, 0.5f);
    glVertex3f(0.5f, 0.5f, -0.5f);

    glEnd();
}

void processInput(sf::Window& window) {
    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Escape)) {
        window.close();
    }

    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left)) {
        horizontalAngle -= 0.01f;
    }

    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right)) {
        horizontalAngle += 0.01f;
    }

    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up)) {
        verticalAngle += 0.01f;
        if (verticalAngle > 89.0f) verticalAngle = 89.0f;
    }

    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down)) {
        verticalAngle -= 0.01f;
        if (verticalAngle < -89.0f) verticalAngle = -89.0f;
    }
}

int main() {
    sf::Window window(sf::VideoMode(800, 600), "3D-Camera", sf::Style::Default, sf::ContextSettings(32));
    window.setMouseCursorGrabbed(true);
    window.setMouseCursorVisible(false);

    glEnable(GL_DEPTH_TEST);

    glm::mat4 projection = glm::perspective(glm::radians(45.0f), 800.0f / 600.0f, 0.1f, 100.0f);

    while (window.isOpen()) {
        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed)
                window.close();
        }

        processInput(window);

        cameraPos.x = radius * cos(glm::radians(horizontalAngle)) * cos(glm::radians(verticalAngle));
        cameraPos.y = radius * sin(glm::radians(verticalAngle));
        cameraPos.z = radius * sin(glm::radians(horizontalAngle)) * cos(glm::radians(verticalAngle));

        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        glm::mat4 view = glm::lookAt(cameraPos, glm::vec3(0.0f, 0.0f, 0.0f), cameraUp);
        glMatrixMode(GL_PROJECTION);
        glLoadMatrixf(glm::value_ptr(projection));
        glMatrixMode(GL_MODELVIEW);
        glLoadMatrixf(glm::value_ptr(view));

        glPushMatrix();
        glTranslatef(0.0f, 0.0f, 0.0f);
        drawCube();
        glPopMatrix();

        window.display();
    }

    return 0;
}
