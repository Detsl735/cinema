# Cinema

REST API для онлайн-кинотеки на .NET 8.0 (ASP.NET Core).  
Проект предоставляет функционал пользователей, аутентификации (JWT), хранения/передачи медиафайлов (MinIO), истории просмотров и файл-обработки (FFmpeg).

---

## Краткий обзор

- Язык/платформа: .NET 8.0 (C#)
- Web framework: ASP.NET Core (Minimal/Web API + Controllers)
- Аутентификация: JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- DB: PostgreSQL (`Npgsql`)
- Файловое хранилище: MinIO (`Minio` SDK)
- Логирование: Serilog
- Документация API: Swagger
- Обработка медиа: FFmpeg (`FFmpegService`)

Структура проекта (ключевые папки/файлы):
- `Controllers/` — `AuthController`, `MovieController`, `FileController`, `ReviewController`, `WatchHistoryController`, `UserController`
- `Services/` — сервисы бизнес-логики
- `Data/` — repository + `DatabaseHelper`
- `Models/` — сущности (Movie, User, Review, File, WatchHistory)
- `DTOs/` — DTO для запросов/ответов
- `Configurations/` — `MinioSettings`
- `Program.cs` — конфигурация приложения, auth, swagger, CORS и т.д.

---
