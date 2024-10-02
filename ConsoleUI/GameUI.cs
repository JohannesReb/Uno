using BLL;
using Domain;

namespace ConsoleUI;

public class GameUI
{
    public UnoGameEngine Engine { get; set; }
    private string DisplaySeparator { get; set; } = "====================";
    private const int UILineOffset = 8;
    public int UserChoice { get; set; } = UILineOffset - 1;
    public HashSet<int> Selected { get; set; } = new HashSet<int>();

    public GameUI(UnoGameEngine unoGameEngine)
    {
        Engine = unoGameEngine;
    }

    private ConsoleKey InitializePlayers()
    {
        string? nickname = null;
        ConsoleKey key = ConsoleKey.Enter;
        
        int j;
        for (int i = 0; i < Engine.GameState.PlayerCount; i++)
        {
            var userChoice = 4;
            do
            {
                j = userChoice;
                Console.Clear();
                Console.WriteLine($"Player nr {i + 1}");
                Console.WriteLine("Choose Player Type: ");
                Console.WriteLine(DisplaySeparator);
                if (j == 4)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                }
                Console.WriteLine("Human");
                Console.BackgroundColor = ConsoleColor.Black;
                if (j == 5)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                }
                Console.WriteLine("AI");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine(DisplaySeparator);
                Console.WriteLine("x) Exit");
                Console.SetCursorPosition(0, userChoice);
                key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                    {
                        if (userChoice == 5) userChoice--;
                        else userChoice = 5;
                        break;
                    }
                    case ConsoleKey.DownArrow:
                    {
                        if (userChoice == 4) userChoice++;
                        else userChoice = 4;
                        break;
                    }
                    case ConsoleKey.X:
                    {
                        Engine.ResetGame();
                        return ConsoleKey.M;
                    }
                }
            } while (key != ConsoleKey.Enter);
            
            while (nickname == null)
            {
                Console.Clear();
                Console.CursorVisible = true;
                Console.Write($"Choose Player {i + 1}(default)'s nickname: ");
                nickname = Console.ReadLine()?.Trim();
                nickname = nickname == "" ? $"Player {i + 1}" : nickname;
                Console.CursorVisible = false;
            }
            Engine.AddPlayer(nickname, userChoice == 4 ? EPlayerType.Human : EPlayerType.Ai, null);
            nickname = null;
        }
        return key;
    }

    private void DrawHeader()
    {
        Console.WriteLine(">>>UNO<<<");
        Console.WriteLine($"{Engine.GetActivePlayerName()}'s turn!");
        DrawPlayers();
        Console.WriteLine("Last played card: " + Engine.GetLastPlayedCard());
        if (Engine.GetLastPlayedCard().CardColor == ECardColor.Wild)
        {
            Console.WriteLine($"Chosen Color: {Engine.GetChosenColor()}");
        }
        Console.WriteLine($"You have {Engine.GetActivePlayerCardCount()} cards");
        Console.WriteLine(DisplaySeparator);
    }

    private void DrawPlayers()
    {
        foreach (var player in Engine.GetPlayers())
        {
            Console.WriteLine($"{player} Card count: {Engine.GetPlayerCardCount(player)}");
        }
    }
    private void DrawFooter()
    {
        Console.WriteLine(DisplaySeparator);
        Console.WriteLine("s) Save game");
        Console.WriteLine("q) Quit game");
        Console.WriteLine("x) Exit game");
    }
    
    private void DrawPrivacyNote()
    {
        Console.Clear();
        Console.WriteLine(">>>UNO<<<");
        Console.WriteLine($"{Engine.GetActivePlayerName()}'s turn!");
        Console.WriteLine("Last played card: " + Engine.GetLastPlayedCard());
        Console.Write("Press any key to start your turn and reveal your cards on screen!: ");
        Console.ReadKey();
    }

    private void DrawCardsWithColors()
    {
        int i = UILineOffset;
        foreach (var card in Engine.GetActivePlayerCards())
        {
            if (Engine.IsLegalPlay(card))
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
            }
            if (Selected.Contains(i))
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            }
            if (i == UserChoice)
            {
                Console.BackgroundColor = Console.BackgroundColor == ConsoleColor.DarkGreen ? ConsoleColor.DarkCyan : ConsoleColor.DarkBlue;
            }
            Console.WriteLine(card);
            Console.BackgroundColor = ConsoleColor.Black;
            i++;
        }
    }
    private void DrawCards()
    {
        int i = UILineOffset;
        foreach (var card in Engine.GetActivePlayerCards())
        {
            Console.WriteLine(card);
            i++;
        }
    }
    private void DrawUI()
    {
        Console.Clear();
        DrawHeader();
        if (UILineOffset - 1 == UserChoice)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
        }
        Console.WriteLine("d) Draw a Card");
        Console.BackgroundColor = ConsoleColor.Black;
        DrawCardsWithColors();
        DrawFooter();
        
    }

    public ConsoleKey StartGame()
    {
        var key = InitializePlayers();
        if (key == ConsoleKey.M) return key;
        Engine.InitializeGame();
        return Run();
    }
    public ConsoleKey LoadGame(Guid id)
    {
        Engine.LoadGame(id);
        return Run();
    }

    private ConsoleKey Run()
    {
        Console.Clear();
        Player activePlayer;
        DrawPrivacyNote();
        ConsoleKey key;
        bool saved;
        bool aiPlayed;
        do
        {
            Selected.Clear();
            aiPlayed = false;
            saved = false;
            do
            {
                Console.WriteLine(Engine.GetPlayerCount());
                Console.WriteLine(Engine.GetActivePlayerName());
                activePlayer = Engine.GetActivePlayer();

                if (activePlayer.PlayerType == EPlayerType.Ai)
                {
                    
                    UserChoice++;
                    Card card;
                    do
                    {
                        card = activePlayer.Cards[UserChoice - UILineOffset];
                        if (Engine.IsLegalPlay(card))
                        {
                            Engine.PlayCard(card);
                            if (activePlayer.Cards.Count == 0)
                            {
                                Engine.SetGameOver();
                                break;
                            }

                            if (Engine.CardValueHasEffect(card.CardValue, ECardEffect.ChangeColor))
                            {
                                Engine.ChangeColor(ECardColor.Red);
                            }

                            if (Engine.CardValueHasEffect(card.CardValue, ECardEffect.SwapCards))
                            {
                                Engine.SwapCards(Engine.GetPlayers()[Engine.GetActivePlayerIndex() - 1 < 0 ? Engine.GetActivePlayerIndex() + 1 : Engine.GetActivePlayerIndex() - 1]);
                            }
                            break;
                        }
                        UserChoice++;
                    } while (UserChoice < activePlayer.Cards.Count + UILineOffset);

                    if (Engine.IsGameOver()) break;
                    if (!Engine.IsLegalPlay(card))
                    {
                        if (!Engine.Draw())
                        {
                            Engine.SetGameOver();
                            break;
                        }
                    }

                    if (!Engine.EndTurn())
                    {
                        Engine.SetGameOver();
                        break;
                    }
                    aiPlayed = true;
                    DrawPrivacyNote();
                    UserChoice = UILineOffset - 1;
                    break;
                }

                DrawUI();
                Console.SetCursorPosition(0, UserChoice);
                key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                    {
                        if (UserChoice > UILineOffset - 1) UserChoice--;
                        else UserChoice = activePlayer.Cards.Count + UILineOffset - 1;
                        break;
                    }
                    case ConsoleKey.DownArrow:
                    {
                        if (UserChoice < activePlayer.Cards.Count + UILineOffset - 1) UserChoice++;
                        else UserChoice = UILineOffset - 1;
                        break;
                    }
                    case ConsoleKey.LeftArrow:
                    {
                        Selected.Remove(UserChoice);
                        break;
                    }
                    case ConsoleKey.RightArrow:
                    {
                        if (Selected.Count == 0)
                        {
                            Selected.Add(UserChoice);
                        }
                        else if (Selected.Count == 1)
                        {
                            if (Engine.GameHasRule(EGameRule.PlaySameCardTogether))
                            {
                                int userChoice = default;
                                for (int i = UILineOffset; i < activePlayer.Cards.Count + UILineOffset; i++)
                                {
                                    if (Selected.TryGetValue(i, out userChoice))
                                    {
                                        break;
                                    }
                                }
                                if (Equals(activePlayer.Cards[userChoice - UILineOffset], activePlayer.Cards[UserChoice - UILineOffset])
                                    && activePlayer.Cards[userChoice - UILineOffset].CardColor != ECardColor.Wild)
                                {
                                    Selected.Add(UserChoice);
                                }
                                else
                                {
                                    Selected.Clear();
                                    Selected.Add(UserChoice);
                                }
                            }
                            else
                            {
                                Selected.Clear();
                                Selected.Add(UserChoice);
                            }
                        }
                        else if (Selected.Count == 2 && !Selected.Contains(UserChoice))
                        {
                            Selected.Clear();
                            Selected.Add(UserChoice);
                        }
                        break;
                    }
                    case ConsoleKey.S:
                    {
                        Engine.SaveGame();
                        saved = true;
                        if (ExitQuery(saved))
                        {
                            Engine.ResetGame();
                            return ConsoleKey.M;
                        }
                        
                        break;
                    }
                    case ConsoleKey.X:
                    {
                        if (saved)
                        {
                            Engine.ResetGame();
                            return ConsoleKey.M;
                        }

                        if (ExitQuery(saved))
                        {
                            Engine.ResetGame();
                            return ConsoleKey.M;
                        }
                        break;
                    }
                    case ConsoleKey.Q:
                    {
                        Engine.RemoveActivePlayer();
                        if (Engine.GetPlayerCount() < 2)
                        {
                            Engine.ResetGame();
                            return ConsoleKey.M;
                        }

                        if (!Engine.EndTurn())
                        {
                            Engine.SetGameOver();
                        }
                        
                        UserChoice = UILineOffset - 1;
                        break;
                    }
                    case ConsoleKey.D:
                    {
                        if (!Engine.HasPlayableCard(activePlayer))
                        {
                            if (!Engine.Draw())
                            {
                                Engine.SetGameOver();
                                break;
                            }
                            if (!Engine.EndTurn())
                            {
                                Engine.SetGameOver();
                                break;
                            }
                            UserChoice = UILineOffset - 1;
                            DrawPrivacyNote();
                            break;
                        }
                        Console.Clear();
                        Console.WriteLine("You can draw only when there are no playable cards in hand");
                        Console.ReadKey();
                        break;
                    }
                }
       
            } while (key != ConsoleKey.Enter);

            if (aiPlayed) continue;
            if (Engine.IsGameOver()) break;
            if (UserChoice == UILineOffset - 1)
            {
                if (!Engine.HasPlayableCard(activePlayer))
                {
                    if (!Engine.Draw())
                    {
                        Engine.SetGameOver();
                        break;
                    }
                    if (!Engine.EndTurn())
                    {
                        Engine.SetGameOver();
                        break;
                    }
                    
                    UserChoice = UILineOffset - 1;
                    DrawPrivacyNote();
                    continue;
                }
                Console.Clear();
                Console.WriteLine("You can draw only when there are no playable cards in hand");
                Console.ReadKey();
                continue;
            }
            
            if (Selected.Count == 0)
            {
                Selected.Add(UserChoice);
            }
            int prevChoice = activePlayer.Cards.Count + UILineOffset;
            int offSet = 0;
            bool second = false;
            
            foreach (var userChoice in Selected)
            {
                if (prevChoice < userChoice)
                {
                    offSet = 1;
                }

                prevChoice = userChoice;
                Card card = activePlayer.Cards[userChoice - UILineOffset - offSet];
            
                if (Engine.PlayCard(card))
                {
                    if (activePlayer.Cards.Count == 0)
                    {
                        Engine.SetGameOver();
                        break;
                    }
                    if (Engine.CardValueHasEffect(card.CardValue, ECardEffect.ChangeColor))
                    {
                        Engine.ChangeColor(AskColor());
                    }

                    if (Engine.CardValueHasEffect(card.CardValue, ECardEffect.SwapCards))
                    {
                        Engine.SwapCards(AskPlayer());
                    }

                    if (Selected.Count == 2 && second || Selected.Count == 1)
                    {
                        if (!Engine.EndTurn())
                        {
                            Engine.SetGameOver();
                            break;
                        }
                        UserChoice = UILineOffset - 1;
                        DrawPrivacyNote();
                    }
                    second = true;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine($"Card {card} cannot be played on {Engine.GetLastPlayedCard()}");
                    Console.ReadKey();
                    break;

                }
            }

            if (Engine.IsGameOver()) break;
            
        } while (Engine.GetPlayerCount() >= 2);
        Console.Clear();
        Console.WriteLine(!Engine.Draw() ? "The Game Resulted in a Draw!!!" : $"The Winner is {activePlayer}!!!");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Engine.ResetGame();
        return ConsoleKey.M;
    }

    private bool ExitQuery(bool isSaved)
    {
        var userChoice = 1;
        ConsoleKey key;
        do
        {
            Console.Clear();
            if (isSaved)
            {
                Console.WriteLine("Game is Saved!");
                Console.WriteLine("Do you wish to exit?");
            }
            else
            {
                Console.WriteLine("Exit without saving?");
            }
            if (userChoice == 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }
            Console.WriteLine("Yes");
            Console.BackgroundColor = ConsoleColor.Black;
            if (userChoice == 2)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }
            Console.WriteLine("No");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(0, userChoice);
            key = Console.ReadKey().Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                {
                    if (userChoice > 1) userChoice--;
                    break;
                }
                case ConsoleKey.DownArrow:
                {
                    if (userChoice < 2) userChoice++;
                    break;
                }
            }
       
        } while (key != ConsoleKey.Enter);

        if (userChoice == 1)
        {
            return true;
        }

        return false;
    }
    private ECardColor AskColor()
    {
        
        ConsoleKey key;
        var userChoice = UILineOffset + 1;
        do
        {
            Console.Clear();
            DrawHeader();
            Console.WriteLine("Choose color: ");
            if (userChoice == UILineOffset + 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }
            Console.WriteLine("red");
            Console.BackgroundColor = ConsoleColor.Black;
            if (userChoice == UILineOffset + 2)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }
            Console.WriteLine("green");
            Console.BackgroundColor = ConsoleColor.Black;
            if (userChoice == UILineOffset + 3)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }
            Console.WriteLine("blue");
            Console.BackgroundColor = ConsoleColor.Black;
            if (userChoice == UILineOffset + 4)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }
            Console.WriteLine("yellow");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(DisplaySeparator);
            Console.WriteLine("Your cards: ");
            DrawCards();
            Console.SetCursorPosition(0, userChoice);
            key = Console.ReadKey().Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                {
                    if (userChoice > UILineOffset + 1) userChoice--;
                    break;
                }
                case ConsoleKey.DownArrow:
                {
                    if (userChoice < UILineOffset + 4) userChoice++;
                    break;
                }
            }
       
        } while (key != ConsoleKey.Enter);
        return (ECardColor) userChoice - UILineOffset- 1;
        
    }

    
    private Player AskPlayer()
    {
        Console.Clear();
        DrawHeader();
        Console.WriteLine("Choose player to swap cards with: ");
        foreach (var player in Engine.GetPlayers())
        {
            Console.WriteLine($"{player} Card count: {player.Cards.Count}");
        }
        Console.WriteLine(DisplaySeparator);
        Console.WriteLine("Your cards: ");
        DrawCards();
        ConsoleKey key;
        var userChoice = UILineOffset + 1;
        var i = UILineOffset + 1;
        do
        {
            Console.Clear();
            DrawHeader();
            Console.WriteLine("Choose player to swap cards with: ");
            foreach (var player in Engine.GetPlayers())
            {
                if (i == userChoice)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                }
                Console.WriteLine($"{player} Card count: {player.Cards.Count}");
                Console.BackgroundColor = ConsoleColor.Black;
                i++;
            }
            Console.SetCursorPosition(0, userChoice);
            key = Console.ReadKey().Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                {
                    if (userChoice > UILineOffset + 1) userChoice--;
                    break;
                }
                case ConsoleKey.DownArrow:
                {
                    if (userChoice < Engine.GetPlayerCount() + UILineOffset) userChoice++;
                    break;
                }
            }
       
        } while (key != ConsoleKey.Enter);
        return Engine.GetPlayers()[userChoice - UILineOffset - 1];
        
    }
    
}