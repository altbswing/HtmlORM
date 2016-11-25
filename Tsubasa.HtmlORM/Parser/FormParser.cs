using System;
using System.Linq;
using System.Reflection;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Net.Http;

namespace Tsubasa.HtmlORM {

    /// <summary>
    /// Entityをhttpフォームに解析
    /// 
    /// by tsubasa
    /// </summary>
    public static class FormParser {

        /// <summary>
        /// オブジェクトによってフォーム情報を作成
        /// </summary>
        /// <param name="formData">フォーム情報を格納するオブジェクト</param>
        /// <returns>httpget文字列</returns>
        public static string ToUrl(string url, object entity) {
            // マッピング作成
            var map = ToKeyValue(entity);
            var list = map.Select(p => string.Format("{0}={1}", p.Key, p.Value));
            var paramters = string.Join("&", list);
            var fullUrl = string.Join("{0}?{1}", (url ?? "").TrimEnd('/'), paramters);
            // 作成したフォーム情報を返す
            return fullUrl;
        }

        /// <summary>
        /// オブジェクトによってフォーム情報を作成
        /// </summary>
        /// <param name="formData">フォーム情報を格納するオブジェクト</param>
        /// <returns>webclientに対応するNameValueCollection</returns>
        public static FormUrlEncodedContent CreatePostForm(object entity) {
            // パラメータリスト
            var map = ToKeyValue(entity);
            // Formを作成
            var content = new FormUrlEncodedContent(map);
            // 作成したフォーム情報を返す
            return content;
        }

        /// <summary>
        /// オブジェクトをフォームに解析
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> ToKeyValue(object entity) {
            // 該当オブジェクトの型
            var type = entity.GetType();
            // フォームの「name => value」のペアを作成
            var query = from p in entity.GetType().GetProperties()
                        let attr = p.GetCustomAttribute<FormValueAttribute>()
                        let name = attr == null ? p.Name : attr.Name
                        select new {
                            Name = name,
                            Value = (p.GetValue(entity) ?? "").ToString()
                        };
            // マッピングを作成
            var map = query.ToDictionary(p => p.Name, p => p.Value);
            return map;
        }
    }
}