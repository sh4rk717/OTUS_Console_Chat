namespace FirstInteract;

public class TaskCountLimitException(int taskCountLimit) : Exception($"Превышено максимальное количество задач равное {taskCountLimit}");

public class TaskLengthLimitException(int taskLength, int taskLengthLimit) : Exception($"Длина задачи '{taskLength}' превышает максимально допустимое значение {taskLengthLimit}");

public class DuplicateTaskException(string task) : Exception($"Задача '{task}' уже существует");
