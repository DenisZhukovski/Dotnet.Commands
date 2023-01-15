using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dotnet.Commands
{
    public class SafeCommands : ICommands
    {
        private readonly ICommands _commands;
        private readonly Func<Exception, bool> _onError;

        public SafeCommands(ICommands commands, Func<Exception, bool> onError)
        {
            _commands = commands;
            _onError = onError;
        }
        
        public IAsyncCommand AsyncCommand(
            Func<CancellationToken, Task> execute,
            Func<bool>? canExecute = null, 
            bool forceExecution = false,
            [CallerMemberName] string? name = null)
        {
            return _commands.AsyncCommand(
                execute.Safe(_onError),
                (Func<bool>)(() => CanExecute(canExecute)),
                forceExecution,
                name
            );
        }

        public IAsyncCommand<TParam> AsyncCommand<TParam>(
            Func<TParam, CancellationToken, Task> execute,
            Func<TParam, bool>? canExecute = null, 
            bool forceExecution = false,
            [CallerMemberName] string? name = null)
        {
            return _commands.AsyncCommand(
                execute.Safe(_onError),
                p =>
                {
                    if (canExecute == null)
                    {
                        return true;
                    }

                    return CanExecute(p, canExecute);
                },
                forceExecution,
                name
            );
        }

        public IAsyncCommand<TParam> AsyncCommand<TParam>(
            Func<TParam, CancellationToken, Task> execute,
            Func<TParam, Task<bool>> canExecute = null,
            bool forceExecution = false,
            [CallerMemberName] string? name = null)
        {
            return _commands.AsyncCommand(
                execute.Safe(_onError),
                (p) => canExecute == null 
                    ? Task.FromResult(true) :
                    canExecute.Safe(_onError)(p),
                forceExecution, 
                name
            );
        }

        public ICommand Command(
            Action execute, 
            Func<bool> canExecute = null, 
            bool forceExecution = false,
            [CallerMemberName] string? name = null)
        {
            return _commands.Command(
                execute.Safe(_onError),
                () => CanExecute(canExecute),
                forceExecution, 
                name
            );
        }

        public ICommand Command<TParam>(
            Action<TParam> execute, 
            Func<TParam, bool> canExecute = null,
            bool forceExecution = false,
            [CallerMemberName] string? name = null)
        {
            return _commands.Command(
                execute.Safe(_onError),
                (p) => CanExecute(p, canExecute),
                forceExecution,
                name
            );
        }
        
        private bool CanExecute<TParam>(TParam par, Func<TParam, bool>? canExecute = null)
        {
            return canExecute == null || canExecute.Safe(_onError)(par);
        }

        private bool CanExecute(Func<bool>? canExecute = null)
        {
            return canExecute == null || canExecute.Safe(_onError)();
        }
    }
}