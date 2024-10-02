namespace Domain;

public class Card
{
    public ECardValue CardValue { get; set; }
    public ECardColor CardColor { get; set; }

    public Card(ECardValue cardValue, ECardColor cardColor)
    {
        CardValue = cardValue;
        CardColor = cardColor;
    }

    public override string ToString()
    {
        return $"{CardColor} {CardValue}";
    }

    public override bool Equals(object? obj)
    {
        var item = obj as Card;
        if (item == null)
        {
            return false;
        }
        return CardColor == item.CardColor && CardValue == item.CardValue;
    }
    public override int GetHashCode()
    {
        return (CardValue, CardColor).GetHashCode();
    }
}