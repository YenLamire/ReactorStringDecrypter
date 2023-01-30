using System;
using System.Collections.Generic;
using System.Reflection;
using dnlib.DotNet;
using ReactorStringDecrypter.Context;
using ReactorStringDecrypter.Decrypter;
using ReactorStringDecrypter.Logger;

namespace ReactorStringDecrypter
{
    internal class Program
    {
        private static List<IDecrypters> Decrypters { get; set; }

        private static void InitializeDecrypters()
        {
            Decrypters = new List<IDecrypters>
            {
                new BlocksDecrypter(),
                new FieldDecrypter(),
                new StringDecrypter()
            };
        }
        private static void ExecuteDecrypters(FileContext ctx)
        {
            foreach (var decrypter in Decrypters)
            {
                CLogger.Instance.Write_Debug("Executing " + decrypter.Information);
                decrypter.Decrypt(ctx);
            }
        }
        private static void Main(string[] args)
        {
            Console.Title = "Simple .NET Reactor String Decrypter using Invocation {Yen Lamire}";
            string tmp_fileName;
            try
            {
                tmp_fileName = args[0];
            }
            catch
            {
                CLogger.Instance.Write_Info("Please enter your file path:");
                tmp_fileName = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(tmp_fileName)) throw new Exception(nameof(args));

            var ctx = new FileContext(ModuleDefMD.Load(tmp_fileName), Assembly.LoadFile(tmp_fileName));
            StringDecrypter.StacktracePatcher.Patch();
            InitializeDecrypters();
            ExecuteDecrypters(ctx);
            ctx.Save();
            CLogger.Instance.Write_Info("File saved!");
            Console.ReadKey();
        }
    }
}