-- Получить все задачи пользователя
SELECT * FROM "ToDoItem" WHERE "UserId" = '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f';

-- Получить активные задачи пользователя
SELECT * FROM "ToDoItem" WHERE "UserId" = '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f' AND "State" = 'Active';

-- Получить задачу по Id
SELECT * FROM "ToDoItem" WHERE "Id" = '9fee7d58-4c83-4292-96dd-adb888dc0d59';

-- Проверка существования задачи с именем
SELECT EXISTS (SELECT 1 FROM "ToDoItem" WHERE "UserId" = '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f' AND "Name" = 'Item2');

-- Подсчитать количество активных задач пользователя
SELECT COUNT(*) cnt FROM "ToDoItem" WHERE "UserId" = '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f' AND "State" = 'Active';

-- Поиск задач по пользователю и имени задачи
SELECT * FROM "ToDoItem" WHERE "UserId" = '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f' AND "Name" LIKE 'за%';
SELECT * FROM "ToDoItem" WHERE "UserId" = '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f' AND "Name" ILIKE 'за%';

--Получить задачи пользователя из списка
SELECT * FROM "ToDoItem" WHERE "UserId" = '1d5eeffe-7984-41b3-b3ce-f0a78c33e43f' AND "ListId" = '2b31cfb2-e46f-4952-8249-4916ab5600be';

