using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsubasa.HtmlORM {

    /// <summary>
    /// フォームの値を示す
    /// 
    /// by tsubasa
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class FormValueAttribute : Attribute {

        /// <summary>フォームのName属性</summary>
        public string Name { get; set; }
    }

}
