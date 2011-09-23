using System;
using NuGet;

namespace Get
{
    public class ConsoleLogger : ILogger
    {
        public void Log(MessageLevel level, string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }
}