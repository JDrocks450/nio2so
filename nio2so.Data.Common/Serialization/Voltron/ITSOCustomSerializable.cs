using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Data.Common.Serialization.Voltron
{
    public interface ITSOCustomSerialize
    {
        public byte[] OnSerialize();        
    }
    public interface ITSOCustomDeserialize
    {
        public void OnDeserialize(Stream Stream);
    }
}
