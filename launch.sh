#!/bin/bash

# Configuration
BACKEND="docker-compose.backend.yml"
FRONTEND="docker-compose.frontend.yml"
# Merging files into a single command variable
COMPOSE_CMD="docker compose -f $BACKEND -f $FRONTEND"

up() {
    echo "--- Building and starting containers ---"
    $COMPOSE_CMD up --build
}

down() {
    echo "--- Stopping and removing containers ---"
    $COMPOSE_CMD down
}

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
