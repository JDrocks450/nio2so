﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Util
{
    /// <summary>
    /// A helper class to offer an extension method for byte[]s to be filled with:
    /// <code>BAADF00D</code>
    /// </summary>
    internal static class TSOVoltronArrayFillFunction
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
            for(uint index = (uint)Target.Length; index < FillSize; index++)
            {
                byte fillByte = 0xBA;
                if (index % 4 == 0) ;
                else if (index % 4 == 1) fillByte = 0xAD;
                else if (index % 4 == 2) fillByte = 0xF0;
                else fillByte = 0x0D;
                Target[index] = fillByte;
            }
            return Target;
        }
    }
}
