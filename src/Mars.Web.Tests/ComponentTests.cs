using Bunit;
using Mars.MissionControl;
using Mars.Web.Components;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;

namespace Mars.Web.Tests;

internal class ComponentTests
{
    private GameManager gameManager;
    private MultiGameHoster multiGameHoster;
    private Microsoft.Extensions.Configuration.IConfiguration config;

    [SetUp]
    public void Setup()
    {
        var mockMapProvider = new Mock<IMapProvider>();
        mockMapProvider.Setup(m => m.LoadMaps()).Returns(new[] { Helpers.CreateMap(10, 10) });
        var loggerMock = new Mock<ILogger<Game>>();
        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(m => m[ConfigKeys.MaxMaps]).Returns("1");
        gameManager = new GameManager(mockMapProvider.Object.LoadMaps().ToList(), loggerMock.Object, configMock.Object);

        var mockLogFactory = new Mock<ILoggerFactory>();

        multiGameHoster = new MultiGameHoster(mockMapProvider.Object, mockLogFactory.Object, configMock.Object);

        var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfig.Setup(c => c["GAME_PASSWORD"]).Returns("password");
        config = mockConfig.Object;
    }

    [Test]
    public void GameStatusShowsJoining()
    {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<GameStatus>(parameters => parameters.Add(p => p.gameManager, gameManager));
        cut.Markup.Should().Contain("Joining");
    }

    [Test]
    public void RestartGameWithBadTargetValues()
    {
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(multiGameHoster);
        ctx.Services.AddSingleton(config);
        ctx.Services.AddSingleton(new Mock<ILogger<NewGame>>().Object);

        var cut = ctx.RenderComponent<NewGame>(parameters => parameters.Add(p => p.gameManager, gameManager));
        var targetsElement = cut.Find("#targets");
        targetsElement.Change("250;250");

        var errorSpan = cut.Find("#errorSpan");
        errorSpan.InnerHtml.Should().Be("Locations must be in a (x,y);(x1,y1) format");
    }
}
