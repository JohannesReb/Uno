namespace MenuSystem;

public class Menu
{
    public string MenuLabel { get; set; }
    public EMenuLevel MenuLevel { get; set; }
    public List<MenuItem> MenuItems { get; set; }
    public string MenuSeparator { get; set; } = "=============================";
    private const int UiLineOffset = 2;
    private int UserChoice { get; set; } = UiLineOffset;
    public List<int>? Selected { get; set; }

    public Menu(EMenuLevel menuLevel, string menuLabel, List<MenuItem> menuItems, List<int>? selected = null)
    {
        MenuLabel = menuLabel;
        MenuLevel = menuLevel;
        MenuItems = menuItems;
        Selected = selected;
    }

    void Draw()
    {
        Console.Clear();
        Console.WriteLine(MenuLabel);
        Console.WriteLine(MenuSeparator);
        int i = UiLineOffset;
        foreach (var item in MenuItems)
        {
            if (Selected?.Contains(i - UiLineOffset) ?? false)
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            }
            if (i == UserChoice)
            {
                Console.BackgroundColor = Console.BackgroundColor == ConsoleColor.DarkGreen ? ConsoleColor.DarkCyan : ConsoleColor.DarkBlue;
            }
            Console.WriteLine(item.DynamicTitle != null ? item.DynamicTitle() : item.Title);
            Console.BackgroundColor = ConsoleColor.Black;
            i++;
        }
        Console.WriteLine(MenuSeparator);
        if (MenuLevel != EMenuLevel.Main)
        {
            Console.WriteLine("b) Back");
        }

        if (MenuLevel == EMenuLevel.Other)
        {
            Console.WriteLine("m) return to Main");
        }
        Console.WriteLine("x) eXit");
    }
    public ConsoleKey Run()
    {
        ConsoleKey key;
        do
        {
            do
            {
                Draw();
                Console.SetCursorPosition(0, UserChoice);
                key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                    {
                        if (UserChoice > UiLineOffset) UserChoice--;
                        else UserChoice = MenuItems.Count + UiLineOffset - 1;
                        break;
                    }
                    case ConsoleKey.DownArrow:
                    {
                        if (UserChoice < MenuItems.Count + UiLineOffset - 1) UserChoice++;
                        else UserChoice = UiLineOffset;
                        break;
                    }
                    case ConsoleKey.RightArrow:
                    {
                        if (Selected?.Count < 10)
                        {
                            Selected?.Add(UserChoice - UiLineOffset);
                        }
                        break;
                    }
                    case ConsoleKey.LeftArrow:
                    {
                        Selected?.Remove(UserChoice - UiLineOffset);
                        break;
                    }
                    case ConsoleKey.R:
                    {
                        return key;
                    }
                    
                    case ConsoleKey.X: return key;
                    case ConsoleKey.M:
                    {
                        if (MenuLevel == EMenuLevel.Other) return key;
                        break;
                    }
                    case ConsoleKey.B:
                    {
                        if (MenuLevel != EMenuLevel.Main) return key;
                        break;
                    }
                }
                
            } while (key != ConsoleKey.Enter);
            
            if (MenuItems[UserChoice - 2].MethodToRun != null)
            {
                key = MenuItems[UserChoice - 2].MethodToRun!();
            }
            if (MenuItems[UserChoice - 2].MethodWithInputGuid != null)
            {
                key = MenuItems[UserChoice - 2].MethodWithInputGuid!(MenuItems[UserChoice - 2].GameId);
            }
            if (MenuItems[UserChoice - 2].MethodWithInputString != null)
            {
                key = MenuItems[UserChoice - 2].MethodWithInputString!(MenuItems[UserChoice - 2].Title);
            }
        } while ((key != ConsoleKey.M || MenuLevel == EMenuLevel.Main) && key != ConsoleKey.X);
        return key;
    }
}