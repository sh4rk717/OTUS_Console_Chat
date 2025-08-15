-- INSERT ToDoUser
INSERT INTO "ToDoUser"("UserId", "TelegramUserId", "TelegramUserName", "RegisteredAt")
VALUES
	('1d5eeffe-7984-41b3-b3ce-f0a78c33e43f', 156896485 , 'sh4rk717'   , NOW()                   ),
	('9c2dc079-c5dc-46ac-9534-f2d16f6bb09e', 5212017623, 'anotherUser', NOW() - INTERVAL '1 day');

-- INSERT ToDoList
INSERT INTO "ToDoList"("Id", "Name", "UserId", "CreatedAt")
VALUES
	('2b31cfb2-e46f-4952-8249-4916ab5600be', 'Лист1', '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f', NOW() + INTERVAL '1 hour'),
	('17ffb1be-cc89-434c-adca-08cb7d33e0f4', 'Список2', '9c2dc079-c5dc-46ac-9534-f2d16f6bb09e', NOW() - INTERVAL '1 hour');

-- INSERT ToDoItem
INSERT INTO "ToDoItem"("Id", "UserId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline", "ListId")
VALUES
	('6cc8e942-a3b3-4c0d-89ee-33e2c7206452', '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f', 'Задача1', NOW() + INTERVAL '2 hours', 'Active'   , NULL , NOW() + INTERVAL '1 day', '2b31cfb2-e46f-4952-8249-4916ab5600be'),
	('9fee7d58-4c83-4292-96dd-adb888dc0d59', '9c2dc079-c5dc-46ac-9534-f2d16f6bb09e', 'Item2'  , NOW()                     , 'Completed', NOW(), NOW() + INTERVAL '2 years', '17ffb1be-cc89-434c-adca-08cb7d33e0f4');

--SELECT * FROM public."ToDoUser";
--SELECT * FROM public."ToDoItem" ORDER BY "Id";
--SELECT * FROM public."ToDoList" ORDER BY "Id";

