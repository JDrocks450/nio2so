using nio2so.Formats.TSOData;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.Core.Services
{
    /// <summary>
    /// A service that provides access to the TSODataDefinition.dat which the game uses to find the structure of networked types in ITSONetMessageStandard messages.
    /// </summary>
    public class TSODataDefinitionService : ITSOService
    {
        public TSODataDefinitionService(string TSODataDefinitionPath)
        {
            this.TSODataDefinitionPath = TSODataDefinitionPath;            
        }

        public string TSODataDefinitionPath { get; }
        public bool Ready => file != null;
        public ITSOServer Parent { get; set; } = null;

        private TSODataFile file;

        public void Dispose()
        {
            file = null;
        }

        public void Init(ITSOServer Server)
        {
            LoadDefinition();
        }

        /// <summary>
        /// Creates a new <see cref="ExpandoObject"/> with the fields of the <see cref="ITSODataStruct"/> with default values.
        /// </summary>
        /// <param name="StringID"></param>
        /// <param name="TSODataStruct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object GetStructObject(uint StringID, out ITSODataStruct TSODataStruct)
        {            
            if (!Ready)
                throw new InvalidOperationException(nameof(TSODataDefinitionService) + " is not ready. Did the TSODataDefinition.dat file fail to load?");
            return file.GetObject(StringID, out TSODataStruct);
        }
        /// <summary>
        /// <inheritdoc cref="TSODataFile.TryGetObject(string, out TSODataRuntimeObject, object[])"/>
        /// </summary>
        /// <param name="Struct"></param>
        /// <param name="StructObject"></param>
        /// <param name="FieldValues"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool CreateObject(ITSODataStruct Struct, out TSODataRuntimeObject StructObject, params object[] FieldValues)
        {
            if (!Ready)
                throw new InvalidOperationException(nameof(TSODataDefinitionService) + " is not ready. Did the TSODataDefinition.dat file fail to load?");
            return file.TryGetObject(Struct.NameID, out StructObject, FieldValues);
        }

        private void LoadDefinition() => file = TSODataImporter.Import(TSODataDefinitionPath);

        public bool TryGetDataStruct(uint TSOData_TypeID, out ITSODataStruct Struct) => file.TryGetStructByID(TSOData_TypeID, out Struct);

        /// <summary>
        /// Verifies that the provided <see cref="Enum"/> <see langword="struct"/> matches the <see cref="TSODataFile.Strings"/> constants table. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Errors"></param>
        /// <returns></returns>
        public bool VerifyEnum<T>(out string[] Errors) where T : struct, Enum
        {
            List<string> errors = new List<string>();

            string[] valueNames = Enum.GetNames<T>();
            IEnumerable<uint> values = Enum.GetValuesAsUnderlyingType<T>().Cast<uint>();

            if (file.StringsCount != valueNames.Length)
                errors.Add($"Definition Count Mismatch: TSOData File has: {file.StringsCount} entries, {typeof(T).Name} has: {valueNames.Length} entries.");

            foreach(TSODataString StringValue in file.Strings.Select(x => x.Value))
            {
                bool nameMatch = valueNames.Contains(StringValue.Value);
                bool valueMatch = values.Contains(StringValue.ID);
                if (!nameMatch)
                { // name not found
                    if (!valueMatch) // value not found either
                        errors.Add($"Missing definition: TSOData: {StringValue.Value}::{StringValue.ID:X} not present in enum.");
                    else // value was found under a different name
                    {
                        T value = Enum.Parse<T>(StringValue.ID.ToString());
                        errors.Add($"Value Mismatch: TSOData: {StringValue.Value}::{StringValue.ID:X} is instead: {value}::{Enum.Format(typeof(T), value, "X")}");
                    }
                }
                else
                { // name is found
                    T value = Enum.Parse<T>(StringValue.ID.ToString());
                    if (StringValue.Value != value.ToString())
                        errors.Add($"Incorrect Value: TSOData: {StringValue.Value}::{StringValue.ID:X} not matching {value}::{Enum.Format(typeof(T), value, "X")}");
                }
            }

            Errors = errors.ToArray();
            return errors.Count == 0;
        }
    }
}
