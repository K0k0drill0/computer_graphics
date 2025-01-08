cmake_minimum_required(VERSION 3.10)

# Название вашего проекта
project(MySFMLProject)

# Укажите стандарт C++
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

# Добавьте исполняемый файл
add_executable(MySFMLProject main.cpp)

# Найдите пакет SFML
# Убедитесь, что путь к SFML установлен правильно,
# например: set(SFML_DIR "path_to_sfml/SFML/cmake/SFML")
set(SFML_DIR "/usr/local/lib/cmake/SFML")  # Укажите здесь корректный путь, если нужно
find_package(SFML 2.5 COMPONENTS graphics window system REQUIRED)

# Подключите SFML к вашему проекту
target_link_libraries(MySFMLProject sfml-graphics sfml-window sfml-system)
