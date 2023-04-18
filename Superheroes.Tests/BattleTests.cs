using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Superheroes.Tests
{
    public class BattleTests
    {
        [Fact]
        public async Task Battle_Hero_Wins()
        {
            var charactersProvider = new FakeCharactersProvider();
            var client = Setup(charactersProvider);
            SetupDataFixture(charactersProvider, heroName: "Batman", heroScore: 8.3, villainName: "Joker", villainScore: 8.2);

            var response = await RunBattle(client);

            response.Value<string>("name").Should().Be("Batman");
        }

        [Fact]
        public async Task Battle_Hero_Wins_When_Matches()
        {
            var charactersProvider = new FakeCharactersProvider();
            var client = Setup(charactersProvider);
            SetupDataFixture(charactersProvider, heroName: "Batman", heroScore: 8.2, villainName: "Joker", villainScore: 8.2);

            var response = await RunBattle(client);

            response.Value<string>("name").Should().Be("Batman");
        }

        private static async Task<JObject> RunBattle(HttpClient client)
        {
            var response = await client.GetAsync("battle?hero=Batman&villain=Joker");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<JObject>(responseJson);
            return responseObject;
        }

        private static void SetupDataFixture(FakeCharactersProvider charactersProvider, string heroName, double heroScore, string villainName, double villainScore)
        {
            charactersProvider.FakeResponse(new CharactersResponse
            {
                Items = new[]
                {
                    new CharacterResponse
                    {
                        Name = heroName,
                        Score = heroScore,
                        Type = "hero"
                    },
                    new CharacterResponse
                    {
                        Name = villainName,
                        Score = villainScore,
                        Type = "villain"
                    }
                }
            });
        }

        private static HttpClient Setup(FakeCharactersProvider charactersProvider)
        {
            var startup = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(x => { x.AddSingleton<ICharactersProvider>(charactersProvider); });
            var testServer = new TestServer(startup);
            var client = testServer.CreateClient();
            return client;
        }
    }
}
