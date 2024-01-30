docker-compose -f docker-compose-postgres.yml up -d && rmdir /S /Q API\wwwroot

cd client && ng build --configuration production && cd .. && cd API && dotnet clean && dotnet run -e Development  --urls=https://localhost:5001