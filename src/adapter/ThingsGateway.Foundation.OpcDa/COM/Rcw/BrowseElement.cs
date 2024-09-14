//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.OpcDa.Rcw;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

[Serializable]
public class BrowseElement : ICloneable
{
    private bool m_hasChildren;
    private bool m_isItem;
    private string m_itemName;
    private string m_itemPath;
    private string m_name;
    private ItemProperty[] m_properties = [];

    public bool HasChildren
    {
        get
        {
            return m_hasChildren;
        }
        set
        {
            m_hasChildren = value;
        }
    }

    public bool IsItem
    {
        get
        {
            return m_isItem;
        }
        set
        {
            m_isItem = value;
        }
    }

    public string ItemName
    {
        get
        {
            return m_itemName;
        }
        set
        {
            m_itemName = value;
        }
    }

    public string ItemPath
    {
        get
        {
            return m_itemPath;
        }
        set
        {
            m_itemPath = value;
        }
    }

    public string Name
    {
        get
        {
            return m_name;
        }
        set
        {
            m_name = value;
        }
    }

    public ItemProperty[] Properties
    {
        get
        {
            return m_properties;
        }
        set
        {
            m_properties = value;
        }
    }

    public virtual object Clone()
    {
        BrowseElement obj = (BrowseElement)MemberwiseClone();
        obj.m_properties = (ItemProperty[])Comn.Convert.Clone(m_properties);
        return obj;
    }
}