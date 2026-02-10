bash
./launch.sh up
bash
./launch.sh down

docker compose -f docker-compose.backend.yml up --build

docker compose -p translation-system --project-directory . -f backend/Translation.API/docker-compose.yml -f presentation/docker-compose.frontend.yml up --build

docker compose -f docker-compose.backend.yml -f docker-compose.frontend.yml up --build
docker compose -f docker-compose.frontend.yml up --build

docker compose run --rm backend dotnet ef database update

docker run -d --name translation-ui -p 8000:80 -v "${PWD}:/var/www/html" php-frontend
docker build -t translation-ui .


psql -U myuser -d translation_db
\dt

UPDATE "OpenIddictApplications" SET "RedirectUris" = '["http://localhost:8000/login/callback"]' WHERE "ClientId" = 'php-client';
