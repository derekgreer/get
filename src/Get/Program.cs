using System;

namespace Get
{
    public class Program
    {
        static void Main(string[] args)
        {
            Environment.Exit(new Engine().Process(args));
        }
    }
}