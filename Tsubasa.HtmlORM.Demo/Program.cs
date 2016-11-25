using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsubasa.HtmlORM.Demo {

    public class Program {

        private const string _URL =
            "https://share.dmhy.org/topics/list/page/3?keyword=%E7%81%AB%E5%BD%B1%E5%BF%8D%E8%80%85%E7%96%BE%E9%A3%8E%E4%BC%A0+%E7%AE%80%E4%BD%93+1024x576+&sort_id=0&team_id=370&order=date-asc";

        public static void Main(string[] args) {
            try {
                var task = _MainAsync();
                task.Wait();
            } catch(Exception ex) {
                Console.WriteLine(ex);
            }
            Console.WriteLine("Hello");
            Console.Read();
        }

        /// <summary>
        /// 非同期処理
        /// </summary>
        /// <returns></returns>
        private async static Task _MainAsync() {
            using(var dmhy = new EntityHttpClient("http://share.dmhy.org")) {
                var pageEntity = await dmhy.HttpGetAsync<PageEntity>(_URL);
                foreach(var tr in pageEntity.TrList) {
                    var name = tr.Name.Replace("\"", "").Trim();
                    var url = string.Format("{0}\r\n{1}\r\n", tr.Name, tr.Link.Substring(0, tr.Link.IndexOf("&")));
                    Console.WriteLine(url);
                }
            }
        }
    }

    public class PageEntity {

        [NodeList(XPath = "//*[@id=\"topic_list\"]/tbody/tr")]
        [NodeObject]
        public List<SeedInfo> TrList { get; set; }
    }

    public class SeedInfo {

        [NodeInner(XPath = "td[3]/a")]
        public string Name { get; set; }

        [NodeValue(XPath = "td[4]/a", Attribute = "href")]
        public string Link { get; set; }
    }
}
