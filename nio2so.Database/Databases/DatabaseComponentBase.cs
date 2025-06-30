using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Database.Databases
{
    internal abstract class DatabaseComponentBase<T1, T2>
    {
        private string _baseDir;

        /// <summary>
        /// Creates a new <see cref="DatabaseComponentBase{T1, T2}"/> with the given home directory
        /// </summary>
        /// <param name="HomeDirectory"></param>
        protected DatabaseComponentBase(string HomeDirectory)
        {
            _baseDir = HomeDirectory;
            if (typeof(T1) != typeof(uint) || typeof(T1) != typeof(string))            
                throw new ArgumentException("You can only have a key type of UInt32 or String for this implementation");            
        }

        public void Set(T1 Key, T2 Value)
        {

        }

        public bool TryGet(T1 Key, out T2? Value)
        {
            return false;
        }
    }
}
