using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsubasa.HtmlORM {

    /// <summary>要素群のXPathを設定する</summary>
    public class NodeListAttribute : HtmlNodeAttribute {

        public int Skip { get; set; }
    }
}
