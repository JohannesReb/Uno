using BLL;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain.Database;
using Newtonsoft.Json;
using Player = Domain.Player;

namespace WebApp.Pages_Games
{
    public class CreateModel : PageModel
    {
        private readonly DAL.AppDbContext _context;
        public UnoGameEngine Engine { get; set; }

        public CreateModel(DAL.AppDbContext context)
        {
            _context = context;
            Engine = new UnoGameEngine(new GameRepositoryEF(_context));
        }

        public GameState State { get; set; } = new GameState();
        [BindProperty]
        public int PlayerCount { get; set; }

        public IActionResult OnGet(string? playerCount)
        {
            if (playerCount != null)
            {
                PlayerCount = int.Parse(playerCount);
            }
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public void OnPostAsync()
        {
            Guid? currentPlayerId = null;
            for (int i = 0; i < PlayerCount; i++)
            {
                var nickName = Request.Form[i.ToString()].ToString();
                var playerTypeStr = Request.Form[i + "type"];
                var playerType = EPlayerType.Ai.ToString() == playerTypeStr ? EPlayerType.Ai : EPlayerType.Human;
                Guid id = Guid.NewGuid();
                if (Request.Form["radio"] == i.ToString())
                {
                    currentPlayerId = id;
                }
                State.Players.Add(new Player(nickName, playerType){Id = id});
            }
            
            foreach (ECardValue value in Enum.GetValuesAsUnderlyingType(typeof(ECardValue)))
            {
                string? effectsAsString = Request.Form[value.ToString()];
                var effectsWithAmountStrArr = effectsAsString?.Split(",");
                var effectList = new List<ECardEffect>();
                if (effectsWithAmountStrArr != null)
                {
                    foreach (ECardEffect effect in Enum.GetValuesAsUnderlyingType(typeof(ECardEffect)))
                    {
                        foreach (var effStrWithAmount in effectsWithAmountStrArr)
                        {
                            var nameAmount = effStrWithAmount.Split(" ");
                            int amount = int.Parse(nameAmount[1]);
                            string effName = nameAmount[0];
                            if (effName == effect.ToString())
                            {
                                for (int i = 0; i < amount; i++)
                                {
                                    effectList.Add(effect);
                                }
                            }
                        }
                    }
                }
                State.CardEffects[value] = effectList;
                foreach (var propertiesAsString in Request.Form[value + "%prop%"])
                {
                    var propertiesStrArr = propertiesAsString?.Split(",");
                    var propertyList = new List<ECardProperty>();
                    if (propertiesStrArr != null)
                    {
                        foreach (ECardProperty property in Enum.GetValuesAsUnderlyingType(typeof(ECardProperty)))
                        {
                            foreach (var propertyStr in propertiesStrArr)
                            {
                                if (propertyStr == property.ToString())
                                {
                                    propertyList.Add(property);
                                }
                            }
                        }
                    }
                    State.CardProperties[value] = propertyList;
                }
            }
            var gameRulesList = new List<EGameRule>();
            foreach (var gameRuleAsString in Request.Form["gameRule"])
            {
                var gameRulesStrArr = gameRuleAsString?.Split(",");
                if (gameRulesStrArr != null)
                {
                    gameRulesList.AddRange(
                        from EGameRule gameRule in Enum.GetValuesAsUnderlyingType(typeof(EGameRule))
                        from gameRuleStr in gameRulesStrArr
                        where gameRuleStr == gameRule.ToString()
                        select gameRule);
                }
            }
            State.GameRules = gameRulesList;
            State.PlayerCount = PlayerCount;
            Engine.GameState = State;
            Engine.InitializeGame();
            Engine.SaveGame();
            Response.Redirect($"/Play/Index?gameId={State.Id}&playerId={currentPlayerId}");
        }
    }
}
