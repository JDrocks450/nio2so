namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    internal class TSOValue_Uint32 : TSOValueBase<uint>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Uint32;

        public TSOValue_Uint32()
        {
        }

        public TSOValue_Uint32(uint Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
    internal class TSOValue_Sint32 : TSOValueBase<int>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Sint32;

        public TSOValue_Sint32()
        {
        }

        public TSOValue_Sint32(int Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Uint16 : TSOValueBase<ushort>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Uint16;

        public TSOValue_Uint16()
        {
        }

        public TSOValue_Uint16(ushort Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
    internal class TSOValue_Sint16 : TSOValueBase<short>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Sint16;

        public TSOValue_Sint16()
        {
        }

        public TSOValue_Sint16(short Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Uint8 : TSOValueBase<byte>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Uint8;

        public TSOValue_Uint8()
        {
        }

        public TSOValue_Uint8(byte Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
    internal class TSOValue_Sint8 : TSOValueBase<sbyte>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Sint8;

        public TSOValue_Sint8()
        {
        }

        public TSOValue_Sint8(sbyte Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }

    internal class TSOValue_Uint64 : TSOValueBase<ulong>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Uint64;

        public TSOValue_Uint64()
        {
        }

        public TSOValue_Uint64(ulong Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
    internal class TSOValue_Sint64 : TSOValueBase<long>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Sint64;

        public TSOValue_Sint64()
        {
        }

        public TSOValue_Sint64(long Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
    internal class TSOValue_iunknown : TSOValueBase<byte[]>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = (TSO_NewImproved_TSOValueBase_CLSIDs)TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Unknown;

        public TSOValue_iunknown()
        {
        }

        public TSOValue_iunknown(byte[] Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
}
