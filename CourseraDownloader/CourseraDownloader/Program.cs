using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CourseraDownloader
{
    class Program
    {

        private static CookieContainer cookies;
        static Logger logger;
        
        static void Main(string[] args)
        {

            logger = LogManager.GetCurrentClassLogger();

            string courseId = "compinvesting1-003";

            logger.Info("Starting logger for CourseId: {0}", courseId);

            string outputRoot = @"E:\Coursera";
            string outputDirectory = Path.Combine(outputRoot, courseId);

            logger.Info("Output root:\t{0}", outputRoot);
            logger.Debug("Output path:\t{0}", outputDirectory);

            if (!CleanOutputDirectory(outputDirectory))
            {

                logger.Error("Error cleaning output directory. Exiting.");
                Environment.Exit(1);

            }

            cookies = new CookieContainer();

            cookies.Add(new Cookie("CAUTH", "CAUTH=rOW2CeRiLNyXtRL4YCVynx8HmQFF5iuqo0QW34FpCv3-CNDUgOLSUsiL49wTZ0hQB0DrnacH4XGuoSDXVw1xNg.4icwuSn2_eIWxA8ZzTdLqA.LLIbVkqYGUDSFDpVFXV26_8VRyWsUPw6KvfrhNcRpsDEg7ybal7tRvwVN_pamKdUOgDl5Jht9-bLRNZqmX7peZ7enxlXQ4FQDb6liosj0gr_lC6v63W4HZid6rnWwK0aQL7b_Ru4RLp_XBSfKwhMyLH7eaa0a2SzQz7VvJ0gaVU", "/", "class.coursera.org"));

            HtmlDocument document = GetPage(String.Format("https://class.coursera.org/{0}", courseId));



            if (!DownloadStaticContent(ref document, Path.Combine(outputDirectory, "static")))
            {

                logger.Error("Error downloading static content. Exiting.");
                Environment.Exit(1);

            }






            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//ul[@class='course-navbar-list']/li[@class='course-navbar-item']/a"))
            {

                Console.WriteLine("{0}: {1}", node.InnerText.Trim(), node.Attributes["href"].Value);


            }



            //Console.WriteLine(document.DocumentNode.OuterHtml);

            document.Save(Path.Combine(outputDirectory, "index.html"));

            Console.ReadKey();

        }

        public static bool DownloadStaticContent(ref HtmlDocument document, string resourceDir)
        {

            logger.Debug("Loading static content from document.");

            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//script[@src]"))
            {

                logger.Trace("Node outer HTML: {0}", node.OuterHtml);

                try
                {

                    string fileName = Path.Combine(resourceDir, String.Format("{0}.js", Guid.NewGuid().ToString()));
                    string scriptUrl = node.Attributes["src"].Value;

                    logger.Trace("Downloading {0}", scriptUrl);
                    logger.Trace("      {0}", fileName);

                    if (DownloadFile(scriptUrl, fileName))
                    {

                        logger.Trace("File downloaded.");
                        node.Attributes["src"].Value = String.Format("static/", Path.GetFileName(fileName));

                    }

                }
                catch (Exception e)
                {

                    logger.ErrorException("Could not download static content: ", e);

                }

            }

            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//link[@rel='stylesheet' and @href]"))
            {

                logger.Trace("Node outer HTML: {0}", node.OuterHtml);

                try
                {

                    string fileName = Path.Combine(resourceDir, String.Format("{0}.css", Guid.NewGuid().ToString()));
                    string scriptUrl = node.Attributes["href"].Value;

                    logger.Trace("Downloading {0}", scriptUrl);
                    logger.Trace("      {0}", fileName);

                    if (DownloadFile(scriptUrl, fileName))
                    {

                        logger.Trace("File downloaded.");
                        node.Attributes["href"].Value = String.Format("static/{0}", Path.GetFileName(fileName));

                    }

                }
                catch (Exception e)
                {

                    logger.ErrorException("Could not download static content: ", e);

                }

            }







            return true;

        }

        public static bool CleanOutputDirectory(string outputDirectory)
        {

            logger.Info("Start cleaning directory {0}", outputDirectory);

            logger.Debug("Deleting {0}", outputDirectory);

            try
            {

                Directory.Delete(outputDirectory, true);

            }
            catch (Exception e)
            {

                logger.ErrorException(String.Format("An error occured deleting directory: {0}", outputDirectory), e);
                return false;

            }

            logger.Debug("Creating directory structure.");

            try
            {

                Directory.CreateDirectory(Path.Combine(outputDirectory, "static"));
                Directory.CreateDirectory(Path.Combine(outputDirectory, "resources"));

            }
            catch (Exception e)
            {

                logger.ErrorException("Could not create directory structure:", e);
                return false;

            }

            return true;

        }


        public static HtmlDocument GetPage(string url)
        {

            string html;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
            request.Timeout = 10000;

            request.CookieContainer = cookies;

            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            using (StreamReader streamReader = new StreamReader(responseStream))
            {

                html = streamReader.ReadToEnd();

            }

            response.Close();

            HtmlDocument document = new HtmlDocument();

            document.LoadHtml(html);

            return document;

        }

        public static bool DownloadFile(string url, string outputPath)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
            request.Timeout = 10000;

            request.CookieContainer = cookies;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            using (StreamReader streamReader = new StreamReader(responseStream))
            using (StreamWriter streamWriter = new StreamWriter(outputPath))
            {

                streamWriter.Write(streamReader.ReadToEnd());

            }

            response.Close();

            return true;

        }

    }
}
