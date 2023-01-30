using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using ReactorStringDecrypter.Context;
using ReactorStringDecrypter.Logger;

namespace ReactorStringDecrypter.Decrypter
{
    internal class FieldDecrypter : IDecrypters
    {
        private readonly Dictionary<IField, int> _fieldCache = new Dictionary<IField, int>();
        public string Information => "Decrypts fields";

        public void Decrypt(FileContext ctx)
        {
            Initialize_Fields(ctx);
            foreach (var type in ctx._module.GetTypes().Where(t => t.HasMethods))
                foreach (var method in type.Methods
                             .Where(m => m.HasBody && m.Body.HasInstructions))
                {
                    var instructions = method.Body.Instructions;
                    for (var curIndex = 0; curIndex < instructions.Count; curIndex++)
                    {
                        if (instructions[curIndex].OpCode.Code != Code.Ldsfld ||
                            instructions[curIndex + 1].OpCode.Code != Code.Ldfld) continue;

                        var ldfldField = instructions[curIndex + 1].Operand as IField;
                        if (!_fieldCache.TryGetValue(ldfldField, out var xx)) continue; //try access cache for field's value

                        instructions[curIndex].OpCode = OpCodes.Ldc_I4;
                        instructions[curIndex].Operand = xx;
                        instructions[curIndex + 1].OpCode = OpCodes.Nop;
                        BlocksDecrypter.Decrypt_ReorderBlocks(method); //Decrypt blocks again and reorder them again
                    }
                }
        }

        private void Initialize_Fields(FileContext ctx)
        {
            foreach (var type in ctx._module.GetTypes().Where(t => t.HasMethods))
            {
                if (type.Fields.Count < 100) //.NET Reactor doesn't have less than 100
                    continue;
                if (!type.Name.Contains("<Module>")) //Initializer's name always starts with <Module>{guid}
                    continue;

                foreach (var method in type.Methods
                             .Where(m => m.HasBody && m.Body.HasInstructions))
                {
                    var instructions = method.Body.Instructions;
                    for (var curIndex = 0; curIndex < instructions.Count; curIndex++)
                    {
                        if (!instructions[curIndex].IsLdcI4() ||
                            instructions[curIndex + 1].OpCode.Code != Code.Stfld) continue;

                        var value = instructions[curIndex].GetLdcI4Value();
                        if (!(instructions[curIndex + 1].Operand is IField secondField)) continue;
                        CLogger.Instance.Write_Info(
                            $"Field MDToken: {secondField.MDToken.ToInt32()}, Value: {value}");
                        _fieldCache.Add(secondField, value);

                        //add field and value to cache
                    }
                }
            }
        }
    }
}