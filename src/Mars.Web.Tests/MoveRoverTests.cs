using Mars.Web.Types;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Mars.Web.Tests
{
    internal class MoveRoverTests
    {
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {

                    });
                });
        }

        [Test]
        public async Task CanMoveRover()
        {
            var client = _factory.CreateClient();
            var joinResponse = await client.GetFromJsonAsync<JoinResponse>("/game/join?name=Caleb");

            var moveResponse = await client.GetAsync($"/move?token={joinResponse.Token}&direction=Forward");

            moveResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}