# Указываем минимальную версию CMake
cmake_minimum_required(VERSION 3.10)

# Указываем имя проекта
project(Lab1)

# Указываем стандарты C++
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

# Добавляем исполняемый файл
add_executable(Lab1 main.cpp)

# Указываем путь к SFML
set(SFML_DIR "/usr/include/SFML")

# Поиск библиотек SFML
find_package(SFML 2.5 COMPONENTS system window graphics REQUIRED)

# Добавляем библиотеки SFML
target_link_libraries(Lab1 sfml-system sfml-window sfml-graphics)

# Указываем, где искать заголовочные файлы SFML
target_include_directories(Lab1 PRIVATE "/usr/include/SFML")
