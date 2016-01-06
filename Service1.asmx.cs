using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.IO;
using System.Web.SessionState;
using System.Drawing;
using System.Text;
using HtmlAgilityPack;

namespace WebService1
{
    /// <summary>
    /// Service1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService,IRequiresSessionState
    {

        HttpWebRequest webrequest;
        HttpWebResponse webresponse;
        string mainUrl = "http://jwxt.hubu.edu.cn/";
        string loginUrl = "http://jwxt.hubu.edu.cn/Logon.do?method=logon";
        string login2Url = "http://jwxt.hubu.edu.cn/Logon.do?method=logonBySSO";
        string cjcxUrl = "http://jwxt.hubu.edu.cn/xszqcjglAction.do?method=queryxscj	";
        string verifyUrl = "http://jwxt.hubu.edu.cn/verifycode.servlet";

        Cookie cookie;
        StreamReader reader;

        [WebMethod(EnableSession = true)]
        public void GetSessionId()
        {
            webrequest = (HttpWebRequest)WebRequest.Create(mainUrl);
            webrequest.Method = "GET";
            webrequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)";

            webresponse = (HttpWebResponse)webrequest.GetResponse();
            //Response.Write(webresponse.Headers["Set-Cookie"]);
            if (!string.IsNullOrEmpty(webresponse.Headers["Set-Cookie"]))
            {
                cookie = new Cookie("JSESSIONID", this.GetSessionId(webresponse.Headers["Set-Cookie"]));
            }
            if (HttpContext.Current.Session["cookie"] == null)
                HttpContext.Current.Session.Add("cookie", cookie);
            else
                HttpContext.Current.Session["cookie"] = cookie;

            string fun = HttpContext.Current.Request["jsoncallback"];
            HttpContext.Current.Response.Write(string.Format("{0}({1})", fun, "{status:true}"));
        }

        private string GetSessionId(string value)
        {
            string startTag = "JSESSIONID=";
            int indexEnd = value.IndexOf(";");
            return value.Substring(startTag.Length, indexEnd - startTag.Length);
        }

        [WebMethod(EnableSession = true)]
        public void GetImage()
        {
            webrequest = (HttpWebRequest)WebRequest.Create(verifyUrl);
            webrequest.Method = "GET";
            cookie = (Cookie)HttpContext.Current.Session["cookie"];
            webrequest.Headers["Cookie"] = cookie.Name + "=" + cookie.Value;

            webresponse = (HttpWebResponse)webrequest.GetResponse();


            System.Drawing.Image img;
            img = new Bitmap(webresponse.GetResponseStream());
            HttpContext.Current.Response.ContentType = "image/jpeg";
            HttpContext.Current.Response.Clear();
            img.Save(HttpContext.Current.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

        [WebMethod(EnableSession = true)]
        public void DoLogin(string username, string password, string validate)
        {
            webrequest = (HttpWebRequest)WebRequest.Create(loginUrl);
            //webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.Method = "POST";
            webrequest.AllowAutoRedirect = false;
            webrequest.Referer = "http://jwxt.hubu.edu.cn";
            webrequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.KeepAlive = true;
            Cookie cookie = (Cookie)Session["cookie"];
            webrequest.Headers["Cookie"] = cookie.Name + "=" + cookie.Value;

            //webrequest.Headers.Add();     
            string postdata = "USERNAME=" + username + "&PASSWORD=" + password + "&useDogCode=&useDogCode=&RANDOMCODE=" + validate + "&x=20&y=15";
            Byte[] postByte = Encoding.ASCII.GetBytes(postdata);

            webrequest.ContentLength = postByte.Length;

            Stream stream = webrequest.GetRequestStream();
            stream.Write(postByte, 0, postByte.Length);
            stream.Close();
            webresponse = (HttpWebResponse)webrequest.GetResponse();


            if (webresponse.StatusCode == HttpStatusCode.Redirect)
            {
                webrequest = (HttpWebRequest)WebRequest.Create(login2Url);
                webrequest.Method = "POST";
                webrequest.Referer = "http://jwxt.hubu.edu.cn/framework/main.jsp";
                webrequest.KeepAlive = true;
                webrequest.ContentLength = 0;
                webrequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)";
                webrequest.Headers["Cookie"] = cookie.Name + "=" + cookie.Value;

                webresponse = (HttpWebResponse)webrequest.GetResponse();

                string fun = HttpContext.Current.Request["jsoncallback"];
                HttpContext.Current.Response.Write(string.Format("{0}({1})", fun, "{status:true}"));
            }
        }

        [WebMethod(EnableSession = true)]
        public void GetScore()
        {
            webrequest = (HttpWebRequest)WebRequest.Create(cjcxUrl);
            webrequest.Method = "POST";
            webrequest.AllowAutoRedirect = false;
            webrequest.Referer = " http://jwxt.hubu.edu.cn/jiaowu/cjgl/xszq/query_xscj.jsp?tktime=1433425587000";
            webrequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.KeepAlive = true;
            string postCjcx = "kksj=&kcxz=&kcmc=&xsfs=qbcj&ok=";
            Byte[] post = Encoding.ASCII.GetBytes(postCjcx);
            webrequest.ContentLength = post.Length;

            cookie = (Cookie)Session["cookie"];
            webrequest.Headers["Cookie"] = cookie.Name + "=" + cookie.Value;

            Stream stream = webrequest.GetRequestStream();
            stream.Write(post, 0, post.Length);

            webresponse = (HttpWebResponse)webrequest.GetResponse();

            reader = new StreamReader(webresponse.GetResponseStream());
            string content = reader.ReadToEnd();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            HtmlNode node = doc.GetElementbyId("mxh");

            StringBuilder sbuild = new StringBuilder();
            sbuild.Append("<table border=\"1\" cellpadding=\"3\" cellspacing=\"2\" style=\"width:100%;border:1px solid green;\">");

            foreach (HtmlNode n in node.ChildNodes)
            {
                sbuild.Append("<tr>");
                // sbuild.Append("<td>" + n.InnerText + "</td>");
                HtmlNodeCollection coll = n.SelectNodes("td");
                foreach (HtmlNode hn in coll)
                    sbuild.Append("<td>" + hn.InnerText + "</td>");
                // sbuild.Append("<td>" + n.ChildNodes[5].InnerText + "</td>");
                //sbuild.Append("<td>" + n.ChildNodes[6].InnerText + "</td>");
                sbuild.Append("</tr>");
            }
            sbuild.Append("</table>");

            string fun = HttpContext.Current.Request["jsoncallback"];
            HttpContext.Current.Response.Write(string.Format("{0}({1})", fun, "{status:"+sbuild.ToString()+"}"));
        }

        private string getUTF8(string p)
        {
            byte[] b = Encoding.ASCII.GetBytes(p);
            return Encoding.UTF8.GetString(b);
        }
    }
}