using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeFormat;


namespace Objects
{
    [Serializable]
    public partial class Variable : IValidate
    {
        // Enumerations
        public enum Kind
        {
            Input = 0,
            Output = 1,
            Local = 2,
            Global = 3
        }
        public enum State
        {
            Free = 0,
            Filled = 1,
            Used = 2,
            Not_Initialized = 3
        }
        public enum Purpose
        {
            Original = 1,
            Temporary = 2,
            Fake = 3,
            ConstRecalculation = 4,
            Parameter = 5
        }

        // Attributes
        private IDManager _ID;
        public string ID
        {
            get { return _ID.ToString(); }
        }
        public int? memoryUnitSize { get; private set; }
        public bool pointer { get; private set; }
        public string name { get; private set; }
        public int memoryRegionSize { get; private set; }
        public string fixedValue { get; private set; }
        public string globalID { get; private set; }
        public int? fixedMin { get; set; }
        public int? fixedMax { get; set; }
        public bool fake { get; private set; }
        public Kind kind { get; set; }


        public static int varID = 32100000;
        private static String getID(int num)
        {
            return "ID_" + num + "-0000-464F-A363-485CE6CC25F7";
        }

        // Constructor
        public Variable(VariableType var, Kind kind1)
        {

            _ID = new IDManager(var.ID.Value);
            name = var.Value;
            memoryRegionSize = Convert.ToInt32(var.MemoryRegionSize.Value);
            if (var.MemoryUnitSize.Exists())
                memoryUnitSize = Convert.ToInt32(var.MemoryUnitSize.Value);
            else
                memoryUnitSize = null;
            pointer = var.Pointer.Value;
            fixedValue = var.FixedValue.Exists() ? var.FixedValue.Value : string.Empty;
            globalID = var.GlobalID.Exists() ? var.GlobalID.Value : string.Empty;
            fake = var.Fake.Exists() ? var.Fake.Value : false;
            kind = kind1;
            if (var.MinValue.Exists())
                fixedMin = Convert.ToInt32(var.MinValue.Value);
            else
                fixedMin = null;
            if (var.MaxValue.Exists())
                fixedMax = Convert.ToInt32(var.MaxValue.Value);
            else
                fixedMax = null;
        }

        public Variable(Kind kind, Purpose purpose, Common.MemoryRegionSize memory_region_size = Common.MemoryRegionSize.Integer, int? min_value = null, int? max_value = null)
        {
            _ID = new IDManager();
            switch (purpose)
            {
                case Purpose.Original:
                    name = string.Concat("v_", ID);
                    break;
                case Purpose.Temporary:
                    name = string.Concat("t_", ID);
                    break;
                case Purpose.Fake:
                    name = string.Concat("f_", ID);
                    break;
                case Purpose.ConstRecalculation:
                    name = string.Concat("c_", ID);
                    break;
                case Purpose.Parameter:
                    name = string.Concat("p_", ID);
                    break;
                default:
                    throw new ObjectException("Unsupported Variable.Purpose value.");
            }
            memoryRegionSize = Convert.ToInt32(memory_region_size);
            pointer = false;
            fake = true;
            fixedMin = min_value;
            fixedMax = max_value;
            this.kind = kind;

        }


        public Variable(Variable var)
        {
            this._ID = new IDManager(var.ID);
            this.name = var.name;
            this.memoryRegionSize = var.memoryRegionSize;
            this.pointer = var.pointer;
            this.fake = var.fake;
            this.fixedMin = var.fixedMin;
            this.fixedMax = var.fixedMax;
            this.kind = var.kind;

        }

        public override bool Equals(object obj)
        {
            return (obj as Variable) == null ? base.Equals(obj) : ((Variable)obj).ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public String ResetID()
        {
            _ID = new IDManager(getID(++varID));
            resetName();
            return ID;
        }

        /// <summary>
        /// Set the variable's ID to a new value.
        /// </summary>
        /// <param name="id">The new ID.</param>
        public void ResetID(String id)
        {
            _ID = new IDManager(id);
            resetName();
        }

        /// <summary>
        /// Set the variable's name, by it's original first character and it's current ID.
        /// </summary>
        private void resetName()
        {
            name = String.Concat(this.name[0], "_", ID);
            //name = String.Concat("p_", ID); //TODO
            //kind = Kind.Input;
        }
    }
}
