# training-api-gateway

API Gateway for the training platform. REST → gRPC.

## Назначение

Единая точка входа для фронта. Принимает REST-запросы на порту 5000, проксирует в gRPC-сервисы (auth 5002, training 5003, ai 5004). Проверяет JWT (кроме `/api/auth/*`).

## Структура

- `Controllers/` — REST-контроллеры
- `Middleware/` — JWT-валидация
- `proto/contracts/` — submodule с proto-файлами

## Эндпоинты

- `POST /api/auth/register` — регистрация
- `POST /api/auth/login` — логин
- `POST /api/auth/refresh` — обновление токена
- `POST /api/auth/logout` — выход
- `POST /api/auth/validate` — валидация токена
- `GET/POST/DELETE /api/training/exercises` — упражнения
- `GET/POST/DELETE /api/training/plans` — планы
- `GET/POST /api/training/cycle/:planId` — циклы
- `POST /api/training/progress/:planId` — прогресс
- `POST /api/ai/generate` — генерация плана через Groq
- `GET /api/ai/history` — история генераций

## Команды

```bash
dotnet build
dotnet run --project src/Training.ApiGateway
```

## Зависимости

- Docker: `training-postgres` (порт 5434)
- gRPC сервисы на портах 5002–5004
