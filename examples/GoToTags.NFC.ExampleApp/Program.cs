using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToTags.NFC.ExampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
            finally
            {
                Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("Done. Press 'Enter' to quit.");
            Console.ReadLine();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
        }

        private static void Dispose()
        {
        }
    }
}
