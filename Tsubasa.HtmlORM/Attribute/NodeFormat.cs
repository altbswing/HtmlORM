using System;

namespace Tsubasa.HtmlORM {

    /// <summary>
    /// フォーマットを設定する
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class NodeFormatAttribute : Attribute {

        /// <summary>書式</summary>
        public string Format { get; set; }

    }
}
