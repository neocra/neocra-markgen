using System;
using System.CommandLine;

namespace Neocra.Markgen.Tools
{
    public class HandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public HandlerFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public Command Command<T>()
            where T : ICommandDefinition, new()
        {
            return new T().Command(this);
        }

        public Command AddHandler<T>(Command command)
        {
            command.Handler = new CustomHandler<T>(this._serviceProvider);

            return command;
        }
    }
}