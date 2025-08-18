namespace nio2so.Voltron.Core.TSO.Util
{
    /// <summary>
    /// A helper class to offer an extension method for byte[]s to be filled with:
    /// <code>BAADF00D</code>
    /// </summary>
    public static class TSOVoltronArrayFillFunction
    {
        /// <summary>
        /// Takes this array, extends it to the desired length, and fills the garbage bytes with <c>0xBA AD F0 0D</c>
        /// </summary>
        /// <param name="Target"></param>
        /// <param name="FillSize"></param>
        /// <returns></returns>
        public static byte[] TSOFillArray(this byte[] Target, uint FillSize)
        {
            if (Target.Length >= FillSize) return Target;
            byte[] returnArray = new byte[FillSize];
            Target.CopyTo(returnArray, 0);
            for (uint index = (uint)Target.Length; index < FillSize; index++)
            {
                byte fillByte = 0xBA;
                if (index % 4 == 0) ;
                else if (index % 4 == 1) fillByte = 0xAD;
                else if (index % 4 == 2) fillByte = 0xF0;
                else fillByte = 0x0D;
                returnArray[index] = fillByte;
            }
            return returnArray;
        }
        /// <summary>
        /// Takes this array and fills the whole thing with garbage bytes: <c>0xBA AD F0 0D</c>
        /// </summary>
        /// <param name="Target"></param>
        /// <param name="FillSize"></param>
        /// <returns></returns>
        public static byte[] TSOFillArray(this byte[] Target)
        {
            uint FillSize = (uint)Target.Length;
            if (Target.Length >= FillSize) return Target;
            byte[] returnArray = new byte[FillSize];
            Target.CopyTo(returnArray, 0);
            for (uint index = (uint)Target.Length; index < FillSize; index++)
            {
                byte fillByte = 0xBA;
                if (index % 4 == 0) ;
                else if (index % 4 == 1) fillByte = 0xAD;
                else if (index % 4 == 2) fillByte = 0xF0;
                else fillByte = 0x0D;
                returnArray[index] = fillByte;
            }
            return returnArray;
        }
    }
}
