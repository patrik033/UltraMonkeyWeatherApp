using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMonkeyLibrary
{

    public abstract class Types
    {
        public abstract Task<List<string>> ReturnResult(bool orderBy, string roomType);
        protected abstract Task<List<string>> OrderByType(string roomType, List<string> temp);
        protected abstract Task<List<string>> OrderByDescendingType(string roomType, List<string> temp);
    }
}
