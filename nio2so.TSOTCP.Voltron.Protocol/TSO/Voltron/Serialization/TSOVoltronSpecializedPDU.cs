using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization
{
    public interface ITSOVoltronSpecializedPDUHeader
    {
        public uint MessageLength { get; set; }
    }

    /// <summary>
    /// The <see cref="TSOVoltronSpecializedPacket{TAttribute, THeader}"/> is a PDU that provides a deeper layer of functionality
    /// beneath that of a normal <see cref="TSOVoltronPacket"/> by offering support for a Header structure and the use of a specialized
    /// <see cref="Attribute"/> for its properties
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <typeparam name="THeader"></typeparam>
    public abstract class TSOVoltronSpecializedPacket<TAttribute, THeader> : TSOVoltronPacket where TAttribute : Attribute where THeader : ITSOVoltronSpecializedPDUHeader
    {
        protected abstract THeader Header { get; }

        protected IEnumerable<PropertyInfo> GetSpecializedWrapperProperties()
        {
            return GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
            .Where(
                //Hard-Coded absolutely avoided names
                x => x.Name != "VoltronPacketType" && x.Name != "FriendlyPDUName" &&
                //Get only DBWrapperField attributed properties
                x.GetCustomAttribute<TAttribute>() != default
            );
        }

        protected override IEnumerable<PropertyInfo> GetPropertiesToCopy()
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            properties.AddRange(base.GetPropertiesToCopy());
            properties.AddRange(GetSpecializedWrapperProperties());
            return properties;
        }

        protected string GetParameterListString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var property in GetPropertiesToCopy())
                sb.Append($"{property.Name}: {property.GetValue(this)}, ");
            string text = sb.ToString();
            if (text.Length > 1)
                text = text.Remove(text.Length - 2);
            return text;
        }
    }
}
