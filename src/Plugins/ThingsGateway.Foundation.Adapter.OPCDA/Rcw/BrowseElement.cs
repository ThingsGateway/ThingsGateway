namespace OpcDaClient.Da
{
    [Serializable]
    public class BrowseElement : ICloneable
    {
        private string m_name;

        private string m_itemName;

        private string m_itemPath;

        private bool m_isItem;

        private bool m_hasChildren;

        private ItemProperty[] m_properties = new ItemProperty[0];

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
            obj.m_properties = (ItemProperty[])OpcDaClient.Comn.Convert.Clone(m_properties);
            return obj;
        }
    }
}