#!/bin/sh
set -e

echo "Running database migrations..."
goose -dir /app/Migrations postgres "$CONNECTIONSTRINGS__DEFAULTCONNECTION" up

echo "Starting service..."
exec dotnet /app/Training.ApiGateway.dll
