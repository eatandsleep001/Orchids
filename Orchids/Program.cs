using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchidsNamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                DoWithoutArgs();
        }

        static void DoWithoutArgs()
        {
            string url = null;
            int totalView = 0;
            int timeout = 0;
            Orchids orchids = null;

            Console.Write("Url: ");
            url = Console.ReadLine();

            do
                Console.Write("Total view: ");
            while (!int.TryParse(Console.ReadLine(), out totalView));

            do
                Console.Write("Timeout: ");
            while (!int.TryParse(Console.ReadLine(), out timeout));

            orchids = new Orchids(url, totalView, timeout);
            orchids.Run();
        }
    }
}
