// See https://aka.ms/new-console-template for more information


using BLL;
using ConsoleUI;
using DAL;
using Domain;
using MenuSystem;
using Microsoft.EntityFrameworkCore;

string connectionString = "Data Source=<%temppath%>uno.db";
connectionString = connectionString.Replace("<%temppath%>", Path.GetTempPath());


var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connectionString)
    .EnableDetailedErrors()
    .EnableSensitiveDataLogging()
    .Options;

using var db = new AppDbContext(contextOptions);


db.Database.Migrate();


// IGameRepository repository = new GameRepositoryFileSystem();
IGameRepository repository = new GameRepositoryEF(db);
UnoGameEngine gameEngine = new UnoGameEngine(repository);
GameUI consoleGameUi = new GameUI(gameEngine);


// ============ Menu Initializations ===============

// Effects, Properties, Game Rules

var selected = new List<int>();

List<MenuItem> effectItems = new List<MenuItem>();

foreach (ECardEffect effect in Enum.GetValuesAsUnderlyingType(typeof(ECardEffect)))
{
    effectItems.Add(new MenuItem()
    {
        DynamicTitle = () => $"{effect} {selected.FindAll((x) => x == (int) effect).Count}"
    });
}

var effects = new Menu(
    EMenuLevel.Other,
    "Effects",
    effectItems,
    selected);


List<MenuItem> propertyItems = new List<MenuItem>();

foreach (ECardProperty property in Enum.GetValuesAsUnderlyingType(typeof(ECardProperty)))
{
    propertyItems.Add(new MenuItem()
    {
        DynamicTitle = () => $"{property} {selected.FindAll((x) => x == (int) property).Count}"
    });
}

var properties = new Menu(
    EMenuLevel.Other,
    "Properties",
    propertyItems,
    selected);


List<MenuItem> gameRuleItems = new List<MenuItem>();

foreach (EGameRule gameRule in Enum.GetValuesAsUnderlyingType(typeof(EGameRule)))
{
    gameRuleItems.Add(new MenuItem()
    {
        Title = gameRule.ToString()
    });
}

var gameRules = new Menu(
    EMenuLevel.Other,
    "Rules",
    gameRuleItems,
    selected);


//=============== Rule Methods Start ==============

ConsoleKey SetRegularCardEffects()
{
    ConsoleKey userChoice = ConsoleKey.C;
    do
    {
        selected.Clear();
        bool valueHasEffect;
        foreach (ECardEffect effect in Enum.GetValuesAsUnderlyingType(typeof(ECardEffect)))
        {
            valueHasEffect = true;
            for (int value = 0; value < 10; value++)
            {
                if (userChoice == ConsoleKey.R)
                {
                    if (!gameEngine.CardValueHasEffectByDefault((ECardValue) value, effect))
                    {
                        valueHasEffect = false;
                        break;
                    }
                }
                else
                {
                    if (!gameEngine.CardValueHasEffect((ECardValue) value, effect))
                    {
                        valueHasEffect = false;
                        break;
                    }
                }
            }

            if (valueHasEffect)
            {
                selected.Add((int) effect);
            }
        }
        
        userChoice = effects.Run();
        
    } while (userChoice == ConsoleKey.R);

    
    for (int value = 0; value < 10; value++)
    {
        foreach (var i in selected)
        {
            if (!gameEngine.CardValueHasEffect((ECardValue) value, (ECardEffect) i))
            {
                gameEngine.AddEffectToCardValue((ECardValue) value, (ECardEffect) i);
            }
        }
    }
    
    return userChoice;
}
ConsoleKey SetAllCardEffects()
{
    ConsoleKey userChoice = ConsoleKey.C;
    do
    {
        selected.Clear();
        bool valueHasEffect;
        foreach (ECardEffect effect in Enum.GetValuesAsUnderlyingType(typeof(ECardEffect)))
        {
            valueHasEffect = true;
            foreach (ECardValue value in Enum.GetValuesAsUnderlyingType(typeof(ECardValue)))
            {
                if (userChoice == ConsoleKey.R)
                {
                    if (!gameEngine.CardValueHasEffectByDefault(value, effect))
                    {
                        valueHasEffect = false;
                        break;
                    }
                }
                else
                {
                    if (!gameEngine.CardValueHasEffect(value, effect))
                    {
                        valueHasEffect = false;
                        break;
                    }
                }
            }

            if (valueHasEffect)
            {
                selected.Add((int) effect);
            }
        }
        
        userChoice = effects.Run();
        
    } while (userChoice == ConsoleKey.R);

    
    foreach (ECardValue value in Enum.GetValuesAsUnderlyingType(typeof(ECardValue)))
    {
        foreach (var i in selected)
        {
            if (!gameEngine.CardValueHasEffect(value, (ECardEffect) i))
            {
                gameEngine.AddEffectToCardValue(value, (ECardEffect) i);
            }
        }
    }
    
    return userChoice;
}
ConsoleKey SetRegularCardProperties()
{
    ConsoleKey userChoice = ConsoleKey.C;
    do
    {
        selected.Clear();
        bool valueHasProperty;
        foreach (ECardProperty property in Enum.GetValuesAsUnderlyingType(typeof(ECardProperty)))
        {
            valueHasProperty = true;
            for (int value = 0; value < 10; value++)
            {
                if (userChoice == ConsoleKey.R)
                {
                    if (!gameEngine.CardValueHasPropertyByDefault((ECardValue) value, property))
                    {
                        valueHasProperty = false;
                        break;
                    }
                }
                else
                {
                    if (!gameEngine.CardValueHasProperty((ECardValue) value, property))
                    {
                        valueHasProperty = false;
                        break;
                    }
                }
                
            }

            if (valueHasProperty)
            {
                selected.Add((int) property);
            }
        }
        
        userChoice = properties.Run();
        
    } while (userChoice == ConsoleKey.R);

    
    for (int value = 0; value < 10; value++)
    {
        foreach (var i in selected)
        {
            if (!gameEngine.CardValueHasProperty((ECardValue) value, (ECardProperty) i))
            {
                gameEngine.AddPropertyToCardValue((ECardValue) value, (ECardProperty) i);
            }
        }
    }
    
    return userChoice;
}
ConsoleKey SetAllCardProperties()
{
    ConsoleKey userChoice = ConsoleKey.C;
    do
    {
        selected.Clear();
        bool valueHasProperty;
        foreach (ECardProperty property in Enum.GetValuesAsUnderlyingType(typeof(ECardProperty)))
        {
            valueHasProperty = true;
            foreach (ECardValue value in Enum.GetValuesAsUnderlyingType(typeof(ECardValue)))
            {
                if (userChoice == ConsoleKey.R)
                {
                    if (!gameEngine.CardValueHasPropertyByDefault(value, property))
                    {
                        valueHasProperty = false;
                        break;
                    }
                }
                else
                {
                    if (!gameEngine.CardValueHasProperty(value, property))
                    {
                        valueHasProperty = false;
                        break;
                    }
                }
                
            }

            if (valueHasProperty)
            {
                selected.Add((int) property);
            }
        }
        
        userChoice = properties.Run();
        
    } while (userChoice == ConsoleKey.R);

    
    foreach (ECardValue value in Enum.GetValuesAsUnderlyingType(typeof(ECardValue)))
    {
        foreach (var i in selected)
        {
            if (!gameEngine.CardValueHasProperty(value, (ECardProperty) i))
            {
                gameEngine.AddPropertyToCardValue(value, (ECardProperty) i);
            }
        }
    }
    
    return userChoice;
}

ConsoleKey SetCardEffects(string cardValue)
{
    var value = gameEngine.GetCardValueFromString(cardValue);
    ConsoleKey userChoice = ConsoleKey.C;
    do
    {
        selected.Clear();
        selected.AddRange(userChoice == ConsoleKey.R
            ? gameEngine.GetDefaultCardEffects(value).Select(effect => (int)effect)
            : gameEngine.GetCardEffects(value).Select(effect => (int)effect));

        userChoice = effects.Run();
        
    } while (userChoice == ConsoleKey.R);
    
    gameEngine.SetCardEffects(value, selected);
    
    return userChoice;
}

ConsoleKey SetCardProperties(string cardValue)
{
    var value = gameEngine.GetCardValueFromString(cardValue);
    ConsoleKey userChoice = ConsoleKey.C;
    do
    {
        selected.Clear();
        selected.AddRange(userChoice == ConsoleKey.R
            ? gameEngine.GetDefaultCardProperties(value).Select(property => (int)property)
            : gameEngine.GetCardProperties(value).Select(property => (int)property));

        userChoice = properties.Run();
        
    } while (userChoice == ConsoleKey.R);
    
    gameEngine.SetCardProperties(value, selected);
    
    return userChoice;
}

ConsoleKey SetGameRules()
{
    ConsoleKey userChoice = ConsoleKey.C;
    do
    {
        selected.Clear();
        selected.AddRange(userChoice == ConsoleKey.R
            ? gameEngine.GetDefaultGameRules().Select(rule => (int)rule)
            : gameEngine.GetGameRules().Select(rule => (int)rule));

        userChoice = gameRules.Run();
        
    } while (userChoice == ConsoleKey.R);

    gameEngine.SetGameRules(selected);
    
    return userChoice;
}


//=============== Rule Methods End ==============




// Card Effects and Properties, Game Rules


List<MenuItem> cardEffectItems = new List<MenuItem>();

cardEffectItems.Add(new MenuItem()
{
    Title = "Cards one through nine",
    MethodToRun = SetRegularCardEffects
});
cardEffectItems.Add(new MenuItem()
{
    Title = "All cards",
    MethodToRun = SetAllCardEffects
});

foreach (ECardValue value in Enum.GetValuesAsUnderlyingType(typeof(ECardValue)))
{
    cardEffectItems.Add(new MenuItem()
    {
        Title = value.ToString(),
        MethodWithInputString = SetCardEffects
    });
}

var setCardEffects = new Menu(
    EMenuLevel.Other,
    "Card Effects",
    cardEffectItems);

List<MenuItem> cardPropertyItems = new List<MenuItem>();

cardPropertyItems.Add(new MenuItem()
{
    Title = "Cards one through nine",
    MethodToRun = SetRegularCardProperties
});
cardPropertyItems.Add(new MenuItem()
{
    Title = "All cards",
    MethodToRun = SetAllCardProperties
});

foreach (ECardValue value in Enum.GetValuesAsUnderlyingType(typeof(ECardValue)))
{
    cardPropertyItems.Add(new MenuItem()
    {
        Title = value.ToString(),
        MethodWithInputString = SetCardProperties
    });
}


var setCardProperties = new Menu(
    EMenuLevel.Other,
    "Card Properties",
    cardPropertyItems);


var rules = new Menu(
    EMenuLevel.Other,
    "Rules",
    new List<MenuItem>()
    {
        new MenuItem()
        {
            Title = "Card Effects",
            MethodToRun = setCardEffects.Run
        },
        new MenuItem()
        {
            Title = "Card Properties",
            MethodToRun = setCardProperties.Run
        },
        new MenuItem()
        {
            Title = "Game Rules",
            MethodToRun = SetGameRules
        }
    });


// Players

ConsoleKey AddPlayer()
{
    if (gameEngine.GetPlayerCount() < 7) gameEngine.IncreasePlayerCount();
    return ConsoleKey.B;
}
ConsoleKey RemovePlayer()
{
    if (gameEngine.GetPlayerCount() > 2) gameEngine.DecreasePlayerCount();
    return ConsoleKey.B;
}

var players = new Menu(EMenuLevel.Other, "Players", 
    new List<MenuItem>()
{
    new MenuItem()
    {
        DynamicTitle = () => $"Players (2-7): {gameEngine.GetPlayerCount()}",
        MethodToRun = () => ConsoleKey.B
    },
    new MenuItem()
    {
        Title = "Add player",
        MethodToRun = AddPlayer
    },
    new MenuItem()
    {
        Title = "Remove player",
        MethodToRun = RemovePlayer
    }
});

// Basic menus


var newGame = new Menu(
    EMenuLevel.Second,
    "New Game",
    new List<MenuItem>()
    {
        new MenuItem()
        {
            Title = "Start Game",
            MethodToRun = consoleGameUi.StartGame
        },
        new MenuItem()
        {
            DynamicTitle = () => $"Players (max 7): {gameEngine.GetPlayerCount()}",
            MethodToRun = players.Run
        },
        new MenuItem()
        {
            Title = "Rules",
            MethodToRun = rules.Run
        }
        
    });



var savedGames = new List<MenuItem>();

var loadGame = new Menu(
    EMenuLevel.Second,
    "Load Game",
    savedGames);


ConsoleKey LoadGame()
{
    savedGames.Clear();
    foreach (var game in repository.GetSaveGames())
    {
        savedGames.Add(new MenuItem()
        {
            Title = $"{game.id} - {game.dt}",
            GameId = game.id,
            MethodWithInputGuid = consoleGameUi.LoadGame
        });
    }
    return loadGame.Run();
}

var consoleColors = new Menu(
    EMenuLevel.Second,
    "Console color",
    new List<MenuItem>(){
        new MenuItem()
        {
            Title = "Black",
            MethodToRun = () =>
            {
                Console.BackgroundColor = ConsoleColor.Black;
                return ConsoleKey.B;
            }
        }
    });


var options = new Menu(
    EMenuLevel.Second,
    "Options",
    new List<MenuItem>(){
        new MenuItem()
        {
            Title = "Console color",
            MethodToRun = consoleColors.Run
        }
    });

var mainMenu = new Menu(
    EMenuLevel.Main,
    "Main Menu",
    new List<MenuItem>(){
        new MenuItem()
        {
            Title = "New Game",
            MethodToRun = newGame.Run
        },

        new MenuItem()
        {
            Title = "Load Game",
            MethodToRun = LoadGame
        },

        new MenuItem()
        {
            Title = "Options",
            MethodToRun = options.Run
        }
    });

Console.ForegroundColor = ConsoleColor.Red;
Console.CursorVisible = false;
mainMenu.Run();
Console.Clear();
