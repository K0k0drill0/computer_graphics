# Указываем минимальную версию CMake
cmake_minimum_required(VERSION 3.10)

# Указываем имя проекта
project(OpenGLWithSFMLGLM)

# Устанавливаем стандарт C++
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

# Добавляем исполняемый файл
add_executable(OpenGLWithSFMLGLM main.cpp)

# Настройка для SFML библиотеки
set(SFML_DIR "path/to/SFML/lib/cmake/SFML") # измените этот путь, если необходимо
find_package(SFML 2.5 COMPONENTS system window REQUIRED)

# Настройка для OpenGL и GLEW
find_package(OpenGL REQUIRED)
find_package(GLEW REQUIRED)

# Настройка для GLM
find_package(glm REQUIRED)

# Включение директорий заголовочных файлов
include_directories(${SFML_INCLUDE_DIRS} ${OPENGL_INCLUDE_DIR} ${GLEW_INCLUDE_DIRS} ${GLM_INCLUDE_DIRS})

# Связывание с необходимыми библиотеками
target_link_libraries(OpenGLWithSFMLGLM sfml-system sfml-window ${OPENGL_gl_LIBRARY} GLEW::GLEW glm::glm)
