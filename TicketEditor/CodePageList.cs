using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TicketEditor
{
    public struct CodePage
    {
        public int CpNumber { get; set; }
        public string CpName { get; set; }
    }
    class CodePageList : List<CodePage> 
    {
        

        public CodePageList()
        {

            string[] codepages = Properties.Resources.codepages.Split('\n');

            foreach (string cp in codepages)
            {
                CodePage codepage = new CodePage();
                codepage.CpName = cp.Split(':')[0];
                codepage.CpNumber = Convert.ToInt16(cp.Split(':')[1]);
                this.Add(codepage);


            }
            
        }
        
    }
}
