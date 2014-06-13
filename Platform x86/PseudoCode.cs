using ExchangeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Platform_x86
{
    public static class PseudoCode
    {
        public static XmlDocument GetTAC(string path2PC)
        {
            XmlDocument doc = new XmlDocument();
            ExchangeFormat.Exchange ex = new Exchange(doc);
            RoutineType routine = ex.Routine.Append();
            routine.Description.Value = "zzz";
            Variables glob_vars = routine.Global.Append();
            VariableType gv = glob_vars.Variable.Append();
            gv.GlobalID.Value = "x";
            gv.Pointer.Value = false;
            gv.ID.Value = string.Concat("ID_", Guid.NewGuid().ToString()).ToUpper();
            gv.Value = string.Concat("v_", gv.ID.Value);
            gv.MemoryRegionSize.Value = 4;
            FunctionType f = routine.Function.Append();
            f.GlobalID.Value = "myfunc";
            BasicBlockType bb = f.BasicBlock.Append();
            InstructionType inst = bb.Instruction.Append();
            inst.RefVars.Value = gv.ID.Value;
            inst.StatementType.EnumerationValue = StatementTypeType.EnumValues.eCopy;
            inst.Value = "x := 7";
            return doc;
            
        }
    }
}
