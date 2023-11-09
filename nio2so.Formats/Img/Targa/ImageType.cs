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
    /// The type of image read from the file.
    /// </summary>
    public enum ImageType : byte
    {
        /// <summary>
        /// No image data was found in file.
        /// </summary>
        NO_IMAGE_DATA = 0,

        /// <summary>
        /// Image is an uncompressed, indexed color-mapped image.
        /// </summary>
        UNCOMPRESSED_COLOR_MAPPED = 1,

        /// <summary>
        /// Image is an uncompressed, RGB image.
        /// </summary>
        UNCOMPRESSED_TRUE_COLOR = 2,

        /// <summary>
        /// Image is an uncompressed, Greyscale image.
        /// </summary>
        UNCOMPRESSED_BLACK_AND_WHITE = 3,

        /// <summary>
        /// Image is a compressed, indexed color-mapped image.
        /// </summary>
        RUN_LENGTH_ENCODED_COLOR_MAPPED = 9,

        /// <summary>
        /// Image is a compressed, RGB image.
        /// </summary>
        RUN_LENGTH_ENCODED_TRUE_COLOR = 10,

        /// <summary>
        /// Image is a compressed, Greyscale image.
        /// </summary>
        RUN_LENGTH_ENCODED_BLACK_AND_WHITE = 11
    }
}
