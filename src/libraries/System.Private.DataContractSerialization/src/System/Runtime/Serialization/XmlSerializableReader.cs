// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace System.Runtime.Serialization
{
    internal sealed class XmlSerializableReader : XmlReader, IXmlLineInfo, IXmlTextParser
    {
        private XmlReaderDelegator _xmlReader = null!; // initialized in BeginRead
        private int _startDepth;
        private bool _isRootEmptyElement;
        private XmlReader _innerReader = null!; // initialized in BeginRead

        // State for a coalesced run of consecutive character-data nodes (Text, Whitespace,
        // SignificantWhitespace). Some readers (for example the one DataContractSerializer creates
        // over a Stream) surface a single logical text value as many small nodes, e.g. when a
        // carriage return is encoded as a character reference and split out as its own whitespace
        // node. While _hasMergedText is true, InnerReader has already been advanced to the node that
        // follows the run, so the position-dependent properties below are served from the saved state.
        private bool _hasMergedText;
        private string? _mergedTextValue;
        private XmlNodeType _mergedTextNodeType;
        private int _mergedTextDepth;
        private bool _mergedTextHasFollowingNode;

        private XmlReader InnerReader
        {
            get { return _innerReader; }
        }

        internal void BeginRead(XmlReaderDelegator xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw XmlObjectSerializerReadContext.CreateUnexpectedStateException(XmlNodeType.Element, xmlReader);
            _xmlReader = xmlReader;
            _startDepth = xmlReader.Depth;
            _innerReader = xmlReader.UnderlyingReader;
            _isRootEmptyElement = InnerReader.IsEmptyElement;
        }

        internal void EndRead()
        {
            if (_isRootEmptyElement)
                _xmlReader.Read();
            else
            {
                if (_xmlReader.IsStartElement() && _xmlReader.Depth == _startDepth)
                    _xmlReader.Read();
                while (_xmlReader.Depth > _startDepth)
                {
                    if (!_xmlReader.Read())
                        throw XmlObjectSerializerReadContext.CreateUnexpectedStateException(XmlNodeType.EndElement, _xmlReader);
                }
            }
        }

        public override bool Read()
        {
            if (_hasMergedText)
            {
                // The InnerReader was advanced to the node that follows the coalesced run while that
                // run was produced. Surface that node now (or end of the element's scope if the run
                // was the last content). The next Read() re-applies the normal start-depth boundary
                // check against InnerReader's current position.
                _hasMergedText = false;
                _mergedTextValue = null;
                return _mergedTextHasFollowingNode;
            }

            XmlReader reader = this.InnerReader;
            if (reader.Depth == _startDepth)
            {
                if (reader.NodeType == XmlNodeType.EndElement ||
                     (reader.NodeType == XmlNodeType.Element && reader.IsEmptyElement))
                {
                    return false;
                }
            }

            if (!reader.Read())
            {
                return false;
            }

            // Coalesce a run of consecutive character-data nodes into a single logical text node.
            // Some readers surface one logical text value as many small nodes: the reader that
            // DataContractSerializer creates over a Stream, for example, encodes a carriage return
            // as the "&#xD;" character reference and emits it as a separate whitespace node, so
            // "line1\r\nline2\r\n" arrives as Text/Whitespace/Whitespace/Text/... Consumers such as
            // XElement.ReadXml append each node's Value with string concatenation, making a value
            // delivered as k chunks cost O(k^2). Merging the run mirrors what a plain
            // XmlReader.Create over the same document already reports (a single Text node) and keeps
            // deserialization linear. CDATA is intentionally excluded so XCData/XmlCDataSection
            // sections remain distinct, matching XmlReader.Create.
            if (IsCoalescibleCharacterData(reader.NodeType))
            {
                XmlNodeType runType = reader.NodeType;
                int depth = reader.Depth;
                string firstValue = reader.Value;

                bool hasFollowingNode = reader.Read();
                if (hasFollowingNode && IsCoalescibleCharacterData(reader.NodeType))
                {
                    StringBuilder builder = new StringBuilder(firstValue);
                    do
                    {
                        runType = StrongerCharacterDataType(runType, reader.NodeType);
                        builder.Append(reader.Value);
                        hasFollowingNode = reader.Read();
                    }
                    while (hasFollowingNode && IsCoalescibleCharacterData(reader.NodeType));

                    _mergedTextValue = builder.ToString();
                }
                else
                {
                    // Single node in the run; InnerReader has still advanced one node past it.
                    _mergedTextValue = firstValue;
                }

                _mergedTextNodeType = runType;
                _mergedTextDepth = depth;
                _mergedTextHasFollowingNode = hasFollowingNode;
                _hasMergedText = true;
            }

            return true;
        }

        private static bool IsCoalescibleCharacterData(XmlNodeType nodeType) =>
            nodeType is XmlNodeType.Text or XmlNodeType.SignificantWhitespace or XmlNodeType.Whitespace;

        // Reports the type a coalesced run should surface as, matching how XmlReader.Create classifies
        // a single merged run: any non-whitespace text wins, otherwise significant whitespace wins.
        private static XmlNodeType StrongerCharacterDataType(XmlNodeType current, XmlNodeType next)
        {
            if (current == XmlNodeType.Text || next == XmlNodeType.Text)
                return XmlNodeType.Text;
            if (current == XmlNodeType.SignificantWhitespace || next == XmlNodeType.SignificantWhitespace)
                return XmlNodeType.SignificantWhitespace;
            return XmlNodeType.Whitespace;
        }

        public override void Close()
        {
            throw XmlObjectSerializer.CreateSerializationException(SR.IXmlSerializableIllegalOperation);
        }

        public override XmlReaderSettings? Settings { get { return InnerReader.Settings; } }
        public override XmlNodeType NodeType { get { return _hasMergedText ? _mergedTextNodeType : InnerReader.NodeType; } }
        public override string Name { get { return _hasMergedText ? string.Empty : InnerReader.Name; } }
        public override string LocalName { get { return _hasMergedText ? string.Empty : InnerReader.LocalName; } }
        public override string NamespaceURI { get { return _hasMergedText ? string.Empty : InnerReader.NamespaceURI; } }
        public override string Prefix { get { return _hasMergedText ? string.Empty : InnerReader.Prefix; } }
        public override bool HasValue { get { return _hasMergedText || InnerReader.HasValue; } }
        public override string Value { get { return _hasMergedText ? _mergedTextValue! : InnerReader.Value; } }
        public override int Depth { get { return _hasMergedText ? _mergedTextDepth : InnerReader.Depth; } }
        public override string BaseURI { get { return InnerReader.BaseURI; } }
        public override bool IsEmptyElement { get { return !_hasMergedText && InnerReader.IsEmptyElement; } }
        public override bool IsDefault { get { return !_hasMergedText && InnerReader.IsDefault; } }
        public override char QuoteChar { get { return InnerReader.QuoteChar; } }
        public override XmlSpace XmlSpace { get { return InnerReader.XmlSpace; } }
        public override string XmlLang { get { return InnerReader.XmlLang; } }
        public override IXmlSchemaInfo? SchemaInfo { get { return _hasMergedText ? null : InnerReader.SchemaInfo; } }
        public override Type ValueType { get { return _hasMergedText ? typeof(string) : InnerReader.ValueType; } }
        public override int AttributeCount { get { return _hasMergedText ? 0 : InnerReader.AttributeCount; } }
        public override string this[int i] { get { return InnerReader[i]; } }
        public override string? this[string name] { get { return InnerReader[name]; } }
        public override string? this[string name, string? namespaceURI] { get { return InnerReader[name, namespaceURI]; } }
        public override bool EOF { get { return InnerReader.EOF; } }
        public override ReadState ReadState { get { return InnerReader.ReadState; } }
        public override XmlNameTable NameTable { get { return InnerReader.NameTable; } }
        public override bool CanResolveEntity { get { return InnerReader.CanResolveEntity; } }
        public override bool CanReadBinaryContent { get { return InnerReader.CanReadBinaryContent; } }
        public override bool CanReadValueChunk { get { return InnerReader.CanReadValueChunk; } }
        public override bool HasAttributes { get { return !_hasMergedText && InnerReader.HasAttributes; } }

        public override string? GetAttribute(string name) { return InnerReader.GetAttribute(name); }
        public override string? GetAttribute(string name, string? namespaceURI) { return InnerReader.GetAttribute(name, namespaceURI); }
        public override string GetAttribute(int i) { return InnerReader.GetAttribute(i); }
        public override bool MoveToAttribute(string name) { return InnerReader.MoveToAttribute(name); }
        public override bool MoveToAttribute(string name, string? ns) { return InnerReader.MoveToAttribute(name, ns); }
        public override void MoveToAttribute(int i) { InnerReader.MoveToAttribute(i); }
        public override bool MoveToFirstAttribute() { return InnerReader.MoveToFirstAttribute(); }
        public override bool MoveToNextAttribute() { return InnerReader.MoveToNextAttribute(); }
        public override bool MoveToElement() { return InnerReader.MoveToElement(); }
        public override string? LookupNamespace(string prefix) { return InnerReader.LookupNamespace(prefix); }
        public override bool ReadAttributeValue() { return InnerReader.ReadAttributeValue(); }
        public override void ResolveEntity() { InnerReader.ResolveEntity(); }
        public override bool IsStartElement() { return _hasMergedText ? base.IsStartElement() : InnerReader.IsStartElement(); }
        public override bool IsStartElement(string name) { return _hasMergedText ? base.IsStartElement(name) : InnerReader.IsStartElement(name); }
        public override bool IsStartElement(string localname, string ns) { return _hasMergedText ? base.IsStartElement(localname, ns) : InnerReader.IsStartElement(localname, ns); }
        public override XmlNodeType MoveToContent() { return _hasMergedText ? base.MoveToContent() : InnerReader.MoveToContent(); }

        // While _hasMergedText is true, InnerReader has already been advanced past the coalesced run, so
        // the content-reading and content-navigation methods below must not delegate to it directly (that
        // would read from the wrong position and lose the merged text). Instead they run the base XmlReader
        // implementations, which consume content through this reader's overridden NodeType/Value/Read/
        // AttributeCount and therefore observe the merged node correctly. In the common, non-merged case
        // they keep delegating to InnerReader to preserve its exact behavior. The binary and value-chunk
        // streaming reads are not reachable for a coalesced character-data run (DataContractSerializer does
        // not split base64/binhex content across nodes), so falling back to the base implementations there
        // is acceptable.
        public override object ReadContentAsObject() { return _hasMergedText ? base.ReadContentAsObject() : InnerReader.ReadContentAsObject(); }
        public override bool ReadContentAsBoolean() { return _hasMergedText ? base.ReadContentAsBoolean() : InnerReader.ReadContentAsBoolean(); }
        public override DateTime ReadContentAsDateTime() { return _hasMergedText ? base.ReadContentAsDateTime() : InnerReader.ReadContentAsDateTime(); }
        public override double ReadContentAsDouble() { return _hasMergedText ? base.ReadContentAsDouble() : InnerReader.ReadContentAsDouble(); }
        public override int ReadContentAsInt() { return _hasMergedText ? base.ReadContentAsInt() : InnerReader.ReadContentAsInt(); }
        public override long ReadContentAsLong() { return _hasMergedText ? base.ReadContentAsLong() : InnerReader.ReadContentAsLong(); }
        public override string ReadContentAsString() { return _hasMergedText ? base.ReadContentAsString() : InnerReader.ReadContentAsString(); }
        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver? namespaceResolver) { return _hasMergedText ? base.ReadContentAs(returnType, namespaceResolver) : InnerReader.ReadContentAs(returnType, namespaceResolver); }
        public override int ReadContentAsBase64(byte[] buffer, int index, int count) { return _hasMergedText ? base.ReadContentAsBase64(buffer, index, count) : InnerReader.ReadContentAsBase64(buffer, index, count); }
        public override int ReadContentAsBinHex(byte[] buffer, int index, int count) { return _hasMergedText ? base.ReadContentAsBinHex(buffer, index, count) : InnerReader.ReadContentAsBinHex(buffer, index, count); }
        public override int ReadValueChunk(char[] buffer, int index, int count) { return _hasMergedText ? base.ReadValueChunk(buffer, index, count) : InnerReader.ReadValueChunk(buffer, index, count); }
        public override string ReadString() { return _hasMergedText ? base.ReadString() : InnerReader.ReadString(); }

        // IXmlTextParser members
        bool IXmlTextParser.Normalized
        {
            get
            {
                IXmlTextParser? xmlTextParser = InnerReader as IXmlTextParser;
                return (xmlTextParser == null) ? _xmlReader.Normalized : xmlTextParser.Normalized;
            }
            set
            {
                if (InnerReader is not IXmlTextParser xmlTextParser)
                    _xmlReader.Normalized = value;
                else
                    xmlTextParser.Normalized = value;
            }
        }

        WhitespaceHandling IXmlTextParser.WhitespaceHandling
        {
            get
            {
                IXmlTextParser? xmlTextParser = InnerReader as IXmlTextParser;
                return (xmlTextParser == null) ? _xmlReader.WhitespaceHandling : xmlTextParser.WhitespaceHandling;
            }
            set
            {
                if (InnerReader is not IXmlTextParser xmlTextParser)
                    _xmlReader.WhitespaceHandling = value;
                else
                    xmlTextParser.WhitespaceHandling = value;
            }
        }

        // IXmlLineInfo members
        bool IXmlLineInfo.HasLineInfo()
        {
            IXmlLineInfo? xmlLineInfo = InnerReader as IXmlLineInfo;
            return (xmlLineInfo == null) ? _xmlReader.HasLineInfo() : xmlLineInfo.HasLineInfo();
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                IXmlLineInfo? xmlLineInfo = InnerReader as IXmlLineInfo;
                return (xmlLineInfo == null) ? _xmlReader.LineNumber : xmlLineInfo.LineNumber;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                IXmlLineInfo? xmlLineInfo = InnerReader as IXmlLineInfo;
                return (xmlLineInfo == null) ? _xmlReader.LinePosition : xmlLineInfo.LinePosition;
            }
        }
    }
}
