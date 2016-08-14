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
        private int success = 0;
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
            this.success = 0;
            this.countView = 0;
            this.timeout = Math.Abs(Timeout); ;

            this.mutex = new Mutex();
        }

        private HttpStatusCode Get()
        {
            HttpStatusCode result = HttpStatusCode.BadRequest;

            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this.uri);
                HttpWebResponse httpWebResponse = null;

                httpWebRequest.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                httpWebRequest.Headers.Add(@"Accept-Language", @"vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2");
                httpWebRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.82 Safari/537.36";
                httpWebRequest.Timeout = this.timeout;

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (httpWebResponse != null)
                {
                    result = httpWebResponse.StatusCode;

                    httpWebResponse.Close();
                }
            }
            catch (Exception ex) { }

            return result;
        }

        private void Do(int threadID)
        {
            string shortUrl = null;
            HttpStatusCode httpStatusCode;

            if (this.uri.AbsoluteUri.Length > 33)
                shortUrl = this.uri.AbsoluteUri.Substring(0, 15) +
                    @"..." +
                    this.uri.AbsoluteUri.Substring(this.uri.AbsoluteUri.Length - 15, 15);

            while (true)
            {
                httpStatusCode = this.Get();

                this.mutex.WaitOne();

                this.countView++;

                if (httpStatusCode == HttpStatusCode.OK)
                {
                    this.success++;
                }

                Console.WriteLine("Thread {0,3}:{1,30}\t{2,5}|{3,0}|{4,0}",
                    threadID, shortUrl, this.success, this.countView, httpStatusCode);

                if (this.countView >= this.totalView)
                {
                    this.mutex.ReleaseMutex();
                    break;
                }

                this.mutex.ReleaseMutex();
            }
        }

        public void Run()
        {
            List<Thread> threads = new List<Thread>();
            int threadCount = 1;
            IniFile iniFile = new IniFile(@"Settings.ini");

            int.TryParse(iniFile.Read(@"Threads", @"Settings"), out threadCount);

            if (threadCount > this.totalView)
                threadCount = this.totalView;

            for (int i = 0; i < threadCount; i++)
            {
                threads.Add(new Thread(delegate () { this.Do(i); }));
                threads[i].Start();
            }
        }
    }
}
