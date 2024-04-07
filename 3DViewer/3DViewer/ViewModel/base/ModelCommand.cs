﻿using System;

using System.Windows.Input;

namespace _3DViewer.Command
{
    public class ModelCommand : ICommand
    {
        public ModelCommand(Action<object> execute): this(execute, null)
        {}

        public ModelCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
    }
}
