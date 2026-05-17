using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM.Console.States
{
    public interface IAtmState
    {
        void DisplayMenu();
        void HandleInput(string input);
    }
}
