namespace Domain;

public class GameState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<Player> Players { get; set; } = new List<Player>();
    public Stack<Guid> HandInMiddlePlayers { get; set; } = new Stack<Guid>();
    public bool HandInMiddle { get; set; } = false;
    public int PlayerCount { get; set; } = 2;
    public int ActivePlayerIndex { get; set; }
    public bool GameOver { get; set; } = false;
    public Player? Winner { get; set; }
    public ECycleDirection CycleDirection { get; set; } = ECycleDirection.Clockwise;
    public List<Card> PlayedCards { get; set; } = new();
    public List<Card> NotInPlayCards { get; set; } = new();
    public ECardColor ChosenColor { get; set; }
    public Dictionary<ECardValue, List<ECardEffect>> CardEffects { get; set; }
    public Dictionary<ECardValue, List<ECardProperty>> CardProperties { get; set; }
    public Dictionary<string, Card> StringToCard { get; set; } = new Dictionary<string, Card>();
    
    public List<EGameRule> GameRules { get; set; }
    public List<ECardEffect> EffectsToBeExecuted { get; set; } = new List<ECardEffect>();

    public GameState()
    {
        CardEffects = new Dictionary<ECardValue, List<ECardEffect>>();
        CardProperties = new Dictionary<ECardValue, List<ECardProperty>>();
        GameRules = new List<EGameRule>();
        GenerateDefaultEffects();
        GenerateDefaultProperties();
    }

    public void GenerateDefaultEffects()
    {
        CardEffects.Clear();
        foreach (ECardValue value in Enum.GetValuesAsUnderlyingType<ECardValue>())
        {
            if (value == ECardValue.WildDrawFour)
            {
                CardEffects.Add(value, new List<ECardEffect>()
                {
                    ECardEffect.Skip,
                    ECardEffect.Draw,
                    ECardEffect.Draw,
                    ECardEffect.Draw,
                    ECardEffect.Draw,
                    ECardEffect.ChangeColor
                });
            }
            else if (value == ECardValue.Wild)
            {
                CardEffects.Add(value, new List<ECardEffect>()
                {
                    ECardEffect.ChangeColor
                });
            }
            else if (value == ECardValue.Skip)
            {
                CardEffects.Add(value, new List<ECardEffect>()
                {
                    ECardEffect.Skip
                });
            }
            else if (value == ECardValue.Reverse)
            {
                CardEffects.Add(value, new List<ECardEffect>()
                {
                    ECardEffect.Reverse
                });
            }
            else if (value == ECardValue.DrawTwo)
            {
                CardEffects.Add(value, new List<ECardEffect>()
                {
                    ECardEffect.Skip,
                    ECardEffect.Draw,
                    ECardEffect.Draw
                });
            }
            else if (value <= ECardValue.Nine)
            {
                CardEffects.Add(value, new List<ECardEffect>());
            }
        }
    }

    public void GenerateDefaultProperties()
    {
        CardProperties.Clear();
        foreach (ECardValue value in Enum.GetValuesAsUnderlyingType<ECardValue>())
        {
            CardProperties.Add(value, new List<ECardProperty>());
        }
    }

    public void AddRules(ECardValue value, List<ECardEffect> effects)
    {
        CardEffects[value].AddRange(effects);
    }
    public void AddRule(ECardValue value, ECardEffect effect)
    {
        CardEffects[value].Add(effect);
    }
}