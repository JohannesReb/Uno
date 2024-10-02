using DAL;
using Domain;
namespace BLL;

public class UnoGameEngine
{
    public IGameRepository GameRepository { get; set; }
    public GameState GameState { get; set; } = new GameState();
    public GameState DefaultState { get; set; } = new GameState();
    public List<Card> Cards { get; set; } = new List<Card>();
    public Random rnd { get; set; } = new Random();

    public UnoGameEngine(IGameRepository repository)
    {
        GameRepository = repository;
    }
    
    public void SaveGame()
    {
        GameRepository.Save(GameState);
    }

    public void LoadGame(Guid id)
    {
        GameState = GameRepository.LoadGame(id);
    }
    public void DeleteGameFromDb()
    {
        GameRepository.DeleteGame(GameState.Id);
    }
    
    public void ResetGame()
    {
        GameState = new GameState();
    }
    public void InitializeGame()
    {
        if (GameState.NotInPlayCards.Count != 0 || GameState.PlayedCards.Count != 0 || GameState.Players.Any(player => player.Cards.Count != 0))
        {
            Console.WriteLine("Game is already in progress!");
            return;
        }
        CreateNewCards();
        ShuffleCards();
        DistributeCards();
        SetStartingCard();
    }

    public void CreateNewCards()
    {
        foreach (ECardColor color in Enum.GetValuesAsUnderlyingType<ECardColor>())
        {
            foreach (ECardValue value in Enum.GetValuesAsUnderlyingType<ECardValue>())
            {
                GameState.StringToCard.Add(new Card(value, color).ToString().Trim().Replace(" ", ""), new Card(value, color));
                if (color == ECardColor.Wild)
                {
                    if (value >= ECardValue.WildDrawFour)
                    {
                        Cards.Add(new Card(value, color));
                        Cards.Add(new Card(value, color));
                        Cards.Add(new Card(value, color));
                        Cards.Add(new Card(value, color));
                    }
                }
                else if (value == ECardValue.Zero)
                {
                    Cards.Add(new Card(value, color));
                }
                else if (value < ECardValue.WildDrawFour)
                {
                    Cards.Add(new Card(value, color));
                    Cards.Add(new Card(value, color));
                }
            }
        }
    }

    private void ShuffleCards()
    {
        while (Cards.Count > 0)
        {
            int i = rnd.Next(Cards.Count);
            GameState.NotInPlayCards.Add(Cards[i]);
            Cards.RemoveAt(i);
        }
    }
    public void ShuffleCardsDuringGame()
    {
        Card topCard = GameState.PlayedCards[^1];
        GameState.PlayedCards.Remove(topCard);
        while (GameState.PlayedCards.Count > 0)
        {
            int i = rnd.Next(GameState.PlayedCards.Count);
            GameState.NotInPlayCards.Add(GameState.PlayedCards[i]);
            GameState.PlayedCards.RemoveAt(i);
        }
        GameState.PlayedCards.Add(topCard);
    }

    private void DistributeCards()
    {
        foreach (var player in GameState.Players)
        {
            for (int i = 0; i < 7; i++)
            {
                player.Cards.Add(GameState.NotInPlayCards[0]);
                GameState.NotInPlayCards.RemoveAt(0);
            }
        }
    }
    
    private void SetStartingCard()
    {
        GameState.PlayedCards.Add(GameState.NotInPlayCards[0]);
        GameState.NotInPlayCards.RemoveAt(0);
        while (GameState.PlayedCards[^1].CardColor == ECardColor.Wild)
        {
            GameState.PlayedCards.Add(GameState.NotInPlayCards[0]);
            GameState.NotInPlayCards.RemoveAt(0);
        }
        
        GameState.EffectsToBeExecuted.AddRange(GameState.CardEffects[GameState.PlayedCards[0].CardValue]);
        ExecuteEffects();
    }

    private bool ExecuteEffects()
    {
        int i = 0;
        Console.WriteLine(i);
        Console.WriteLine(GameState.EffectsToBeExecuted.Count);
        while (GameState.EffectsToBeExecuted.Count > i)
        {
            ECardEffect effect = GameState.EffectsToBeExecuted[i];
            switch (effect)
            {
                case ECardEffect.Draw:
                    if (!Draw()) return false;
                    GameState.EffectsToBeExecuted.RemoveAt(i);
                    i--;
                    break;
                case ECardEffect.AllSwapCardsInOrder:
                    AllSwapCardsInOrder();
                    GameState.EffectsToBeExecuted.RemoveAt(i);
                    i--;
                    break;
            }
            i++;
        }

        foreach (var skip in GameState.EffectsToBeExecuted.Where
                     (effect => 
                         effect == ECardEffect.Reverse && GameState.Players.Count == 2 ||
                         effect == ECardEffect.Skip))
        {
            Skip();
        }
        GameState.EffectsToBeExecuted.Clear();
        return true;
    }

    public void SetRandomStartingPlayer()
    {
        GameState.ActivePlayerIndex = rnd.Next(GameState.Players.Count);
    }

    public bool HasPlayableCard(Player player)
    {
        foreach (var card in player.Cards)
        {
            if (IsLegalPlay(card))
            {
                return true;
            }
        }
        return false;
    }
    public bool IsLegalPlay(Card card)
    {
        Card lastPlayedCard = GameState.PlayedCards[^1];
        if (GameState.EffectsToBeExecuted.Count != 0)
        {
            if (GameState.CardProperties[card.CardValue].Contains(ECardProperty.Forwardable))
            {
                return lastPlayedCard.CardValue == card.CardValue;
            }
            return false;
        }
        return lastPlayedCard.CardColor == card.CardColor || 
               lastPlayedCard.CardValue == card.CardValue || 
               card.CardColor == ECardColor.Wild ||
               lastPlayedCard.CardColor == ECardColor.Wild && card.CardColor == GameState.ChosenColor;
    }

    public bool PlayCard(Card card)
    {
        if (!IsLegalPlay(card)) return false;
        GameState.PlayedCards.Add(card);
        GetActivePlayerCards().Remove(card);
        GameState.EffectsToBeExecuted.AddRange(GameState.CardEffects[card.CardValue]);
        GameState.ChosenColor = ECardColor.Wild;
        if (GameState.NotInPlayCards.Count == 1)
        {
            ShuffleCardsDuringGame();
        }
        return true;
    }

    public bool Draw()
    {
        if (GameState.NotInPlayCards.Count == 0)
        {
            return false;
        }
        GameState.Players[GameState.ActivePlayerIndex].Cards.Add(GameState.NotInPlayCards[0]);
        GameState.NotInPlayCards.RemoveAt(0);
        if (GameState.NotInPlayCards.Count == 1)
        {
            ShuffleCardsDuringGame();
        }

        return true;
    }

    public bool EndTurn()
    {
        if (GameState.Players.Count > 2)
        {
            while (GameState.EffectsToBeExecuted.Remove(ECardEffect.Reverse))
            {
                Reverse();
            }
        }
        GameState.ActivePlayerIndex += (int) GameState.CycleDirection;
        if (GameState.ActivePlayerIndex == GameState.Players.Count)
        {
            GameState.ActivePlayerIndex = 0;
        }
        else if (GameState.ActivePlayerIndex == -1)
        {
            GameState.ActivePlayerIndex = GameState.Players.Count - 1;
        }

        if (GameState.EffectsToBeExecuted.TrueForAll(effect => effect is ECardEffect.SwapCards or ECardEffect.AllSwapCardsInOrder or ECardEffect.HandInMiddle or ECardEffect.ChangeColor) ||
            !GameState.CardProperties[GameState.PlayedCards[^1].CardValue].Contains(ECardProperty.Forwardable) ||
            !HasPlayableCard(GetActivePlayer()))
        {
            if(!ExecuteEffects()) return false;
        }

        return true;
    }


    private void Skip()
    {
        GameState.ActivePlayerIndex += (int) GameState.CycleDirection;
        if (GameState.ActivePlayerIndex == GameState.Players.Count)
        {
            GameState.ActivePlayerIndex = 0;
        }
        else if (GameState.ActivePlayerIndex == -1)
        {
            GameState.ActivePlayerIndex = GameState.Players.Count - 1;
        }
    }
    private void Reverse()
    {
        GameState.CycleDirection = GameState.CycleDirection == ECycleDirection.Clockwise ? ECycleDirection.CounterClockwise : ECycleDirection.Clockwise;
    }
    public void ChangeColor(ECardColor color)
    {
        GameState.ChosenColor = color;
    }
    public void ChangeActivePlayer(Player player)
    {
        for (int i = 0; i < GetPlayerCount(); i++)
        {
            if (GameState.Players[i].Id == player.Id)
            {
                GameState.ActivePlayerIndex = i;
                break;
            }
        }
        
    }
    public void SwapCards(Player otherPlayer)
    {
        List<Card> cards = otherPlayer.Cards;
        Player currentPlayer = GameState.Players[GameState.ActivePlayerIndex];
        otherPlayer.Cards = currentPlayer.Cards;
        currentPlayer.Cards = cards;
    }
    private void AllSwapCardsInOrder()
    {
        List<Card> cards = GameState.Players[0].Cards;
        Player? lastPlayer = null;
        foreach (var player in GameState.Players)
        {
            
            if (lastPlayer == null)
            {
                lastPlayer = player;
                continue;
            }
            lastPlayer.Cards = player.Cards;
            lastPlayer = player;
        }
        lastPlayer!.Cards = cards;
    }
    public bool HandInMiddle(Player player)
    {
        if (GameState.NotInPlayCards.Count == 0)
        {
            return false;
        }
        player.Cards.Add(GameState.NotInPlayCards[0]);
        GameState.NotInPlayCards.RemoveAt(0);
        if (GameState.NotInPlayCards.Count == 1)
        {
            ShuffleCardsDuringGame();
        }
        return true;
    }
    public bool PlaySameCardAnyTime(Card card, Player player)
    {
        if (!(GameState.PlayedCards[^1].CardColor == card.CardColor && 
              GameState.PlayedCards[^1].CardValue == card.CardValue &&
              GameState.CardEffects[card.CardValue].TrueForAll(effect => effect is ECardEffect.SwapCards or ECardEffect.AllSwapCardsInOrder or ECardEffect.HandInMiddle or ECardEffect.ChangeColor))
            ) return false;
        GameState.PlayedCards.Add(card);
        player.Cards.Remove(card);
        return true;
    }
    
    
    // Simple query and setup methods
    
    public ECardValue GetCardValueFromString(string cardValue)
    {
        foreach (var value in GameState.CardEffects.Keys.Where(value => value.ToString().Equals(cardValue)))
        {
            return value;
        }
        throw new ArgumentException("String input Must be that of a ECardValue!");
    }
    public void AddPropertyToCardValue(ECardValue value, ECardProperty property)
    {
        GameState.CardProperties[value].Add(property);
    }
    public bool CardValueHasProperty(ECardValue value, ECardProperty property)
    {
        return GameState.CardProperties[value].Contains(property);
    }
    public bool CardValueHasPropertyByDefault(ECardValue value, ECardProperty property)
    {
        return DefaultState.CardProperties[value].Contains(property);
    }

    public void AddEffectToCardValue(ECardValue value, ECardEffect effect)
    {
        GameState.CardEffects[value].Add(effect);
    }
    public bool CardValueHasEffect(ECardValue value, ECardEffect effect)
    {
        return GameState.CardEffects[value].Contains(effect);
    }
    public bool CardValueHasEffectByDefault(ECardValue value, ECardEffect effect)
    {
        return DefaultState.CardEffects[value].Contains(effect);
    }

    public List<ECardEffect> GetCardEffects(ECardValue value)
    {
        return GameState.CardEffects[value];
    }
    public List<ECardProperty> GetCardProperties(ECardValue value)
    {
        return GameState.CardProperties[value];
    }
    public List<EGameRule> GetGameRules()
    {
        return GameState.GameRules;
    }
    
    public List<ECardEffect> GetDefaultCardEffects(ECardValue value)
    {
        return DefaultState.CardEffects[value];
    }
    public List<ECardProperty> GetDefaultCardProperties(ECardValue value)
    {
        return DefaultState.CardProperties[value];
    }
    public List<EGameRule> GetDefaultGameRules()
    {
        return DefaultState.GameRules;
    }

    public void SetCardEffects(ECardValue value, List<int> selected)
    {
        GameState.CardEffects[value].Clear();
        foreach (var i in selected)
        {
            GameState.CardEffects[value].Add((ECardEffect) i);
        }
    }
    public void SetCardProperties(ECardValue value, List<int> selected)
    {
        GameState.CardProperties[value].Clear();
        foreach (var i in selected)
        {
            GameState.CardProperties[value].Add((ECardProperty) i);
        }
    }
    public void SetGameRules(List<int> selected)
    {
        GameState.GameRules.Clear();
        foreach (var i in selected.Where(i => !GameState.GameRules.Contains((EGameRule) i)))
        {
            GameState.GameRules.Add((EGameRule) i);
        }
    }

    public bool GameHasRule(EGameRule rule)
    {
        return GameState.GameRules.Contains(rule);
    }

    public void AddPlayer(string nickName, EPlayerType playerType, Guid? id)
    {
        GameState.Players.Add(new Player(nickName, playerType){Id = id ?? Guid.NewGuid()});
    }
    public void RemovePlayer(Guid playerId)
    {
        GameState.Players.Remove(GameState.Players.First(p => p.Id.Equals(playerId)));
    }
    
    public int GetPlayerCount()
    {
        return GameState.PlayerCount;
    }

    public void IncreasePlayerCount()
    {
        GameState.PlayerCount++;
    }
    public void DecreasePlayerCount()
    {
        GameState.PlayerCount--;
    }

    public string GetActivePlayerName()
    {
        Player player = GetActivePlayer();
        return player.ToString()[..(player.PlayerType.ToString().Length + player.NickName.Length + 3)];
    }
    public string GetPlayerNameById(Guid id)
    {
        var player = GameState.Players.First(player => player.Id == id);
        return player.ToString()[..(player.PlayerType.ToString().Length + player.NickName.Length + 3)];
    }

    public Card GetLastPlayedCard()
    {
        return GameState.PlayedCards[^1];
    }

    public ECardColor GetChosenColor()
    {
        return GameState.ChosenColor;
    }

    public int GetActivePlayerCardCount()
    {
        return GetActivePlayerCards().Count;
    }
    public List<Card> GetActivePlayerCards()
    {
        return GetActivePlayer().Cards;
    }

    public int GetPlayerCardCount(Player player)
    {
        return player.Cards.Count;
    }

    public List<Player> GetPlayers()
    {
        return GameState.Players;
    }
    public Player GetActivePlayer()
    {
        return GameState.Players[GameState.ActivePlayerIndex];
    }

    public int GetActivePlayerIndex()
    {
        return GameState.ActivePlayerIndex;
    }
    public void SetActivePlayerByIndex(int index)
    {
        GameState.ActivePlayerIndex = index;
    }
    public void RemoveActivePlayer()
    {
        GameState.Players.Remove(GetActivePlayer());
    }
    public bool ActivePlayerHasCard(Card card)
    {
        return GetActivePlayer().Cards.Contains(card);
    }
    public void SetGameOver()
    {
        GameState.GameOver = true;
    }
    public bool IsGameOver()
    {
        return GameState.GameOver;
    }
    public void SetGameWinner()
    {
        GameState.Winner = GetActivePlayer();
    }
    public Player? GetGameWinner()
    {
        return GameState.Winner;
    }

    public Dictionary<string, Card> GetCardDict()
    {
        return GameState.StringToCard;
    }
    public bool GetHandInMiddle()
    {
        return GameState.HandInMiddle;
    }
    public void SetHandInMiddle(bool boolean)
    {
        GameState.HandInMiddle = boolean;
    }
    public void AddPlayerIdToHandInMiddle(Guid id)
    {
        if (!GameState.HandInMiddlePlayers.Contains(id))
        {
            GameState.HandInMiddlePlayers.Push(id);
        }
    }
    public bool IsPlayerIdInHandInMiddle(Guid id)
    {
        return GameState.HandInMiddlePlayers.Contains(id);
    }
    public bool HaveAllVotedInHandInMiddle()
    {
        return GameState.HandInMiddlePlayers.Count == GameState.PlayerCount;
    }
    public Guid GetLastFromHandInMiddle()
    {
        return GameState.HandInMiddlePlayers.Pop();
    }

    public void ClearHandInMiddle()
    {
        GameState.HandInMiddlePlayers.Clear();
    }
}