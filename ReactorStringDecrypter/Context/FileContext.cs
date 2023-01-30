using System.IO;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace ReactorStringDecrypter.Context
{
    internal class FileContext
    {
        public Assembly _asm;
        public ModuleDefMD _module;
        public string _outPath;

        public FileContext(ModuleDefMD module, Assembly asm)
        {
            var tmpLoc = module.Location;
            _module = module;
            _asm = asm;
            _outPath = Path.GetFileNameWithoutExtension(tmpLoc) + "_decrypted" + Path.GetExtension(tmpLoc);
        }

        public string Save()
        {
            var options = new ModuleWriterOptions(_module) //Initialize new ModuleWriterOptions to manipulate writer
            {
                MetadataLogger = DummyLogger.NoThrowInstance //Suppress metadata errors
            };

            _module.Write(_outPath, options); //Save to "{filepath}_decrypted.{extension}"
            return _outPath;
        }
    }
}