namespace MenuSystem;

public class MenuItem
{
    public string? Title { get; set; }
    public Guid GameId { get; set; }
    public Func<ConsoleKey>? MethodToRun { get; set; }
    public Func<string?>? DynamicTitle { get; set; }
    public Func<string?, ConsoleKey>? MethodWithInputString { get; set; }
    public Func<Guid, ConsoleKey>? MethodWithInputGuid { get; set; }
    
    public List<int>? Selected { get; set; }
}