-- Удаление таблиц если существуют
DROP TABLE IF EXISTS "ToDoItem";
DROP TABLE IF EXISTS "ToDoList";
DROP TABLE IF EXISTS "ToDoUser";

-- Пользователи
CREATE TABLE "ToDoUser" (
    "UserId" uuid PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL UNIQUE,
    "TelegramUserName" TEXT,
    "RegisteredAt" TIMESTAMP NOT NULL
);

-- Списки
CREATE TABLE "ToDoList" (
    "Id" uuid PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "UserId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "FK_ToDoList_User" FOREIGN KEY ("UserId") REFERENCES "ToDoUser"("UserId") ON DELETE CASCADE
);

-- Задачи
CREATE TABLE "ToDoItem" (
    "Id" uuid PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Name" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "Deadline" TIMESTAMP NOT NULL,
    "State" TEXT NOT NULL,
    "StateChangedAt" TIMESTAMP,
    "ListId" uuid,
    CONSTRAINT "FK_ToDoItem_User" FOREIGN KEY ("UserId") REFERENCES "ToDoUser"("UserId") ON DELETE CASCADE,
    CONSTRAINT "FK_ToDoItem_List" FOREIGN KEY ("ListId") REFERENCES "ToDoList"("Id") ON DELETE SET NULL
);

-- Индексы для внешних ключей
CREATE INDEX IF NOT EXISTS "IDX_ToDoList_UserId" ON "ToDoList"("UserId");
CREATE INDEX IF NOT EXISTS "IDX_ToDoItem_UserId" ON "ToDoItem"("UserId");
CREATE INDEX IF NOT EXISTS "IDX_ToDoItem_ListId" ON "ToDoItem"("ListId");

-- Уникальный индекс по TelegramUserId
CREATE UNIQUE INDEX IF NOT EXISTS "IDX_ToDoUser_TelegramUserId" ON "ToDoUser"("TelegramUserId");

