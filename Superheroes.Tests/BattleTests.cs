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
        // TODO: these are not round numbers in base-2 floating point so this is a bit misleading
        [InlineData("Potter", 8.2000001, "Voldemort", 8.2, "Potter")] // boundary testing
        [InlineData("Potter", 8.1999999, "Voldemort", 8.2, "Voldemort")] // boundary testing
        [InlineData("Potter", 8.2, "Voldemort", 8.2, "Potter")] // tied
        [InlineData("Superman", 9, "Lex", 8, "Superman")] // prove names aren't hard-coded
        [InlineData("Superman", 1, "Lex", 9, "Lex")]
        [Theory]
        public async Task HighestScoreWins(string heroName, double heroScore, string villainName, double villainScore, string expectedWinner)
        {
            // Arrange
            var client = SetupBattle(heroName: heroName, heroScore: heroScore, villainName: villainName, villainScore: villainScore); // Why the redundant argument names you ask? Because they aren't redundant 👀 👉 https://timwise.co.uk/2023/04/18/always-add-argument-names/

            // Act
            var winner = await RunBattle(client, heroName, villainName);

            // Assert
            winner.Should().Be(expectedWinner);
        }

        private static HttpClient SetupBattle(string heroName, double heroScore, string villainName, double villainScore)
        {
            var charactersProvider = new FakeCharactersProvider()
                .WithFakeResponse(BuildCharactersResponse(heroName: heroName,
                    heroScore: heroScore,
                    villainName: villainName,
                    villainScore: villainScore));
            return GetClient(charactersProvider);
        }

        private static async Task<string> RunBattle(HttpClient client, string hero, string villain)
        {
            var response = await client.GetAsync($"battle?hero={hero}&villain={villain}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseJson = await response.Content.ReadAsStringAsync();
            var winner = JsonConvert.DeserializeObject<JObject>(responseJson).Value<string>("name");
            return winner;
        }

        private static CharactersResponse BuildCharactersResponse(string heroName, double heroScore, string villainName, double villainScore)
        {
            return new CharactersResponse
            {
                Items = new[]
                {
                    new CharacterResponse
                    {
                        Name = heroName,
                        Score = heroScore,
                        Type = "hero",
                    },
                    new CharacterResponse
                    {
                        Name = villainName,
                        Score = villainScore,
                        Type = "villain",
                    },
                },
            };
        }

        private static HttpClient GetClient(ICharactersProvider charactersProvider)
        {
            var startup = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(x => { x.AddSingleton(charactersProvider); });
            var testServer = new TestServer(startup);
            return testServer.CreateClient();
        }
    }
}
