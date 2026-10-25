using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    internal abstract class TSOValue_VectorBase<T> : TSOValueBase<T>, ITSOValueVector
    {
        public override abstract TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; }
        [Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes.TSOVoltronArrayLength(nameof(Value))]
        public uint Length { get; set; }            
        public override T Value { get => base.Value; set => base.Value = value; }

        protected TSOValue_VectorBase() : base() { }
        protected TSOValue_VectorBase(T Value) : base(Value) { }

        public override string ToString() => 
            $"{TypeClassID} {GetType().Name} = ({typeof(T).Name}) {GetItemCount()} items.";

        public ICollection GetItemCollection() => (ICollection)Value;

        public int GetItemCount() => GetItemCollection().Count;
    }

    internal class TSOValue_PropertyVector : TSOValue_VectorBase<TSOValue_TSODataStructProperty[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Property;

        public TSOValue_PropertyVector()
        {
        }

        public TSOValue_PropertyVector(TSOValue_TSODataStructProperty[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Uint32Vector : TSOValue_VectorBase<uint[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint32;

        public TSOValue_Uint32Vector()
        {
        }

        public TSOValue_Uint32Vector(uint[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
    internal class TSOValue_StringVector : TSOValue_VectorBase<string[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_String;

        public TSOValue_StringVector()
        {
        }

        public TSOValue_StringVector(string[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_BooleanVector : TSOValue_VectorBase<bool[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint8;

        public TSOValue_BooleanVector()
        {
        }

        public TSOValue_BooleanVector(bool[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Uint8Vector : TSOValue_VectorBase<byte[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint8;

        public TSOValue_Uint8Vector()
        {
        }

        public TSOValue_Uint8Vector(byte[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Sint8Vector : TSOValue_VectorBase<sbyte[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Sint8;

        public TSOValue_Sint8Vector()
        {
        }

        public TSOValue_Sint8Vector(sbyte[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Uint16Vector : TSOValue_VectorBase<ushort[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint16;

        public TSOValue_Uint16Vector()
        {
        }

        public TSOValue_Uint16Vector(ushort[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Sint16Vector : TSOValue_VectorBase<short[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Sint16;

        public TSOValue_Sint16Vector()
        {
        }

        public TSOValue_Sint16Vector(short[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Sint32Vector : TSOValue_VectorBase<int[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Sint32;

        public TSOValue_Sint32Vector()
        {
        }

        public TSOValue_Sint32Vector(int[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Uint64Vector : TSOValue_VectorBase<ulong[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint64;

        public TSOValue_Uint64Vector()
        {
        }

        public TSOValue_Uint64Vector(ulong[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Sint64Vector : TSOValue_VectorBase<long[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Sint64;

        public TSOValue_Sint64Vector()
        {
        }

        public TSOValue_Sint64Vector(long[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
}
