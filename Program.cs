using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media;
using Yarhl.Media.Text;
using System.Text.RegularExpressions;
using PspEbootToolkit.Size;

namespace PspEbootToolkit
{
    class Program
    {
        static void Main(string[] args)
        {
            string option = args[0].ToUpper();

            try
            {
                if (option == "INCREASE")
                {
                    Increaser increase = new Increaser(args[1], args[2], Int32.Parse(args[3], System.Globalization.NumberStyles.HexNumber));
                    increase.ReadHeader();
                }
                
                else
                {
                    Console.WriteLine("Opcion desconocida");
                }
                Console.Write("Done!");
            }

            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
                Console.Read();
            }
        }
    }
}
