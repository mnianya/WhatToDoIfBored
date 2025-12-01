# WhatToDoIfBored — WPF приложение

**WhatToDoIfBored** — это учебное WPF-приложение, созданное для практики работы с:
- XAML и WPF
- PostgreSQL
- Регистрацией и авторизацией пользователей
- Профильной системой и загрузкой пользовательского контента
- Хранением изображений и путей в базе данных и JSON-файлах

Приложение является учебным проектом и содержит базовую функциональность для работы с профилями, любимыми активностями и их отображением.

## Основной функционал

- Регистрация и авторизация пользователей  
- Валидация email и паролей  
- Хранение пользователей и данных в PostgreSQL  
- Загрузка и вывод изображений из JSON и файловой системы  
- Профиль пользователя с любимыми активностями 

## Важная информация о путях (Paths)

Так как это мой **первый полноценный проект**, многие пути в коде были заданы **абсолютными**.

Это касается:
- JSON-файлов с путями к изображениям
- загрузки изображений в профиле

### Поэтому после скачивания проекта:
Вам необходимо **заменить абсолютные пути на свои**.  
Без этого часть функционала (например, загрузка фото или данные профиля) работать не будет.

Это важное ограничение текущей версии проекта.


## Работа с базой данных

В проекте используется **PostgreSQL**.  
Dump базы можно найти в databases.sql.

Внимание:  
Dump содержит **только структуру**.

## Настройка проекта после загрузки

1. Открыть решение в Visual Studio  
2. Установить недостающие NuGet-пакеты (они восстановятся автоматически)  
3. Обновить пути в:
   - JSON-файлах с изображениями
   - классах, где путь прописан вручную
4. Указать правильную строку подключения к вашей БД PostgreSQL

## Screenshots
1. Main Window
<img width="924" height="491" alt="Main Window" src="https://github.com/user-attachments/assets/968a2282-8812-4d08-ae5b-1067e1bdfb1f" />

2. Registration Window
<img width="916" height="491" alt="Registration Window" src="https://github.com/user-attachments/assets/38fc5d0d-5746-4054-bc18-554e7e3088b7" />

3. First Screen (Activity Request)
<img width="921" height="491" alt="Initial Activity Screen" src="https://github.com/user-attachments/assets/bc30e71b-537a-4066-96ab-8f9e0fd1de58" />

4. Activity Display
<img width="919" height="491" alt="Activity Display" src="https://github.com/user-attachments/assets/7094399c-e3cb-49b3-98e4-3500e47086ee" />

5. Filters
<img width="918" height="491" alt="Filters Window" src="https://github.com/user-attachments/assets/f147c037-e0da-4818-847f-97e55f3611d2" />

6. Profile
<img width="924" height="491" alt="Profile Window" src="https://github.com/user-attachments/assets/f3a55e75-bfab-4a7d-a838-50ebdbb9e2b0" />





