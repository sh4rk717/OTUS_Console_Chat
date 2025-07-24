namespace FirstInteract.TelegramBot.Dto;

public class CallbackDto(string action)
{
    public string Action { get; } = action; // с помощью него будем определять за какое действие отвечает кнопка
    
    // На вход принимает строку вида "{action}|{prop1}|{prop2}...". Нужно создать CallbackDto с Action = action. Нужно учесть что в строке может не быть |, тогда всю строку сохраняем в Action.
    public static CallbackDto FromString(string input)
    {
        var action = input.Split("|")[0];
        var cbDto = new CallbackDto(action);
        return cbDto;
    }
    
    // Переопределить метод. Он должен возвращать Action
    public override string ToString()
    {
        return Action;
    }
}