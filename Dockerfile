FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY proto/contracts/ proto/contracts/
COPY "src/Training.ApiGateway/Training.ApiGateway.csproj" "src/Training.ApiGateway/"
RUN dotnet restore "src/Training.ApiGateway/Training.ApiGateway.csproj"
COPY . .
RUN dotnet publish "src/Training.ApiGateway/Training.ApiGateway.csproj" -c Release -o /out

FROM alpine:3.21 AS goose
RUN apk add --no-cache curl && \
    curl -sL https://github.com/pressly/goose/releases/download/v3.24.1/goose-linux-amd64 -o /goose && \
    chmod +x /goose

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
COPY --from=goose /goose /usr/local/bin/goose
COPY --from=build /out /app
COPY --from=build /src/src/Training.ApiGateway/Migrations /app/Migrations
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh
ENTRYPOINT ["/docker-entrypoint.sh"]
