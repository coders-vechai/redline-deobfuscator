
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
class Program
{

    static void noping(int start,int end, IList<Instruction> instructions)
    {
        for (int index = start; index <=end; index++)
        {
            instructions[index].OpCode = OpCodes.Nop;
        }

    }

    static bool IsStringArrayObfuscation(IList<Instruction> instructions, int index) {

        try
        {

            String operand = (instructions[index].GetOperand() == null) ? "" : instructions[index].GetOperand().ToString();

            if (instructions[index].OpCode == OpCodes.Newobj && operand.Contains("String::.ctor(System.Char[])"))
            {
                if (index - 2 >= 0 && instructions[index - 2].OpCode == OpCodes.Ldtoken)
                {
                    return true;
                }
            }


        }
        catch { }

        return false;
    }


    static bool IsReplaceObfuscation(IList<Instruction> instructions, int index) {
        
        try
        {
            String operand = (instructions[index].GetOperand() == null) ? "" : instructions[index].GetOperand().ToString();

            if ( (instructions[index].OpCode == OpCodes.Call || instructions[index].OpCode == OpCodes.Callvirt) && operand.Contains("String::Replace"))
            {
                if (index - 3 >= 0 && instructions[index - 1].OpCode == OpCodes.Ldsfld &&
                    instructions[index - 2].OpCode == OpCodes.Ldstr &&
                    instructions[index - 3].OpCode == OpCodes.Ldstr)
                {

                    return true;
                }
            }
        }

        catch { }

       return false;
    
    }

    static bool deobfuscateStringCharArray(ModuleDefMD module, IList<Instruction> instructions, int index) {

        try
        {
            if (instructions[index - 2].OpCode == OpCodes.Ldtoken)
            {
                FieldDef field = (FieldDef)instructions[index - 2].Operand;
                byte[] Bytes = module.ReadDataAt(field.RVA, Convert.ToInt32(field.GetFieldSize()));
                StringBuilder str = new StringBuilder();
               
                for (int i = 0 ;i < Bytes.Length; i+=2)
                {
                    str.Append(Convert.ToChar(Bytes[i]));
                }
                
                Console.WriteLine(str.ToString());

                instructions[index].OpCode = OpCodes.Ldstr;
                instructions[index].Operand = str.ToString();

                noping(index - 5, index - 1, instructions);
               
                return true;
            }
        }

        catch {}

        return false;

    }

    static bool deobfuscateReplaceObfus(ModuleDefMD module, IList<Instruction> instructions, int index)
    {
      try
        {
            String str = (instructions[index - 3].GetOperand() == null) ? null : instructions[index - 3].GetOperand().ToString();
            String str2 = (instructions[index - 2].GetOperand() == null) ? null : instructions[index - 2].GetOperand().ToString();
            String str3 = (instructions[index - 1].GetOperand() == null) ? null : instructions[index - 1].GetOperand().ToString();

            if (!String.IsNullOrEmpty(str) && !String.IsNullOrEmpty(str2) && str3.Contains("String::Empty"))
            {

                String newStr = str.Replace(str2, String.Empty);
                instructions[index].OpCode = OpCodes.Ldstr;
                instructions[index].Operand = newStr;

                noping(index - 3, index - 1, instructions);
                return true;
            }

        }

        catch { }

        return false;
    }

        static void Main(string[] args)
    {


        try
        {

            if (args.Length < 1) { Console.WriteLine("[x] Input file not supplied"); return; }
            if (args.Length < 2) { Console.WriteLine("[x] Output file not supplied"); return; }


            var module = ModuleDefMD.Load(args[0]);

            if (module == null)
            {
                Console.WriteLine("[x] module failed to load...");
                return;
            }

            foreach (var Class in module.GetTypes())
            {

                foreach (var method in Class.Methods)
                {
                    if (!method.HasBody || !method.Body.HasInstructions) continue;

                    IList<Instruction> instructions = method.Body.Instructions;

                    for (int index = 0; index < instructions.Count; index++)
                    {

                        if (IsStringArrayObfuscation(instructions, index))
                        {

                            if (!deobfuscateStringCharArray(module, instructions, index))
                            {
                                Console.WriteLine("[x] Failed @" + "Class = " + Class.Name + ", methodName = " + method.Name + ", Instruction # " + index + 1);
                                return;
                            }

                        }

                        if (IsReplaceObfuscation(instructions, index))
                        {

                            if (!deobfuscateReplaceObfus(module, instructions, index))
                            {
                                Console.WriteLine("[x] Failed @" + "Class = " + Class.Name + ", methodName = " + method.Name + ", Instruction # " + index + 1);
                                return;
                            }


                        }

                    }


                    Console.WriteLine("=> Success @ class = " + Class.Name + ",  methodName = " + method.Name);

                }

            }


            Console.WriteLine("=> deobfuscation is done successfully....");

            ModuleWriterOptions options = new ModuleWriterOptions(module);

            options.Logger = dnlib.DotNet.DummyLogger.NoThrowInstance;

            module.Write(args[1], options);
        }

        catch (Exception e)
        {

            Console.WriteLine( "[x] Exceptions ==> "+ e.Message);


        }



    }
}