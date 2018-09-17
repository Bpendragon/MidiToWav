using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiToWav
{
    class Program
    {
        static void Main(string[] args)
        {
            MidiStream ms = new MidiStream(args[0]);

        }
    }
}
