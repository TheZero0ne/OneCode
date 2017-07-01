using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OneCode {
    class RelayCommand : ICommand {
        private Func<bool> WhenToExecute;
        private Action WhatToExecute;

        public RelayCommand(Action What, Func<bool> When) {
            WhatToExecute = What;
            WhenToExecute = When;
        }

        public bool CanExecute(object parameter) {
            return WhenToExecute();
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public void Execute(object parameter) {
            WhatToExecute();
        }
    }
}
