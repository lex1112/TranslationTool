#!/bin/bash

# Определение общих путей
PROJECT_NAME="translation-system"
BACKEND="backend/docker-compose.yml"
FRONTEND="presentation/docker-compose.frontend.yml"

# Функция для запуска (аналог up: в Makefile)
up() {
    docker compose -p $PROJECT_NAME --project-directory . -f $BACKEND -f $FRONTEND up --build
}

# Функция для остановки (аналог down: в Makefile)
down() {
    docker compose -p $PROJECT_NAME -f $BACKEND -f $FRONTEND down
}

# Логика выбора команды
case "$1" in
    up)
        up
        ;;
    down)
        down
        ;;
    *)
        echo "Usage: $0 {up|down}"
        exit 1
        ;;
esac
