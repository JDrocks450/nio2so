using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace nio2so.TSOProtocol.Packets.TSOXML
{
    /// <summary>
    /// This Attribute can be applied to a Property to override the name it is given in the function <see cref="TSOXMLPacket.MakePacket(ITSOXMLStructure, XElement?)"/>
    /// <para>Writing in an outside assembly? Use <see cref="JsonPropertyNameAttribute"/> instead.</para>
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class TSOXMLElementName : Attribute
    {
        public TSOXMLElementName(string ElementName)
        {
            this.ElementName = ElementName;
        }
        
        public string ElementName { get; }
    }

    /// <summary>
    /// When making a TSOXMLStructure positional record, inherit from this to use the <see cref="TSOXMLPacket.MakePacket(ITSOXMLStructure, XElement?)"/> function
    /// </summary>
    public interface ITSOXMLStructure
    {

    }

    /// <summary>
    /// A base class for Packets that are based on TSOXML. 
    /// <para>TSOXML is packets that the TSOClient can interpret that use XML as their structure.</para>
    /// </summary>
    public abstract class TSOXMLPacket
    {
        public const string TSOXMLBaseNodeName = "TSOXMLRoot";

        /// <summary>
        /// The root element of this Packet
        /// </summary>
        public XElement RootElement { get; }

        /// <summary>
        /// An empty packet with a root node that equals <see cref="TSOXMLBaseNodeName"/> by default.
        /// </summary>
        public TSOXMLPacket(string RootName = TSOXMLBaseNodeName)
        {
            RootElement = new XElement(RootName);
        }
        public TSOXMLPacket(XElement Root) { RootElement = Root; }

        /// <summary>
        /// Fills the <paramref name="BaseNode"/> with data found in the <paramref name="structuredData"/>
        /// </summary>
        /// <param name="structuredData">Elements can use the <see cref="TSOXMLElementName"/> attribute to customize their serialized name.</param>
        /// <param name="BaseNode"></param>
        protected void SerializeXML(object structuredData, XElement? BaseNode = default)
        {
            if (BaseNode == null) BaseNode = RootElement;
            foreach (var property in structuredData.GetType().GetProperties())
            {
                //NAME
                string name = property.Name; // set name to be property name
                name = property.GetCustomAttribute<TSOXMLElementName>()?.ElementName ?? name; // attribute present, reset name to be attribute name
                name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? name; // attribute present, reset name to be attribute name again
                //VALUE
                object? myValue = property.GetValue(structuredData);
                var newElement = new XElement(name, Convert.ToString(myValue ?? ""));
                BaseNode.Add(newElement);
            }
        }

        //---TOSTRING---
        public override string ToString() => ToString(true, SaveOptions.None);
        public string ToString(bool xmlFormat = true, SaveOptions xmlOptions = SaveOptions.None)
        {
            if (!xmlFormat) return base.ToString();
            return RootElement.ToString(xmlOptions);
        }
    }
}
