#include <SFML/Window.hpp>
#include <SFML/Graphics.hpp>
#include <SFML/OpenGL.hpp>
#include <iostream>
#include <GL/glu.h>

class CubeWithPerspective {
    public:
    CubeWithPerspective(sf::Window& _window) : window(_window){
        window.setVerticalSyncEnabled(true);

        glEnable(GL_DEPTH_TEST);

        targetAngle = 45.0f;
        targetOrthoSize = 5.0f;
        usePerspective = true;
        transitioning = false;
        transitionProgress = 0.0f;

        rotationX = 0.0f;
        rotationY = 0.0f;
    }

    void run() {
        while (window.isOpen()) {
            processInput(window);
            updateTransition();

            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

            setupProjection();
            setupCamera();

            glRotatef(rotationX, 1.0f, 0.0f, 0.0f);
            glRotatef(rotationY, 0.0f, 1.0f, 0.0f);

            drawCube();

            window.display();
        }
    }

    private:
    sf::Window& window;
    float targetAngle;            // Целевой угол поля зрения в перспективной проекции
    float targetOrthoSize;         // Целевые размеры ортографической проекции
    bool usePerspective;           // Выбор между проекциями
    bool transitioning;            // Флаг перехода между проекциями
    float transitionProgress;      // Прогресс перехода (от 0 до 1)

    float rotationX;               // Угол поворота по оси X
    float rotationY;               // Угол поворота по оси Y

    void drawCube() {
        glBegin(GL_QUADS);

        // Передняя грань
        glColor3f(1.0f, 0.0f, 0.0f); // Красный (начало градиента)
        glVertex3f(-1.0f, -1.0f, 1.0f);
        glColor3f(1.0f, 0.5f, 0.5f); // Светло-красный (градиент)
        glVertex3f(1.0f, -1.0f, 1.0f);
        glColor3f(1.0f, 0.8f, 0.8f); // Ещё светлее
        glVertex3f(1.0f, 1.0f, 1.0f);
        glColor3f(1.0f, 0.0f, 0.0f);
        glVertex3f(-1.0f, 1.0f, 1.0f);

        // Задняя грань
        glColor3f(0.0f, 1.0f, 0.0f);
        glVertex3f(-1.0f, -1.0f, -1.0f);
        glColor3f(0.5f, 1.0f, 0.5f);
        glVertex3f(-1.0f, 1.0f, -1.0f);
        glColor3f(0.8f, 1.0f, 0.8f);
        glVertex3f(1.0f, 1.0f, -1.0f);
        glColor3f(0.0f, 1.0f, 0.0f);
        glVertex3f(1.0f, -1.0f, -1.0f);

        // Левая грань
        glColor3f(0.0f, 0.0f, 1.0f);
        glVertex3f(-1.0f, -1.0f, -1.0f);
        glColor3f(0.3f, 0.3f, 1.0f);
        glVertex3f(-1.0f, -1.0f, 1.0f);
        glColor3f(0.6f, 0.6f, 1.0f);
        glVertex3f(-1.0f, 1.0f, 1.0f);
        glColor3f(0.0f, 0.0f, 1.0f);
        glVertex3f(-1.0f, 1.0f, -1.0f);

        // Правая грань
        glColor3f(1.0f, 1.0f, 0.0f);
        glVertex3f(1.0f, -1.0f, -1.0f);
        glColor3f(1.0f, 1.0f, 0.3f);
        glVertex3f(1.0f, -1.0f, 1.0f);
        glColor3f(1.0f, 1.0f, 0.6f);
        glVertex3f(1.0f, 1.0f, 1.0f);
        glColor3f(1.0f, 1.0f, 0.0f);
        glVertex3f(1.0f, 1.0f, -1.0f);

        // Верхняя грань
        glColor3f(1.0f, 0.0f, 1.0f);
        glVertex3f(-1.0f, 1.0f, -1.0f);
        glColor3f(1.0f, 0.3f, 1.0f);
        glVertex3f(-1.0f, 1.0f, 1.0f);
        glColor3f(1.0f, 0.6f, 1.0f);
        glVertex3f(1.0f, 1.0f, 1.0f);
        glColor3f(1.0f, 0.0f, 1.0f);
        glVertex3f(1.0f, 1.0f, -1.0f);

        // Нижняя грань
        glColor3f(0.0f, 1.0f, 1.0f);
        glVertex3f(-1.0f, -1.0f, -1.0f);
        glColor3f(0.3f, 1.0f, 1.0f);
        glVertex3f(1.0f, -1.0f, -1.0f);
        glColor3f(0.6f, 1.0f, 1.0f);
        glVertex3f(1.0f, -1.0f, 1.0f);
        glColor3f(0.0f, 1.0f, 1.0f);
        glVertex3f(-1.0f, -1.0f, 1.0f);

        glEnd();
    }

    void setupProjection() {
        glMatrixMode(GL_PROJECTION);
        glLoadIdentity();

        float currentAngle = targetAngle * transitionProgress + 
                            (usePerspective ? 0.0f : (1.0f - transitionProgress) * 90.0f);
        float currentOrthoSize = targetOrthoSize * transitionProgress + 
                                (usePerspective ? (1.0f - transitionProgress) * 3.0f : 0.0f);

        if (usePerspective) {
            gluPerspective(currentAngle, 1.0, 0.1, 100.0);
        } else {
            glOrtho(-currentOrthoSize, currentOrthoSize, -currentOrthoSize, currentOrthoSize, -100.0, 100.0);
        }

        glMatrixMode(GL_MODELVIEW);
    }

    void setupCamera() {
        glLoadIdentity();
        gluLookAt(0.0, 0.0, 5.0,  // Положение 
                0.0, 0.0, 0.0,  // Центр куба
                0.0, 1.0, 0.0); // Вектор "вверх"
    }

    void processInput(sf::Window &window) {
        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed) {
                window.close();
            }

            if (event.type == sf::Event::KeyPressed) {
                // Переключение между проекциями
                if (event.key.code == sf::Keyboard::P) {
                    transitioning = true;
                    usePerspective = !usePerspective;
                    transitionProgress = 0.0f;
                }

                if (event.key.code == sf::Keyboard::W) rotationX += 5.0f;
                if (event.key.code == sf::Keyboard::S) rotationX -= 5.0f;
                if (event.key.code == sf::Keyboard::A) rotationY -= 5.0f;
                if (event.key.code == sf::Keyboard::D) rotationY += 5.0f;
            }
        }
    }

    void updateTransition() {
        if (transitioning) {
            transitionProgress += 0.01f;

            if (transitionProgress >= 1.0f) {
                transitioning = false;
                transitionProgress = 1.0f;
            }
        }
    }

};


int main() {
    sf::Window window(sf::VideoMode(800, 600), "Smooth Transition between Projections", sf::Style::Close | sf::Style::Titlebar);

    CubeWithPerspective cube(window);
    cube.run();

    return 0;
}
