using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsubasa.HtmlORM {

    /// <summary>ノード共通のXPathプロパティを定義する属性</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public abstract class HtmlNodeAttribute : Attribute {

        private string _xPath = ".";

        /// <summary>該当要素のXPath</summary>
        public string XPath {
            get { return _xPath; }
            set { _xPath = value; }
        }
    }
}
