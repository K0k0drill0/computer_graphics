# Указываем минимальную версию CMake
cmake_minimum_required(VERSION 3.10)

# Указываем имя проекта
project(OpenGLWithSFMLAndGLM)

# Устанавливаем стандарт C++
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

# Добавляем исполняемый файл
add_executable(OpenGLWithSFMLAndGLM main.cpp)

# Настройка для SFML библиотеки
set(SFML_DIR "path/to/SFML/lib/cmake/SFML")
find_package(SFML 2.5 COMPONENTS system window REQUIRED)

# Настройка для OpenGL
find_package(OpenGL REQUIRED)

# Настройка для GLM
find_package(glm REQUIRED)

# Включение директорий заголовочных файлов
include_directories(${SFML_INCLUDE_DIRS} ${OPENGL_INCLUDE_DIR} ${GLM_INCLUDE_DIRS})

# Связывание с необходимыми библиотеками
target_link_libraries(OpenGLWithSFMLAndGLM sfml-system sfml-window ${OPENGL_gl_LIBRARY} glm::glm)
