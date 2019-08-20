using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace SiteMapUsingCrawler.Controllers
{
    public class SiteMapUsingCrawlerController : Controller
    {      
        public XmlSitemapResult CrawlSiteGeneratingSitemapOnFly()
        {
        //if i want to crawl the site
        //string sitePrimaryUrl = System.Web.HttpContext.Current.Request.Url.OriginalString;
        
            string sitePrimaryUrl = "http://myplastikkarten.edigitstore.it/";
            //string sitePrimaryUrl = "https://www.myplastikkarten.de";
            //for main site without any action or controller below section will be skipped
            if (System.Web.HttpContext.Current.Request.Url.PathAndQuery != "/")
                sitePrimaryUrl = sitePrimaryUrl.Replace(System.Web.HttpContext.Current.Request.Url.PathAndQuery, "");

            UriBuilder uri = new UriBuilder("http://myplastikkarten.edigitstore.it/");

            //UriBuilder uri = new UriBuilder("https://www.myplastikkarten.de");
            Crawl c1 = new Crawl();
            c1.PrimaryUrl = sitePrimaryUrl;
            c1.PrimaryHost = uri.Host;
            c1.GetUrlsOfSite(sitePrimaryUrl);

            List<LocationUrls_Result> lstSItemapResult = new List<LocationUrls_Result>();
            foreach (string singleUrl in Crawl.Urls)
            {
                lstSItemapResult.Add(new LocationUrls_Result() { Url = singleUrl, Changefreq = "weekly" });
            }
            //escludere ai e psd

            return new XmlSitemapResult(lstSItemapResult);
        }

        public string CrawlSite()
        {
            //if i want to crawl the site
            //string sitePrimaryUrl = System.Web.HttpContext.Current.Request.Url.OriginalString;

            string sitePrimaryUrl = "http://myplastikkarten.edigitstore.it/";
            //string sitePrimaryUrl = "https://www.myplastikkarten.de";
            //for main site without any action or controller below section will be skipped
            if (System.Web.HttpContext.Current.Request.Url.PathAndQuery != "/")
                sitePrimaryUrl = sitePrimaryUrl.Replace(System.Web.HttpContext.Current.Request.Url.PathAndQuery, "");

            UriBuilder uri = new UriBuilder("http://myplastikkarten.edigitstore.it/");

            //UriBuilder uri = new UriBuilder("https://www.myplastikkarten.de");
            Crawl c1 = new Crawl();
            c1.PrimaryUrl = sitePrimaryUrl;
            c1.PrimaryHost = uri.Host;
            c1.GetUrlsOfSite(sitePrimaryUrl);

            List<LocationUrls_Result> lstSItemapResult = new List<LocationUrls_Result>();
            foreach (string singleUrl in Crawl.Urls)
            {
                //qui scartiamo gli url che non sono html o aspx
                Regex rx = new Regex(@"^.*\.(?!html$|aspx$)[^.]+$");
                MatchCollection matches = rx.Matches(singleUrl);
                if (matches.Count == 0)
                {
                    lstSItemapResult.Add(new LocationUrls_Result() { Url = singleUrl, Changefreq = "weekly" });
                }
                   
            }
            //escludere ai e psd
            return GetSitemap(lstSItemapResult);
            //return new XmlSitemapResult(lstSItemapResult);
        }

      
        private string GetSitemap(IEnumerable<LocationUrls_Result> items)
        {

            XNamespace xsi = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
            XNamespace schemaLocation = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

            XDocument sitemap = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                 new XElement("urlset",
                      new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                      new XAttribute(xsi + "schemaLocation", schemaLocation),
                      from item in items
                      select CreateItemElement(item)
                      )
                 );

            return sitemap.Declaration + sitemap.ToString();

        }
        private XElement CreateItemElement(LocationUrls_Result item)
        {
            XElement itemElement = new XElement("url", new XElement("loc", item.Url.ToLower()));

            //if (item.LastModified.HasValue)
            //    itemElement.Add(new XElement("lastmod", item.LastModified.Value.ToString("yyyy-MM-dd")));

            if (item.Changefreq != null)
                itemElement.Add(new XElement("changefreq", item.Changefreq.ToString().ToLower()));

            if (item.Priority.HasValue)
                itemElement.Add(new XElement("priority", item.Priority.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            return itemElement;
        }
    }


    public class XmlSitemapResult : ActionResult
    {
        private IEnumerable<LocationUrls_Result> _items;

        public XmlSitemapResult(IEnumerable<LocationUrls_Result> items)
        {
            _items = items;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            string encoding = context.HttpContext.Response.ContentEncoding.WebName;
            XNamespace xsi = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
            XNamespace schemaLocation = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

            XDocument sitemap = new XDocument(new XDeclaration("1.0", encoding, "yes"),
                 new XElement("urlset",
                      new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                      new XAttribute(xsi + "schemaLocation", schemaLocation),
                      from item in _items
                      select CreateItemElement(item)
                      )
                 );

            context.HttpContext.Response.ContentType = "sitemap.xml";
            context.HttpContext.Response.Flush();
            context.HttpContext.Response.Write(sitemap.Declaration + sitemap.ToString());
        }

        private XElement CreateItemElement(LocationUrls_Result item)
        {
            XElement itemElement = new XElement("url", new XElement("loc", item.Url.ToLower()));

            //if (item.LastModified.HasValue)
            //    itemElement.Add(new XElement("lastmod", item.LastModified.Value.ToString("yyyy-MM-dd")));

            if (item.Changefreq != null)
                itemElement.Add(new XElement("changefreq", item.Changefreq.ToString().ToLower()));

            if (item.Priority.HasValue)
                itemElement.Add(new XElement("priority", item.Priority.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            return itemElement;
        }
    }

    public class Crawl
    {
        public static List<string> Urls = null;

        public string PrimaryUrl { get; set; }
        public string PrimaryHost { get; set; }
        string CurrentUrl { get; set; }

        public Crawl()
        {
            Urls = new List<string>();
            CurrentUrl = System.Web.HttpContext.Current.Request.Url.OriginalString;
        }

        public void GetUrlsOfSite(string url)
        {
            //if compilation debug="true" in web.config then bypass ssl or else use SSL for the site and skip this section
            if (HttpContext.Current.IsDebuggingEnabled)
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            WebRequest webRequest = WebRequest.Create(url);
            if (webRequest != null)
            {
                WebResponse webResponse = webRequest.GetResponse();
                Stream data = webResponse.GetResponseStream();
                string html = String.Empty;
                using (StreamReader sr = new StreamReader(data))
                {
                    html = sr.ReadToEnd();
                    //DumpHRefs(html, PrimaryUrl);

                    Match m;
                    string HRefPattern = "a href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";

                    try
                    {
                        m = Regex.Match(html, HRefPattern, RegexOptions.IgnoreCase);
                        while (m.Success)
                        {
                            string urlValue = m.Groups[1].Value;
                            if (urlValue.Trim().Length > 0 && !urlValue.Trim().StartsWith("#"))
                            {
                                string tempStr = urlValue;

                                if (tempStr.StartsWith(".."))
                                    tempStr = tempStr.Replace("..", "").Trim();

                                if (!urlValue.Contains("http://") && !urlValue.Contains("https://"))
                                    tempStr = PrimaryUrl + tempStr;

                                //for current page do not allow
                                if (tempStr != CurrentUrl)
                                {
                                    UriBuilder uri = new UriBuilder(tempStr);
                                    if (uri.Host == PrimaryHost)
                                    {
                                        if (!Urls.Contains(uri.Uri.ToString()))
                                        {
                                            Urls.Add(uri.Uri.ToString());
                                            GetUrlsOfSite(uri.Uri.ToString());
                                        }
                                    }
                                }
                            }
                            m = m.NextMatch();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

    public class LocationUrls_Result
    {
        public string Url { get; set; }
        public string Changefreq { get; set; }
        public Nullable<double> Priority { get; set; }
        public Nullable<int> OrderBy { get; set; }
    }
}
