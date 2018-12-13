using System;
using System.Windows.Input;

namespace HueSyncClone.Core
{
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _action;

        public DelegateCommand(Action<T> action) => _action = action;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _action((T)parameter);

        public event EventHandler CanExecuteChanged;
    }
}