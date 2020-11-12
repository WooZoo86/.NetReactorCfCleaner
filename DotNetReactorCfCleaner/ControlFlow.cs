using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using de4dot.blocks.cflow;
using de4dot.blocks;
using System.Reflection;

namespace DotNetReactorCfCleaner
{
    internal static class ControlFlow
    {
        internal class ControlFlow_BlockDeobfuscator : BlockDeobfuscator
        {
            protected override bool Deobfuscate(Block block)
            {
                bool modified = false;
                bool flag = block.LastInstr.OpCode == OpCodes.Switch;
                if (flag)
                {

                }
                return modified;
            }
            public InstructionEmulator ins = new InstructionEmulator();
        }
        public static void ExecuteArithmetic()
        {
            foreach (TypeDef type in Context.module.GetTypes())
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Brtrue && method.Body.Instructions[i + 1].OpCode == OpCodes.Pop && method.Body.Instructions[i - 1].OpCode == OpCodes.Call)
                            {
                                if (method.Body.Instructions[i - 1].Operand.ToString().Contains("System.Boolean"))
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                                }
                                else
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                }
                            }
                            else if (method.Body.Instructions[i].OpCode == OpCodes.Brfalse && method.Body.Instructions[i + 1].OpCode == OpCodes.Pop && method.Body.Instructions[i - 1].OpCode == OpCodes.Call)
                            {
                                if (method.Body.Instructions[i - 1].Operand.ToString().Contains("System.Boolean"))
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                }
                                else
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Execute()
        {
            foreach (TypeDef type in Context.module.GetTypes())
            {
                foreach (MethodDef method in type.Methods)
                {
                    bool flag = method.HasBody && ContainsControlFlow(method);
                    if (flag)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            CfDeob = new BlocksCflowDeobfuscator();
                            Blocks blocks = new Blocks(method);
                            List<Block> test = blocks.MethodBlocks.GetAllBlocks();
                            blocks.RemoveDeadBlocks();
                            blocks.RepartitionBlocks();
                            blocks.UpdateBlocks();
                            blocks.Method.Body.SimplifyBranches();
                            blocks.Method.Body.OptimizeBranches();
                            CfDeob.Initialize(blocks);
                            CfDeob.Add(new ControlFlow_BlockDeobfuscator());
                            CfDeob.Deobfuscate();
                            blocks.RepartitionBlocks();
                            IList<Instruction> instructions;
                            IList<ExceptionHandler> exceptionHandlers;
                            blocks.GetCode(out instructions, out exceptionHandlers);
                            DotNetUtils.RestoreBody(method, instructions, exceptionHandlers);
                        }
                    }
                }
            }

        }
        private static bool ContainsControlFlow(MethodDef method)
        {
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                bool flag = method.Body.Instructions[i].OpCode == OpCodes.Switch;
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }
        private static BlocksCflowDeobfuscator CfDeob;
    }
}
