FROM php:8.2-apache AS php-ui

# Install system dependencies
# Added libssl-dev and openssl for OIDC secure connections
RUN apt-get update && apt-get install -y \
    libpng-dev libonig-dev libxml2-dev \
    zip unzip git curl libssl-dev \
    && docker-php-ext-install mbstring exif pcntl bcmath gd \
    && a2enmod rewrite

# --- XDEBUG SETUP ---
RUN pecl install xdebug && docker-php-ext-enable xdebug

RUN echo "xdebug.mode=debug" >> /usr/local/etc/php/conf.d/docker-php-ext-xdebug.ini \
    && echo "xdebug.start_with_request=yes" >> /usr/local/etc/php/conf.d/docker-php-ext-xdebug.ini \
    && echo "xdebug.client_host=host.docker.internal" >> /usr/local/etc/php/conf.d/docker-php-ext-xdebug.ini \
    && echo "xdebug.client_port=9003" >> /usr/local/etc/php/conf.d/docker-php-ext-xdebug.ini \
    && echo "xdebug.log=/var/log/xdebug.log" >> /usr/local/etc/php/conf.d/docker-php-ext-xdebug.ini
# --------------------

ENV APACHE_DOCUMENT_ROOT /var/www/html/public
RUN sed -ri -e 's!/var/www/html!${APACHE_DOCUMENT_ROOT}!g' /etc/apache2/sites-available/*.conf

COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

WORKDIR /var/www/html

# AGGRESSIVE AUTO-INSTALL & CACHE FIX SCRIPT
RUN echo '#!/bin/bash\n\
if [ ! -f "composer.json" ]; then\n\
  echo "--- Empty folder detected. Installing Laravel... ---"\n\
  composer create-project laravel/laravel temp_laravel --stability=stable --prefer-dist --no-interaction\n\
  cp -rn temp_laravel/. .\n\
  rm -rf temp_laravel\n\
  php artisan key:generate\n\
fi\n\
\n\
echo "--- Ensuring OIDC dependencies are installed... ---"\n\
# This installs the provider needed for the [auth.oidc] guard\n\
composer require socialiteproviders/oidc --no-interaction\n\
\n\
echo "--- Clearing Laravel Cache to register new guards... ---"\n\
php artisan config:clear\n\
php artisan cache:clear\n\
\n\
echo "--- Fixing permissions... ---"\n\
chown -R www-data:www-data storage bootstrap/cache\n\
chmod -R 775 storage bootstrap/cache\n\
apache2-foreground' > /usr/local/bin/start-app.sh && chmod +x /usr/local/bin/start-app.sh

ENTRYPOINT ["/usr/local/bin/start-app.sh"]
