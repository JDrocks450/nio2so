﻿namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    internal class TSOGetCharBlobByIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// The remainder comes from the SetCharBlobByID response packet made by Pre-Alpha
		/// in response to clicking OK in CAS.It is still not completely understood.
        /// </summary>
        private static byte[] niotso_TSOPreAlphaDefaultSimData =
        {
#region DATA
            0x01,0x00,0x00,0x00,0x0A,0x6E,0x6F,0x74,0x20,0x6E,0x65,0x65,0x64,0x65,0x64,0xA1,
            0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x51,0x00,0x00,0x00,0x00,0x80,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1B,0x00,0x00,
            0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x10,0x00,0x00,0x00,0x16,
            0x17,0x18,0x19,0x1A,0x1B,0x1C,0x1D,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,
            0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,
            0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,
            0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,
            0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,
            0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0x00,0x00,0x00,0x00,0xC8,0x42,0x00,
            0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,
            0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x00,0x00,0xC8,0x42,0x03,
            0x00,0x20,0x21,0x22,0x23,0x01,0x00,0x00,0x00,0x13,0x12,0x11,0x10,0x02,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x1B,0x1A,0x19,0x18,0x3A,0x05,0x00,0x00,0x01,0x00,0x00,
            0x00,0x04,0x00,0x00,0x00,0x6F,0x38,0xD0,0xFC,0x04,0x00,0x00,0x00,0x0F,0x0E,0x0D,
            0x0C,0x37,0x36,0x35,0x34,0x3F,0x3E,0x3D,0x3C,0x3B,0x3A,0x39,0x38,0x01,0x1E,0x00,
            0x00,0x00,0x05,0x61,0x64,0x75,0x6C,0x74,0x00,0x31,0x62,0x39,0x38,0x30,0x6D,0x61,
            0x66,0x69,0x74,0x5F,0x63,0x74,0x2D,0x72,0x61,0x76,0x65,0x2D,0x31,0x30,0x2C,0x42,
            0x4F,0x44,0x59,0x3D,0x62,0x39,0x38,0x30,0x6D,0x61,0x66,0x69,0x74,0x6C,0x67,0x74,
            0x5F,0x63,0x74,0x2D,0x72,0x61,0x76,0x65,0x2D,0x31,0x30,0x00,0x27,0x63,0x30,0x31,
            0x39,0x6D,0x61,0x5F,0x68,0x61,0x6C,0x6A,0x6F,0x72,0x2C,0x48,0x45,0x41,0x44,0x2D,
            0x48,0x45,0x41,0x44,0x3D,0x63,0x30,0x31,0x39,0x6D,0x61,0x6C,0x67,0x74,0x5F,0x72,
            0x61,0x76,0x65,0x33,0x00,0x04,0x48,0x45,0x41,0x44,0x00,0x03,0x54,0x6F,0x70,0x00,
            0x06,0x52,0x5F,0x48,0x41,0x4E,0x44,0x00,0x04,0x50,0x61,0x6C,0x6D,0x00,0x00,0x00,
            0x12,0x30,0x78,0x30,0x30,0x30,0x30,0x30,0x32,0x42,0x36,0x30,0x30,0x30,0x30,0x30,
            0x30,0x30,0x44,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x04,0x6D,0x61,0x6C,0x65,0x00,
            0x02,0x32,0x37,0x00,0x03,0x6C,0x67,0x74,0x00,0x19,0x6E,0x6D,0x66,0x69,0x74,0x5F,
            0x30,0x31,0x2C,0x42,0x4F,0x44,0x59,0x3D,0x6E,0x6D,0x66,0x69,0x74,0x6C,0x67,0x74,
            0x5F,0x30,0x31,0x00,0x1F,0x6E,0x6D,0x66,0x69,0x74,0x5F,0x30,0x31,0x2C,0x42,0x4F,
            0x44,0x59,0x3D,0x75,0x6D,0x66,0x69,0x74,0x6C,0x67,0x74,0x5F,0x62,0x72,0x69,0x65,
            0x66,0x73,0x30,0x31,0x00,0x11,0x48,0x6D,0x4C,0x4F,0x2C,0x48,0x41,0x4E,0x44,0x3D,
            0x68,0x75,0x61,0x6F,0x6C,0x67,0x74,0x00,0x11,0x48,0x6D,0x52,0x4F,0x2C,0x48,0x41,
            0x4E,0x44,0x3D,0x68,0x75,0x61,0x6F,0x6C,0x67,0x74,0x00,0x11,0x48,0x6D,0x4C,0x50,
            0x2C,0x48,0x41,0x4E,0x44,0x3D,0x68,0x75,0x61,0x70,0x6C,0x67,0x74,0x00,0x11,0x48,
            0x6D,0x52,0x50,0x2C,0x48,0x41,0x4E,0x44,0x3D,0x68,0x75,0x61,0x70,0x6C,0x67,0x74,
            0x00,0x11,0x48,0x6D,0x4C,0x43,0x2C,0x48,0x41,0x4E,0x44,0x3D,0x68,0x75,0x61,0x63,
            0x6C,0x67,0x74,0x00,0x11,0x48,0x6D,0x52,0x43,0x2C,0x48,0x41,0x4E,0x44,0x3D,0x68,
            0x75,0x61,0x63,0x6C,0x67,0x74,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x01,0x74,0x14,0x48,0x6D,0x4C,0x4F,0x2C,0x48,0x41,0x4E,0x44,0x3D,0x67,0x75,0x61,
            0x6F,0x5F,0x70,0x69,0x6E,0x6B,0x67,0x01,0x74,0x14,0x48,0x6D,0x52,0x4F,0x2C,0x48,
            0x41,0x4E,0x44,0x3D,0x67,0x75,0x61,0x6F,0x5F,0x70,0x69,0x6E,0x6B,0x67,0x01,0x74,
            0x08,0x4A,0x6F,0x6C,0x6C,0x79,0x53,0x69,0x6D,0x01,0x42
#endregion
        };
        
        private static byte[] ReadDefaultSimData() => File.ReadAllBytes("/packets/const/TSOSimData.dat");

        [TSOVoltronDBWrapperField] public uint AvatarID { get; private set; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetCharBlobByIDResponse(string AriesID, string MasterID, uint avatarID) :
            base(
                    AriesID,
                    MasterID,
                    0x00,
                    TSODBWrapperMessageSize.AutoSize,
                    TSO_PreAlpha_DBStructCLSIDs.GZCLSID_cCrDMStandardMessage,
                    0x21,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByIDResponse,
                    CombineArrays(new byte[]
                    {
                        0x00,0x00,0x05,0x39, // <--- AVATARID
                        0x01,
                        0x00,0x00,0x03,0x20,
                        0x00,0x1B,0x03,0x00,0x00,
                    },
                    niotso_TSOPreAlphaDefaultSimData)
                )
        {
            AvatarID = avatarID;

            MoveBufferPositionToDBMessageBody();
            EmplaceBody(AvatarID); // <--- overwrite avatarid here for now
                                   // 
            ReadAdditionalMetadata();
        }
    }
}