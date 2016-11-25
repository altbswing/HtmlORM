using System;
using HtmlAgilityPack;
using Tsubasa.HtmlORM.Properties;

namespace Tsubasa.HtmlORM {

    /// <summary>
    /// 該当ノード見つからなかった時のエラー
    /// </summary>
    public class NotFindNodeException : Exception {

        /// <summary>
        /// 親ノード
        /// </summary>
        public HtmlNode ParentNode { get; private set; }

        /// <summary>
        /// XPath
        /// </summary>
        public string XPath { get; private set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public override string Message {
            get {
                var msg = string.Format(Resources.E001, XPath);
                msg = string.Format("{0}\r\n{1}", msg, ParentNode);
                return string.Format(Resources.E001, XPath);
            }
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public NotFindNodeException(HtmlNode parentNode, string xPath) {
            ParentNode = parentNode;
            XPath = xPath;
        }
    }
}