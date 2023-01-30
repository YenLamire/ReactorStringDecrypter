using System.Linq;
using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using ReactorStringDecrypter.Context;

namespace ReactorStringDecrypter.Decrypter
{
    internal class BlocksDecrypter : IDecrypters
    {
        private static BlocksCflowDeobfuscator _blocksCflowDeob;
        public string Information => "Decrypts Blocks";

        public void Decrypt(FileContext ctx)
        {
            foreach (var type in
                     ctx._module.GetTypes()
                         .Where(t => t
                             .HasMethods)) //Loops to all types inside the module and returns types with methods
            foreach (var method in type.Methods
                         .Where(m => m.HasBody &&
                                     m.Body
                                         .HasInstructions)) //Loop all methods and select which method has a body and instructions
                Decrypt_ReorderBlocks(method);
        }

        public static void Decrypt_ReorderBlocks(MethodDef method)
        {
            try
            {
                _blocksCflowDeob =
                    new BlocksCflowDeobfuscator(); //create a new instance per method, improves block readability
                var blocks = new Blocks(method);
                blocks.MethodBlocks.GetAllBlocks();
                blocks.RemoveDeadBlocks();
                blocks.RepartitionBlocks();
                blocks.UpdateBlocks();
                blocks.Method.Body.SimplifyBranches();
                blocks.Method.Body.OptimizeBranches();
                _blocksCflowDeob.Initialize(blocks);
                _blocksCflowDeob.Deobfuscate();
                blocks.RepartitionBlocks();
                blocks.GetCode(out var instructions, out var exceptionHandlers);
                DotNetUtils.RestoreBody(method, instructions, exceptionHandlers);
            }
            catch
            {
                // ignored
            }
        }
    }
}