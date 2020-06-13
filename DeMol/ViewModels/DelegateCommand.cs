using System;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    internal class DelegateCommand : ICommand
    {
        private readonly Func<string, bool> canExecuteDelegate;
        private readonly Action<string> executeDelegate;

        public DelegateCommand(Action<string> executeDelegate, Func<string, bool> canExecuteDelegate)
        {
            this.executeDelegate = executeDelegate;
            this.canExecuteDelegate = canExecuteDelegate;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return canExecuteDelegate((string) parameter);
        }

        public void Execute(object parameter)
        {
            executeDelegate((string) parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}