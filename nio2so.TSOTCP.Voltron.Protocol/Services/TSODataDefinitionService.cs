using nio2so.Formats.TSOData;
using System;
using System.Collections.Generic;
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

        public object GetObject(uint StringID, out ITSODataStruct TSODataStruct)
        {
            if (!Ready)
                throw new InvalidOperationException(nameof(TSODataDefinitionService) + " is not ready. Did the TSODataDefinition.dat file fail to load?");
            return file.GetObject(StringID, out TSODataStruct);
        }

        private void LoadDefinition() => file = TSODataImporter.Import(TSODataDefinitionPath);
    }
}
