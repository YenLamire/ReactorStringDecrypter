using System;

namespace ReactorStringDecrypter.Logger
{
    internal class CLogger
    {
        internal static CLogger Instance = new CLogger();

        public void Write_Info(object message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Concat("[$] ", message));
            Console.ResetColor();
        }

        public void Write_Debug(object message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(string.Concat("[*] ", message));
            Console.ResetColor();
        }

        public void Write_Error(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Concat("[!] ", message));
            Console.ResetColor();
        }

        public void Write_Verbose(object message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(string.Concat("[?] ", message));
            Console.ResetColor();
        }
    }
}