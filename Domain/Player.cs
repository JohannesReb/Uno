namespace Domain;

public class Player(string nickName, EPlayerType playerType)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NickName { get; set; } = nickName;
    public EPlayerType PlayerType { get; set; } = playerType;
    public List<Card> Cards { get; set; } = new List<Card>();
    public bool HasCalledUno { get; set; } = false;

    public override string ToString()
    {
        return $"({PlayerType}) {NickName} - {Id}";
    }
}