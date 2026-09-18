# Деплой на VPS (Docker Compose, доступ по IP)

Инструкция для ручного деплоя на чистый VPS (например, DigitalOcean droplet)
без домена — приложение будет доступно по `http://IP_СЕРВЕРА:8080`.

## 1. Подготовка сервера

Подключитесь по SSH и установите Docker:

```bash
ssh root@IP_СЕРВЕРА

curl -fsSL https://get.docker.com | sh
```

Docker Compose v2 идёт вместе с Docker (команда `docker compose`, без дефиса).
Проверьте:

```bash
docker compose version
```

Создайте отдельного пользователя без root (по желанию, но рекомендуется):

```bash
adduser deploy
usermod -aG docker deploy
su - deploy
```

## 2. Файрвол

Откройте только нужные порты — SSH и порт приложения:

```bash
sudo ufw allow OpenSSH
sudo ufw allow 8080/tcp
sudo ufw enable
sudo ufw status
```

Порт SQL Server наружу не открываем — в `docker-compose.yml` он и так
не публикуется (`db` доступна только внутри сети compose).

## 3. Клонирование репозитория

```bash
git clone https://github.com/dolbolesya/conference-booking.git
cd conference-booking
```

Для будущих обновлений код будет обновляться через `git pull` (см. раздел 6).

## 4. Настройка `.env`

```bash
cp .env.example .env
nano .env
```

Заполните **реальными продакшен-значениями**, не оставляйте примеры из
`.env.example`:

```env
DB_PASSWORD=<сложный пароль, минимум 12 символов, разные регистры/цифры/спецсимволы>
JWT_KEY=<случайная строка минимум 32 символа>
ADMIN_EMAIL=<ваша почта админа>
ADMIN_PASSWORD=<сложный пароль админа>
```

Сгенерировать случайный `JWT_KEY` можно так:

```bash
openssl rand -base64 48
```

`.env` не попадает в git (проверьте `.gitignore`) — храните его только на
сервере.

`ASPNETCORE_ENVIRONMENT` теперь по умолчанию `Production` (задаётся в
`docker-compose.yml`), явно указывать в `.env` не нужно.

## 5. Запуск

```bash
docker compose up -d --build
```

Флаг `-d` — фоновый режим. Первый запуск дольше: SQL Server поднимается,
проходит healthcheck, затем стартует API и применяет миграции.

Проверка:

```bash
docker compose ps
docker compose logs -f api
```

Приложение должно быть доступно на `http://IP_СЕРВЕРА:8080/swagger`.

Оба сервиса теперь запускаются с `restart: unless-stopped` — переживут
перезагрузку VPS.

## 6. Обновление после изменений в репозитории

```bash
cd conference-booking
git pull
docker compose up -d --build
```

Пересоберётся только то, что изменилось (кэш слоёв Docker), база данных
(volume `mssql-data`) сохранится.

## 7. Бэкап базы данных

Данные SQL Server лежат в volume `mssql-data`. Пример бэкапа через
`sqlcmd`/`BACKUP DATABASE` внутри контейнера:

```bash
docker compose exec db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$DB_PASSWORD" -C \
  -Q "BACKUP DATABASE ConferenceBooking TO DISK = N'/var/opt/mssql/backup.bak'"

docker compose cp db:/var/opt/mssql/backup.bak ./backup.bak
```

## 8. Полная остановка

```bash
docker compose down        # оставить данные
docker compose down -v     # удалить и volume с базой (осторожно!)
```

## Дальше (по желанию)

- Домен + HTTPS: поставить Nginx или Caddy как reverse proxy перед `api`
  и получить сертификат через Let's Encrypt (certbot / встроенный auto-TLS
  в Caddy). Тогда порт 8080 наружу можно закрыть, оставить 80/443.
- Автодеплой: GitHub Actions workflow, который по push в `main` подключается
  по SSH и выполняет шаги из раздела 6.
