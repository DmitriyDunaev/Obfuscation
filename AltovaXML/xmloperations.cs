// xmloperations.cs
// This file contains generated code and will be overwritten when you rerun code generation.

using System.Xml;
using Altova.TypeInfo;

namespace Altova.Xml
{
	/// <summary>
	/// Fault exception propagates fault as a node back to main WS method
	/// </summary>
	public class FaultException : System.Exception
	{
		private XmlNode node;
		private string thrower;
			
		public FaultException(XmlNode n, string t)
		{
			node = n;
			thrower = t;
		}
		
		public XmlNode Node { get { return node; } }
		public string Thrower { get { return thrower; } }
	}
	
    public class UTF16Encoding : System.Text.UnicodeEncoding
    {
        public UTF16Encoding( bool bBigEndian, bool bBOM )
            : base( bBigEndian, bBOM )
        {  }

        public override string BodyName { get { return "UTF-16"; } }
        public override string EncodingName { get { return "UTF-16"; } }
        public override string WebName { get { return "UTF-16"; } }
    }

    public class XmlTreeOperations
    {

		public static void CopyAll(XmlNode src, XmlNode tgt)
        {
            //nodes
            foreach (XmlNode node in src.ChildNodes)
                tgt.AppendChild(tgt.OwnerDocument.ImportNode(node, true));
            // attributes
            foreach (XmlNode attr in src.Attributes)
                tgt.Attributes.Append((XmlAttribute)tgt.OwnerDocument.ImportNode(attr, true));
        }
		
        private static XmlFormatter GetFormatter(MemberInfo member)
        {
            if (member.DataType.Formatter != null)
                return (XmlFormatter) member.DataType.Formatter;
            else
                return (XmlFormatter) Xs.AnySimpleTypeFormatter;
        }

        public class AllIterator : System.Collections.IEnumerable, System.IDisposable
        {
            private XmlNodeList list;

            public AllIterator(XmlNodeList list)
            {
                this.list = list;
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return list.GetEnumerator();
            }
            
            public void Dispose()
            {
            }
        }

        public class MemberIterator : System.Collections.IEnumerable, System.IDisposable
        {
            private AllIterator iterator;
            private MemberInfo memberInfo;

            public MemberIterator(XmlNodeList list, MemberInfo info)
            {
                iterator = new AllIterator(list);
                memberInfo = info;
            }
            
            public System.Collections.IEnumerator GetEnumerator() 
            {
                return new Enumerator(iterator, memberInfo);
            }
            
            public void Dispose()
            {
                iterator.Dispose();
            }
            
            class Enumerator : System.Collections.IEnumerator
            {
                System.Collections.IEnumerator iterator;
                MemberInfo info;
                
                public Enumerator(AllIterator list, MemberInfo info)
                {
                    this.iterator = list.GetEnumerator();
                    this.info = info;
                }
                
                public void Reset()
                {
                    iterator.Reset();
                }

                public object Current
                {
                    get
                    {
                        return iterator.Current;
                    }
                }

                public bool MoveNext()
                {
                    while (iterator.MoveNext())
                    {
                        if (IsMember((XmlNode)iterator.Current, info))
                            return true;
                    }
                    return false;
                }
            }
        }

        public static bool IsEqualString(string a, string b)
        {
            if (a == b) return true;
            if (a == null) a="";
            if (b == null) a="";
            return a == b;
        }

        public static bool IsMember( XmlNode node,  MemberInfo member)
        {
            if (member.LocalName == "")
                return node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA;

            if (node.NodeType != XmlNodeType.Element)
                return false;


            string nodeURI = node.NamespaceURI == null ? "" : node.NamespaceURI;
			string nodeLocalName = node.LocalName == null ? "" : node.LocalName;
			string memberURI = member.NamespaceURI == null ? "" : member.NamespaceURI;
			string memberLocalName = member.LocalName == null ? "" : member.LocalName;

			// soap-array specialty: no-namespace elements are array members.
			if ((member.Flags & MemberFlags.SpecialName) != 0)
				return nodeURI == "";

			if (nodeURI == memberURI && nodeLocalName == memberLocalName)
				return true;
			return false;
        }

		public static XmlNode Dereference(XmlNode node)
		{
			XmlAttribute hrefAtt = node.Attributes["href", ""];
			if (hrefAtt == null)	// common case
				return node;

			string target = hrefAtt.Value;
			if (!target.StartsWith("#"))
				throw new System.InvalidOperationException("Cannot dereference external references.");

			string nodeId = target.Substring(1);
			XmlNode targetNode = node.OwnerDocument.SelectSingleNode("//*[@id='" + nodeId + "']");
			if (targetNode != null)
				return targetNode;
			return node;
		}

	    public static bool Exists( XmlNode node)
        {
            return node != null;
        }

	    public static AllIterator GetElements( XmlNode node)
        {
            return new AllIterator(node.ChildNodes);
        }

	    public static MemberIterator GetElements( XmlNode node,  MemberInfo member)
        {
            return new MemberIterator(node.ChildNodes, member);
        }

	    public static void SetTextValue( XmlNode node, string value)
        {
			node.InnerXml = "";
			if (value != "")	// don't create empty text nodes
				node.InnerText = value;
        }

	    public static string GetTextValue( XmlNode node)
        {
            return node.InnerText;
        }

		static string FindPrefixForNamespace(XmlNode node, string uri)
		{
			if (uri == "http://www.w3.org/XML/1998/namespace")
				return "xml";

			return node.GetPrefixOfNamespace(uri);
		}

		static string FindUnusedPrefix(XmlNode node)
        {
            return FindUnusedPrefix(node, null);
        }

		public static string FindUnusedPrefix(XmlNode node, string prefixHint)
		{
            string pp = prefixHint;
            if (prefixHint == null || prefixHint.Length == 0)
                pp = "n";
            else
            {
                string uri = node.GetNamespaceOfPrefix(pp);
                if (uri == null || uri.Length == 0)
                    return pp;
            }

			int n = 1;
			while (true)
			{
				string s = string.Format(pp+"{0}", n++);
                string uri = node.GetNamespaceOfPrefix(s);
                if (uri == null || uri.Length == 0)
                    return s;
			}
		}
		public static void SetAttribute(XmlNode node, string localName, string namespaceURI, string value)
		{
			XmlAttribute att = node.OwnerDocument.CreateAttribute(localName, namespaceURI);
			att.Value = value;
			node.Attributes.Append(att);
		}

		public static void SetAttribute(XmlNode node, string localName, string namespaceURI, XmlQualifiedName value)
		{
			XmlAttribute att = node.OwnerDocument.CreateAttribute(localName, namespaceURI);
			if (value.Namespace == null || value.Namespace == "")
			{
				att.Value = value.Name;
			}
			else
			{
				string prefix = FindPrefixForNamespace(node, value.Namespace);
				if (prefix == null || prefix == "")
				{
					prefix = FindUnusedPrefix(node);
					XmlAttribute nsatt = node.OwnerDocument.CreateAttribute("xmlns", prefix, "http://www.w3.org/2000/xmlns/");
					nsatt.Value = value.Namespace;
					node.Attributes.Append(nsatt);
				}
				att.Value = prefix + ":" + value.Name;
			}
			node.Attributes.Append(att);
		}

	    public static void SetValue( XmlNode node,  MemberInfo member,  string value)
        {
            if (member.LocalName != "")
            {
				string prefix = "";
				if( member.NamespaceURI != "" )
				{
					prefix = FindPrefixForNamespace(node, member.NamespaceURI );
					if( prefix.Length > 0 )
						prefix += ":";
					else
						prefix = FindUnusedPrefix(node) + ":";
				}				

				XmlElement el = (XmlElement) node;
                XmlAttribute attr = node.OwnerDocument.CreateAttribute(prefix + member.LocalName, member.NamespaceURI);
                attr.Value = value;
                el.SetAttributeNode(attr);
            }
            else
            	SetTextValue(node, value);
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, bool b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, int b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, uint b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, long b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, ulong b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, double b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

        public static void SetValue(XmlNode node, MemberInfo member, Altova.Types.DateTime b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

        public static void SetValue(XmlNode node, MemberInfo member, Altova.Types.Duration b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }
        
        public static void SetValue(XmlNode node, MemberInfo member, byte[] b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

		public static void SetValue(XmlNode node,  MemberInfo member, decimal d)
		{
			SetValue(node, member, GetFormatter(member).Format(d));
		}

		public static void SetValue(XmlNode node, MemberInfo member, Altova.Types.QName qn)
        {
			if (qn.Uri == null)
            {
                SetValue(node, member, qn.LocalName);
                return;
            }
			
            string prefix = node.GetPrefixOfNamespace(qn.Uri);
            if (prefix == null || prefix.Length == 0)
            {
                prefix = FindUnusedPrefix(node, qn.Prefix);
                ((XmlElement)node).SetAttribute("xmlns:" + prefix, qn.Uri);
            }

            SetValue(node, member, prefix + ":" + qn.LocalName);
        }
		
	    public static XmlNode AddElement( XmlNode node, MemberInfo member)
        {
			string prefix = "";
			if( member.NamespaceURI != null )
			{
				prefix = FindPrefixForNamespace(node, member.NamespaceURI );
				if( prefix.Length > 0 )
					prefix += ":";
			}

			XmlDocument doc = node.OwnerDocument;
            if (doc == null)
                doc = (XmlDocument)node;            
            XmlNode newNode = doc.CreateElement(prefix + member.LocalName, member.NamespaceURI);
            node.AppendChild(newNode);
            return newNode;
        }

	    public static double CastToDouble( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToDouble(node.InnerText);
        }

	    public static string CastToString( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToString(node.InnerText);
        }

	    public static long CastToInt64( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToInt64(node.InnerText);
        }

	    public static ulong CastToUInt64( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToUInt64(node.InnerText);
        }

	    public static uint CastToUInt( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToUInt(node.InnerText);
        }

	    public static int CastToInt( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToInt(node.InnerText);
        }

	    public static bool CastToBool( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToBool(node.InnerText);
        }

        public static Altova.Types.DateTime CastToDateTime(XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToDateTime(node.InnerText);
        }

        public static Altova.Types.Duration CastToDuration(XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToDuration(node.InnerText);
        }

        public static byte[] CastToBinary(XmlNode node, MemberInfo member)
        {
            return GetFormatter(member).ParseBinary(node.InnerText);
        }

		public static decimal CastToDecimal(XmlNode node, MemberInfo member)
		{
			return Altova.CoreTypes.CastToDecimal(node.InnerText);
		}
		
		public static Altova.Types.QName CastToQName(XmlNode node, MemberInfo member)
        {
            int i = node.InnerText.IndexOf(":");
            if (i == -1)
                return new Altova.Types.QName(node.GetNamespaceOfPrefix(""), node.InnerText);

            string prefix = node.InnerText.Substring(0, i);
            string local = node.InnerText.Substring(i + 1);
            
            string uri = node.GetNamespaceOfPrefix(prefix);

            return new Altova.Types.QName(uri, prefix, local);
        }

	    public static XmlNode FindAttribute( XmlNode node,  MemberInfo member)
        {
            XmlElement el = (XmlElement) node;
            XmlAttributeCollection attrs = el.Attributes;
            return attrs.GetNamedItem(member.LocalName, member.NamespaceURI);
        }

		public static XmlDocument LoadDocument(Altova.IO.Input input)
        {
			if (input is Altova.IO.FileInput)
			{
				return LoadDocument(((Altova.IO.FileInput)input).Filename);
			}
			switch (input.Type)
			{
                case Altova.IO.Input.InputType.Stream:
                    return LoadDocument(input.Stream);

                case Altova.IO.Input.InputType.Reader:
                    return LoadDocument(input.Reader);

                case Altova.IO.Input.InputType.XmlDocument:
                    return input.Document;

                default:
                    throw new System.Exception("Unknown input type");
            }
        }

        public static XmlDocument LoadDocument(System.IO.TextReader reader)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            return doc;
        }
		
		public static XmlDocument LoadDocument( string filename )
		{
			XmlDocument doc = new XmlDocument();
			doc.Load( filename );
			return doc;
		}

		public static XmlDocument LoadXmlBinary( byte[] xmlTree )
		{
			System.IO.MemoryStream strm = new System.IO.MemoryStream( xmlTree );

			XmlDocument doc = new XmlDocument();
			doc.Load( strm );
			return doc;
		}

		public static XmlDocument LoadXml( string xmlString )
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( xmlString );
			return doc;
		}
		
		public static XmlDocument LoadDocument(System.IO.Stream stream)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            return doc;
        }

		public static void SaveDocument(XmlDocument doc, Altova.IO.Output output, string encoding, bool bBigEndian, bool bBOM, bool prettyPrint)
        {
            SaveDocument(doc, output, GetEncodingObject(encoding, bBigEndian, bBOM), prettyPrint);
        }

        public static void SaveDocument(XmlDocument doc, Altova.IO.Output output, System.Text.Encoding encoding, bool prettyPrint)
        {
            switch (output.Type)
            {
                case Altova.IO.Output.OutputType.Stream:
                    SaveDocument(doc, output.Stream, encoding, prettyPrint);
                    break;

                case Altova.IO.Output.OutputType.Writer:
                    SaveDocument(doc, output.Writer, prettyPrint);
                    break;

                case Altova.IO.Output.OutputType.XmlDocument:
                    break;

                default:
                    throw new System.Exception("Unknown output type");
            }
        }
		
		public static void SaveDocument( XmlDocument doc, string filename, bool prettyPrint )
		{
			SaveDocument( doc, filename, "UTF-8", false, false, prettyPrint );
		}

		public static void SaveDocument( XmlDocument doc, string filename, string encoding, bool bBigEndian, bool bBOM, bool prettyPrint )
		{
			SaveDocument( doc, filename, GetEncodingObject(encoding, bBigEndian, bBOM), prettyPrint );
		}
		
		public static void SaveDocument( XmlDocument doc, string filename, string encoding, bool prettyPrint )
		{
			SaveDocument( doc, filename, System.Text.Encoding.GetEncoding(encoding), prettyPrint );
		}
		
		public static void SaveDocument(XmlDocument doc, System.IO.Stream stream, bool prettyPrint)
        {
            SaveDocument(doc, stream, "UTF-8", false, false, prettyPrint);
        }

        public static void SaveDocument(XmlDocument doc, System.IO.Stream stream, string encoding, bool bBigEndian, bool bBOM, bool prettyPrint)
        {
            SaveDocument(doc, stream, GetEncodingObject(encoding, bBigEndian, bBOM), prettyPrint);
        }

        public static void SaveDocument(XmlDocument doc, System.IO.Stream stream, string encoding, bool prettyPrint)
        {
            SaveDocument(doc, stream, System.Text.Encoding.GetEncoding(encoding), prettyPrint);
        }
		
		public static void SaveDocument( XmlDocument doc, System.IO.Stream stream, System.Text.Encoding encoding, bool prettyPrint )
        {
            SaveDocument(doc, new XmlTextWriter( stream, encoding ), prettyPrint);
        }

        public static void SaveDocument(XmlDocument doc, System.IO.TextWriter writer, bool prettyPrint)
        {
            SaveDocument(doc, new XmlTextWriter(writer), prettyPrint);
        }
		
		private static void SaveDocument(XmlDocument doc, XmlTextWriter xmlWriter, bool prettyPrint)
		{
			if (prettyPrint)
            {
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.IndentChar = '\t';
                xmlWriter.Indentation = 1;
            }
            else
                xmlWriter.Formatting = Formatting.None;

            doc.Save(xmlWriter);
            xmlWriter.Flush();
		}
		
		public static void SaveDocument( XmlDocument doc, string filename, System.Text.Encoding encoding, bool prettyPrint )
		{
            System.IO.Stream stream = System.IO.File.Create(filename);
            SaveDocument(doc, stream, encoding, prettyPrint);
            stream.Close();
		}

		public static byte[] SaveXmlBinary( XmlDocument doc, string encoding, bool bBigEndian, bool bBOM, bool prettyPrint )
		{
			return SaveXmlBinary( doc, GetEncodingObject(encoding, bBigEndian, bBOM), prettyPrint );
		}

		public static byte[] SaveXmlBinary( XmlDocument doc, string encoding, bool prettyPrint )
		{
			return SaveXmlBinary( doc, System.Text.Encoding.GetEncoding(encoding), prettyPrint );
		}

		public static byte[] SaveXmlBinary( XmlDocument doc, System.Text.Encoding encoding, bool prettyPrint )
		{
			System.IO.MemoryStream strm = new System.IO.MemoryStream();
			XmlTextWriter writer = new XmlTextWriter( strm, encoding );
			if( prettyPrint )
			{
				writer.Formatting = Formatting.Indented;
				writer.IndentChar = '\t';
				writer.Indentation = 1;
			}
			else
				writer.Formatting = Formatting.None;

			doc.Save( writer );
			writer.Close();

			return strm.ToArray();  // don't use GetBuffer() - it adds unused zero bytes
		}

		public static string SaveXml( XmlDocument doc, bool prettyPrint )
		{
			return SaveXml( doc, prettyPrint, false );
		}
		
		public static string SaveXml( XmlDocument doc, bool prettyPrint, bool omitXmlDec )
		{
			System.IO.StringWriter strwriter = new System.IO.StringWriter();
			XmlTextWriter writer = new XmlTextWriter( strwriter );
			if( prettyPrint )
			{
				writer.Formatting = Formatting.Indented;
				writer.IndentChar = '\t';
				writer.Indentation = 1;
			}
			else
				writer.Formatting = Formatting.None;

			doc.Save( writer );
			writer.Close();
			strwriter.Close();

			string str = strwriter.ToString();
            if (omitXmlDec)
            {
                int i = str.IndexOf("<?xml");
                if (i >= 0)
                {
                    i = str.IndexOf('>', i);
                    str = str.Substring(i+1);
                }
            }

            return str;
		}

		private static System.Text.Encoding GetEncodingObject( string encoding, bool bBigEndian, bool bBOM )
		{
			int unisize = GetUnicodeSizeFromEncodingName( encoding );

			if( unisize == 1 )
				return new System.Text.UTF8Encoding( bBOM );

			if( unisize == 2 )
				return new UTF16Encoding( bBigEndian, bBOM );

			return System.Text.Encoding.GetEncoding( encoding );
		}

		private static int GetUnicodeSizeFromEncodingName( string encoding )
		{
			if( encoding == null ) return 0;
			encoding = encoding.ToUpper();

			if( encoding.IndexOf("UTF-8") >= 0 )
				return 1;

			if( encoding.IndexOf("UTF-16") >= 0 || encoding.IndexOf("UCS-2") >= 0 )
				return 2;

			return 0;
		}

		public static XmlDocument CreateDocument()
		{
			return new XmlDocument();
		}
	}; // class XmlTreeOperations
} // namespace Altova.Xml