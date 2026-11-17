# HomeWork 9 на основе структуры HomeWork 6

Проект сохраняет структуру и базовый HTTP-фреймворк из HomeWork 6. Добавлены возможности из HomeWork 9:
- отправка домашней работы на email через `GET /home-work/send?to=...`;
- обработка POST-формы на `/login`;
- вывод пользователей на `/user/list`;
- отдельный проект `MiniHttpServer.ORM` с CRUD-операциями `GetAllAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync`;
- SQL-скрипт создания базы и таблицы `Users`.

## Запуск
1. Установить .NET 10 SDK.
2. Открыть `HomeWork_6.slnx` в Visual Studio или собрать `MiniHttpServer.csproj`.
3. Выполнить `Database/OrisDb.sql` в SQL Server Management Studio.
4. Указать строку подключения в `settings.json` в поле `ConnectionString`.
5. Запустить проект.

Сервер по умолчанию доступен по адресу `http://127.0.0.1:9999/connection/`.

## Маршруты
- `GET /connection/` — стартовая страница Steam из HomeWork 6.
- `GET /connection/test/index` — пример HTML-ответа контроллера из HomeWork 6.
- `GET /connection/test/info` — JSON-ответ.
- `GET /connection/user/list` — пользователи из SQL Server.
- `GET /connection/home-work/send?to=адрес` — отправка письма с домашней работой.
- `POST /connection/login` — демонстрационная обработка формы.

Для отправки почты задать переменные окружения `SMTP_USER` и `SMTP_APP_PASSWORD`. Пароль приложения не хранить в исходниках. Обработчик `/login` намеренно не пересылает и не сохраняет введённый пароль пользователя.

ORM: пример использования находится в `Controllers/UserController.cs`. Для добавления, изменения и удаления записей используйте методы `InsertAsync`, `UpdateAsync` и `DeleteAsync` из `OrmContext`.
