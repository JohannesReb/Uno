using BLL;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Pages.Play;

public class Index : PageModel
{
    private readonly DAL.AppDbContext _context;
    public UnoGameEngine Engine { get; set; }

    public Index(AppDbContext context)
    {
        _context = context;
        Engine = new UnoGameEngine(new GameRepositoryEF(_context));
    }

    [BindProperty(SupportsGet = true)]
    public Guid GameId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid PlayerId { get; set; }

    // [BindProperty(SupportsGet = true)]
    // public MultiSelectList? CardNames { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public bool Color { get; set; } = false;
    [BindProperty(SupportsGet = true)]
    public ECardColor? CardColor { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public bool Swap { get; set; } = false;
    
    [BindProperty(SupportsGet = true)]
    public Guid? SwapPlayer { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid? HandPlayer { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Uno { get; set; } = false;

    public string ErrorMessage { get; set; } = "";

    public Dictionary<string, Card> Dict { get; set; } = default!;
    
    
    public void OnGet()
    {
        Engine.LoadGame(GameId);
        if (Engine.IsGameOver()) return;
        // AI actions
        if (Engine.GetActivePlayer().PlayerType == EPlayerType.Ai)
        {
            foreach (var c in Engine.GetActivePlayerCards())
            {
                if (Engine.GetHandInMiddle())
                {
                    Engine.AddPlayerIdToHandInMiddle(Engine.GetActivePlayer().Id);
                    foreach (var player in Engine.GetPlayers().Where(p => p.PlayerType == EPlayerType.Ai))
                    {
                        Engine.AddPlayerIdToHandInMiddle(player.Id);
                    }
                    return;
                }
                
                if (Engine.IsLegalPlay(c))
                {
                    Engine.PlayCard(c);
                    if (Engine.GetActivePlayerCardCount() == 0)
                    {
                        Engine.SetGameOver();
                        Engine.SetGameWinner();
                        Engine.SaveGame();
                        return;
                    }
                    if (Engine.GetActivePlayerCardCount() == 1)
                    {
                        Engine.GetActivePlayer().HasCalledUno = true;
                    }

                    if (Engine.CardValueHasEffect(c.CardValue, ECardEffect.ChangeColor))
                    {
                        Engine.ChangeColor(ECardColor.Red);
                    }

                    if (Engine.CardValueHasEffect(c.CardValue, ECardEffect.SwapCards))
                    {
                        Engine.SwapCards(Engine.GetPlayers()[Engine.GetActivePlayerIndex() - 1 < 0 ? Engine.GetActivePlayerIndex() + 1 : Engine.GetActivePlayerIndex() - 1]);
                    }

                    if (!Engine.EndTurn())
                    {
                        Engine.SetGameOver();
                    }
                    Engine.SaveGame();
                    return;
                }
            }
            if (!Engine.Draw())
            {
                Engine.SetGameOver();
            }

            if (!Engine.EndTurn())
            {
                Engine.SetGameOver();
            }
            Engine.SaveGame();
            return;
        }
        // return on refresh with no input when effects are active
        if (Swap || Color || Engine.GetHandInMiddle())
        {
            return;
        }
        if (!Engine.HasPlayableCard(Engine.GetActivePlayer()))
        {
            if (!Engine.Draw())
            {
                Engine.SetGameOver();
                Engine.SaveGame();
                return;
            }
            Engine.EndTurn();
            Engine.SaveGame();
        }
    }
    public void OnPostUno()
    {
        Engine.LoadGame(GameId);
        if (Engine.IsGameOver()) return;
        foreach (var plyr in Engine.GetPlayers().Where(p => (p.Id == PlayerId || p.PlayerType == EPlayerType.Ai) && p.Cards.Count != 1))
        {
            plyr.HasCalledUno = false;
            Engine.SaveGame();
        }
        var playersWithOneCard = Engine.GetPlayers().Where(p => p.Cards.Count == 1).ToList();

        if (playersWithOneCard.Count == 0)
        {
            Engine.HandInMiddle(Engine.GetPlayers().First(p => p.Id == PlayerId)); // draw
        }
        else
        {
            foreach (var player in playersWithOneCard)
            {
                if (PlayerId == player.Id)
                {
                    player.HasCalledUno = true;
                }
                if (!player.HasCalledUno)
                {
                    Engine.HandInMiddle(player); // draw
                    Engine.HandInMiddle(player); // draw
                }
            }
        }
        Engine.SaveGame();
    }

    public void OnPostHand()
    {
        Engine.LoadGame(GameId);
        if (Engine.IsGameOver()) return;
        
        foreach (var player in Engine.GetPlayers().Where(p => p.PlayerType == EPlayerType.Ai))
        {
            Engine.AddPlayerIdToHandInMiddle(player.Id);
        }
        Engine.AddPlayerIdToHandInMiddle(PlayerId);
        Engine.SaveGame();
        
        if (Engine.HaveAllVotedInHandInMiddle())
        {
            Engine.HandInMiddle(Engine.GetPlayers().First(p => p.Id == Engine.GetLastFromHandInMiddle()));
            Engine.SetHandInMiddle(false);
            Engine.ClearHandInMiddle();
            Engine.SaveGame();
        }
        
        if (HandPlayer != null && !Swap && !Color && !Engine.GetHandInMiddle())
        {
            if (!Engine.EndTurn())
            {
                Engine.SetGameOver();
            }
            Engine.SaveGame();
        }
    }
    public void OnPostSwap()
    {
        Engine.LoadGame(GameId);
        if (Engine.IsGameOver()) return;
        
        Engine.SwapCards(Engine.GetPlayers().First(p => p.Id == SwapPlayer));
        Engine.SaveGame();
        Swap = false;
        
        if (SwapPlayer != null && !Swap && !Color && !Engine.GetHandInMiddle())
        {
            if (!Engine.EndTurn())
            {
                Engine.SetGameOver();
            }
            Engine.SaveGame();
        }
    }
    public void OnPostColor()
    {
        Engine.LoadGame(GameId);
        if (Engine.IsGameOver()) return;
        
        Engine.ChangeColor((ECardColor) CardColor);
        Engine.SaveGame();
        Color = false;
        
        if (CardColor != null && !Swap && !Color && !Engine.GetHandInMiddle())
        {
            if (!Engine.EndTurn())
            {
                Engine.SetGameOver();
            }
            Engine.SaveGame();
        }
    }
    [BindProperty(SupportsGet = true)]
    public string? CardName { get; set; }
    public void OnPostPlay()
    {
        Engine.LoadGame(GameId);
        if (Engine.IsGameOver()) return;
        
        
        Dict = Engine.GetCardDict();

        // Play when not your turn
        if (CardName != null && Engine.GetActivePlayer().Id != PlayerId && Engine.GameHasRule(EGameRule.PlaySameCardWithoutForwardableEffectsAnyTime))
        {
            Card sameCard = Dict[CardName];

            Player player = Engine.GetPlayers().First(player => player.Id == PlayerId);
            if (Engine.PlaySameCardAnyTime(sameCard, player) && Engine.GameHasRule(EGameRule.ChangeActivePlayer))
            {
                Engine.ChangeActivePlayer(player);
                if (Engine.CardValueHasEffect(sameCard.CardValue, ECardEffect.ChangeColor))
                {
                    Color = true;
                }
                if (Engine.CardValueHasEffect(sameCard.CardValue, ECardEffect.SwapCards))
                {
                    Swap = true;
                }
                if (Engine.CardValueHasEffect(sameCard.CardValue, ECardEffect.HandInMiddle))
                {
                    Engine.SetHandInMiddle(true);
                }
                Engine.SaveGame();
            }
        }
        // return if not your turn
        if (Engine.GetActivePlayer().Id != PlayerId) return;

        // List<string?> cardNames = [];
        // if (Engine.GameHasRule(EGameRule.PlaySameCardTogether))
        // {
        //     if (CardNames == null)
        //     {
        //         return;
        //     }
        //     cardNames = CardNames.Select(s => s!.Text).ToList();
        // }
        // else
        // {
        //     cardNames.Add(CardName);
        // }
        Card? card = null;
        // foreach (var cardName in cardNames)
        // {
        //     card = null;
            // Get card
        if (CardName != null) // cardName
        {
            card = Dict[CardName]; // cardName
        }
        if (card == null)
        {
            return; //continue
        }
        // Play card
        if (Engine.PlayCard(card))
        {
            if (Engine.GetActivePlayer().Cards.Count == 0)
            {
                Engine.SetGameOver();
                Engine.SetGameWinner();
                Engine.SaveGame();
                return;
            }
        
            if (Engine.CardValueHasEffect(card.CardValue, ECardEffect.ChangeColor))
            {
                Color = true;
            }
            if (Engine.CardValueHasEffect(card.CardValue, ECardEffect.SwapCards))
            {
                Swap = true;
            }
            if (Engine.CardValueHasEffect(card.CardValue, ECardEffect.HandInMiddle))
            {
                Engine.SetHandInMiddle(true);
            }

            if (!Color && !Swap && !Engine.GetHandInMiddle())
            {
                if (!Engine.EndTurn())
                {
                    Engine.SetGameOver();
                }
            }
            Engine.SaveGame();
        }
        else
        {
            ErrorMessage = $"{card} cannot be played on {Engine.GetLastPlayedCard()}";
        }
        // }
    }
}