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
    /// <summary>
    /// The Targa format of the file.
    /// </summary>
    public enum TGAFormat
    {
        /// <summary>
        /// Unknown Targa Image format.
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// Original Targa Image format.
        /// </summary>
        /// <remarks>Targa Image does not have a Signature of ""TRUEVISION-XFILE"".</remarks>
        ORIGINAL_TGA = 100,

        /// <summary>
        /// New Targa Image format
        /// </summary>
        /// <remarks>Targa Image has a TargaFooter with a Signature of ""TRUEVISION-XFILE"".</remarks>
        NEW_TGA = 200
    }
}
