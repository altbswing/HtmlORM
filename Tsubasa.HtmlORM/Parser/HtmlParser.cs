using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;

namespace Tsubasa.HtmlORM {

    /// <summary>
    /// Html文字列を解析し、指定したオブジェクトを作成するクラス
    /// </summary>
    public static class HtmlParser {

        /// <summary>
        /// 初期化
        /// 
        /// HtmlAgilityPackでは
        /// 勝手に form タグをドキュメントから外す処理があります。
        /// 故に、初期化するときに、formを保留する方がいいと思います。
        /// </summary>
        static HtmlParser() {
            SafeTag("form", "tbody", "option");
        }

        /// <summary>
        /// 保留するタグを設定する
        /// </summary>
        /// <param name="safeTags">
        /// 保留するタグ
        /// </param>
        public static void SafeTag(params string[] safeTags) {
            // HtmlAgilityPackの排除対象から外す
            safeTags.ToList().ForEach(t => HtmlNode.ElementsFlags.Remove(t));
        }

        /// <summary>
        /// htmlを解析して、指定したオブジェクトを返す
        /// </summary>
        /// <typeparam name="T">オブジェクトの型</typeparam>
        /// <param name="html">html文字列</param>
        /// <returns>指定した型のオブジェクト</returns>
        public static TEntity ToEntity<TEntity>(string html)
                where TEntity : class, new() {
            // 新規オブジェクト作成
            HtmlDocument _htmlDoc = new HtmlDocument();
            // xmlモード
            _htmlDoc.OptionOutputAsXml = true;
            // htmlを読み込む
            _htmlDoc.LoadHtml(html);
            // ルート要素を取得
            var root = _htmlDoc.DocumentNode;
            // 該当実体を作成
            TEntity entity = _ParseObject(root, typeof(TEntity)) as TEntity;
            // 該当実体を戻す
            return entity;
        }

        /// <summary>
        /// 再帰関数、型の各プロパティの属性設定によって
        /// htmlを詳細的に解析する
        /// </summary>
        /// <param name="htmlNode">htmlオブジェクト</param>
        /// <param name="entityType">作成するオブジェクトの型</typeparam>
        /// <returns>作成したオブジェクト</returns>
        private static object _ParseObject(HtmlNode htmlNode, Type entityType) {
            // 戻る用のオブジェクトを作成
            var entity = Activator.CreateInstance(entityType);
            // 該当型にて、解析対象となるプロパティを反復する
            var propList = entityType.GetProperties()
                    .Where(p => p._IsDefine<HtmlNodeAttribute>());
            foreach (var prop in propList) {
                // リスト判断
                if (prop._IsDefine<NodeListAttribute>()) {
                    // リストを作成
                    var list = _ParseList(htmlNode, prop);
                    // リストを設定
                    prop.SetValue(entity, list, null);
                } else {
                    // オブジェクトを作成
                    var obj = _ParseValue(htmlNode, prop.PropertyType, prop);
                    // オブジェクトを設定
                    prop.SetValue(entity, obj, null);
                }
            }
            return entity;
        }

        /// <summary>
        /// リスト型を解析
        /// </summary>
        /// <param name="propType">該当プロパティの型</param>
        /// <param name="htmlNode">XPathによって取得済みのノード</param>
        /// <param name="xAttr">該当属性を取得</param>
        /// <returns>リスト型のオブジェクト</returns>
        private static object _ParseList(HtmlNode htmlNode, PropertyInfo listProp) {
            // 該当XPathを取得
            string xPath = listProp.GetCustomAttribute<NodeListAttribute>().XPath;
            // 該当ノードを取得
            var nodes = htmlNode.SelectNodes(xPath);
            if (nodes == null) {
                throw new NotFindNodeException(htmlNode, xPath);
            }
            // リストの汎用型を取得
            var genericType = listProp.PropertyType.GetGenericArguments()[0];
            // リストを作成
            var list = Activator.CreateInstance(listProp.PropertyType);
            // Addメッソドを取得
            var addMethod = listProp.PropertyType.GetMethod("Add", new Type[] { genericType });
            // 該当属性を取得
            var listAttr = listProp.GetCustomAttribute<NodeListAttribute>();
            // すべてのノードを処理する
            for (int i = listAttr.Skip; i < nodes.Count; i++) {
                // 要素属性を取得
                var emlAttr = listProp.GetCustomAttribute<SingleNodeAttribute>();
                // 要素を作成
                var listElm = _ParseValue(nodes[i], genericType, listProp);
                // 要素を追加
                addMethod.Invoke(list, new object[] { listElm });
            }
            return list;
        }

        /// <summary>
        /// 該当プロパティの型を判断し、
        /// リスト、オブジェクト、値の属性設定によって別処理を行う
        /// </summary>
        /// <param name="htmlNode">ノード</param>
        /// <param name="type">リストの場合はジェネリック型を渡す</param>
        /// <param name="prop">リストの場合はリストのプロパティを渡す</param>
        /// <returns>作成したオブジェクト</returns>
        private static object _ParseValue(HtmlNode htmlNode, Type type, PropertyInfo prop) {
            // オブジェクトの場合、
            if (prop._IsDefine<NodeObjectAttribute>()) {
                // 該当xPathを取得
                string xPath = prop.GetCustomAttribute<NodeObjectAttribute>().XPath;
                // XPathによって該当要素を取得
                var subNode = htmlNode.SelectSingleNode(xPath);
                // TODO ヌルで設定するか、例外をスローか、要検討
                if (subNode == null) {
                    throw new NotFindNodeException(htmlNode, xPath);
                }
                // オブジェクトの場合、現ノードを持って、_parseObjectを再帰的に呼び出す
                return _ParseObject(subNode, type);
            }
            // 該当型のコンバータを取得
            var converter = TypeDescriptor.GetConverter(type);
            // コンバート文字列を作成
            string convStr = _CreateConvertString(htmlNode, prop);
            // コンバート文字列によってオブジェクトを作成
            return converter.ConvertFromString(convStr);
        }

        /// <summary>
        /// プロパティの属性定義によって
        /// </summary>
        /// <param name="node"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        private static string _CreateConvertString(HtmlNode node, PropertyInfo prop) {
            // すべてのシングル設定を取得、Sequenceによってソート
            var xAttrList = prop.GetCustomAttributes(typeof(SingleNodeAttribute), false)
                                .Cast<SingleNodeAttribute>()
                                .OrderBy(attr => attr.Sequence)
                                .ToList();
            // シングル設定によってhtmlから抽出した文字列値を格納するリスト
            var singleList = new List<string>();
            // シングル設定毎処理する
            foreach (var attr in xAttrList) {
                // 該当ノードを取得
                var subNode = node.SelectSingleNode(attr.XPath);
                // nullの場合デフォルトを追加
                if (subNode == null) {
                    singleList.Add(attr.Default);
                    continue;
                }
                // 値取得用
                var htmlStr = attr.Default;
                // html取得形式判断
                if (attr is NodeInnerAttribute) {
                    // 内部htmlの場合
                    htmlStr = subNode.InnerText;
                    // htmlのエスケープ文字を置換
                    htmlStr = htmlStr.Replace("&nbsp;", " ").Trim();
                } else if (attr is NodeValueAttribute) {
                    // 要素の属性の場合
                    var name = (attr as NodeValueAttribute).Attribute;
                    htmlStr = subNode.Attributes[name].Value.Trim();
                }
                // nullの場合、既定値を使用
                htmlStr = string.IsNullOrEmpty(htmlStr) ? attr.Default : htmlStr;
                // 取得した文字列を追加
                singleList.Add(htmlStr);
            }
            // フォーマットが定義している場合
            if (prop._IsDefine<NodeFormatAttribute>()) {
                // フォーマットによって文字列を作成
                string fmt = prop.GetCustomAttribute<NodeFormatAttribute>().Format;
                return string.Format(fmt, singleList.ToArray());
            }
            // 文字列直接つながる
            return string.Join("", singleList);
        }

        /// <summary>
        /// 属性が定義しているか
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="menber"></param>
        /// <returns></returns>
        private static bool _IsDefine<TAttribute>(this MemberInfo menber) where TAttribute : Attribute {
            var count = menber.GetCustomAttributes<TAttribute>().Count();
            return count > 0;
        }
    }
}