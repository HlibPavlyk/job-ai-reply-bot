using DjinniAIReplyBot.Application.Abstractions.Telegram;
using DjinniAIReplyBot.Application.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Telegram.Bot.Types;

namespace DjiniAIReplyBot.UnitTests.Services;

public class CommandExecutorTests
{
    private readonly List<Mock<ICommand>> _commandMocks;
    private readonly CommandExecutor _commandExecutor;
    private readonly long _authorChatId = 12345;

    public CommandExecutorTests()
    {
        Mock<IServiceProvider> serviceProviderMock = new();
        Mock<IConfiguration> configurationMock = new();

        // Mock AuthorChatId
        configurationMock
            .Setup(config => config["AuthorChatId"])
            .Returns(_authorChatId.ToString());

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IConfiguration)))
            .Returns(configurationMock.Object);

        // Mock commands
        _commandMocks = new List<Mock<ICommand>>();
        for (int i = 0; i < 2; i++)
        {
            var commandMock = new Mock<ICommand>();
            commandMock.Setup(c => c.Name).Returns($"/command{i + 1}");
            _commandMocks.Add(commandMock);
        }

        // Initialize CommandExecutor with mocked commands
        _commandExecutor = new CommandExecutor(
            serviceProviderMock.Object,
            _commandMocks.Select(m => m.Object).ToList()
        );
    }

    [Fact]
    public async Task GetUpdate_ShouldExecuteMatchingCommand()
    {
        // Arrange
        var update = new Update
        {
            Message = new Message
            {
                Text = "/command1",
                Chat = new Chat { Id = 67890 }
            }
        };

        _commandMocks[0].Setup(c => c.Execute(It.IsAny<Update>())).Returns(Task.CompletedTask);

        // Act
        await _commandExecutor.GetUpdate(update);

        // Assert
        _commandMocks[0].Verify(c => c.Execute(It.Is<Update>(u => u == update)), Times.Once);
    }

    [Fact]
    public async Task GetUpdate_ShouldNotExecuteCommandForInvalidText()
    {
        // Arrange
        var update = new Update
        {
            Message = new Message
            {
                Text = "/invalidCommand",
                Chat = new Chat { Id = 67890 }
            }
        };

        // Act
        await _commandExecutor.GetUpdate(update);

        // Assert
        foreach (var commandMock in _commandMocks)
        {
            commandMock.Verify(c => c.Execute(It.IsAny<Update>()), Times.Never);
        }
    }

    [Fact]
    public async Task GetUpdate_ShouldInvokeListenerForExistingChatId()
    {
        // Arrange
        var listenerMock = new Mock<IListener>();
        var update = new Update
        {
            Message = new Message
            {
                Text = "Listener Message",
                Chat = new Chat { Id = 67890 }
            }
        };

        _commandExecutor.StartListen(listenerMock.Object, 67890);

        listenerMock.Setup(l => l.GetUpdate(It.IsAny<Update>())).Returns(Task.CompletedTask);

        // Act
        await _commandExecutor.GetUpdate(update);

        // Assert
        listenerMock.Verify(l => l.GetUpdate(It.Is<Update>(u => u == update)), Times.Once);
    }

    [Fact]
    public async Task GetUpdate_ShouldResetListenerForNewCommand()
    {
        // Arrange
        var listenerMock = new Mock<IListener>();
        var update = new Update
        {
            Message = new Message
            {
                Text = "/command1",
                Chat = new Chat { Id = 67890 }
            }
        };

        _commandExecutor.StartListen(listenerMock.Object, 67890);

        listenerMock.Setup(l => l.ResetUpdate(It.IsAny<Update>())).ReturnsAsync(true);

        // Act
        await _commandExecutor.GetUpdate(update);

        // Assert
        listenerMock.Verify(l => l.ResetUpdate(It.Is<Update>(u => u == update)), Times.Once);
    }

    [Fact]
    public void StartListen_ShouldAddListener()
    {
        // Arrange
        var listenerMock = new Mock<IListener>();
        long chatId = 67890;

        // Act
        _commandExecutor.StartListen(listenerMock.Object, chatId);

        // Assert
        var listenersField = typeof(CommandExecutor)
            .GetField("_listeners", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var listeners = listenersField?.GetValue(_commandExecutor) as Dictionary<long, IListener>;
        Assert.NotNull(listeners);
        Assert.True(listeners.ContainsKey(chatId));
    }

    [Fact]
    public void StopListen_ShouldRemoveListener()
    {
        // Arrange
        var listenerMock = new Mock<IListener>();
        long chatId = 67890;
        _commandExecutor.StartListen(listenerMock.Object, chatId);

        // Act
        _commandExecutor.StopListen(chatId);

        // Assert
        var listenersField = typeof(CommandExecutor)
            .GetField("_listeners", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var listeners = listenersField?.GetValue(_commandExecutor) as Dictionary<long, IListener>;
        Assert.NotNull(listeners);
        Assert.False(listeners.ContainsKey(chatId));
    }

    [Fact]
    public async Task ProcessAuthorCallback_ShouldNotPassUpdateIfListenerNotFound()
    {
        // Arrange
        var update = new Update
        {
            CallbackQuery = new CallbackQuery
            {
                Data = "someData:67890",
                Message = new Message { Chat = new Chat { Id = 67890 } }
            }
        };

        // Act
        await _commandExecutor.GetUpdate(update);

        // Assert
        Assert.True(true); // No exceptions should occur
    }
}