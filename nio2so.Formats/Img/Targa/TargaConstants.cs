// ==========================================================
// TargaImage
//
// Design and implementation by
// - David Polomis (paloma_sw@cox.net)
//
//
// This source code, along with any associated files, is licensed under
// The Code Project Open License (CPOL) 1.02
// A copy of this license can be found in the CPOL.html file 
// which was downloaded with this source code
// or at http://www.codeproject.com/info/cpol10.aspx
//
// 
// COVERED CODE IS PROVIDED UNDER THIS LICENSE ON AN "AS IS" BASIS,
// WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,
// INCLUDING, WITHOUT LIMITATION, WARRANTIES THAT THE COVERED CODE IS
// FREE OF DEFECTS, MERCHANTABLE, FIT FOR A PARTICULAR PURPOSE OR
// NON-INFRINGING. THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE
// OF THE COVERED CODE IS WITH YOU. SHOULD ANY COVERED CODE PROVE
// DEFECTIVE IN ANY RESPECT, YOU (NOT THE INITIAL DEVELOPER OR ANY
// OTHER CONTRIBUTOR) ASSUME THE COST OF ANY NECESSARY SERVICING,
// REPAIR OR CORRECTION. THIS DISCLAIMER OF WARRANTY CONSTITUTES AN
// ESSENTIAL PART OF THIS LICENSE. NO USE OF ANY COVERED CODE IS
// AUTHORIZED HEREUNDER EXCEPT UNDER THIS DISCLAIMER.
//
// Use at your own risk!
//
// ==========================================================

namespace nio2so.Formats.Img.Targa
{
    internal static class TargaConstants
    {
        // constant byte lengths for various fields in the Targa format
        internal const int HeaderByteLength = 18;
        internal const int FooterByteLength = 26;
        internal const int FooterSignatureOffsetFromEnd = 18;
        internal const int FooterSignatureByteLength = 16;
        internal const int FooterReservedCharByteLength = 1;
        internal const int ExtensionAreaAuthorNameByteLength = 41;
        internal const int ExtensionAreaAuthorCommentsByteLength = 324;
        internal const int ExtensionAreaJobNameByteLength = 41;
        internal const int ExtensionAreaSoftwareIDByteLength = 41;
        internal const int ExtensionAreaSoftwareVersionLetterByteLength = 1;
        internal const int ExtensionAreaColorCorrectionTableValueLength = 256;
        internal const string TargaFooterASCIISignature = "TRUEVISION-XFILE";
    }
}
