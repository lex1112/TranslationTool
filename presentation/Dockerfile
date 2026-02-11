FROM php:8.2-apache AS php-ui

# Install system dependencies
RUN apt-get update && apt-get install -y \
    libpng-dev libonig-dev libxml2-dev \
    zip unzip git curl libssl-dev \
    && docker-php-ext-install mbstring exif pcntl bcmath gd \
    && a2enmod rewrite

# Configure Apache Document Root
ENV APACHE_DOCUMENT_ROOT /var/www/html/public
RUN sed -ri -e 's!/var/www/html!${APACHE_DOCUMENT_ROOT}!g' /etc/apache2/sites-available/*.conf

# Install Composer
COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

WORKDIR /var/www/html

# ROBUST AUTO-INSTALL & CONFIGURATION SCRIPT
RUN echo '#!/bin/bash\n\
# --- Step 1: Project Initialization ---\n\
if [ ! -f "composer.json" ]; then\n\
  echo "--- Empty folder detected. Installing Laravel... ---"\n\
  composer create-project laravel/laravel temp_laravel --stability=stable --prefer-dist --no-interaction\n\
  cp -rn temp_laravel/. .\n\
  rm -rf temp_laravel\n\
fi\n\
\n\
# ---  Environment (.env) Reconstruction ---\n\
# We create a clean .env with the exact settings that fixed your 302 loop\n\
if [ ! -f ".env" ]; then\n\
  echo "--- Creating fresh .env file ---"\n\
  cat <<EOF > .env\n\
APP_NAME=TranslationTool\n\
APP_ENV=local\n\
APP_KEY=\n\
APP_DEBUG=true\n\
APP_URL=http://localhost:8000\n\
\n\
LOG_CHANNEL=stack\n\
LOG_LEVEL=debug\n\
\n\
DB_CONNECTION=sqlite\n\
DB_DATABASE=/var/www/html/database/database.sqlite\n\
\n\
SESSION_DRIVER=file\n\
SESSION_LIFETIME=120\n\
SESSION_ENCRYPT=false\n\
SESSION_PATH=/\n\
SESSION_DOMAIN=null\n\
\n\
BACKEND_INTERNAL_URL=http://dotnet_backend:8080\n\
BACKEND_BROWSER_URL=http://localhost:8080\n\
\n\
# OIDC / Socialite Settings\n\
SOCIALITE_IGNORE_SSL=true\n\
OIDC_CLIENT_ID=php-client\n\
OIDC_CLIENT_SECRET=secret-123\n\
OIDC_REDIRECT_URI=http://localhost:8000/login/callback\n\
EOF\n\
  mkdir -p database\n\
  touch database/database.sqlite\n\
fi\n\
\n\
# --- Dependencies ---\n\
if [ ! -d "vendor" ]; then\n\
  echo "--- Vendor folder missing. Running composer install... ---"\n\
  composer install --no-interaction --prefer-dist\n\
fi\n\
\n\
if ! composer show socialiteproviders/oidc > /dev/null 2>&1; then\n\
  echo "--- Adding OIDC Provider... ---"\n\
  composer require socialiteproviders/oidc --no-interaction\n\
fi\n\
\n\
# --- Security & Cleanup ---\n\
# Crucial: Generate the key IF it is empty in .env to stop 302 loops\n\
if [ -z "$(grep APP_KEY= .env | cut -d= -f2)" ]; then\n\
    echo "--- Generating Application Key ---"\n\
    php artisan key:generate\n\
fi\n\
\n\
php artisan config:clear\n\
php artisan cache:clear\n\
\n\
# --- Permissions ---\n\
chown -R www-data:www-data storage bootstrap/cache database\n\
chmod -R 775 storage bootstrap/cache database\n\
\n\
echo "--- Starting Apache ---"\n\
apache2-foreground' > /usr/local/bin/start-app.sh && chmod +x /usr/local/bin/start-app.sh

ENTRYPOINT ["/usr/local/bin/start-app.sh"]
