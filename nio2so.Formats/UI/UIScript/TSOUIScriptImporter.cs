using nio2so.Formats.TSOData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.Formats.UI.UIScript
{

    public class TSOUIScriptImporter : TSOFileImporterBase<UIScriptFile>
    {
        public static UIScriptFile Import(string FilePath) => new TSOUIScriptImporter().ImportFromFile(FilePath);

        public override UIScriptFile Import(Stream stream)
        {
            UIScriptFile file = new UIScriptFile();
            Stack<UIScriptGroup> groupStack = new();

            // ---- EMBEDDED FUNCTIONS
            char lastReadUntilDiscard = '\0';
            bool CanRead() => stream.Position < stream.Length;
            char SafeReadOne()
            {                
                int currentVal = stream.ReadByte();
                if (currentVal == -1) throw new OverflowException("Attempted to read beyond the end of the file!");
                return Encoding.UTF8.GetString(new byte[] { (byte)currentVal })[0];
            }
            string ReadUntil(bool ignoreWhitespace = true, params char[] EndChars)
            { // Note: Last 'EndChar' will be discarded.
                string returnValue = "";
                while (stream.Position < stream.Length)
                {
                    char currentChar = SafeReadOne();
                    if (EndChars.Contains(currentChar))
                    {
                        lastReadUntilDiscard = currentChar;
                        return returnValue;
                    }
                    if (char.IsWhiteSpace(currentChar) && ignoreWhitespace) continue;                    
                    returnValue += currentChar;
                }
                return ""; // this will never happen
            }
            string ReadUntilEnsured(bool ignoreWhitespace = true, params char[] EndChars)
            { // Note: Last 'EndChar' will be discarded.
                string value = " ";
                while (string.IsNullOrWhiteSpace(value))
                {
                    value = ReadUntil(ignoreWhitespace, EndChars);
                }
                return value;
            }
            char SafeReadNextAfterWhitespace()
            {
                while (true)
                {
                    char c = SafeReadOne();
                    if (char.IsWhiteSpace(c)) continue;
                    return c;
                }
            }
            bool ReadProperty(out string propertyName, out string value)
            {
                propertyName = ReadUntil(true, '>', '=');
                value = "";
                if (lastReadUntilDiscard == '>') 
                    return false;
                value = ReadUntilEnsured(true, '\t', ' ', '>');
                return true;
            }
            void AddPropertiesToComponent(UIScriptComponentBase Component)
            {
                do {
                    bool success = ReadProperty(out string propertyName, out string value);
                    if (!success) break;
                    UIScriptComponentPropertyValue valueObject = new(value);
                    Component.MyProperties.Add(propertyName, valueObject);
                    if (lastReadUntilDiscard == '>') break;
                }
                while (CanRead());
            }
            void ReadNamedComponent(UIScriptComponentBase Component)
            {
                char nextChar = SafeReadNextAfterWhitespace();
                string name = "";
                if (nextChar == '"')
                    name = ReadUntil(true, '"');
                if(Component is IUIScriptNamedComponent nameComp)
                    nameComp.Name = name;
                nextChar = SafeReadNextAfterWhitespace();
                stream.Seek(-1,SeekOrigin.Current);
                if (nextChar != '>')
                    AddPropertiesToComponent(Component);
            }
            void DiscardSkipLine() => ReadUntil(true,'\n');
            void AddStackGroupInheritedPropertiesToComponent(UIScriptComponentBase Component)
            {                
                Component.CombineProperties(Component.InheritedProperties, groupStack.Peek().GetProperties());
            }
            void AddToStackObject(UIScriptComponentBase Component, bool InheritProperties = true)
            {
                if (groupStack.TryPeek(out var upperLevelGroup))
                {
                    AddStackGroupInheritedPropertiesToComponent(Component);
                    upperLevelGroup.Items.Add(Component);
                }
                else file.Items.Add(Component);
            }

            //---- START IMPORTER LOGIC            
            while (CanRead())
            {
                int currentVal = stream.ReadByte();
                if (currentVal == -1) break;
                char currentChar = Encoding.UTF8.GetString(new byte[] { (byte)currentVal })[0];
                if (currentChar == '#') // COMMENT
                { // skip to next line
                    string text = ReadUntil(false, '\n');
                    AddToStackObject(new UICommentComponent(text), false); // don't inherit properties on a comment. what a waste that would be
                    continue;
                }
                if (currentChar == '\r' || currentChar == '\n' || currentChar == '\t') continue;
                if (currentChar == '<') // BEGIN TAG
                {
                    string tagName = ReadUntil(true,'>',' ').ToLower();
                    switch (tagName)
                    {
                        case "begin": // Push new group
                            var group = new UIScriptGroup();
                            AddToStackObject(group, true);
                            groupStack.Push(group);
                            break;
                        case "end": // Pop previous group
                            groupStack.Pop();
                            break;
                        case "setsharedproperties": // SET SHARED PROPS
                            AddPropertiesToComponent(groupStack.Peek());
                            DefaultAppendLine(TSOImporterBaseChannel.Message, $"Added SharedProperties to group stackobject!");
                            break;
                        case "setcontrolproperties": // set properties for a control by name
                            UIScriptControlPropertiesComponent ctrl = new();
                            ReadNamedComponent(ctrl);
                            groupStack.Peek().Items.Add(ctrl);
                            DefaultAppendLine(TSOImporterBaseChannel.Message, $"SetControlProperties {ctrl}");
                            break;
                        default:
                            if (tagName.StartsWith("define")) // define a new constant
                            {
                                string subType = tagName.Substring(6);
                                UIScriptDefineComponent define = new(subType, "");
                                ReadNamedComponent(define);
                                AddToStackObject(define, true);
                                DefaultAppendLine(TSOImporterBaseChannel.Message, $"Defined {define}");
                                break;
                            }
                            if (tagName.StartsWith("add")) // add a new control to the canvas (group)
                            {
                                string subType = tagName.Substring(3);
                                UIScriptObject obj = new(subType, "");
                                ReadNamedComponent(obj);
                                AddToStackObject(obj, true);
                                DefaultAppendLine(TSOImporterBaseChannel.Message, $"Added {obj}");
                                break;
                            }
                            DefaultAppendLine(TSOImporterBaseChannel.Error, $"Character: {stream.Position} Token is unrecognized Token: {tagName}");
                            DiscardSkipLine(); // ERROR OUT!
                            break;
                    }                    
                    continue;
                }
            }
            return file;
        }
    }
}
