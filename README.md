# Shortener — сервис коротких ссылок

Сервис на ASP.NET Core для создания коротких ссылок с асинхронной обработкой событий через Kafka и кэшированием в Redis.

## Стек

- **ASP.NET Core** — REST API
- **PostgreSQL** — основное хранилище
- **Redis** — кэширование
- **Kafka** — асинхронная обработка событий (producer/consumer)
- **Entity Framework Core** — доступ к данным
- **Docker Compose** — запуск всего стека одной командой

## Архитектура
```
Client → API → Kafka → Consumer → PostgreSQL
          ↓
        Redis (cache)
```

## Что реализовано

- Создание коротких ссылок с уникальным кодом
- Поддержка срока жизни ссылки (`expiresAt`)
- Редирект по короткому коду
- Кэширование через Redis
- Асинхронная обработка событий через Kafka (producer/consumer)
- Учёт количества переходов (`clickCount`)
- Swagger-документация

## API

### Создать короткую ссылку

**POST** `/api/shortener/shorten`

**Request body:**
```json
{
  "url": "https://yandex.ru/pogoda/ru?from=tableau_yabro&lat=54.7343&lon=83.1712",
  "expiresAt": "2026-12-31T23:59:59Z"
}
```

**Response body:**
```json
{
  "code": "cbA1wiG",
  "shortUrl": "http://localhost:5000/cbA1wiG",
  "originalUrl": "https://yandex.ru/pogoda/ru?from=tableau_yabro&lat=54.7343&lon=83.1712",
  "createdAt": "2026-10-09T05:46:27.667519+00:00",
  "expiresAt": "2026-12-31T23:59:59+00:00",
  "clickCount": 0
}
```

## Запуск
```bash
docker compose up
```

**После запуска**
* API: http://localhost:5000
* Swagger: http://localhost:5000/swagger

## Автор
Анисимов Роман — backend-разработчик на C# / ASP.NET Core.
