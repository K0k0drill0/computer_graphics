# Указываем минимальную версию CMake
cmake_minimum_required(VERSION 3.10)

# Указываем имя проекта
project(Lab2WithSFML)

# Указываем стандарты C++
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

# Добавляем исполняемый файл
add_executable(Lab2WithSFML main.cpp)

# Указываем путь к SFML (замените на ваш путь, если необходимо)
set(SFML_DIR "usr/include/SFML")

# Находим SFML
find_package(SFML 2.5 COMPONENTS system window graphics REQUIRED)

# Находим OpenGL
find_package(OpenGL REQUIRED)

# Добавляем директории для поиска заголовочных файлов
include_directories(${SFML_INCLUDE_DIRS} ${OPENGL_INCLUDE_DIR})

# Связываем необходимые библиотеки
target_link_libraries(Lab2WithSFML sfml-system sfml-window sfml-graphics ${OPENGL_gl_LIBRARY} GLU)
