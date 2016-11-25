using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsubasa.HtmlORM {

    /// <summary>要素のXPathを設定する</summary>
    public abstract class SingleNodeAttribute : HtmlNodeAttribute {

        /// <summary>表示順</summary>
        public int Sequence { get; set; }

        /// <summary>既定値</summary>
        public string Default { get; set; }
    }

}
