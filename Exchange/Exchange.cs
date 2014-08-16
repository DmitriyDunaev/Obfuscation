// YOU SHOULD NOT MODIFY THIS FILE


namespace ExchangeFormat
{
    abstract class EnumeratorBase : System.Collections.IEnumerator
    {
        System.Collections.IEnumerator inner;

        protected object InnerCurrent { get { return inner.Current; } }

        public EnumeratorBase(System.Collections.IEnumerator inner)
        {
            this.inner = inner;
        }

        public abstract object Current { get; }// { return new NumberType((System.Xml.XmlNode)inner.Current); } }
        public bool MoveNext() { return inner.MoveNext(); }
        public void Reset() { inner.Reset(); }
    }


    public class Variables : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_Variables]); } }


        public Variables(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

            Variable = new MemberElement_Variable(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_Variables_helper_Variable]);
        }

        internal class Enumerator : EnumeratorBase
        {
            public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
            public override object Current { get { return new Variables((System.Xml.XmlNode)InnerCurrent); } }
        }

        // Attributes


        // Elements

        public MemberElement_Variable Variable;
        public class MemberElement_Variable : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Variable(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public VariableType this[int i] { get { return At(i); } }
            public VariableType At(int index) { return new VariableType(owner.GetElementAt(info, index)); }
            public VariableType First { get { return new VariableType(owner.GetElementFirst(info)); } }
            public VariableType Last { get { return new VariableType(owner.GetElementLast(info)); } }
            public VariableType Append() { return new VariableType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new VariableType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
        public void SetXsiType()
        {
            XmlHelper.Xml.XmlTreeOperations.SetAttribute(Node, "xsi:type", "http://www.w3.org/2001/XMLSchema-instance",
                new System.Xml.XmlQualifiedName("Variables", ""));
        }

    } // class Variables

    public class myID : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_myID]); } }


        public myID(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

        }


        // Attributes


        // Elements

    } // class myID

    public class myIDREFS : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_myIDREFS]); } }


        public myIDREFS(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

        }


        // Attributes


        // Elements

    } // class myIDREFS

    public class myVariable : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_myVariable]); } }


        public myVariable(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

        }


        // Attributes


        // Elements

    } // class myVariable

    public class Exchange : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_Exchange2]); } }


        public static Exchange LoadFromFile(string filename)
        {
            return new Exchange(XmlHelper.Xml.XmlTreeOperations.LoadDocument(filename));
        }

        public static Exchange LoadFromString(string xmlstring)
        {
            return new Exchange(XmlHelper.Xml.XmlTreeOperations.LoadXml(xmlstring));
        }

        public static Exchange LoadFromBinary(byte[] binary)
        {
            return new Exchange(XmlHelper.Xml.XmlTreeOperations.LoadXmlBinary(binary));
        }

        public void SaveToFile(string filename, bool prettyPrint)
        {
            System.Xml.XmlDocument doc = (System.Xml.XmlDocument)Node;
            if (doc.FirstChild is System.Xml.XmlDeclaration)
            {
                string encoding = ((System.Xml.XmlDeclaration)doc.FirstChild).Encoding;
                if (encoding == System.String.Empty)
                    XmlHelper.Xml.XmlTreeOperations.SaveDocument(doc, filename, "UTF-8", false, false, prettyPrint);
                else
                    XmlHelper.Xml.XmlTreeOperations.SaveDocument(doc, filename, encoding, prettyPrint);
            }
            else
                XmlHelper.Xml.XmlTreeOperations.SaveDocument(doc, filename, "UTF-8", false, false, prettyPrint);
        }

        public void SaveToFile(string filename, bool prettyPrint, string encoding)
        {
            SaveToFile(filename, prettyPrint, encoding, System.String.Compare(encoding, "UTF-16BE", true) == 0, System.String.Compare(encoding, "UTF-16", true) == 0);
        }

        public void SaveToFile(string filename, bool prettyPrint, string encoding, bool bBigEndian, bool bBOM)
        {
            System.Xml.XmlDocument doc = (System.Xml.XmlDocument)Node;
            XmlHelper.Xml.XmlTreeOperations.SaveDocument(doc, filename, encoding, bBigEndian, bBOM, prettyPrint);
        }

        public string SaveToString(bool prettyPrint)
        {
            System.Xml.XmlDocument doc = (System.Xml.XmlDocument)Node;
            return XmlHelper.Xml.XmlTreeOperations.SaveXml(doc, prettyPrint);
        }

        public byte[] SaveToBinary(bool prettyPrint)
        {
            System.Xml.XmlDocument doc = (System.Xml.XmlDocument)Node;
            if (doc.FirstChild is System.Xml.XmlDeclaration)
            {
                string encoding = ((System.Xml.XmlDeclaration)doc.FirstChild).Encoding;
                if (encoding == System.String.Empty)
                    return XmlHelper.Xml.XmlTreeOperations.SaveXmlBinary(doc, "UTF-8", false, false, prettyPrint);
                else
                    return XmlHelper.Xml.XmlTreeOperations.SaveXmlBinary(doc, encoding, prettyPrint);
            }
            else
                return XmlHelper.Xml.XmlTreeOperations.SaveXmlBinary(doc, "UTF-8", false, false, prettyPrint);
        }

        public byte[] SaveToBinary(bool prettyPrint, string encoding)
        {
            return SaveToBinary(prettyPrint, encoding, System.String.Compare(encoding, "UTF-16BE", true) == 0, System.String.Compare(encoding, "UTF-16", true) == 0);
        }

        public byte[] SaveToBinary(bool prettyPrint, string encoding, bool bBigEndian, bool bBOM)
        {
            System.Xml.XmlDocument doc = (System.Xml.XmlDocument)Node;
            return XmlHelper.Xml.XmlTreeOperations.SaveXmlBinary(doc, encoding, bBigEndian, bBOM, prettyPrint);
        }

        public static Exchange CreateDocument()
        {
            return CreateDocument("UTF-8");
        }

        public static Exchange CreateDocument(string encoding)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", encoding, null));
            return new Exchange(doc);
        }

        public void SetDTDLocation(string dtdLocation)
        {
            System.Xml.XmlDocument doc = (System.Xml.XmlDocument)Node;
            string publicId = null;
            string internalSubset = null;
            if (doc.DocumentElement == null)
                throw new System.InvalidOperationException("SetDTDLocation requires a root element.");
            if (doc.DocumentType != null)
            {
                publicId = doc.DocumentType.PublicId;
                internalSubset = doc.DocumentType.InternalSubset;

                doc.RemoveChild(doc.DocumentType);
            }
            doc.InsertBefore(doc.DocumentElement, doc.CreateDocumentType(doc.DocumentElement.Name, publicId, dtdLocation, internalSubset));
        }

        public void SetSchemaLocation(string schemaLocation)
        {
            System.Xml.XmlDocument doc = (System.Xml.XmlDocument)Node;
            if (doc.DocumentElement == null)
                throw new System.InvalidOperationException("SetSchemaLocation requires a root element.");
            System.Xml.XmlAttribute att;
            if (doc.DocumentElement.NamespaceURI == "")
            {
                att = doc.CreateAttribute("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
                att.Value = schemaLocation;
            }
            else
            {
                att = doc.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
                att.Value = doc.DocumentElement.NamespaceURI + " " + schemaLocation;
            }
            doc.DocumentElement.Attributes.Append(att);
        }


        public Exchange(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

            BasicBlock = new MemberElement_BasicBlock(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_Exchange2_helper_BasicBlock]);
            Function = new MemberElement_Function(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_Exchange2_helper_Function]);
            Instruction = new MemberElement_Instruction(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_Exchange2_helper_Instruction]);
            Routine = new MemberElement_Routine(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_Exchange2_helper_Routine]);
            Variable = new MemberElement_Variable(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_Exchange2_helper_Variable]);
        }

        internal class Enumerator : EnumeratorBase
        {
            public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
            public override object Current { get { return new Exchange((System.Xml.XmlNode)InnerCurrent); } }
        }

        // Attributes


        // Elements

        public MemberElement_BasicBlock BasicBlock;
        public class MemberElement_BasicBlock : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_BasicBlock(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public BasicBlockType this[int i] { get { return At(i); } }
            public BasicBlockType At(int index) { return new BasicBlockType(owner.GetElementAt(info, index)); }
            public BasicBlockType First { get { return new BasicBlockType(owner.GetElementFirst(info)); } }
            public BasicBlockType Last { get { return new BasicBlockType(owner.GetElementLast(info)); } }
            public BasicBlockType Append() { return new BasicBlockType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new BasicBlockType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
        public MemberElement_Function Function;
        public class MemberElement_Function : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Function(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public FunctionType this[int i] { get { return At(i); } }
            public FunctionType At(int index) { return new FunctionType(owner.GetElementAt(info, index)); }
            public FunctionType First { get { return new FunctionType(owner.GetElementFirst(info)); } }
            public FunctionType Last { get { return new FunctionType(owner.GetElementLast(info)); } }
            public FunctionType Append() { return new FunctionType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new FunctionType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
        public MemberElement_Instruction Instruction;
        public class MemberElement_Instruction : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Instruction(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public InstructionType this[int i] { get { return At(i); } }
            public InstructionType At(int index) { return new InstructionType(owner.GetElementAt(info, index)); }
            public InstructionType First { get { return new InstructionType(owner.GetElementFirst(info)); } }
            public InstructionType Last { get { return new InstructionType(owner.GetElementLast(info)); } }
            public InstructionType Append() { return new InstructionType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new InstructionType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
        public MemberElement_Routine Routine;
        public class MemberElement_Routine : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Routine(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public RoutineType this[int i] { get { return At(i); } }
            public RoutineType At(int index) { return new RoutineType(owner.GetElementAt(info, index)); }
            public RoutineType First { get { return new RoutineType(owner.GetElementFirst(info)); } }
            public RoutineType Last { get { return new RoutineType(owner.GetElementLast(info)); } }
            public RoutineType Append() { return new RoutineType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new RoutineType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
        public MemberElement_Variable Variable;
        public class MemberElement_Variable : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Variable(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public VariableType this[int i] { get { return At(i); } }
            public VariableType At(int index) { return new VariableType(owner.GetElementAt(info, index)); }
            public VariableType First { get { return new VariableType(owner.GetElementFirst(info)); } }
            public VariableType Last { get { return new VariableType(owner.GetElementLast(info)); } }
            public VariableType Append() { return new VariableType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new VariableType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
        public void SetXsiType()
        {
            XmlHelper.Xml.XmlTreeOperations.SetAttribute(Node, "xsi:type", "http://www.w3.org/2001/XMLSchema-instance",
                new System.Xml.XmlQualifiedName("Exchange", ""));
        }

    } // class Exchange2

    public class BasicBlockType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_BasicBlockType]); } }


        public BasicBlockType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {
            ID = new MemberAttribute_ID(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_BasicBlockType_helper_ID]);
            Predecessors = new MemberAttribute_Predecessors(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_BasicBlockType_helper_Predecessors]);
            Successors = new MemberAttribute_Successors(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_BasicBlockType_helper_Successors]);
            PolyRequired = new MemberAttribute_PolyRequired(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_BasicBlockType_helper_PolyRequired]);
            Instruction = new MemberElement_Instruction(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_BasicBlockType_helper_Instruction]);
        }

        internal class Enumerator : EnumeratorBase
        {
            public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
            public override object Current { get { return new BasicBlockType((System.Xml.XmlNode)InnerCurrent); } }
        }

        // Attributes
        public MemberAttribute_ID ID;
        public class MemberAttribute_ID
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_ID(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_PolyRequired PolyRequired;
        public class MemberAttribute_PolyRequired
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_PolyRequired(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public bool Value
            {
                get
                {
                    return (bool)XmlHelper.Xml.XmlTreeOperations.CastToBool(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_Predecessors Predecessors;
        public class MemberAttribute_Predecessors
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_Predecessors(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_Successors Successors;
        public class MemberAttribute_Successors
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_Successors(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }


        // Elements

        public MemberElement_Instruction Instruction;
        public class MemberElement_Instruction : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Instruction(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public InstructionType this[int i] { get { return At(i); } }
            public InstructionType At(int index) { return new InstructionType(owner.GetElementAt(info, index)); }
            public InstructionType First { get { return new InstructionType(owner.GetElementFirst(info)); } }
            public InstructionType Last { get { return new InstructionType(owner.GetElementLast(info)); } }
            public InstructionType Append() { return new InstructionType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new InstructionType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
    } // class BasicBlockType

    public class InstructionType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_InstructionType]); } }


        public InstructionType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {
            ID = new MemberAttribute_ID(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_InstructionType_helper_ID]);
            StatementType = new MemberAttribute_StatementType(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_InstructionType_helper_StatementType]);
            RefVars = new MemberAttribute_RefVars(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_InstructionType_helper_RefVars]);

        }

        internal class Enumerator : EnumeratorBase
        {
            public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
            public override object Current { get { return new InstructionType((System.Xml.XmlNode)InnerCurrent); } }
        }

        // Attributes
        public string Value
        {
            get
            {
                XmlHelper.TypeInfo.MemberInfo member = Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_InstructionType_helper_unnamed];
                return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(Node, member);
            }
            set
            {
                XmlHelper.TypeInfo.MemberInfo member = Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_InstructionType_helper_unnamed];
                XmlHelper.Xml.XmlTreeOperations.SetValue(Node, member, value);
            }
        }
        public MemberAttribute_ID ID;
        public class MemberAttribute_ID
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_ID(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_StatementType StatementType;
        public class MemberAttribute_StatementType
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_StatementType(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }


            public StatementTypeType.EnumValues EnumerationValue
            {
                get
                {
                    return (StatementTypeType.EnumValues)GetEnumerationIndex(info, XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info), 0, 9);
                }

                set
                {
                    if ((int)value >= 0 && (int)value < 9)
                        XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, info.DataType.facets[(int)value + 0].stringValue);
                    else
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
        public MemberAttribute_RefVars RefVars;
        public class MemberAttribute_RefVars
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_RefVars(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }


        // Elements

        public void SetXsiType()
        {
            XmlHelper.Xml.XmlTreeOperations.SetAttribute(Node, "xsi:type", "http://www.w3.org/2001/XMLSchema-instance",
                new System.Xml.XmlQualifiedName("anySimpleType", "http://www.w3.org/2001/XMLSchema"));
        }

    } // class InstructionType

    public class FunctionType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_FunctionType]); } }


        public FunctionType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {
            ID = new MemberAttribute_ID(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_FunctionType_helper_ID]);
            GlobalID = new MemberAttribute_GlobalID(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_FunctionType_helper_GlobalID]);
            CalledFrom = new MemberAttribute_CalledFrom(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_FunctionType_helper_CalledFrom]);
            RefInputVars = new MemberAttribute_RefInputVars(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_FunctionType_helper_RefInputVars]);
            RefOutputVars = new MemberAttribute_RefOutputVars(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_FunctionType_helper_RefOutputVars]);

            Local = new MemberElement_Local(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_FunctionType_helper_Local]);
            BasicBlock = new MemberElement_BasicBlock(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_FunctionType_helper_BasicBlock]);
        }

        internal class Enumerator : EnumeratorBase
        {
            public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
            public override object Current { get { return new FunctionType((System.Xml.XmlNode)InnerCurrent); } }
        }

        // Attributes
        public MemberAttribute_ID ID;
        public class MemberAttribute_ID
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_ID(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_GlobalID GlobalID;
        public class MemberAttribute_GlobalID
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_GlobalID(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_CalledFrom CalledFrom;
        public class MemberAttribute_CalledFrom
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_CalledFrom(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }


            public CalledFromType.EnumValues EnumerationValue
            {
                get
                {
                    return (CalledFromType.EnumValues)GetEnumerationIndex(info, XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info), 0, 3);
                }

                set
                {
                    if ((int)value >= 0 && (int)value < 3)
                        XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, info.DataType.facets[(int)value + 0].stringValue);
                    else
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
        public MemberAttribute_RefInputVars RefInputVars;
        public class MemberAttribute_RefInputVars
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_RefInputVars(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_RefOutputVars RefOutputVars;
        public class MemberAttribute_RefOutputVars
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_RefOutputVars(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }


        // Elements

        public MemberElement_Local Local;
        public class MemberElement_Local : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Local(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public Variables this[int i] { get { return At(i); } }
            public Variables At(int index) { return new Variables(owner.GetElementAt(info, index)); }
            public Variables First { get { return new Variables(owner.GetElementFirst(info)); } }
            public Variables Last { get { return new Variables(owner.GetElementLast(info)); } }
            public Variables Append() { return new Variables(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new Variables.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
        public MemberElement_BasicBlock BasicBlock;
        public class MemberElement_BasicBlock : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_BasicBlock(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public BasicBlockType this[int i] { get { return At(i); } }
            public BasicBlockType At(int index) { return new BasicBlockType(owner.GetElementAt(info, index)); }
            public BasicBlockType First { get { return new BasicBlockType(owner.GetElementFirst(info)); } }
            public BasicBlockType Last { get { return new BasicBlockType(owner.GetElementLast(info)); } }
            public BasicBlockType Append() { return new BasicBlockType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new BasicBlockType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
    } // class FunctionType

    public class CalledFromType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_CalledFromType]); } }


        public enum EnumValues
        {
            eInternalOnly = 0, // InternalOnly
            eExternalOnly = 1, // ExternalOnly
            eBoth = 2, // Both
            Invalid = -1, // Invalid value
        };

        public CalledFromType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

        }


        // Attributes


        // Elements

    } // class CalledFromType

    public class RoutineType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_RoutineType]); } }


        public RoutineType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {
            Description = new MemberAttribute_Description(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_RoutineType_helper_Description]);

            Global = new MemberElement_Global(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_RoutineType_helper_Global]);
            Function = new MemberElement_Function(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_RoutineType_helper_Function]);
        }

        internal class Enumerator : EnumeratorBase
        {
            public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
            public override object Current { get { return new RoutineType((System.Xml.XmlNode)InnerCurrent); } }
        }

        // Attributes
        public MemberAttribute_Description Description;
        public class MemberAttribute_Description
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_Description(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }


        // Elements

        public MemberElement_Global Global;
        public class MemberElement_Global : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Global(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public Variables this[int i] { get { return At(i); } }
            public Variables At(int index) { return new Variables(owner.GetElementAt(info, index)); }
            public Variables First { get { return new Variables(owner.GetElementFirst(info)); } }
            public Variables Last { get { return new Variables(owner.GetElementLast(info)); } }
            public Variables Append() { return new Variables(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new Variables.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
        public MemberElement_Function Function;
        public class MemberElement_Function : System.Collections.IEnumerable
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberElement_Function(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public FunctionType this[int i] { get { return At(i); } }
            public FunctionType At(int index) { return new FunctionType(owner.GetElementAt(info, index)); }
            public FunctionType First { get { return new FunctionType(owner.GetElementFirst(info)); } }
            public FunctionType Last { get { return new FunctionType(owner.GetElementLast(info)); } }
            public FunctionType Append() { return new FunctionType(owner.CreateElement(info)); }
            public bool Exists { get { return Count > 0; } }
            public int Count { get { return owner.CountElement(info); } }
            public void Remove() { owner.RemoveElement(info); }
            public void RemoveAt(int index) { owner.RemoveElementAt(info, index); }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new FunctionType.Enumerator(XmlHelper.Xml.XmlTreeOperations.GetElements(owner.Node, info).GetEnumerator());
            }

            public XmlHelper.Xml.Meta.Element Info { get { return new XmlHelper.Xml.Meta.Element(info); } }
        }
    } // class RoutineType

    public class VariableType : myVariableType
    {
        public static new XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_VariableType]); } }


        public VariableType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {
            ID = new MemberAttribute_ID(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_ID]);
            Pointer = new MemberAttribute_Pointer(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_Pointer]);
            MemoryRegionSize = new MemberAttribute_MemoryRegionSize(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_MemoryRegionSize]);
            MemoryUnitSize = new MemberAttribute_MemoryUnitSize(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_MemoryUnitSize]);
            FixedValue = new MemberAttribute_FixedValue(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_FixedValue]);
            MinValue = new MemberAttribute_MinValue(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_MinValue]);
            MaxValue = new MemberAttribute_MaxValue(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_MaxValue]);
            GlobalID = new MemberAttribute_GlobalID(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_GlobalID]);
            Fake = new MemberAttribute_Fake(this, Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_VariableType_helper_Fake]);

        }

        internal new class Enumerator : EnumeratorBase
        {
            public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
            public override object Current { get { return new VariableType((System.Xml.XmlNode)InnerCurrent); } }
        }

        // Attributes
        public MemberAttribute_ID ID;
        public class MemberAttribute_ID
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_ID(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_Pointer Pointer;
        public class MemberAttribute_Pointer
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_Pointer(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public bool Value
            {
                get
                {
                    return (bool)XmlHelper.Xml.XmlTreeOperations.CastToBool(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_MemoryRegionSize MemoryRegionSize;
        public class MemberAttribute_MemoryRegionSize
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_MemoryRegionSize(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public decimal Value
            {
                get
                {
                    return (decimal)XmlHelper.Xml.XmlTreeOperations.CastToDecimal(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_MemoryUnitSize MemoryUnitSize;
        public class MemberAttribute_MemoryUnitSize
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_MemoryUnitSize(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public decimal Value
            {
                get
                {
                    return (decimal)XmlHelper.Xml.XmlTreeOperations.CastToDecimal(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_FixedValue FixedValue;
        public class MemberAttribute_FixedValue
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_FixedValue(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_MinValue MinValue;
        public class MemberAttribute_MinValue
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_MinValue(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public decimal Value
            {
                get
                {
                    return (decimal)XmlHelper.Xml.XmlTreeOperations.CastToDecimal(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_MaxValue MaxValue;
        public class MemberAttribute_MaxValue
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_MaxValue(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public decimal Value
            {
                get
                {
                    return (decimal)XmlHelper.Xml.XmlTreeOperations.CastToDecimal(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_GlobalID GlobalID;
        public class MemberAttribute_GlobalID
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_GlobalID(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public string Value
            {
                get
                {
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }
        public MemberAttribute_Fake Fake;
        public class MemberAttribute_Fake
        {
            private XmlHelper.Xml.TypeBase owner;
            private XmlHelper.TypeInfo.MemberInfo info;
            public MemberAttribute_Fake(XmlHelper.Xml.TypeBase owner, XmlHelper.TypeInfo.MemberInfo info) { this.owner = owner; this.info = info; }
            public bool Value
            {
                get
                {
                    return (bool)XmlHelper.Xml.XmlTreeOperations.CastToBool(XmlHelper.Xml.XmlTreeOperations.FindAttribute(owner.Node, info), info);
                }
                set
                {
                    XmlHelper.Xml.XmlTreeOperations.SetValue(owner.Node, info, value);
                }
            }
            public bool Exists() { return owner.GetAttribute(info) != null; }
            public void Remove() { owner.RemoveAttribute(info); }

            public XmlHelper.Xml.Meta.Attribute Info { get { return new XmlHelper.Xml.Meta.Attribute(info); } }
        }


        // Elements

        public new void SetXsiType()
        {
            XmlHelper.Xml.XmlTreeOperations.SetAttribute(Node, "xsi:type", "http://www.w3.org/2001/XMLSchema-instance",
                new System.Xml.XmlQualifiedName("myVariable", ""));
        }

    } // class VariableType

    public class myVariableType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_myVariableType]); } }


        public myVariableType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

        }

        internal class Enumerator : EnumeratorBase
        {
            public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
            public override object Current { get { return new myVariableType((System.Xml.XmlNode)InnerCurrent); } }
        }

        // Attributes
        public string Value
        {
            get
            {
                XmlHelper.TypeInfo.MemberInfo member = Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_myVariableType_helper_unnamed];
                return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(Node, member);
            }
            set
            {
                XmlHelper.TypeInfo.MemberInfo member = Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_helper_myVariableType_helper_unnamed];
                XmlHelper.Xml.XmlTreeOperations.SetValue(Node, member, value);
            }
        }


        // Elements

        public void SetXsiType()
        {
            XmlHelper.Xml.XmlTreeOperations.SetAttribute(Node, "xsi:type", "http://www.w3.org/2001/XMLSchema-instance",
                new System.Xml.XmlQualifiedName("myVariable", ""));
        }

    } // class myVariableType

    public class MemoryRegionSizeType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_MemoryRegionSizeType]); } }


        public MemoryRegionSizeType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

        }


        // Attributes


        // Elements

    } // class MemoryRegionSizeType

    public class MemoryUnitSizeType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_MemoryUnitSizeType]); } }


        public MemoryUnitSizeType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

        }


        // Attributes


        // Elements

    } // class MemoryUnitSizeType

    public class StatementTypeType : XmlHelper.Xml.TypeBase
    {
        public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_helper_StatementTypeType]); } }


        public enum EnumValues
        {
            eFullAssignment = 0, // FullAssignment
            eUnaryAssignment = 1, // UnaryAssignment
            eCopy = 2, // Copy
            eUnconditionalJump = 3, // UnconditionalJump
            eConditionalJump = 4, // ConditionalJump
            eProcedural = 5, // Procedural
            eIndexedAssignment = 6, // IndexedAssignment
            ePointerAssignment = 7, // PointerAssignment
            eNoOperation = 8, // NoOperation
            Invalid = -1, // Invalid value
        };

        public StatementTypeType(System.Xml.XmlNode init)
            : base(init)
        {
            instantiateMembers();
        }

        private void instantiateMembers()
        {

        }


        // Attributes


        // Elements

    } // class StatementTypeType


    namespace xs
    {
        public class ENTITIES : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_ENTITIES]); } }


            public ENTITIES(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class ENTITIES

        public class ENTITY : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_ENTITY]); } }


            public ENTITY(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class ENTITY

        public class ID : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_ID]); } }


            public ID(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class ID

        public class IDREF : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_IDREF]); } }


            public IDREF(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class IDREF

        public class IDREFS : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_IDREFS]); } }


            public IDREFS(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class IDREFS

        public class NCName : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_NCName]); } }


            public NCName(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class NCName

        public class NMTOKEN : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_NMTOKEN]); } }


            public NMTOKEN(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class NMTOKEN

        public class NMTOKENS : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_NMTOKENS]); } }


            public NMTOKENS(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class NMTOKENS

        public class NOTATION : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_NOTATION]); } }


            public NOTATION(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class NOTATION

        public class Name : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_Name]); } }


            public Name(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class Name

        public class QName : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_QName]); } }


            public QName(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class QName

        public class anySimpleType : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_anySimpleType]); } }


            public anySimpleType(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class anySimpleType

        public class anyType : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.ComplexType StaticInfo { get { return new XmlHelper.Xml.Meta.ComplexType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_anyType]); } }


            public anyType(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }

            internal class Enumerator : EnumeratorBase
            {
                public Enumerator(System.Collections.IEnumerator inner) : base(inner) { }
                public override object Current { get { return new anyType((System.Xml.XmlNode)InnerCurrent); } }
            }

            // Attributes
            public string Value
            {
                get
                {
                    XmlHelper.TypeInfo.MemberInfo member = Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_xs_helper_anyType_helper_unnamed];
                    return (string)XmlHelper.Xml.XmlTreeOperations.CastToString(Node, member);
                }
                set
                {
                    XmlHelper.TypeInfo.MemberInfo member = Exchange_TypeInfo.binder.Members[Exchange_TypeInfo._helper_mi_xs_helper_anyType_helper_unnamed];
                    XmlHelper.Xml.XmlTreeOperations.SetValue(Node, member, value);
                }
            }


            // Elements

            public void SetXsiType()
            {
                XmlHelper.Xml.XmlTreeOperations.SetAttribute(Node, "xsi:type", "http://www.w3.org/2001/XMLSchema-instance",
                    new System.Xml.XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema"));
            }

        } // class anyType

        public class anyURI : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_anyURI]); } }


            public anyURI(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class anyURI

        public class base64Binary : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_base64Binary]); } }


            public base64Binary(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class base64Binary

        public class boolean : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_boolean]); } }


            public boolean(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class boolean

        public class byte2 : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_byte2]); } }


            public byte2(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class byte2

        public class date : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_date]); } }


            public date(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class date

        public class dateTime : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_dateTime]); } }


            public dateTime(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class dateTime

        public class decimal2 : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_decimal2]); } }


            public decimal2(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class decimal2

        public class double2 : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_double2]); } }


            public double2(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class double2

        public class duration : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_duration]); } }


            public duration(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class duration

        public class float2 : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_float2]); } }


            public float2(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class float2

        public class gDay : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_gDay]); } }


            public gDay(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class gDay

        public class gMonth : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_gMonth]); } }


            public gMonth(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class gMonth

        public class gMonthDay : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_gMonthDay]); } }


            public gMonthDay(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class gMonthDay

        public class gYear : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_gYear]); } }


            public gYear(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class gYear

        public class gYearMonth : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_gYearMonth]); } }


            public gYearMonth(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class gYearMonth

        public class hexBinary : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_hexBinary]); } }


            public hexBinary(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class hexBinary

        public class int2 : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_int2]); } }


            public int2(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class int2

        public class integer : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_integer]); } }


            public integer(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class integer

        public class language : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_language]); } }


            public language(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class language

        public class long2 : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_long2]); } }


            public long2(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class long2

        public class negativeInteger : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_negativeInteger]); } }


            public negativeInteger(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class negativeInteger

        public class nonNegativeInteger : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_nonNegativeInteger]); } }


            public nonNegativeInteger(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class nonNegativeInteger

        public class nonPositiveInteger : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_nonPositiveInteger]); } }


            public nonPositiveInteger(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class nonPositiveInteger

        public class normalizedString : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_normalizedString]); } }


            public normalizedString(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class normalizedString

        public class positiveInteger : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_positiveInteger]); } }


            public positiveInteger(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class positiveInteger

        public class short2 : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_short2]); } }


            public short2(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class short2

        public class string2 : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_string2]); } }


            public string2(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class string2

        public class time : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_time]); } }


            public time(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class time

        public class token : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_token]); } }


            public token(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class token

        public class unsignedByte : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_unsignedByte]); } }


            public unsignedByte(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class unsignedByte

        public class unsignedInt : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_unsignedInt]); } }


            public unsignedInt(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class unsignedInt

        public class unsignedLong : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_unsignedLong]); } }


            public unsignedLong(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class unsignedLong

        public class unsignedShort : XmlHelper.Xml.TypeBase
        {
            public static XmlHelper.Xml.Meta.SimpleType StaticInfo { get { return new XmlHelper.Xml.Meta.SimpleType(Exchange_TypeInfo.binder.Types[Exchange_TypeInfo._helper_ti_xs_helper_unsignedShort]); } }


            public unsignedShort(System.Xml.XmlNode init)
                : base(init)
            {
                instantiateMembers();
            }

            private void instantiateMembers()
            {

            }


            // Attributes


            // Elements

        } // class unsignedShort


    } // namespace xs


}
