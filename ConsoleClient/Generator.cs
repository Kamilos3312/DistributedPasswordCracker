using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    public class Generator
    {
        private int maxlength;
        private string ValidChars;
        private List<string> data = new List<string>();

        public Generator() {}
        public Generator(string alphabet)
        {
            this.ValidChars = alphabet;
        }

        public List<string> Generate(string prefix, int maxlength, int level = 0)
        {
            this.maxlength = maxlength - prefix.Length;
            this.data.Clear();
            this.Dive(prefix, level);

            return this.data;
        }

        private void Dive(string prefix, int level)
        {
            level += 1;
            foreach (char c in ValidChars)
            {
                this.data.Add(prefix + c);
                if (level < maxlength)
                {
                    Dive(prefix + c, level);
                }
            }
        }
    }
}
