using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tsubasa.HtmlORM {

    /// <summary>
    /// EntityでHttpのやり取りをするHttpClient
    /// 
    /// by tsubasa
    /// </summary>
    public class EntityHttpClient : HttpClient {

        /// <summary>文字コード</summary>
        public Encoding Charset { get; set; } = Encoding.UTF8;

        /// <summary>
        /// HttpClientを初期化する
        /// </summary>
        public EntityHttpClient(string baseAddress)
                : base(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip }) {
            DefaultRequestHeaders.Add("ContentType",
                "application/x-www-form-urlencoded");
            DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; Touch; rv:11.0) like Gecko");
            BaseAddress = new Uri(baseAddress);
        }

        /// <summary>
        /// getのRequest送信
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<TEntity> HttpGetAsync<TEntity>(string url, object formData = null)
                where TEntity : class, new() {
            // パラメータのある場合を作成
            if(formData != null) {
                url = FormParser.ToUrl(url, formData);
            }
            // 送信
            var response = await GetAsync(url);
            response.Content.Headers.ContentType.CharSet = this.Charset.WebName;
            // Htmlを取得
            string html = await response.Content.ReadAsStringAsync();
            // Htmlを解析
            TEntity entity = HtmlParser.ToEntity<TEntity>(html);
            // 情報を返す
            return entity;
        }

        /// <summary>
        /// バイナリデータを取得
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<byte[]> GetBytesAsync(string url) {
            var response = await GetAsync(url);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return bytes;
        }

        /// <summary>
        /// postのRequest送信
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="formData">フォームデータ</param>
        /// <returns></returns>
        public async Task<TEntity> HttpPostAsync<TEntity>(string url, object formData)
                where TEntity : class, new() {
            // フォームを作成
            var form = FormParser.CreatePostForm(formData);
            // form送信
            var response = await PostAsync(url, form);
            response.Content.Headers.ContentType.CharSet = this.Charset.WebName;
            // Htmlを取得
            string html = await response.Content.ReadAsStringAsync();
            // Htmlを解析
            TEntity entity = HtmlParser.ToEntity<TEntity>(html);
            // 情報を返す
            return entity;
        }
    }
}
