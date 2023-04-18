using System.Threading.Tasks;

namespace Superheroes.Tests
{
    public class FakeCharactersProvider : ICharactersProvider
    {
        private CharactersResponse _response;

        public FakeCharactersProvider WithFakeResponse(CharactersResponse response)
        {
            _response = response;
            return this;
        }

        public Task<CharactersResponse> GetCharacters()
        {
            return Task.FromResult(_response);
        }
    }
}
