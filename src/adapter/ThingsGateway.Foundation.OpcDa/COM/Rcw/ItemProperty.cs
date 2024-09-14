//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace ThingsGateway.Foundation.OpcDa.Rcw;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS8605 // 取消装箱可能为 null 的值。

public enum accessRights
{
    readable = 1,
    writable,
    readWritable
}

public enum euType
{
    noEnum = 1,
    analog,
    enumerated
}

public enum limitBits
{
    none,
    low,
    high,
    constant
}

public enum qualityBits
{
    good = 192,
    goodLocalOverride = 216,
    bad = 0,
    badConfigurationError = 4,
    badNotConnected = 8,
    badDeviceFailure = 12,
    badSensorFailure = 0x10,
    badLastKnownValue = 20,
    badCommFailure = 24,
    badOutOfService = 28,
    badWaitingForInitialData = 0x20,
    uncertain = 0x40,
    uncertainLastUsableValue = 68,
    uncertainSensorNotAccurate = 80,
    uncertainEUExceeded = 84,
    uncertainSubNormal = 88
}

public interface IResult
{
    string DiagnosticInfo { get; set; }
    ResultID ResultID { get; set; }
}

[Serializable]
public struct PropertyID : ISerializable
{
    private int m_code;

    private XmlQualifiedName m_name;

    public PropertyID(XmlQualifiedName name)
    {
        m_name = name;
        m_code = 0;
    }

    public PropertyID(int code)
    {
        m_name = null;
        m_code = code;
    }

    public PropertyID(string name, int code, string ns)
    {
        m_name = new XmlQualifiedName(name, ns);
        m_code = code;
    }

    private PropertyID(SerializationInfo info, StreamingContext context)
    {
        SerializationInfoEnumerator enumerator = info.GetEnumerator();
        string name = "";
        string ns = "";
        enumerator.Reset();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Name.Equals("NA"))
            {
                name = (string)enumerator.Current.Value;
            }
            else if (enumerator.Current.Name.Equals("NS"))
            {
                ns = (string)enumerator.Current.Value;
            }
        }

        m_name = new XmlQualifiedName(name, ns);
        m_code = (int)info.GetValue("CO", typeof(int));
    }

    public int Code => m_code;

    public XmlQualifiedName Name => m_name;

    public static bool operator !=(PropertyID a, PropertyID b)
    {
        return !a.Equals(b);
    }

    public static bool operator ==(PropertyID a, PropertyID b)
    {
        return a.Equals(b);
    }

    public override bool Equals(object? target)
    {
        if (target != null && target.GetType() == typeof(PropertyID))
        {
            PropertyID propertyID = (PropertyID)target;
            if (propertyID.Code != 0 && Code != 0)
            {
                return propertyID.Code == Code;
            }

            if (propertyID.Name != null && Name != null)
            {
                return propertyID.Name == Name;
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        if (Code != 0)
        {
            return Code.GetHashCode();
        }

        if (Name != null)
        {
            return Name.GetHashCode();
        }

        return base.GetHashCode();
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (m_name != null)
        {
            info.AddValue("NA", m_name.Name);
            info.AddValue("NS", m_name.Namespace);
        }

        info.AddValue("CO", m_code);
    }

    public override string ToString()
    {
        if (Name != null && Code != 0)
        {
            return $"{Name.Name} ({Code})";
        }

        if (Name != null)
        {
            return Name.Name;
        }

        if (Code != 0)
        {
            return $"{Code}";
        }

        return "";
    }

    private class Names
    {
        internal const string CODE = "CO";
        internal const string NAME = "NA";

        internal const string NAMESPACE = "NS";
    }
}

[Serializable]
public struct Quality
{
    public static readonly Quality Bad = new Quality(qualityBits.bad);
    public static readonly Quality Good = new Quality(qualityBits.good);
    private limitBits m_limitBits;
    private qualityBits m_qualityBits;
    private byte m_vendorBits;

    public Quality(qualityBits quality)
    {
        m_qualityBits = quality;
        m_limitBits = limitBits.none;
        m_vendorBits = 0;
    }

    public Quality(short code)
    {
        m_qualityBits = (qualityBits)(code & 0xFC);
        m_limitBits = (limitBits)(code & 3);
        m_vendorBits = (byte)((code & -253) >> 8);
    }

    public limitBits LimitBits
    {
        get
        {
            return m_limitBits;
        }
        set
        {
            m_limitBits = value;
        }
    }

    public qualityBits QualityBits
    {
        get
        {
            return m_qualityBits;
        }
        set
        {
            m_qualityBits = value;
        }
    }

    public byte VendorBits
    {
        get
        {
            return m_vendorBits;
        }
        set
        {
            m_vendorBits = value;
        }
    }

    public static bool operator !=(Quality a, Quality b)
    {
        return !a.Equals(b);
    }

    public static bool operator ==(Quality a, Quality b)
    {
        return a.Equals(b);
    }

    public override bool Equals(object? target)
    {
        if (target == null || target.GetType() != typeof(Quality))
        {
            return false;
        }

        Quality quality = (Quality)target;
        if (QualityBits != quality.QualityBits)
        {
            return false;
        }

        if (LimitBits != quality.LimitBits)
        {
            return false;
        }

        if (VendorBits != quality.VendorBits)
        {
            return false;
        }

        return true;
    }

    public short GetCode()
    {
        ushort num = 0;
        num = (ushort)(num | (ushort)QualityBits);
        num = (ushort)(num | (ushort)LimitBits);
        num = (ushort)(num | (ushort)(VendorBits << 8));
        if (num > 32767)
        {
            return (short)(-(65536 - num));
        }

        return (short)num;
    }

    public override int GetHashCode()
    {
        return GetCode();
    }

    public void SetCode(short code)
    {
        m_qualityBits = (qualityBits)(code & 0xFC);
        m_limitBits = (limitBits)(code & 3);
        m_vendorBits = (byte)((code & -253) >> 8);
    }

    public override string ToString()
    {
        string text = QualityBits.ToString();
        if (LimitBits != 0)
        {
            text += $"[{LimitBits}]";
        }

        if (VendorBits != 0)
        {
            text += string.Format(":{0,0:X}", VendorBits);
        }

        return text;
    }
}

[Serializable]
public struct ResultID : ISerializable
{
    public static readonly ResultID E_ACCESS_DENIED = new ResultID("E_ACCESS_DENIED", "http://opcfoundation.org/DataAccess/");

    public static readonly ResultID E_FAIL = new ResultID("E_FAIL", "http://opcfoundation.org/DataAccess/");

    public static readonly ResultID E_INVALIDARG = new ResultID("E_INVALIDARG", "http://opcfoundation.org/DataAccess/");

    public static readonly ResultID E_NETWORK_ERROR = new ResultID("E_NETWORK_ERROR", "http://opcfoundation.org/DataAccess/");

    public static readonly ResultID E_NOTSUPPORTED = new ResultID("E_NOTSUPPORTED", "http://opcfoundation.org/DataAccess/");

    public static readonly ResultID E_OUTOFMEMORY = new ResultID("E_OUTOFMEMORY", "http://opcfoundation.org/DataAccess/");

    public static readonly ResultID E_TIMEDOUT = new ResultID("E_TIMEDOUT", "http://opcfoundation.org/DataAccess/");

    public static readonly ResultID S_FALSE = new ResultID("S_FALSE", "http://opcfoundation.org/DataAccess/");

    public static readonly ResultID S_OK = new ResultID("S_OK", "http://opcfoundation.org/DataAccess/");

    private int m_code;

    private XmlQualifiedName m_name;

    public ResultID(XmlQualifiedName name)
    {
        m_name = name;
        m_code = -1;
    }

    public ResultID(long code)
    {
        m_name = null;
        if (code > int.MaxValue)
        {
            code = -(4294967296L - code);
        }

        m_code = (int)code;
    }

    public ResultID(string name, string ns)
    {
        m_name = new XmlQualifiedName(name, ns);
        m_code = -1;
    }

    public ResultID(ResultID resultID, long code)
    {
        m_name = resultID.Name;
        if (code > int.MaxValue)
        {
            code = -(4294967296L - code);
        }

        m_code = (int)code;
    }

    private ResultID(SerializationInfo info, StreamingContext context)
    {
        string name = (string)info.GetValue("NA", typeof(string));
        string ns = (string)info.GetValue("NS", typeof(string));
        m_name = new XmlQualifiedName(name, ns);
        m_code = (int)info.GetValue("CO", typeof(int));
    }

    public int Code => m_code;

    public XmlQualifiedName Name => m_name;

    public static bool operator !=(ResultID a, ResultID b)
    {
        return !a.Equals(b);
    }

    public static bool operator ==(ResultID a, ResultID b)
    {
        return a.Equals(b);
    }

    public override bool Equals(object? target)
    {
        if (target != null && target.GetType() == typeof(ResultID))
        {
            ResultID resultID = (ResultID)target;
            if (resultID.Code != -1 && Code != -1)
            {
                if (resultID.Code == Code)
                {
                    return resultID.Name == Name;
                }

                return false;
            }

            if (resultID.Name != null && Name != null)
            {
                return resultID.Name == Name;
            }
        }

        return false;
    }

    public bool Failed()
    {
        if (Code != -1)
        {
            return Code < 0;
        }

        if (Name != null)
        {
            return Name.Name.StartsWith("E_");
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (m_name != null)
        {
            info.AddValue("NA", m_name.Name);
            info.AddValue("NS", m_name.Namespace);
        }

        info.AddValue("CO", m_code);
    }

    public bool Succeeded()
    {
        if (Code != -1)
        {
            return Code >= 0;
        }

        if (Name != null)
        {
            return Name.Name.StartsWith("S_");
        }

        return false;
    }

    public override string ToString()
    {
        if (Name != null)
        {
            return Name.Name;
        }

        return string.Format("0x{0,0:X}", Code);
    }

    public class Ae
    {
        public static readonly ResultID E_BUSY = new ResultID("E_BUSY", "http://opcfoundation.org/AlarmAndEvents/");
        public static readonly ResultID E_INVALIDBRANCHNAME = new ResultID("E_INVALIDBRANCHNAME", "http://opcfoundation.org/AlarmAndEvents/");
        public static readonly ResultID E_INVALIDTIME = new ResultID("E_INVALIDTIME", "http://opcfoundation.org/AlarmAndEvents/");
        public static readonly ResultID E_NOINFO = new ResultID("E_NOINFO", "http://opcfoundation.org/AlarmAndEvents/");
        public static readonly ResultID S_ALREADYACKED = new ResultID("S_ALREADYACKED", "http://opcfoundation.org/AlarmAndEvents/");

        public static readonly ResultID S_INVALIDBUFFERTIME = new ResultID("S_INVALIDBUFFERTIME", "http://opcfoundation.org/AlarmAndEvents/");

        public static readonly ResultID S_INVALIDKEEPALIVETIME = new ResultID("S_INVALIDKEEPALIVETIME", "http://opcfoundation.org/AlarmAndEvents/");
        public static readonly ResultID S_INVALIDMAXSIZE = new ResultID("S_INVALIDMAXSIZE", "http://opcfoundation.org/AlarmAndEvents/");
    }

    public class Cpx
    {
        public static readonly ResultID E_FILTER_DUPLICATE = new ResultID("E_FILTER_DUPLICATE", "http://opcfoundation.org/ComplexData/");
        public static readonly ResultID E_FILTER_ERROR = new ResultID("E_FILTER_ERROR", "http://opcfoundation.org/ComplexData/");
        public static readonly ResultID E_FILTER_INVALID = new ResultID("E_FILTER_INVALID", "http://opcfoundation.org/ComplexData/");
        public static readonly ResultID E_TYPE_CHANGED = new ResultID("E_TYPE_CHANGED", "http://opcfoundation.org/ComplexData/");
        public static readonly ResultID S_FILTER_NO_DATA = new ResultID("S_FILTER_NO_DATA", "http://opcfoundation.org/ComplexData/");
    }

    public class Da
    {
        public static readonly ResultID E_BADTYPE = new ResultID("E_BADTYPE", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_INVALID_FILTER = new ResultID("E_INVALID_FILTER", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_INVALID_ITEM_NAME = new ResultID("E_INVALID_ITEM_NAME", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_INVALID_ITEM_PATH = new ResultID("E_INVALID_ITEM_PATH", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_INVALID_PID = new ResultID("E_INVALID_PID", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_INVALIDCONTINUATIONPOINT = new ResultID("E_INVALIDCONTINUATIONPOINT", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_INVALIDHANDLE = new ResultID("E_INVALIDHANDLE", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_NO_ITEM_BUFFERING = new ResultID("E_NO_ITEM_BUFFERING", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_NO_ITEM_DEADBAND = new ResultID("E_NO_ITEM_DEADBAND", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_NO_ITEM_SAMPLING = new ResultID("E_NO_ITEM_SAMPLING", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_NO_WRITEQT = new ResultID("E_NO_WRITEQT", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_RANGE = new ResultID("E_RANGE", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_READONLY = new ResultID("E_READONLY", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_UNKNOWN_ITEM_NAME = new ResultID("E_UNKNOWN_ITEM_NAME", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_UNKNOWN_ITEM_PATH = new ResultID("E_UNKNOWN_ITEM_PATH", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID E_WRITEONLY = new ResultID("E_WRITEONLY", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID S_CLAMP = new ResultID("S_CLAMP", "http://opcfoundation.org/DataAccess/");
        public static readonly ResultID S_DATAQUEUEOVERFLOW = new ResultID("S_DATAQUEUEOVERFLOW", "http://opcfoundation.org/DataAccess/");

        public static readonly ResultID S_UNSUPPORTEDRATE = new ResultID("S_UNSUPPORTEDRATE", "http://opcfoundation.org/DataAccess/");
    }

    public class Dx
    {
        public static readonly ResultID E_CONNECTIONS_EXIST = new ResultID("E_CONNECTIONS_EXIST", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_DUPLICATE_NAME = new ResultID("E_DUPLICATE_NAME", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_BROWSE_PATH = new ResultID("E_INVALID_BROWSE_PATH", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_CONFIG_FILE = new ResultID("E_INVALID_CONFIG_FILE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_DEADBAND = new ResultID("E_INVALID_DEADBAND", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_ITEM_NAME = new ResultID("E_INVALID_ITEM_NAME", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_ITEM_PATH = new ResultID("E_INVALID_ITEM_PATH", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_NAME = new ResultID("E_INVALID_NAME", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_QUEUE_SIZE = new ResultID("E_INVALID_QUEUE_SIZE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_SERVER_TYPE = new ResultID("E_INVALID_SERVER_TYPE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_SERVER_URL = new ResultID("E_INVALID_SERVER_URL", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_SOURCE_ITEM = new ResultID("E_INVALID_SOURCE_ITEM", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_INVALID_TARGET_ITEM = new ResultID("E_INVALID_TARGET_ITEM", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_NOITEMLIST = new ResultID("E_NOITEMLIST", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_OVERRIDE_BADTYPE = new ResultID("E_OVERRIDE_BADTYPE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_OVERRIDE_RANGE = new ResultID("E_OVERRIDE_RANGE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_PERSIST_FAILED = new ResultID("E_PERSIST_FAILED", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_PERSISTING = new ResultID("E_PERSISTING", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SERVER_STATE = new ResultID("E_SERVER_STATE", "http://opcfoundation.org/DataExchange/");

        public static readonly ResultID E_SOURCE_ITEM_BAD_QUALITY = new ResultID("E_SOURCE_ITEM_BAD_QUALITY", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SOURCE_ITEM_BADRIGHTS = new ResultID("E_SOURCE_ITEM_BADRIGHTS", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SOURCE_ITEM_BADTYPE = new ResultID("E_SOURCE_ITEM_BADTYPE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SOURCE_ITEM_RANGE = new ResultID("E_SOURCE_ITEM_RANGE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SOURCE_SERVER_FAULT = new ResultID("E_SOURCE_SERVER_FAULT", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SOURCE_SERVER_NO_ACCESSS = new ResultID("E_SOURCE_SERVER_NO_ACCESSS", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SOURCE_SERVER_NOT_CONNECTED = new ResultID("E_SOURCE_SERVER_NOT_CONNECTED", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SOURCE_SERVER_TIMEOUT = new ResultID("E_SOURCE_SERVER_TIMEOUT", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SUBSCRIPTION_FAULT = new ResultID("E_SUBSCRIPTION_FAULT", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SUBSTITUTE_BADTYPE = new ResultID("E_SUBSTITUTE_BADTYPE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_SUBSTITUTE_RANGE = new ResultID("E_SUBSTITUTE_RANGE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_TARGET_ALREADY_CONNECTED = new ResultID("E_TARGET_ALREADY_CONNECTED", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_TARGET_FAULT = new ResultID("E_TARGET_FAULT", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_TARGET_ITEM_BADTYPE = new ResultID("E_TARGET_ITEM_BADTYPE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_TARGET_ITEM_DISCONNECTED = new ResultID("E_TARGET_ITEM_DISCONNECTED", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_TARGET_ITEM_RANGE = new ResultID("E_TARGET_ITEM_RANGE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_TARGET_NO_ACCESSS = new ResultID("E_TARGET_NO_ACCESSS", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_TARGET_NO_WRITES_ATTEMPTED = new ResultID("E_TARGET_NO_WRITES_ATTEMPTED", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_TOO_MANY_CONNECTIONS = new ResultID("E_TOO_MANY_CONNECTIONS", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_UNKNOWN_ITEM_NAME = new ResultID("E_UNKNOWN_ITEM_NAME", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_UNKNOWN_ITEM_PATH = new ResultID("E_UNKNOWN_ITEM_PATH", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_UNKNOWN_SERVER_NAME = new ResultID("E_UNKNOWN_SERVER_NAME", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_UNKNOWN_SOURCE_ITEM = new ResultID("E_UNKNOWN_SOURCE_ITEM", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_UNKNOWN_TARGET_ITEM = new ResultID("E_UNKNOWN_TARGET_ITEM", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_UNSUPPORTED_SERVER_TYPE = new ResultID("E_UNSUPPORTED_SERVER_TYPE", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID E_VERSION_MISMATCH = new ResultID("E_VERSION_MISMATCH", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID S_CLAMP = new ResultID("S_CLAMP", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID S_TARGET_OVERRIDEN = new ResultID("S_TARGET_OVERRIDEN", "http://opcfoundation.org/DataExchange/");
        public static readonly ResultID S_TARGET_SUBSTITUTED = new ResultID("S_TARGET_SUBSTITUTED", "http://opcfoundation.org/DataExchange/");
    }

    public class Hda
    {
        public static readonly ResultID E_DATAEXISTS = new ResultID("E_DATAEXISTS", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID E_INVALIDAGGREGATE = new ResultID("E_INVALIDAGGREGATE", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID E_INVALIDATTRID = new ResultID("E_INVALIDATTRID", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID E_INVALIDDATATYPE = new ResultID("E_INVALIDDATATYPE", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID E_MAXEXCEEDED = new ResultID("E_MAXEXCEEDED", "http://opcfoundation.org/HistoricalDataAccess/");

        public static readonly ResultID E_NODATAEXISTS = new ResultID("E_NODATAEXISTS", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID E_NOT_AVAIL = new ResultID("E_NOT_AVAIL", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID E_UNKNOWNATTRID = new ResultID("E_UNKNOWNATTRID", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID S_CURRENTVALUE = new ResultID("S_CURRENTVALUE", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID S_EXTRADATA = new ResultID("S_EXTRADATA", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID S_INSERTED = new ResultID("S_INSERTED", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID S_MOREDATA = new ResultID("S_MOREDATA", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID S_NODATA = new ResultID("S_NODATA", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID S_REPLACED = new ResultID("S_REPLACED", "http://opcfoundation.org/HistoricalDataAccess/");
        public static readonly ResultID W_NOFILTER = new ResultID("W_NOFILTER", "http://opcfoundation.org/HistoricalDataAccess/");
    }

    private class Names
    {
        internal const string CODE = "CO";
        internal const string NAME = "NA";

        internal const string NAMESPACE = "NS";
    }
}

[Serializable]
public class ItemProperty : ICloneable, IResult
{
    private System.Type m_datatype;
    private string m_description;
    private string m_diagnosticInfo;
    private PropertyID m_id;
    private string m_itemName;
    private string m_itemPath;
    private ResultID m_resultID = ResultID.S_OK;
    private object m_value;

    public System.Type DataType
    {
        get
        {
            return m_datatype;
        }
        set
        {
            m_datatype = value;
        }
    }

    public string Description
    {
        get
        {
            return m_description;
        }
        set
        {
            m_description = value;
        }
    }

    public string DiagnosticInfo
    {
        get
        {
            return m_diagnosticInfo;
        }
        set
        {
            m_diagnosticInfo = value;
        }
    }

    public PropertyID ID
    {
        get
        {
            return m_id;
        }
        set
        {
            m_id = value;
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

    public ResultID ResultID
    {
        get
        {
            return m_resultID;
        }
        set
        {
            m_resultID = value;
        }
    }

    public object Value
    {
        get
        {
            return m_value;
        }
        set
        {
            m_value = value;
        }
    }

    public virtual object Clone()
    {
        ItemProperty obj = (ItemProperty)MemberwiseClone();

        obj.Value = Comn.Convert.Clone(Value);
        return obj;
    }
}

public class Property
{
    public static readonly PropertyID ACCESSRIGHTS = new PropertyID("accessRights", 5, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID ALARM_AREA_LIST = new PropertyID("alarmAreaList", 302, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID ALARM_QUICK_HELP = new PropertyID("alarmQuickHelp", 301, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID CLOSELABEL = new PropertyID("closeLabel", 106, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID CONDITION_LOGIC = new PropertyID("conditionLogic", 304, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID CONDITION_STATUS = new PropertyID("conditionStatus", 300, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID CONSISTENCY_WINDOW = new PropertyID("consistencyWindow", 605, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID DATA_FILTER_VALUE = new PropertyID("dataFilterValue", 609, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID DATATYPE = new PropertyID("dataType", 1, "http://opcfoundation.org/DataAccess/");

    public static readonly PropertyID DEADBAND = new PropertyID("deadband", 306, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID DESCRIPTION = new PropertyID("description", 101, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID DEVIATION_LIMIT = new PropertyID("deviationLimit", 312, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID DICTIONARY = new PropertyID("dictionary", 603, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID DICTIONARY_ID = new PropertyID("dictionaryID", 601, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID ENGINEERINGUINTS = new PropertyID("engineeringUnits", 100, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID EUINFO = new PropertyID("euInfo", 8, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID EUTYPE = new PropertyID("euType", 7, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID HI_LIMIT = new PropertyID("hiLimit", 308, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID HIGHEU = new PropertyID("highEU", 102, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID HIGHIR = new PropertyID("highIR", 104, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID HIHI_LIMIT = new PropertyID("hihiLimit", 307, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID LIMIT_EXCEEDED = new PropertyID("limitExceeded", 305, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID LO_LIMIT = new PropertyID("loLimit", 309, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID LOLO_LIMIT = new PropertyID("loloLimit", 310, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID LOWEU = new PropertyID("lowEU", 103, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID LOWIR = new PropertyID("lowIR", 105, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID MAXIMUM_VALUE = new PropertyID("maximumValue", 110, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID MINIMUM_VALUE = new PropertyID("minimumValue", 109, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID OPENLABEL = new PropertyID("openLabel", 107, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID PRIMARY_ALARM_AREA = new PropertyID("primaryAlarmArea", 303, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID QUALITY = new PropertyID("quality", 3, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID RATE_CHANGE_LIMIT = new PropertyID("rangeOfChangeLimit", 311, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID SCANRATE = new PropertyID("scanRate", 6, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID SOUNDFILE = new PropertyID("soundFile", 313, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID TIMESTAMP = new PropertyID("timestamp", 4, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID TIMEZONE = new PropertyID("timeZone", 108, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID TYPE_DESCRIPTION = new PropertyID("typeDescription", 604, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID TYPE_ID = new PropertyID("typeID", 602, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID TYPE_SYSTEM_ID = new PropertyID("typeSystemID", 600, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID UNCONVERTED_ITEM_ID = new PropertyID("unconvertedItemID", 607, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID UNFILTERED_ITEM_ID = new PropertyID("unfilteredItemID", 608, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID VALUE = new PropertyID("value", 2, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID VALUE_PRECISION = new PropertyID("valuePrecision", 111, "http://opcfoundation.org/DataAccess/");
    public static readonly PropertyID WRITE_BEHAVIOR = new PropertyID("writeBehavior", 606, "http://opcfoundation.org/DataAccess/");
}

public class Type
{
    public static System.Type ANY_TYPE = typeof(object);
    public static System.Type ARRAY_ANY_TYPE = typeof(object[]);
    public static System.Type ARRAY_BOOLEAN = typeof(bool[]);
    public static System.Type ARRAY_DATETIME = typeof(DateTime[]);
    public static System.Type ARRAY_DECIMAL = typeof(decimal[]);
    public static System.Type ARRAY_DOUBLE = typeof(double[]);
    public static System.Type ARRAY_FLOAT = typeof(float[]);
    public static System.Type ARRAY_INT = typeof(int[]);
    public static System.Type ARRAY_LONG = typeof(long[]);
    public static System.Type ARRAY_SHORT = typeof(short[]);
    public static System.Type ARRAY_STRING = typeof(string[]);
    public static System.Type ARRAY_UINT = typeof(uint[]);
    public static System.Type ARRAY_ULONG = typeof(ulong[]);
    public static System.Type ARRAY_USHORT = typeof(ushort[]);
    public static System.Type BINARY = typeof(byte[]);
    public static System.Type BOOLEAN = typeof(bool);
    public static System.Type BYTE = typeof(byte);
    public static System.Type DATETIME = typeof(DateTime);
    public static System.Type DECIMAL = typeof(decimal);
    public static System.Type DOUBLE = typeof(double);
    public static System.Type DURATION = typeof(TimeSpan);
    public static System.Type FLOAT = typeof(float);
    public static System.Type ILLEGAL_TYPE = typeof(Type);
    public static System.Type INT = typeof(int);
    public static System.Type LONG = typeof(long);
    public static System.Type SBYTE = typeof(sbyte);
    public static System.Type SHORT = typeof(short);

    public static System.Type STRING = typeof(string);
    public static System.Type UINT = typeof(uint);
    public static System.Type ULONG = typeof(ulong);
    public static System.Type USHORT = typeof(ushort);

    public static System.Type[] Enumerate()
    {
        ArrayList arrayList = new ArrayList();
        FieldInfo[] fields = typeof(Type).GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (FieldInfo fieldInfo in fields)
        {
            arrayList.Add(fieldInfo.GetValue(typeof(System.Type)));
        }

        return (System.Type[])arrayList.ToArray(typeof(System.Type));
    }
}

[Serializable]
public class PropertyDescription
{
    public static readonly PropertyDescription ACCESSRIGHTS = new PropertyDescription(Property.ACCESSRIGHTS, typeof(accessRights), "Item Access Rights");
    public static readonly PropertyDescription ALARM_AREA_LIST = new PropertyDescription(Property.ALARM_AREA_LIST, typeof(string), "Alarm Area List");
    public static readonly PropertyDescription ALARM_QUICK_HELP = new PropertyDescription(Property.ALARM_QUICK_HELP, typeof(string), "Alarm Quick Help");
    public static readonly PropertyDescription CLOSELABEL = new PropertyDescription(Property.CLOSELABEL, typeof(string), "Contact Close Label");
    public static readonly PropertyDescription CONDITION_LOGIC = new PropertyDescription(Property.CONDITION_LOGIC, typeof(string), "Condition Logic");
    public static readonly PropertyDescription CONDITION_STATUS = new PropertyDescription(Property.CONDITION_STATUS, typeof(string), "Condition Status");
    public static readonly PropertyDescription CONSISTENCY_WINDOW = new PropertyDescription(Property.CONSISTENCY_WINDOW, typeof(string), "Consistency Window");
    public static readonly PropertyDescription DATA_FILTER_VALUE = new PropertyDescription(Property.DATA_FILTER_VALUE, typeof(string), "Data Filter Value");
    public static readonly PropertyDescription DATATYPE = new PropertyDescription(Property.DATATYPE, typeof(System.Type), "Item Canonical DataType");
    public static readonly PropertyDescription DEADBAND = new PropertyDescription(Property.DEADBAND, typeof(double), "Deadband");
    public static readonly PropertyDescription DESCRIPTION = new PropertyDescription(Property.DESCRIPTION, typeof(string), "Item Description");
    public static readonly PropertyDescription DEVIATION_LIMIT = new PropertyDescription(Property.DEVIATION_LIMIT, typeof(double), "Deviation Limit");
    public static readonly PropertyDescription DICTIONARY = new PropertyDescription(Property.DICTIONARY, typeof(object), "Dictionary");
    public static readonly PropertyDescription DICTIONARY_ID = new PropertyDescription(Property.DICTIONARY_ID, typeof(string), "Dictionary ID");
    public static readonly PropertyDescription ENGINEERINGUINTS = new PropertyDescription(Property.ENGINEERINGUINTS, typeof(string), "EU Units");
    public static readonly PropertyDescription EUINFO = new PropertyDescription(Property.EUINFO, typeof(string[]), "Item EU Info");
    public static readonly PropertyDescription EUTYPE = new PropertyDescription(Property.EUTYPE, typeof(euType), "Item EU Type");
    public static readonly PropertyDescription HI_LIMIT = new PropertyDescription(Property.HI_LIMIT, typeof(double), "Hi Limit");
    public static readonly PropertyDescription HIGHEU = new PropertyDescription(Property.HIGHEU, typeof(double), "High EU");
    public static readonly PropertyDescription HIGHIR = new PropertyDescription(Property.HIGHIR, typeof(double), "High Instrument Range");
    public static readonly PropertyDescription HIHI_LIMIT = new PropertyDescription(Property.HIHI_LIMIT, typeof(double), "HiHi Limit");
    public static readonly PropertyDescription LIMIT_EXCEEDED = new PropertyDescription(Property.LIMIT_EXCEEDED, typeof(string), "Limit Exceeded");
    public static readonly PropertyDescription LO_LIMIT = new PropertyDescription(Property.LO_LIMIT, typeof(double), "Lo Limit");
    public static readonly PropertyDescription LOLO_LIMIT = new PropertyDescription(Property.LOLO_LIMIT, typeof(double), "LoLo Limit");
    public static readonly PropertyDescription LOWEU = new PropertyDescription(Property.LOWEU, typeof(double), "Low EU");
    public static readonly PropertyDescription LOWIR = new PropertyDescription(Property.LOWIR, typeof(double), "Low Instrument Range");
    public static readonly PropertyDescription MAXIMUM_VALUE = new PropertyDescription(Property.MAXIMUM_VALUE, typeof(object), "Maximum Value");
    public static readonly PropertyDescription MINIMUM_VALUE = new PropertyDescription(Property.MINIMUM_VALUE, typeof(object), "Minimum Value");
    public static readonly PropertyDescription OPENLABEL = new PropertyDescription(Property.OPENLABEL, typeof(string), "Contact Open Label");
    public static readonly PropertyDescription PRIMARY_ALARM_AREA = new PropertyDescription(Property.PRIMARY_ALARM_AREA, typeof(string), "Primary Alarm Area");
    public static readonly PropertyDescription QUALITY = new PropertyDescription(Property.QUALITY, typeof(Quality), "Item Quality");
    public static readonly PropertyDescription RATE_CHANGE_LIMIT = new PropertyDescription(Property.RATE_CHANGE_LIMIT, typeof(double), "Rate of Change Limit");
    public static readonly PropertyDescription SCANRATE = new PropertyDescription(Property.SCANRATE, typeof(float), "Server Scan Rate");
    public static readonly PropertyDescription SOUNDFILE = new PropertyDescription(Property.SOUNDFILE, typeof(string), "Sound File");
    public static readonly PropertyDescription TIMESTAMP = new PropertyDescription(Property.TIMESTAMP, typeof(DateTime), "Item Timestamp");
    public static readonly PropertyDescription TIMEZONE = new PropertyDescription(Property.TIMEZONE, typeof(int), "Timezone");
    public static readonly PropertyDescription TYPE_DESCRIPTION = new PropertyDescription(Property.TYPE_DESCRIPTION, typeof(string), "Type Description");
    public static readonly PropertyDescription TYPE_ID = new PropertyDescription(Property.TYPE_ID, typeof(string), "Type ID");
    public static readonly PropertyDescription TYPE_SYSTEM_ID = new PropertyDescription(Property.TYPE_SYSTEM_ID, typeof(string), "Type System ID");
    public static readonly PropertyDescription UNCONVERTED_ITEM_ID = new PropertyDescription(Property.UNCONVERTED_ITEM_ID, typeof(string), "Unconverted Item ID");
    public static readonly PropertyDescription UNFILTERED_ITEM_ID = new PropertyDescription(Property.UNFILTERED_ITEM_ID, typeof(string), "Unfiltered Item ID");
    public static readonly PropertyDescription VALUE = new PropertyDescription(Property.VALUE, typeof(object), "Item Value");
    public static readonly PropertyDescription VALUE_PRECISION = new PropertyDescription(Property.VALUE_PRECISION, typeof(object), "Value Precision");
    public static readonly PropertyDescription WRITE_BEHAVIOR = new PropertyDescription(Property.WRITE_BEHAVIOR, typeof(string), "Write Behavior");
    private PropertyID m_id;

    private string m_name;
    private System.Type m_type;

    public PropertyDescription(PropertyID id, System.Type type, string name)
    {
        ID = id;
        Type = type;
        Name = name;
    }

    public PropertyID ID
    {
        get
        {
            return m_id;
        }
        set
        {
            m_id = value;
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

    public System.Type Type
    {
        get
        {
            return m_type;
        }
        set
        {
            m_type = value;
        }
    }

    public static PropertyDescription[] Enumerate()
    {
        ArrayList arrayList = new ArrayList();
        FieldInfo[] fields = typeof(PropertyDescription).GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (FieldInfo fieldInfo in fields)
        {
            arrayList.Add(fieldInfo.GetValue(typeof(PropertyDescription)));
        }

        return (PropertyDescription[])arrayList.ToArray(typeof(PropertyDescription));
    }

    public static PropertyDescription Find(PropertyID id)
    {
        FieldInfo[] fields = typeof(PropertyDescription).GetFields(BindingFlags.Static | BindingFlags.Public);
        for (int i = 0; i < fields.Length; i++)
        {
            PropertyDescription propertyDescription = (PropertyDescription)fields[i].GetValue(typeof(PropertyDescription));
            if (propertyDescription.ID == id)
            {
                return propertyDescription;
            }
        }

        return null;
    }

    public override string ToString()
    {
        return Name;
    }
}