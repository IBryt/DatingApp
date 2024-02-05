docker run -d --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:7.2.0-v8 && docker-compose -f docker-compose-postgres.yml up -d && rmdir /S /Q API\wwwroot

cd client && yarn install && ng build --configuration production && cd .. && cd API && dotnet clean && dotnet run -e Development  --urls=https://localhost:5001