using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using HarmonyLib;
using ReactorStringDecrypter.Context;
using ReactorStringDecrypter.Logger;
using Code = dnlib.DotNet.Emit.Code;

namespace ReactorStringDecrypter.Decrypter
{
    internal class StringDecrypter : IDecrypters
    {
        public string Information => "Decrypts strings inside an assembly";

        public void Decrypt(FileContext ctx)
        {
            foreach (var type in ctx._module.GetTypes().Where(t => t.HasMethods))
            foreach (var method in type.Methods
                         .Where(m => m.HasBody && m.Body.HasInstructions))
            {
                var instructions = method.Body.Instructions;
                for (var curIndex = 0; curIndex < instructions.Count; curIndex++)
                {
                    if (!instructions[curIndex].IsLdcI4()
                        || instructions[curIndex + 1].OpCode.Code != Code.Call) continue;
                    if (!(instructions[curIndex + 1].Operand is MethodDef decrypter))
                        continue;
                    if (!IsDecrypter(decrypter)) continue; //check if method is the correct decrypter 

                    //TODO : better checks???
                    var decrypt_token = decrypter.MDToken.ToInt32();
                    var tk = instructions[curIndex].GetLdcI4Value();

                    var currentDecrypter = StacktracePatcher.PatchStackTraceGetMethod.__reeeee =
                        ctx._asm.ManifestModule
                                .ResolveMethod(
                                    decrypt_token) as
                            MethodInfo; //Set replace method to current Decrypter to prevent exception


                    if (currentDecrypter != null) //Check if current decrypter is null
                    {
                        var decryptedString = (string)currentDecrypter.Invoke(null, new object[] { tk });

                        CLogger.Instance.Write_Info($"Decrypted String: {decryptedString}");
                        instructions[curIndex].OpCode = OpCodes.Ldstr;
                        instructions[curIndex].Operand = decryptedString;
                    }

                    instructions[curIndex + 1].OpCode = OpCodes.Nop;
                }
            }
        }

        private static bool IsDecrypter(MethodDef m)
        {
            if (m.Parameters.Count != 1)
                return false;
            if (m.Parameters[0].Type != m.Module.CorLibTypes.Int32)
                return false;
            if (!m.HasReturnType)
                return false;
            if (m.ReturnType != m.Module.CorLibTypes.String)
                return false;
            if (!m.IsStatic)
                return false;
            return true;
        }

        public class StacktracePatcher
        {
            private const string HarmonyId = "_";
            private static Harmony harmony;

            public static void Patch()
            {
                harmony = new Harmony(HarmonyId);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            [HarmonyPatch(typeof(StackFrame), "GetMethod")]
            public class PatchStackTraceGetMethod
            {
                public static MethodInfo __reeeee;

                public static void Postfix(ref MethodBase __result)
                {
                    if (__result.DeclaringType != typeof(RuntimeMethodHandle))
                        return;
                    __result = __reeeee ?? MethodBase.GetCurrentMethod();
                }
            }
        }
    }
}