using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrchidsNamespace
{
    public class Orchids
    {
        private Uri uri = null;
        private int totalView = 0;
        private int countView = 0;
        private int timeout = 10000;
        private Mutex mutex = null;

        public static string ReadAllTextInFile(string Filename)
        {
            string result = null;
            FileStream fileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);

            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                result = streamReader.ReadToEnd();

            return result;
        }

        public Orchids(string Url, int TotalView, int Timeout)
        {
            if (!Uri.TryCreate(Url, UriKind.Absolute, out this.uri))
                Console.WriteLine("Cannot create Uri");

            this.totalView = Math.Abs(TotalView);
            this.countView = 0;
            this.timeout = Math.Abs(Timeout); ;

            this.mutex = new Mutex();
        }

        private bool Get()
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this.uri);
                httpWebRequest.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                httpWebRequest.Headers.Add(@"Accept-Language", @"vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2");
                httpWebRequest.UserAgent = @"vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2";

                using ((HttpWebResponse)httpWebRequest.GetResponse()) { }

                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return false;
        }

        public void Do()
        {
            string shortUrl = null;

            if (this.uri.AbsoluteUri.Length > 33)
                shortUrl = this.uri.AbsoluteUri.Substring(0, 15) +
                    @"..." +
                    this.uri.AbsoluteUri.Substring(this.uri.AbsoluteUri.Length - 15, 15);

            while (true)
            {
                this.Get();

                this.mutex.WaitOne();
                this.countView++;

                if (this.countView > this.totalView)
                    break;

                Console.WriteLine(shortUrl + "\t" + this.countView);

                this.mutex.ReleaseMutex();
            }
        }
    }
}
