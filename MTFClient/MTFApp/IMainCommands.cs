using MTFApp.UIHelpers;
using System.Collections.Generic;

namespace MTFApp
{
    interface IMainCommands
    {
        IEnumerable<Command> Commands();
    }
}
