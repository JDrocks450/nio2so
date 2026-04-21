using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.Core.TSO.Serialization
{
    public class TSOVoltronSerializerTextWriter
    {
        public TSOVoltronTextWriterOptions Options { get; }

        private int level = -1;

        public class TSOVoltronTextWriterOptions
        {
            public const string MatchCurrentIndentationLevelChar = "[*_IND]";

            private TSOVoltronTextWriterOptions() { }

            public static TSOVoltronTextWriterOptions Markdown_Bulletpoints = new TSOVoltronTextWriterOptions()
            {
                InsertHeaderLineChars = "## ",
                InsertLineChars = " * " // bullet point
            };
            public static TSOVoltronTextWriterOptions Markdown_JsonLike = new TSOVoltronTextWriterOptions()
            {
                InsertHeaderLineChars = "## ",
                InsertLineChars = "", // nothing
                AppendHeaderLineChars = "\n```",
                AppendFooterLineChars = "\n```",
                AppendStartClassLineChars = $"\n{MatchCurrentIndentationLevelChar}{{",
                AppendEndClassLineChars = $"\n{MatchCurrentIndentationLevelChar}}}"
            };

            /// <summary>
            /// All lines except the first line will have this added before the content of the line
            /// <para/> For the first line, use <see cref="InsertHeaderLineChars"/>
            /// </summary>
            public string Indent { get; set; } = "\t";
            public string InsertLineChars { get; set; } = " - ";
            /// <summary>
            /// On the first line of the serialized document, add this string preceeding the content
            /// </summary>
            public string InsertHeaderLineChars { get; set; } = "";
            public string AppendHeaderLineChars { get; set; } = "";
            public string AppendFooterLineChars { get; set; } = "";
            public string AppendStartClassLineChars { get; set; } = "";
            public string AppendEndClassLineChars { get; set; } = "";
        }

        public TSOVoltronSerializerTextWriter(TSOVoltronTextWriterOptions? Options = default)
        {
            if (Options == null)
                Options = TSOVoltronTextWriterOptions.Markdown_JsonLike;
            this.Options = Options;
        }

        /// <summary>
        /// Makes a serialized version of this object in string representation
        /// </summary>
        /// <param name="GraphRootNode"></param>
        /// <returns></returns>
        public string GenerateSerializationSummary(TSOVoltronSerializerGraphItem GraphRootNode, bool ShowValues = true, bool AutoInitializeValues = true)
        {
            if (GraphRootNode == null) return "";            

            bool prevValue = TSOVoltronSerializer.CreatingSerializationGraphs;
            TSOVoltronSerializer.CreatingSerializationGraphs = true;

            try
            {
                StringBuilder builder = new();

                level = 0;
                void PushString(TSOVoltronSerializerGraphItem CurrentItem)
                {
                    builder.AppendLine();
                    builder.Append(CreateIndentationLevel());
                    if (level == 0)
                        builder.Append(ProcessStringMacros(Options.InsertHeaderLineChars));
                    if (level > 0)
                        builder.Append(ProcessStringMacros(Options.InsertLineChars));
                    //**error message
                    if (CurrentItem == null)
                    {
                        builder.Append("ERROR: Serialization frame was null.");
                        goto done;
                    }

                    void processAttributes(IEnumerable<Attribute> CustomAttributes)
                    {
                        if (CustomAttributes == null)
                            return;
                        foreach (var attribute in CustomAttributes)
                        {
                            if (!attribute.GetType().FullName.Contains("nio2so")) continue;
                            builder.Append($"[{attribute.GetType().Name}] ");
                        }
                    }

                    //**add type label
                    void processType(Type type)
                    {
                        processAttributes(type.GetCustomAttributes());

                        bool arraySwitch = type.IsArray;
                        //**name of type only
                        if (type.IsArray)
                            type = type.GetElementType() ?? typeof(object);
                        builder.Append(type.Name);
                        if (arraySwitch)
                            builder.Append($"[{(ShowValues ? (CurrentItem.SerializedValue as Array)?.Length ?? 0 : "")}]");

                    }
                    processAttributes(CurrentItem?.PropertyInfo?.GetCustomAttributes());
                    processType(CurrentItem.SerializedType);
                    //**add name label
                    builder.Append(" " + CurrentItem.PropertyName);
                    //**add runtime value label (if applicable)
                    if (!ShowValues) goto done;

                    builder.Append(" = ");
                    builder.Append(CurrentItem.SerializedValueStringFormat ?? "default");
                done:
                    builder.Append($" - {CurrentItem?.ByteLength ?? 0} bytes");
                    if (level == 0)
                        builder.Append(ProcessStringMacros(Options.AppendHeaderLineChars));
                    return;
                }

                void ProcessOne(TSOVoltronSerializerGraphItem CurrentItem)
                {
                    //level++;

                    if (CurrentItem == null)
                    {
                        builder.Append("ERROR: Serialization frame was null.");
                        return;
                    }

                    //CurrentItem is not null

                    if (CurrentItem.SerializedValue == null && AutoInitializeValues)
                    {
                        if (CurrentItem.SerializedType.IsClass)
                        {
                            try
                            {
                                CurrentItem.SerializedValue = Activator.CreateInstance(CurrentItem.SerializedType);
                                TSOVoltronSerializer.Serialize(CurrentItem.SerializedValue);
                                CurrentItem = TSOVoltronSerializer.GetLastGraph();
                            }
                            catch (MissingMethodException ex)
                            {
                                //builder.AppendLine($"\n{CurrentItem.PropertyName} - ERROR: {ex.Message}");
                            }
                        }
                    }

                    PushString(CurrentItem);

                    if (CurrentItem.Count > 0)
                        builder.Append(ProcessStringMacros(Options.AppendStartClassLineChars));

                    //indent in once
                    level++;

                    foreach (var graphItem in CurrentItem)
                        ProcessOne(graphItem);

                    //indent out once
                    level--;

                    if (CurrentItem.Count > 0)
                        builder.Append(ProcessStringMacros(Options.AppendEndClassLineChars));
                }
                ProcessOne(GraphRootNode);
                builder.Append(ProcessStringMacros(Options.AppendFooterLineChars));

                return builder.ToString();
            }
            finally
            {
                TSOVoltronSerializer.CreatingSerializationGraphs = prevValue;
            }            
        }

        string CreateIndentationLevel()
        {
            string returnValue = "";
            for (int i = 0; i < level; i++)
            {
                 returnValue += Options.Indent;
            }
            return returnValue;
        }

        string ProcessStringMacros(string Value)
        {
            string text = Value;
            string indentation = CreateIndentationLevel();
            text = text.Replace(TSOVoltronTextWriterOptions.MatchCurrentIndentationLevelChar, indentation);
            return text;
        }

    }
}
