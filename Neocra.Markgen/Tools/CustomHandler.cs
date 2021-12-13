using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Neocra.Markgen.Tools
{
    public class CustomHandler<T> : ICommandHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomHandler(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var handler = this._serviceProvider.GetService(typeof(T));
            var methodInfo = typeof(T).GetMethod("RunAsync");

            if (methodInfo == null)
            {
                throw new ArgumentException("Method RunAsync not found");
            }
            
            return await CommandHandler.Create(methodInfo, handler).InvokeAsync(context);
        }
    }
}