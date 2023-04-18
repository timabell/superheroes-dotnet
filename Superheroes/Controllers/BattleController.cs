using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Superheroes.Controllers
{
    [Route("battle")]
    public class BattleController : Controller
    {
        private readonly ICharactersProvider _charactersProvider;

        public BattleController(ICharactersProvider charactersProvider)
        {
            _charactersProvider = charactersProvider;
        }

        public async Task<IActionResult> Get(string hero, string villain)
        {
            CharactersResponse availableCharacters = await _charactersProvider.GetCharacters();

            CharacterResponse heroCharactar = null;
            CharacterResponse villainCharacter = null;
            heroCharactar = availableCharacters.Items.SingleOrDefault(c => c.Name == hero);
            villainCharacter = availableCharacters.Items.SingleOrDefault(c => c.Name == villain);

            var heroCharactarScore = heroCharactar.Score;
            if (villain == heroCharactar.Weakness)
            {
                heroCharactarScore--;
            }
            var villainDefeated = heroCharactarScore >= villainCharacter.Score;

            return Ok(villainDefeated ? heroCharactar : villainCharacter);
        }
    }
}
