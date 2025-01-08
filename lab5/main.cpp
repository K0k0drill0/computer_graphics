#include <SFML/Graphics.hpp>
#include <cmath>
#include <vector>
#include <limits>
#include <iostream>
#include <algorithm>

struct Vec3 {
    float x, y, z;
    Vec3(float x_ = 0, float y_ = 0, float z_ = 0) : x(x_), y(y_), z(z_) {}

    Vec3 operator+(const Vec3& v) const { return Vec3(x + v.x, y + v.y, z + v.z); }
    Vec3 operator-(const Vec3& v) const { return Vec3(x - v.x, y - v.y, z - v.z); }
    Vec3 operator*(float f) const { return Vec3(x * f, y * f, z * f); }
    Vec3 operator/(float f) const { return Vec3(x / f, y / f, z / f); }
    Vec3 operator*(const Vec3& v) const { return Vec3(x * v.x, y * v.y, z * v.z); }

    float dot(const Vec3& v) const { return x * v.x + y * v.y + z * v.z; }
    Vec3 cross(const Vec3& v) const {
        return Vec3(y * v.z - z * v.y, z * v.x - x * v.z, x * v.y - y * v.x);
    }

    float length() const { return std::sqrt(dot(*this)); }
    Vec3 normalize() const { 
        float len = length(); 
        return len > 0 ? (*this) * (1.0f / len) : *this; 
    }
};

struct Object {
    virtual ~Object() {}
    virtual bool intersect(const Vec3& orig, const Vec3& dir, float& tNear, Vec3& hitNormal, Vec3& hitColor) const = 0;

    float reflection = 0.0f;
    float refraction = 0.0f;
    float ior = 1.0f; // Индекс преломления
};

struct Sphere : public Object {
    Vec3 center;
    float radius;
    Vec3 color;

    Sphere(const Vec3& c, float r, const Vec3& col, float refl = 0.0f, float refr = 0.0f, float ior_ = 1.0f) {
        center = c;
        radius = r;
        color = col;
        reflection = refl;
        refraction = refr;
        ior = ior_;
    }

    bool intersect(const Vec3& orig, const Vec3& dir, float& tNear, Vec3& hitNormal, Vec3& hitColor) const override {
        Vec3 L = center - orig;
        float tca = L.dot(dir);
        if (tca < 0) return false;

        float d2 = L.dot(L) - tca * tca;
        float r2 = radius * radius;
        if (d2 > r2) return false;

        float thc = std::sqrt(r2 - d2);
        float t0 = tca - thc;
        float t1 = tca + thc;

        if (t0 < 0) t0 = t1;
        if (t0 < 0) return false;

        tNear = t0;
        Vec3 phit = orig + dir * tNear;
        hitNormal = (phit - center).normalize();
        hitColor = color;

        return true;
    }
};

struct Plane : public Object {
    Vec3 normal;
    float d;
    Vec3 color;

    Plane(const Vec3& n, float d_, const Vec3& col, float refl = 0.0f, float refr = 0.0f, float ior_ = 1.0f) {
        normal = n.normalize();
        d = d_;
        color = col;
        reflection = refl;
        refraction = refr;
        ior = ior_;
    }

    bool intersect(const Vec3& orig, const Vec3& dir, float& tNear, Vec3& hitNormal, Vec3& hitColor) const override {
        float denom = normal.dot(dir);
        if (std::fabs(denom) > 1e-6) {
            float t = -(normal.dot(orig) + d) / denom;
            if (t >= 0) {
                tNear = t;
                hitNormal = normal;
                hitColor = color;
                return true;
            }
        }
        return false;
    }
};

bool refract(const Vec3& I, const Vec3& N, float ior, Vec3& refrDir) {
    float cosi = std::clamp(I.dot(N), -1.0f, 1.0f);
    float etai = 1.0f, etat = ior;
    Vec3 n = N;

    if (cosi < 0) {
        cosi = -cosi;
    } else {
        std::swap(etai, etat);
        n = n * -1.0f;
    }

    float eta = etai / etat;
    float k = 1 - eta * eta * (1 - cosi * cosi);

    if (k < 0) {
        return false;
    } else {
        refrDir = I * eta + n * (eta * cosi - std::sqrt(k));
        return true;
    }
}

float fresnel(const Vec3& I, const Vec3& N, float ior) {
    float cosi = std::clamp(I.dot(N), -1.0f, 1.0f);
    float etai = 1, etat = ior;

    if (cosi > 0) std::swap(etai, etat);

    float sint = etai / etat * std::sqrt(std::max(0.f, 1 - cosi * cosi));
    if (sint >= 1) {
        return 1.0f;
    } else {
        float cost = std::sqrt(std::max(0.f, 1 - sint * sint));
        cosi = std::fabs(cosi);

        float Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
        float Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
        return (Rs * Rs + Rp * Rp) * 0.5f;
    }
}

bool inShadow(const Vec3& phit, const Vec3& nhit, const Vec3& lightPos, const std::vector<Object*>& objects) {
    Vec3 lightDir = (lightPos - phit).normalize();
    for (auto obj : objects) {
        float t = std::numeric_limits<float>::infinity();
        Vec3 n, c;
        if (obj->intersect(phit + nhit * 1e-4f, lightDir, t, n, c)) {
            return true; // Точка находится в тени
        }
    }
    return false; // Точка освещена
}

Vec3 trace(const Vec3& orig, const Vec3& dir, const std::vector<Object*>& objects,
           const Vec3& lightPos1, const Vec3& lightPos2, bool light1On, bool light2On,
           int depth, int maxDepth) {
    if (depth > maxDepth) {
        return Vec3(0, 0, 0);
    }

    float tNear = std::numeric_limits<float>::infinity();
    const Object* hitObject = nullptr;
    Vec3 hitNormal, hitColor;

    for (auto obj : objects) {
        float t = std::numeric_limits<float>::infinity();
        Vec3 n, c;
        if (obj->intersect(orig, dir, t, n, c)) {
            if (t < tNear) {
                tNear = t;
                hitObject = obj;
                hitNormal = n;
                hitColor = c;
            }
        }
    }

    if (!hitObject) {
        return Vec3(0.2f, 0.7f, 1.0f); // Цвет неба
    }

    Vec3 phit = orig + dir * tNear;
    Vec3 nhit = hitNormal;

    float refl = hitObject->reflection;
    float refr = hitObject->refraction;
    float ior = hitObject->ior;

    Vec3 surfaceColor(0, 0, 0);

    auto computeLight = [&](const Vec3& lightPos, bool lightOn) {
        if (!lightOn) return Vec3(0, 0, 0); 

        Vec3 lightDir = (lightPos - phit).normalize();
        bool shadow = inShadow(phit, nhit, lightPos, objects);
        float shade = shadow ? 0.2f : std::max(0.0f, nhit.dot(lightDir));
        return hitColor * shade;
    };

    surfaceColor = computeLight(lightPos1, light1On) + computeLight(lightPos2, light2On);

    // Обработка отражений и преломлений
    if (refl > 0.0f || refr > 0.0f) {
        float kr = fresnel(dir, nhit, ior); // Коэффициент отражения

        Vec3 reflectionColor(0, 0, 0);
        Vec3 refractionColor(0, 0, 0);

        if (refl > 0.0f) {
            Vec3 reflDir = dir - nhit * 2.0f * (dir.dot(nhit));
            reflDir = reflDir.normalize();
            reflectionColor = trace(phit + nhit * 1e-4f, reflDir, objects, lightPos1, lightPos2, light1On, light2On, depth + 1, maxDepth);
        }

        if (refr > 0.0f) {
            Vec3 refrDir;
            if (refract(dir, nhit, ior, refrDir)) {
                refrDir = refrDir.normalize();
                refractionColor = trace(phit - nhit * 1e-4f, refrDir, objects, lightPos1, lightPos2, light1On, light2On, depth + 1, maxDepth);
            }
        }

        Vec3 result = reflectionColor * kr * refl + refractionColor * (1.0f - kr) * refr;
        surfaceColor = surfaceColor * (1.0f - (refl + refr)) + result;
    }

    return surfaceColor;
}

int main() {
    int width = 800;
    int height = 600;
    int maxDepth = 5;

    sf::RenderWindow window(sf::VideoMode(width, height), "Ray Tracing Example with Two Lights");
    window.setFramerateLimit(30);

    Sphere sphere1(Vec3(-1.5f, 0.0f, -5.0f), 1.0f, Vec3(1.0f, 0.0f, 0.0f), 0.5f, 0.0f, 1.0f);
    Sphere sphere2(Vec3(1.5f, 0.0f, -5.0f), 1.0f, Vec3(0.0f, 1.0f, 0.0f), 0.0f, 0.8f, 1.5f);
    Sphere sphere3(Vec3(0.0f, -0.5f, -3.0f), 0.5f, Vec3(0.0f, 0.0f, 1.0f), 0.3f, 0.5f, 1.3f);
    Plane plane(Vec3(0, 1, 0), 1.5f, Vec3(1.0f, 1.0f, 1.0f), 0.1f, 0.0f, 1.0f);

    std::vector<Object*> objects = {&sphere1, &sphere2, &sphere3, &plane};

    Vec3 lightPos1(-2, 5, -3);
    Vec3 lightPos2(2, 5, -2);
    bool light1On = true;
    bool light2On = true;

    std::vector<sf::Uint8> pixels(width * height * 4, 0);

    float fov = 60.0f;
    float aspectRatio = float(width) / float(height);
    float angle = std::tan((fov * 0.5f * M_PI / 180.0f));

    auto renderScene = [&](bool updateTexture) {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float xx = (2 * ((x + 0.5f) / (float)width) - 1) * angle * aspectRatio;
                float yy = (1 - 2 * ((y + 0.5f) / (float)height)) * angle;

                Vec3 rayDir(xx, yy, -1);
                rayDir = rayDir.normalize();

                Vec3 col = trace(Vec3(0, 0, 0), rayDir, objects, lightPos1, lightPos2, light1On, light2On, 0, maxDepth);

                // Применение гамма-коррекции
                float gamma = 2.2f;
                col.x = std::pow(col.x, 1.0f / gamma);
                col.y = std::pow(col.y, 1.0f / gamma);
                col.z = std::pow(col.z, 1.0f / gamma);

                int r = (int)(std::max(0.0f, std::min(1.0f, col.x)) * 255);
                int g = (int)(std::max(0.0f, std::min(1.0f, col.y)) * 255);
                int b = (int)(std::max(0.0f, std::min(1.0f, col.z)) * 255);

                pixels[(y * width + x) * 4 + 0] = (sf::Uint8)r;
                pixels[(y * width + x) * 4 + 1] = (sf::Uint8)g;

                pixels[(y * width + x) * 4 + 2] = (sf::Uint8)b;
                pixels[(y * width + x) * 4 + 3] = 255;
            }
        }
    };

    renderScene(false);
    sf::Texture texture;

    texture.create(width, height);
    texture.update( & pixels[0]);
    sf::Sprite sprite(texture);

    while(window.isOpen()) {
        sf::Event ev;
        while(window.pollEvent(ev)) {
            if(ev.type == sf::Event::Closed) {
                window.close();
            }
            if(ev.type == sf::Event::KeyPressed) {
                if(ev.key.code == sf::Keyboard::Q) {
                    light1On = !light1On;
                    renderScene(false);
                    texture.update( & pixels[0]);
                }
                if(ev.key.code == sf::Keyboard::R) {
                    light2On = !light2On;
                    renderScene(false);
                    texture.update( & pixels[0]);
                }
            }
        }
        window.clear(sf::Color::Black);
        window.draw(sprite);
        window.display();
    }
    return 0;
}