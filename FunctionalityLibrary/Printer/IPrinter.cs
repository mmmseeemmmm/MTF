using System;

namespace Printer
{
    interface IPrinter : IDisposable
    {
        void Print(string line1, string line2, string line3, string reference, PermanentCounter counter);
    }
}
