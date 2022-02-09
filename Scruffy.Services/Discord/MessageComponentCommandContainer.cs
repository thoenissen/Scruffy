using System.Reflection;

using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Discord;

/// <summary>
/// Message component group container
/// </summary>
internal class MessageComponentCommandContainer
{
    #region Fields

    /// <summary>
    /// Command group
    /// </summary>
    private readonly string _group;

    /// <summary>
    /// Service provider
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Module type
    /// </summary>
    private readonly Type _type;

    /// <summary>
    /// Commands
    /// </summary>
    private readonly Dictionary<string, MethodInfo> _commands;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="type">Type</param>
    /// <param name="group">Command group</param>
    public MessageComponentCommandContainer(IServiceProvider serviceProvider, Type type, string group)
    {
        _group = group;
        _serviceProvider = serviceProvider;
        _type = type;
        _commands = new Dictionary<string, MethodInfo>();

        foreach (var method in type.GetMethods())
        {
            var attribute = method.GetCustomAttribute<MessageComponentCommandAttribute>();
            if (string.IsNullOrEmpty(attribute?.Command) == false)
            {
                if (method.GetParameters().Length > 0)
                {
                    throw new InvalidOperationException("Parameters are not supported.");
                }

                if (method.ReturnType != typeof(Task))
                {
                    throw new InvalidOperationException("Return type not supported.");
                }

                if (_commands.TryAdd(attribute.Command, method) == false)
                {
                    throw new InvalidOperationException("Command already added.");
                }
            }
        }
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Execute command
    /// </summary>
    /// <param name="command">Command</param>
    /// <param name="customId">Customer Id</param>
    /// <param name="component">Component</param>
    public void ExecuteCommandAsync(string command, string customId, SocketMessageComponent component)
    {
        if (_commands.TryGetValue(command, out var commandInfo))
        {
            Task.Run(() =>
                     {
                         try
                         {
                             using (var scope = _serviceProvider.CreateScope())
                             {
                                 if (scope.ServiceProvider.GetRequiredService(_type) is MessageComponentCommandModule module)
                                 {
                                     module.CustomId = customId;
                                     module.Component = component;

                                     commandInfo.Invoke(module, null);
                                 }
                             }
                         }
                         catch (Exception ex)
                         {
                             LoggingService.AddMessageComponentCommandLogEntry(LogEntryLevel.Error, _group, command, "Command execution failed.", customId, ex);
                         }
                     });
        }
    }

    #endregion // Methods
}