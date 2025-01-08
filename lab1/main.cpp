#include <SFML/Graphics.hpp>
#include <vector>

class BezierCurve : public sf::RenderWindow {
public:
    BezierCurve(int width, int height, const std::string& title)
        : sf::RenderWindow(sf::VideoMode(width, height), title) {
        controlPoints = { {100, 500}, {400, 100}, {700, 500} };
        scaleControlPoints();
        setView(sf::View(sf::FloatRect(0.0f, 0.0f, static_cast<float>(width), static_cast<float>(height))));
    }

    void run() {
        sf::Clock clock;
        while (isOpen()) {
            sf::Event event;
            while (pollEvent(event)) {
                if (event.type == sf::Event::Closed)
                    close();
                else if (event.type == sf::Event::MouseButtonPressed && event.mouseButton.button == sf::Mouse::Left)
                    onMouseDown(event.mouseButton.x, event.mouseButton.y);
                else if (event.type == sf::Event::MouseMoved)
                    onMouseMove(event.mouseMove.x, event.mouseMove.y);
                else if (event.type == sf::Event::MouseButtonReleased && event.mouseButton.button == sf::Mouse::Left)
                    onMouseUp(event.mouseButton.x, event.mouseButton.y);
            }

            float deltaTime = clock.restart().asSeconds();
            render();
        }
    }

private:
    std::vector<sf::Vector2f> controlPoints;
    int selectedPoint = -1;
    sf::Vector2f mousePosition;
    bool isMouseDown = false;

    void scaleControlPoints() {
        float scaleX = static_cast<float>(getSize().x) / 800.0f;
        float scaleY = static_cast<float>(getSize().y) / 600.0f;
        for (auto& point : controlPoints) {
            point.x *= scaleX;
            point.y *= scaleY;
        }
    }

    void onMouseDown(int x, int y) {
        selectedPoint = getSelectedPoint(x, y);
        isMouseDown = true;
    }

    void onMouseMove(int x, int y) {
        mousePosition = { static_cast<float>(x), static_cast<float>(y) };
        if (isMouseDown && selectedPoint != -1) {
            controlPoints[selectedPoint] = mousePosition;
        }
    }

    void onMouseUp(int x, int y) {
        selectedPoint = -1;
        isMouseDown = false;
    }

    int getSelectedPoint(int x, int y) {
        for (int i = 0; i < controlPoints.size(); i++) {
            if (std::abs(controlPoints[i].x - x) < 10 && std::abs(controlPoints[i].y - y) < 10) {
                return i;
            }
        }
        return -1;
    }

    void render() {
        clear(sf::Color::Black);
        drawBezierCurve();
        drawControlPoints();
        display();
    }

    void drawBezierCurve() {
        sf::VertexArray curve(sf::LinesStrip, 101);
        for (int i = 0; i <= 100; i++) {
            float t = static_cast<float>(i) / 100.0f;
            float oneMinusT = 1.0f - t;

            // P(t) = (1-t)^2 * P0 + 2*(1-t)*t * P1 + t^2 * P2
            float x = oneMinusT * oneMinusT * controlPoints[0].x +
                      2.0f * oneMinusT * t * controlPoints[1].x +
                      t * t * controlPoints[2].x;
            float y = oneMinusT * oneMinusT * controlPoints[0].y +
                      2.0f * oneMinusT * t * controlPoints[1].y +
                      t * t * controlPoints[2].y;

            curve[i].position = { x, y };
            curve[i].color = sf::Color(255, 104, 104);
        }
        draw(curve);
    }

    void drawControlPoints() {
        for (int i = 0; i < controlPoints.size(); i++) {
            sf::CircleShape point(10.0f);
            point.setPosition(controlPoints[i].x - 10.0f, controlPoints[i].y - 10.0f);
            point.setFillColor(i == selectedPoint ? sf::Color(255, 102, 102) : sf::Color(102, 255, 102));
            draw(point);
        }
    }
};

int main() {
    BezierCurve app(1200, 800, "CG_1");
    app.run();
    return 0;
}
