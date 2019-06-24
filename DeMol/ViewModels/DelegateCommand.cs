using System;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    internal class DelegateCommand : ICommand
    {
        private Action<string> executeDelegate;
        private Func<string, bool> canExecuteDelegate;

        public DelegateCommand(Action<string> executeDelegate, Func<string, bool> canExecuteDelegate)
        {
            this.executeDelegate = executeDelegate;
            this.canExecuteDelegate = canExecuteDelegate;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object parameter) => canExecuteDelegate((string)parameter);
        public void Execute(object parameter) => executeDelegate((string)parameter);
    }
}