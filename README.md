# WebSocketChatClient

Проект содержит простой WebSocket чат-клиент на C# и небольшую тестовую Node.js WebSocket-серверную часть.

## Что в репозитории

- `WebSocketChatClient.slnx` — решение (Visual Studio / .NET).
- `WebSocketChatClient/` — .NET консольный клиент (C#).
  - `Program.cs` — точка входа клиента.
  - `WebSocketChatClient.csproj` — проектная конфигурация.
  - `packages.config`, `packages/` — поставляемые пакеты (например, Newtonsoft.Json).
- `websocket-server/` — простая серверная часть на Node.js для тестирования.
  - `package.json` — зависимости сервера.
  - `server.js` — простой WebSocket сервер.

## Требования

- Windows (инструкции ниже ориентированы на Windows).
- Node.js (для запуска сервера) — https://nodejs.org/
- .NET SDK (включая `dotnet` CLI) или Visual Studio (для клиента) — https://dotnet.microsoft.com/

## Быстрый запуск

1) Запустить тестовый сервер (Node.js)

Откройте PowerShell и выполните:

```powershell
cd websocket-server
npm install
node server.js
```

Сервер будет слушать на порту, указанном в `server.js` (по умолчанию 8080 или другой, смотрите код).

2) Запустить C# клиент

В новом окне PowerShell выполните:

```powershell
cd WebSocketChatClient
dotnet build WebSocketChatClient.csproj
dotnet run --project WebSocketChatClient.csproj
```

Или откройте `WebSocketChatClient.slnx` в Visual Studio и запустите проект `WebSocketChatClient` через отладчик.

Клиент подключится к серверу по адресу, указанному в `Program.cs` (измените при необходимости).

## Структура и настройки

- Если нужно поменять адрес/порт сервера, отредактируйте `Program.cs` в проекте клиента.
- `websocket-server/server.js` — минимальный сервер. Для продакшен-использования замените на полноценный WebSocket сервер (например, с проверкой авторизации, SSL и пр.).

## Зависимости

- Клиент использует `Newtonsoft.Json` (см. `packages/` и `packages.config`).
- Серверные зависимости перечислены в `websocket-server/package.json`.

## Отладка и полезные команды

- Сброс кэша npm: `npm cache clean --force`
- Очистка и повторная сборка .NET: `dotnet clean && dotnet build`

## Примечания

- Репозиторий предназначен для учебных и тестовых целей — демонстрация работы WebSocket клиента/сервера.
- Если у вас нет `dotnet` CLI, установите .NET SDK или откройте проект в Visual Studio.
