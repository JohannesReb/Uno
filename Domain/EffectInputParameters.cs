namespace Domain;

public class EffectInputParameters
{
    public ECardColor CardColor { get; set; }
    public int PlayerIndex { get; set; }
    public Player Player { get; set; } = default!;
}