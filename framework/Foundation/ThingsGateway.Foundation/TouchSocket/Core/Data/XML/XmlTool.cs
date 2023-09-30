#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Xml;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// xml主类
    /// </summary>
    public class XmlTool
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path">文件路径，包含文件名</param>
        public XmlTool(string path)
        {
            this.path = path;
        }

        private readonly string path = null;

        #region 存储

        /// <summary>
        /// 单节点，单属性储存
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <param name="Attribute_name">属性名</param>
        /// <param name="Attribute_value">属性值</param>
        public void AttributeStorage(string NodeName, string Attribute_name, string Attribute_value)
        {
            if (File.Exists(this.path))
            {//存在Xml的文件
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                var N = false;//节点判断变量
                foreach (XmlNode item in nodeList)
                {//判断是否存在该节点
                    if (item.Name == NodeName)
                    {
                        N = true;
                        break;
                    }
                }
                if (N == false)
                {//不存在节点，属性，建立节点，属性
                    var PointName = xml.CreateElement(NodeName);
                    PointName.SetAttribute(Attribute_name, Attribute_value);
                    root.AppendChild(PointName);
                }
                else
                {//存在属性进行赋值
                    var PointName = xml.SelectSingleNode("Root/" + NodeName);
                    PointName.Attributes[Attribute_name].Value = Attribute_value;
                }
                xml.Save(this.path);
            }
            else
            {
                var xml = new XmlDocument();
                var dec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = xml.CreateElement("Root");
                xml.AppendChild(root);//根元素

                var PointName = xml.CreateElement(NodeName);
                PointName.SetAttribute(Attribute_name, Attribute_value);
                root.AppendChild(PointName);
                xml.Save(this.path);
            }
        }

        /// <summary>
        /// 单节点，多属性存储
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <param name="Attribute_name">属性集合</param>
        /// <param name="Attribute_value">属性值集合</param>
        public void AttributeStorage(string NodeName, string[] Attribute_name, string[] Attribute_value)
        {
            if (Attribute_name.Length != Attribute_value.Length)
            {
                Console.WriteLine("属性名数量和属性值数量不一致，无法储存");
                return;
            }
            if (File.Exists(this.path))
            {//存在Xml的文件
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                var N = false;//节点变量
                foreach (XmlNode item in nodeList)
                {//判断是否存在该节点
                    if (item.Name == NodeName)
                    {
                        N = true;
                        break;
                    }
                }
                if (N == false)
                {//不存在节点，属性，建立节点，属性
                    var PointName = xml.CreateElement(NodeName);
                    for (var i = 0; i < Attribute_name.Length; i++)
                    {
                        PointName.SetAttribute(Attribute_name[i], Attribute_value[i]);
                        root.AppendChild(PointName);
                    }
                }
                else
                {//存在属性进行赋值
                    var PointName = xml.SelectSingleNode("Root/" + NodeName);
                    for (var i = 0; i < Attribute_name.Length; i++)
                    {
                        PointName.Attributes[Attribute_name[i]].Value = Attribute_value[i];
                    }
                }
                xml.Save(this.path);
            }
            else
            {
                var xml = new XmlDocument();
                var dec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = xml.CreateElement("Root");
                xml.AppendChild(root);//根元素

                var PointName = xml.CreateElement(NodeName);
                for (var i = 0; i < Attribute_name.Length; i++)
                {
                    PointName.SetAttribute(Attribute_name[i], Attribute_value[i]);
                    root.AppendChild(PointName);
                }
                xml.Save(this.path);
            }
        }

        /// <summary>
        /// 单节点，单属性多集合存储
        /// </summary>
        /// <param name="NodeName">节点集合</param>
        /// <param name="Attribute_name">属性名集合</param>
        /// <param name="Attribute_value">属性值集合</param>
        public void AttributeStorage(string[] NodeName, string[] Attribute_name, string[] Attribute_value)
        {
            if ((Attribute_name.Length != Attribute_value.Length) && NodeName.Length != Attribute_name.Length)
            {
                Console.WriteLine("属性名数量和属性值数量不一致，无法储存");
                return;
            }
            if (File.Exists(this.path))
            {//存在Xml的文件
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                for (var i = 0; i < NodeName.Length; i++)
                {
                    var N = false;//节点变量
                    foreach (XmlNode item in nodeList)
                    {//判断是否存在该节点
                        if (item.Name == NodeName[i])
                        {
                            N = true;
                            break;
                        }
                    }
                    if (N == false)
                    {//不存在节点，属性，建立节点，属性
                        var PointName = xml.CreateElement(NodeName[i]);

                        PointName.SetAttribute(Attribute_name[i], Attribute_value[i]);
                        root.AppendChild(PointName);
                    }
                    else
                    {//存在属性进行赋值
                        var PointName = xml.SelectSingleNode("Root/" + NodeName);

                        PointName.Attributes[Attribute_name[i]].Value = Attribute_value[i];
                    }
                    xml.Save(this.path);
                }
            }
            else
            {
                var xml = new XmlDocument();
                var dec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = xml.CreateElement("Root");
                xml.AppendChild(root);//根元素
                for (var i = 0; i < NodeName.Length; i++)
                {
                    var PointName = xml.CreateElement(NodeName[i]);
                    PointName.SetAttribute(Attribute_name[i], Attribute_value[i]);
                    root.AppendChild(PointName);

                    xml.Save(this.path);
                }
            }
        }

        /// <summary>
        /// 多节点，多属性，多集合存储
        /// </summary>
        /// <param name="NodeName">节点集合</param>
        /// <param name="Attribute_name">属性集合</param>
        /// <param name="AttributeNumber">每个节点的属性数量</param>
        /// <param name="Attribute_value">属性值集合</param>
        public void AttributeStorage(string[] NodeName, string[] Attribute_name, int AttributeNumber, params string[][] Attribute_value)
        {
            if (File.Exists(this.path))
            {
                //存在Xml的文件
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                for (var i = 0; i < NodeName.Length; i++)
                {
                    var N = false;//节点变量
                    foreach (XmlNode item in nodeList)
                    {//判断是否存在该节点
                        if (item.Name == NodeName[i])
                        {
                            N = true;
                            break;
                        }
                    }
                    if (N == false)
                    {//不存在节点，属性，建立节点，属性
                        var PointName = xml.CreateElement(NodeName[i]);
                        for (var j = 0; j < AttributeNumber; j++)
                        {
                            PointName.SetAttribute(Attribute_name[j], Attribute_value[j][i]);
                        }

                        root.AppendChild(PointName);
                    }
                    else
                    {//存在属性进行赋值
                        var PointName = xml.SelectSingleNode("Root/" + NodeName[i]);

                        for (var j = 0; j < AttributeNumber; j++)
                        {
                            PointName.Attributes[Attribute_name[j]].Value = Attribute_value[j][i];
                        }
                    }
                }
                xml.Save(this.path);
            }
            else
            {
                var xml = new XmlDocument();
                var dec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = xml.CreateElement("Root");
                xml.AppendChild(root);//根元素
                for (var i = 0; i < NodeName.Length; i++)
                {
                    var PointName = xml.CreateElement(NodeName[i]);
                    for (var j = 0; j < AttributeNumber; j++)
                    {
                        PointName.SetAttribute(Attribute_name[j], Attribute_value[j][i]);
                    }
                    root.AppendChild(PointName);

                    xml.Save(this.path);
                }
            }
        }

        /// <summary>
        /// 节点值存储
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <param name="Text">文本</param>
        public void NodeStorage(string NodeName, string Text)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                var n = false;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == NodeName)
                    {
                        item.InnerText = Text;
                        n = true;
                        break;
                    }
                }
                if (n == false)
                {
                    var other = xml.CreateElement(NodeName);
                    other.InnerText = Text;
                    root.AppendChild(other);
                }
                xml.Save(this.path);
            }
            else
            {
                var doc = new XmlDocument();
                var dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                var Root = doc.CreateElement("Root");
                doc.AppendChild(Root);//根元素

                var Node = doc.CreateElement(NodeName);
                Node.InnerText = Text;
                Root.AppendChild(Node);

                doc.Save(this.path);
            }
        }

        #endregion 存储

        #region

        /// <summary>
        /// 通过节点取值
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <returns>取值失败返回null</returns>
        public string SearchNode(string NodeName)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == NodeName)
                    {
                        return item.InnerText;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 查找数字
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <param name="Attribute_name">属性名</param>
        /// <returns>取值失败返回0</returns>
        public int SearchNumber(string NodeName, string Attribute_name)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == NodeName)
                    {
                        if (item.Attributes[Attribute_name] != null)
                        {
                            return Convert.ToInt32(item.Attributes[Attribute_name].Value);
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 查找属性值
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <param name="Attribute_name">属性名</param>
        /// <returns>取值失败返回null</returns>
        public string SearchWords(string NodeName, string Attribute_name)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == NodeName)
                    {
                        if (item.Attributes[Attribute_name] != null)
                        {
                            return item.Attributes[Attribute_name].Value;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 查找布尔值
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <param name="Attribute_name">属性值</param>
        /// <returns>返回查找结果，查询失败返回false</returns>
        public bool SearchBoolean(string NodeName, string Attribute_name)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == NodeName)
                    {
                        if (item.Attributes[Attribute_name] != null)
                        {
                            try
                            {
                                return Convert.ToBoolean(item.Attributes[Attribute_name].Value);
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 查找属性值集合
        /// </summary>
        /// <param name="NodeName">节点名集合</param>
        /// <param name="Attribute_name">属性名集合</param>
        /// <returns>文件不在返回null，单个属性不在返回“空”</returns>
        public string[] SearchWords(string[] NodeName, string[] Attribute_name)
        {
            if (File.Exists(this.path))
            {
                var s = new string[NodeName.Length];
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                for (var i = 0; i < NodeName.Length; i++)
                {
                    foreach (XmlNode item in nodeList)
                    {
                        if (item.Name == NodeName[i])
                        {
                            s[i] = item.Attributes[Attribute_name[i]] != null ? item.Attributes[Attribute_name[i]].Value : "";
                        }
                    }
                }
                return s;
            }
            return null;
        }

        /// <summary>
        /// 通过确切属性值，属性名，查找其他属性值
        /// </summary>
        /// <param name="Attribute_name1">已知属性名</param>
        /// <param name="Attribute_value">已知属性值</param>
        /// <param name="Attribute_name2">待查属性名</param>
        /// <returns>待查属性值</returns>
        public string[] SearchWords(string Attribute_name1, string Attribute_value, string Attribute_name2)
        {
            var values = new List<string>();
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Attributes[Attribute_name1] != null)
                    {
                        if (item.Attributes[Attribute_name1].Value == Attribute_value)
                        {
                            if (item.Attributes[Attribute_name2] != null)
                            {
                                values.Add(item.Attributes[Attribute_name2].Value);
                            }
                        }
                    }
                }
            }
            return values.ToArray();
        }

        /// <summary>
        /// 查找节点的所有属性值
        /// </summary>
        /// <param name="NodeName">节点 名</param>
        /// <returns>返回查找键值对，查询失败返回null</returns>
        public Dictionary<string, string> SearchAllAttributes(string NodeName)
        {
            var Attributes = new Dictionary<string, string>();
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == NodeName)
                    {
                        var attributeCollection = item.Attributes;
                        if (attributeCollection != null)
                        {
                            foreach (XmlAttribute attribute in attributeCollection)
                            {
                                Attributes.Add(attribute.Name, attribute.Value);
                            }
                        }
                        return Attributes;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 通过确切属性值，属性名，查找其他属性的布尔值
        /// </summary>
        /// <param name="Attribute_name1">已知属性名</param>
        /// <param name="Attribute_value">已知属性值</param>
        /// <param name="Attribute_name2">待查属性名</param>
        /// <returns>待查布尔值，失败返回false</returns>
        public bool SearchBoolean(string Attribute_name1, string Attribute_value, string Attribute_name2)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Attributes[Attribute_name1].Value == Attribute_value)
                    {
                        if (item.Attributes[Attribute_name2] != null)
                        {
                            try
                            {
                                return Convert.ToBoolean(item.Attributes[Attribute_name2].Value);
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// 按节点名移除节点
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveNode(string NodeName)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == NodeName)
                    {
                        root.RemoveChild(item);
                        xml.Save(this.path);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 按确切的属性名，属性值删除节点
        /// </summary>
        /// <param name="Attribute_name">属性名</param>
        /// <param name="Attribute_value">属性值</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveNode(string Attribute_name, string Attribute_value)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Attributes[Attribute_name] != null)
                    {
                        if (item.Attributes[Attribute_name].Value == Attribute_value)
                        {
                            root.RemoveChild(item);
                            xml.Save(this.path);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 如果节点中有日期属性，把日期之前的节点都删除
        /// </summary>
        /// <param name="Attribute_name">属性名</param>
        /// <param name="dateTime">截止时间</param>
        /// <returns>是否删除成功</returns>
        public bool RemoveNode(string Attribute_name, DateTime dateTime)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                for (var i = 0; i < nodeList.Count; i++)
                {
                    if (nodeList[i].Attributes[Attribute_name] != null)
                    {
                        var dt = Convert.ToDateTime(nodeList[i].Attributes[Attribute_name].Value);
                        if (DateTime.Compare(dt, dateTime) < 0)
                        {
                            root.RemoveChild(nodeList[i]);
                        }
                    }
                }
                xml.Save(this.path);

                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断节点是否存在
        /// </summary>
        /// <param name="NodeName">节点名</param>
        /// <returns>返回结果</returns>
        public bool NodeExist(string NodeName)
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                var nodeList = root.ChildNodes;
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == NodeName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 删除所有节点，不包含子节点
        /// </summary>
        /// <returns>返回删除是否成功</returns>
        public bool RemoveAllNode()
        {
            if (File.Exists(this.path))
            {
                var xml = new XmlDocument();
                xml.Load(this.path);
                var root = xml.DocumentElement;
                root.RemoveAll();
                xml.Save(this.path);

                return true;
            }
            return false;
        }
    }
}