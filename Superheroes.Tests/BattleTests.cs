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

        // show doesn't affect outcome when set to unrelated villain
        [InlineData("Bart", "Nelson", 7, "Sideshow Bob", 6.9, "Bart")]
        [InlineData("Bart", "Nelson", 7, "Sideshow Bob", 7, "Bart")]
        [InlineData("Bart", "Nelson", 7, "Sideshow Bob", 7.1, "Sideshow Bob")]
        // show effect when battling villain that is their weakness
        [InlineData("Bart", "Nelson", 7, "Nelson", 5.9, "Bart")]
        [InlineData("Bart", "Nelson", 7, "Nelson", 6, "Bart")]
        [InlineData("Bart", "Nelson", 7, "Nelson", 6.1, "Nelson")]
        [Theory]
        public async Task WeaknessChangesWinner(string heroName, string heroWeakness, double heroScore, string villainName, double villainScore, string expectedWinner)
        {
            // Arrange
            var client = SetupBattle(heroName: heroName, heroScore: heroScore, villainName: villainName, villainScore: villainScore, heroWeakness: heroWeakness);

            // Act
            var winner = await RunBattle(client, heroName, villainName);

            // Assert
            winner.Should().Be(expectedWinner);
        }

        private static HttpClient SetupBattle(string heroName, double heroScore, string villainName, double villainScore, string heroWeakness = null)
        {
            var charactersProvider = new FakeCharactersProvider()
                .WithFakeResponse(BuildCharactersResponse(heroName: heroName,
                    heroScore: heroScore,
                    villainName: villainName,
                    villainScore: villainScore,
                    heroWeakness: heroWeakness));
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

        private static CharactersResponse BuildCharactersResponse(string heroName, double heroScore, string villainName, double villainScore, string heroWeakness)
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
                        Weakness = heroWeakness,
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
