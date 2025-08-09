namespace FirstInteract.TelegramBot.Dto;

public class CallbackDto(string action)
{
    public string Action { get; } = action;
    
    public static CallbackDto FromString(string input)
    {
        var action = input.Split("|")[0];
        var cbDto = new CallbackDto(action);
        return cbDto;
    }
    
    public override string ToString()
    {
        return Action;
    }
}