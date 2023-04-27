using Mars.MissionControl;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Mars.Web.Tests;

public class IntegrationTestHelper
{
    public static WebApplicationFactory<Program> CreateFactory()
    {
        var mockMapProvider = new Mock<IMapProvider>();
        mockMapProvider.Setup(m => m.LoadMaps()).Returns(new[] { Helpers.CreateMap(10, 10) });

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Replace(new ServiceDescriptor(typeof(IMapProvider), (_) => mockMapProvider.Object, ServiceLifetime.Singleton));
                    services.Replace(new ServiceDescriptor(typeof(GameConfig), (_) => new GameConfig(), ServiceLifetime.Singleton));
                });
            });
    }
}

internal class BasicIntegrationTestDemo
{
    private WebApplicationFactory<Program> _factory;

    [SetUp]
    public void Setup()
    {
        _factory = IntegrationTestHelper.CreateFactory();
    }

    [TestCase("/", "Mars Rover")]
    [TestCase("/Index", "Sorry, there's nothing at this address")]
    [TestCase("/About", "Sorry, there's nothing at this address")]
    [TestCase("/Privacy", "Sorry, there's nothing at this address")]
    [TestCase("/Contact", "Sorry, there's nothing at this address")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url, string contentFragment)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.AreEqual("text/html; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
        var actualContent = await response.Content.ReadAsStringAsync();
        actualContent.Should().Contain(contentFragment);
    }

}
