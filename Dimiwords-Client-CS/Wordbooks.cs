using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimiwords_Client_CS
{
    public struct Wordbooks
    {
        public string[] ko;
        public string en;

        public Wordbooks(string[] Ko, string En)
        {
            ko = Ko;
            en = En;
        }
    }
}
