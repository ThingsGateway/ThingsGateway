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
using System.Runtime.InteropServices;

namespace ThingsGateway.Foundation.OpcDa.Rcw;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS8605 // 取消装箱可能为 null 的值。

public class Interop
{
    public static PropertyID GetPropertyID(int input)
    {
        foreach (FieldInfo field in typeof(Property).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            PropertyID propertyId = (PropertyID)field.GetValue(typeof(PropertyID));
            if (input == propertyId.Code)
                return propertyId;
        }
        return new PropertyID(input);
    }

    internal static BrowseElement GetBrowseElement(IntPtr pInput, bool deallocate)
    {
        BrowseElement browseElement = null;
        if (pInput != IntPtr.Zero)
        {
            OPCBROWSEELEMENT structure = (OPCBROWSEELEMENT)Marshal.PtrToStructure(pInput, typeof(OPCBROWSEELEMENT));
            browseElement = new BrowseElement();
            browseElement.Name = structure.szName;
            browseElement.ItemPath = null;
            browseElement.ItemName = structure.szItemID;
            browseElement.IsItem = (structure.dwFlagValue & 2) != 0;
            browseElement.HasChildren = (structure.dwFlagValue & 1) != 0;
            browseElement.Properties = Interop.GetItemProperties(ref structure.ItemProperties, deallocate);
            if (deallocate)
                Marshal.DestroyStructure(pInput, typeof(OPCBROWSEELEMENT));
        }
        return browseElement;
    }

    internal static OPCBROWSEELEMENT GetBrowseElement(
      BrowseElement input,
      bool propertiesRequested)
    {
        OPCBROWSEELEMENT browseElement = new OPCBROWSEELEMENT();
        if (input != null)
        {
            browseElement.szName = input.Name;
            browseElement.szItemID = input.ItemName;
            browseElement.dwFlagValue = 0;
            browseElement.ItemProperties = Interop.GetItemProperties(input.Properties);
            if (input.IsItem)
                browseElement.dwFlagValue |= 2;
            if (input.HasChildren)
                browseElement.dwFlagValue |= 1;
        }
        return browseElement;
    }

    internal static BrowseElement[] GetBrowseElements(
                  ref IntPtr pInput,
      int count,
      bool deallocate)
    {
        BrowseElement[] browseElements = null;
        if (pInput != IntPtr.Zero && count > 0)
        {
            browseElements = new BrowseElement[count];
            IntPtr pInput1 = pInput;
            for (int index = 0; index < count; ++index)
            {
                browseElements[index] = Interop.GetBrowseElement(pInput1, deallocate);
                pInput1 = (IntPtr)(pInput1.ToInt64() + Marshal.SizeOf(typeof(OPCBROWSEELEMENT)));
            }
            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pInput);
                pInput = IntPtr.Zero;
            }
        }
        return browseElements;
    }

    internal static IntPtr GetBrowseElements(BrowseElement[] input, bool propertiesRequested)
    {
        IntPtr browseElements = IntPtr.Zero;
        if (input != null && input.Length != 0)
        {
            browseElements = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OPCBROWSEELEMENT)) * input.Length);
            IntPtr ptr = browseElements;
            for (int index = 0; index < input.Length; ++index)
            {
                Marshal.StructureToPtr((object)Interop.GetBrowseElement(input[index], propertiesRequested), ptr, false);
                ptr = (IntPtr)(ptr.ToInt64() + Marshal.SizeOf(typeof(OPCBROWSEELEMENT)));
            }
        }
        return browseElements;
    }

    internal static ItemProperty[] GetItemProperties(
      ref OPCITEMPROPERTIES input,
      bool deallocate)
    {
        ItemProperty[] itemProperties = null;
        if (input.dwNumProperties > 0)
        {
            itemProperties = new ItemProperty[input.dwNumProperties];
            IntPtr pInput = input.pItemProperties;
            for (int index = 0; index < itemProperties.Length; ++index)
            {
                try
                {
                    itemProperties[index] = Interop.GetItemProperty(pInput, deallocate);
                }
                catch (Exception ex)
                {
                    itemProperties[index] = new ItemProperty();
                    itemProperties[index].Description = ex.Message;
                    itemProperties[index].ResultID = ResultID.E_FAIL;
                }
                pInput = (IntPtr)(pInput.ToInt64() + Marshal.SizeOf(typeof(OPCITEMPROPERTY)));
            }
            if (deallocate)
            {
                Marshal.FreeCoTaskMem(input.pItemProperties);
                input.pItemProperties = IntPtr.Zero;
            }
        }
        return itemProperties;
    }

    internal static OPCITEMPROPERTIES GetItemProperties(ItemProperty[] input)
    {
        OPCITEMPROPERTIES itemProperties = new OPCITEMPROPERTIES();
        if (input != null && input.Length != 0)
        {
            itemProperties.hrErrorID = 0;
            itemProperties.dwReserved = 0;
            itemProperties.dwNumProperties = input.Length;
            itemProperties.pItemProperties = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OPCITEMPROPERTY)) * input.Length);
            bool flag = false;
            IntPtr ptr = itemProperties.pItemProperties;
            for (int index = 0; index < input.Length; ++index)
            {
                Marshal.StructureToPtr((object)Interop.GetItemProperty(input[index]), ptr, false);
                ptr = (IntPtr)(ptr.ToInt64() + Marshal.SizeOf(typeof(OPCITEMPROPERTY)));
                if (input[index].ResultID.Failed())
                    flag = true;
            }
            if (flag)
                itemProperties.hrErrorID = 1;
        }
        return itemProperties;
    }

    internal static ItemProperty GetItemProperty(IntPtr pInput, bool deallocate)
    {
        ItemProperty itemProperty = null;
        if (pInput != IntPtr.Zero)
        {
            OPCITEMPROPERTY structure = (OPCITEMPROPERTY)Marshal.PtrToStructure(pInput, typeof(OPCITEMPROPERTY));
            itemProperty = new ItemProperty()
            {
                ID = Interop.GetPropertyID(structure.dwPropertyID),
                Description = structure.szDescription,
                DataType = Interop.GetType((VarEnum)structure.vtDataType),
                ItemPath = null,
                ItemName = structure.szItemID
            };
            itemProperty.Value = Interop.UnmarshalPropertyValue(itemProperty.ID, structure.vValue);
            itemProperty.ResultID = Interop.GetResultID(structure.hrErrorID);
            if (structure.hrErrorID == -1073479674)
                itemProperty.ResultID = new ResultID(ResultID.Da.E_WRITEONLY, -1073479674L);
            if (deallocate)
                Marshal.DestroyStructure(pInput, typeof(OPCITEMPROPERTY));
        }
        return itemProperty;
    }

    internal static OPCITEMPROPERTY GetItemProperty(ItemProperty input)
    {
        OPCITEMPROPERTY itemProperty = new OPCITEMPROPERTY();
        if (input != null)
        {
            itemProperty.dwPropertyID = input.ID.Code;
            itemProperty.szDescription = input.Description;
            itemProperty.vtDataType = (short)Interop.GetType(input.DataType);
            itemProperty.vValue = Interop.MarshalPropertyValue(input.ID, input.Value);
            itemProperty.wReserved = 0;
            itemProperty.hrErrorID = Interop.GetResultID(input.ResultID);
            PropertyDescription propertyDescription = PropertyDescription.Find(input.ID);
            if (propertyDescription != null)
                itemProperty.vtDataType = (short)Interop.GetType(propertyDescription.Type);
            if (input.ResultID == ResultID.Da.E_WRITEONLY)
                itemProperty.hrErrorID = -1073479674;
        }
        return itemProperty;
    }

    internal static int[] GetPropertyIDs(PropertyID[] propertyIDs)
    {
        ArrayList arrayList = new ArrayList();
        if (propertyIDs != null)
        {
            foreach (PropertyID propertyId in propertyIDs)
                arrayList.Add(propertyId.Code);
        }
        return (int[])arrayList.ToArray(typeof(int));
    }

    internal static ResultID GetResultID(int input)
    {
        switch (input)
        {
            case -2147467262:
                return new ResultID(ResultID.E_NOTSUPPORTED, input);

            case -2147467259:
                return new ResultID(ResultID.E_FAIL, input);

            case -2147352571:
                return new ResultID(ResultID.Da.E_BADTYPE, input);

            case -2147352566:
                return new ResultID(ResultID.Da.E_RANGE, input);

            case -2147217401:
                return new ResultID(ResultID.Hda.W_NOFILTER, input);

            case -2147024882:
                return new ResultID(ResultID.E_OUTOFMEMORY, input);

            case -2147024809:
                return new ResultID(ResultID.E_INVALIDARG, input);

            case -1073479679:
                return new ResultID(ResultID.Da.E_INVALIDHANDLE, input);

            case -1073479676:
                return new ResultID(ResultID.Da.E_BADTYPE, input);

            case -1073479673:
                return new ResultID(ResultID.Da.E_UNKNOWN_ITEM_NAME, input);

            case -1073479672:
                return new ResultID(ResultID.Da.E_INVALID_ITEM_NAME, input);

            case -1073479671:
                return new ResultID(ResultID.Da.E_INVALID_FILTER, input);

            case -1073479670:
                return new ResultID(ResultID.Da.E_UNKNOWN_ITEM_PATH, input);

            case -1073479669:
                return new ResultID(ResultID.Da.E_RANGE, input);

            case -1073479165:
                return new ResultID(ResultID.Da.E_INVALID_PID, input);

            case -1073479164:
                return new ResultID(ResultID.Ae.E_INVALIDTIME, input);

            case -1073479163:
                return new ResultID(ResultID.Ae.E_BUSY, input);

            case -1073479162:
                return new ResultID(ResultID.Ae.E_NOINFO, input);

            case -1073478655:
                return new ResultID(ResultID.Da.E_NO_ITEM_DEADBAND, input);

            case -1073478654:
                return new ResultID(ResultID.Da.E_NO_ITEM_BUFFERING, input);

            case -1073478653:
                return new ResultID(ResultID.Da.E_INVALIDCONTINUATIONPOINT, input);

            case -1073478650:
                return new ResultID(ResultID.Da.E_NO_WRITEQT, input);

            case -1073478649:
                return new ResultID(ResultID.Cpx.E_TYPE_CHANGED, input);

            case -1073478648:
                return new ResultID(ResultID.Cpx.E_FILTER_DUPLICATE, input);

            case -1073478647:
                return new ResultID(ResultID.Cpx.E_FILTER_INVALID, input);

            case -1073478646:
                return new ResultID(ResultID.Cpx.E_FILTER_ERROR, input);

            case -1073477888:
                return new ResultID(ResultID.Dx.E_PERSISTING, input);

            case -1073477887:
                return new ResultID(ResultID.Dx.E_NOITEMLIST, input);

            case -1073477886:
                return new ResultID(ResultID.Dx.E_VERSION_MISMATCH, input);

            case -1073477885:
                return new ResultID(ResultID.Dx.E_VERSION_MISMATCH, input);

            case -1073477884:
                return new ResultID(ResultID.Dx.E_UNKNOWN_ITEM_PATH, input);

            case -1073477883:
                return new ResultID(ResultID.Dx.E_UNKNOWN_ITEM_NAME, input);

            case -1073477882:
                return new ResultID(ResultID.Dx.E_INVALID_ITEM_PATH, input);

            case -1073477881:
                return new ResultID(ResultID.Dx.E_INVALID_ITEM_NAME, input);

            case -1073477880:
                return new ResultID(ResultID.Dx.E_INVALID_NAME, input);

            case -1073477879:
                return new ResultID(ResultID.Dx.E_DUPLICATE_NAME, input);

            case -1073477878:
                return new ResultID(ResultID.Dx.E_INVALID_BROWSE_PATH, input);

            case -1073477877:
                return new ResultID(ResultID.Dx.E_INVALID_SERVER_URL, input);

            case -1073477876:
                return new ResultID(ResultID.Dx.E_INVALID_SERVER_TYPE, input);

            case -1073477875:
                return new ResultID(ResultID.Dx.E_UNSUPPORTED_SERVER_TYPE, input);

            case -1073477874:
                return new ResultID(ResultID.Dx.E_CONNECTIONS_EXIST, input);

            case -1073477873:
                return new ResultID(ResultID.Dx.E_TOO_MANY_CONNECTIONS, input);

            case -1073477872:
                return new ResultID(ResultID.Dx.E_OVERRIDE_BADTYPE, input);

            case -1073477871:
                return new ResultID(ResultID.Dx.E_OVERRIDE_RANGE, input);

            case -1073477870:
                return new ResultID(ResultID.Dx.E_SUBSTITUTE_BADTYPE, input);

            case -1073477869:
                return new ResultID(ResultID.Dx.E_SUBSTITUTE_RANGE, input);

            case -1073477868:
                return new ResultID(ResultID.Dx.E_INVALID_TARGET_ITEM, input);

            case -1073477867:
                return new ResultID(ResultID.Dx.E_UNKNOWN_TARGET_ITEM, input);

            case -1073477866:
                return new ResultID(ResultID.Dx.E_TARGET_ALREADY_CONNECTED, input);

            case -1073477865:
                return new ResultID(ResultID.Dx.E_UNKNOWN_SERVER_NAME, input);

            case -1073477864:
                return new ResultID(ResultID.Dx.E_UNKNOWN_SOURCE_ITEM, input);

            case -1073477863:
                return new ResultID(ResultID.Dx.E_INVALID_SOURCE_ITEM, input);

            case -1073477862:
                return new ResultID(ResultID.Dx.E_INVALID_QUEUE_SIZE, input);

            case -1073477861:
                return new ResultID(ResultID.Dx.E_INVALID_DEADBAND, input);

            case -1073477860:
                return new ResultID(ResultID.Dx.E_INVALID_CONFIG_FILE, input);

            case -1073477859:
                return new ResultID(ResultID.Dx.E_PERSIST_FAILED, input);

            case -1073477858:
                return new ResultID(ResultID.Dx.E_TARGET_FAULT, input);

            case -1073477857:
                return new ResultID(ResultID.Dx.E_TARGET_NO_ACCESSS, input);

            case -1073477856:
                return new ResultID(ResultID.Dx.E_SOURCE_SERVER_FAULT, input);

            case -1073477855:
                return new ResultID(ResultID.Dx.E_SOURCE_SERVER_NO_ACCESSS, input);

            case -1073477854:
                return new ResultID(ResultID.Dx.E_SUBSCRIPTION_FAULT, input);

            case -1073477853:
                return new ResultID(ResultID.Dx.E_SOURCE_ITEM_BADRIGHTS, input);

            case -1073477852:
                return new ResultID(ResultID.Dx.E_SOURCE_ITEM_BAD_QUALITY, input);

            case -1073477851:
                return new ResultID(ResultID.Dx.E_SOURCE_ITEM_BADTYPE, input);

            case -1073477850:
                return new ResultID(ResultID.Dx.E_SOURCE_ITEM_RANGE, input);

            case -1073477849:
                return new ResultID(ResultID.Dx.E_SOURCE_SERVER_NOT_CONNECTED, input);

            case -1073477848:
                return new ResultID(ResultID.Dx.E_SOURCE_SERVER_TIMEOUT, input);

            case -1073477847:
                return new ResultID(ResultID.Dx.E_TARGET_ITEM_DISCONNECTED, input);

            case -1073477846:
                return new ResultID(ResultID.Dx.E_TARGET_NO_WRITES_ATTEMPTED, input);

            case -1073477845:
                return new ResultID(ResultID.Dx.E_TARGET_ITEM_BADTYPE, input);

            case -1073477844:
                return new ResultID(ResultID.Dx.E_TARGET_ITEM_RANGE, input);

            case -1073475583:
                return new ResultID(ResultID.Hda.E_MAXEXCEEDED, input);

            case -1073475580:
                return new ResultID(ResultID.Hda.E_INVALIDAGGREGATE, input);

            case -1073475576:
                return new ResultID(ResultID.Hda.E_UNKNOWNATTRID, input);

            case -1073475575:
                return new ResultID(ResultID.Hda.E_NOT_AVAIL, input);

            case -1073475574:
                return new ResultID(ResultID.Hda.E_INVALIDDATATYPE, input);

            case -1073475573:
                return new ResultID(ResultID.Hda.E_DATAEXISTS, input);

            case -1073475572:
                return new ResultID(ResultID.Hda.E_INVALIDATTRID, input);

            case -1073475571:
                return new ResultID(ResultID.Hda.E_NODATAEXISTS, input);

            case 0:
                return new ResultID(ResultID.S_OK, input);

            case 262157:
                return new ResultID(ResultID.Da.S_UNSUPPORTEDRATE, input);

            case 262158:
                return new ResultID(ResultID.Da.S_CLAMP, input);

            case 262656:
                return new ResultID(ResultID.Ae.S_ALREADYACKED, input);

            case 262657:
                return new ResultID(ResultID.Ae.S_INVALIDBUFFERTIME, input);

            case 262658:
                return new ResultID(ResultID.Ae.S_INVALIDMAXSIZE, input);

            case 262659:
                return new ResultID(ResultID.Ae.S_INVALIDKEEPALIVETIME, input);

            case 263172:
                return new ResultID(ResultID.Da.S_DATAQUEUEOVERFLOW, input);

            case 263179:
                return new ResultID(ResultID.Cpx.S_FILTER_NO_DATA, input);

            case 264064:
                return new ResultID(ResultID.Dx.S_TARGET_SUBSTITUTED, input);

            case 264065:
                return new ResultID(ResultID.Dx.S_TARGET_OVERRIDEN, input);

            case 264066:
                return new ResultID(ResultID.Dx.S_CLAMP, input);

            case 1074008066:
                return new ResultID(ResultID.Hda.S_NODATA, input);

            case 1074008067:
                return new ResultID(ResultID.Hda.S_MOREDATA, input);

            case 1074008069:
                return new ResultID(ResultID.Hda.S_CURRENTVALUE, input);

            case 1074008070:
                return new ResultID(ResultID.Hda.S_EXTRADATA, input);

            case 1074008078:
                return new ResultID(ResultID.Hda.S_INSERTED, input);

            case 1074008079:
                return new ResultID(ResultID.Hda.S_REPLACED, input);

            default:
                if ((input & 2147418112) == 65536)
                    return new ResultID(ResultID.E_NETWORK_ERROR, input);
                return input >= 0 ? new ResultID(ResultID.S_FALSE, input) : new ResultID(ResultID.E_FAIL, input);
        }
    }

    internal static int GetResultID(ResultID input)
    {
        if (input.Name != null && input.Name.Namespace == "http://opcfoundation.org/DataAccess/")
        {
            if (input == ResultID.S_OK)
                return 0;
            if (input == ResultID.E_FAIL)
                return -2147467259;
            if (input == ResultID.E_INVALIDARG)
                return -2147024809;
            if (input == ResultID.Da.E_BADTYPE)
                return -1073479676;
            if (input == ResultID.Da.E_READONLY || input == ResultID.Da.E_WRITEONLY)
                return -1073479674;
            if (input == ResultID.Da.E_RANGE)
                return -1073479669;
            if (input == ResultID.E_OUTOFMEMORY)
                return -2147024882;
            if (input == ResultID.E_NOTSUPPORTED)
                return -2147467262;
            if (input == ResultID.Da.E_INVALIDHANDLE)
                return -1073479679;
            if (input == ResultID.Da.E_UNKNOWN_ITEM_NAME)
                return -1073479673;
            if (input == ResultID.Da.E_INVALID_ITEM_NAME || input == ResultID.Da.E_INVALID_ITEM_PATH)
                return -1073479672;
            if (input == ResultID.Da.E_UNKNOWN_ITEM_PATH)
                return -1073479670;
            if (input == ResultID.Da.E_INVALID_FILTER)
                return -1073479671;
            if (input == ResultID.Da.S_UNSUPPORTEDRATE)
                return 262157;
            if (input == ResultID.Da.S_CLAMP)
                return 262158;
            if (input == ResultID.Da.E_INVALID_PID)
                return -1073479165;
            if (input == ResultID.Da.E_NO_ITEM_DEADBAND)
                return -1073478655;
            if (input == ResultID.Da.E_NO_ITEM_BUFFERING)
                return -1073478654;
            if (input == ResultID.Da.E_NO_WRITEQT)
                return -1073478650;
            if (input == ResultID.Da.E_INVALIDCONTINUATIONPOINT)
                return -1073478653;
            if (input == ResultID.Da.S_DATAQUEUEOVERFLOW)
                return 263172;
        }
        else if (input.Name != null && input.Name.Namespace == "http://opcfoundation.org/ComplexData/")
        {
            if (input == ResultID.Cpx.E_TYPE_CHANGED)
                return -1073478649;
            if (input == ResultID.Cpx.E_FILTER_DUPLICATE)
                return -1073478648;
            if (input == ResultID.Cpx.E_FILTER_INVALID)
                return -1073478647;
            if (input == ResultID.Cpx.E_FILTER_ERROR)
                return -1073478646;
            if (input == ResultID.Cpx.S_FILTER_NO_DATA)
                return 263179;
        }
        else if (input.Name != null && input.Name.Namespace == "http://opcfoundation.org/HistoricalDataAccess/")
        {
            if (input == ResultID.Hda.E_MAXEXCEEDED)
                return -1073475583;
            if (input == ResultID.Hda.S_NODATA)
                return 1074008066;
            if (input == ResultID.Hda.S_MOREDATA)
                return 1074008067;
            if (input == ResultID.Hda.E_INVALIDAGGREGATE)
                return -1073475580;
            if (input == ResultID.Hda.S_CURRENTVALUE)
                return 1074008069;
            if (input == ResultID.Hda.S_EXTRADATA)
                return 1074008070;
            if (input == ResultID.Hda.E_UNKNOWNATTRID)
                return -1073475576;
            if (input == ResultID.Hda.E_NOT_AVAIL)
                return -1073475575;
            if (input == ResultID.Hda.E_INVALIDDATATYPE)
                return -1073475574;
            if (input == ResultID.Hda.E_DATAEXISTS)
                return -1073475573;
            if (input == ResultID.Hda.E_INVALIDATTRID)
                return -1073475572;
            if (input == ResultID.Hda.E_NODATAEXISTS)
                return -1073475571;
            if (input == ResultID.Hda.S_INSERTED)
                return 1074008078;
            if (input == ResultID.Hda.S_REPLACED)
                return 1074008079;
        }
        if (input.Name != null && input.Name.Namespace == "http://opcfoundation.org/DataExchange/")
        {
            if (input == ResultID.Dx.E_PERSISTING)
                return -1073477888;
            if (input == ResultID.Dx.E_NOITEMLIST)
                return -1073477887;
            if (input == ResultID.Dx.E_SERVER_STATE || input == ResultID.Dx.E_VERSION_MISMATCH)
                return -1073477885;
            if (input == ResultID.Dx.E_UNKNOWN_ITEM_PATH)
                return -1073477884;
            if (input == ResultID.Dx.E_UNKNOWN_ITEM_NAME)
                return -1073477883;
            if (input == ResultID.Dx.E_INVALID_ITEM_PATH)
                return -1073477882;
            if (input == ResultID.Dx.E_INVALID_ITEM_NAME)
                return -1073477881;
            if (input == ResultID.Dx.E_INVALID_NAME)
                return -1073477880;
            if (input == ResultID.Dx.E_DUPLICATE_NAME)
                return -1073477879;
            if (input == ResultID.Dx.E_INVALID_BROWSE_PATH)
                return -1073477878;
            if (input == ResultID.Dx.E_INVALID_SERVER_URL)
                return -1073477877;
            if (input == ResultID.Dx.E_INVALID_SERVER_TYPE)
                return -1073477876;
            if (input == ResultID.Dx.E_UNSUPPORTED_SERVER_TYPE)
                return -1073477875;
            if (input == ResultID.Dx.E_CONNECTIONS_EXIST)
                return -1073477874;
            if (input == ResultID.Dx.E_TOO_MANY_CONNECTIONS)
                return -1073477873;
            if (input == ResultID.Dx.E_OVERRIDE_BADTYPE)
                return -1073477872;
            if (input == ResultID.Dx.E_OVERRIDE_RANGE)
                return -1073477871;
            if (input == ResultID.Dx.E_SUBSTITUTE_BADTYPE)
                return -1073477870;
            if (input == ResultID.Dx.E_SUBSTITUTE_RANGE)
                return -1073477869;
            if (input == ResultID.Dx.E_INVALID_TARGET_ITEM)
                return -1073477868;
            if (input == ResultID.Dx.E_UNKNOWN_TARGET_ITEM)
                return -1073477867;
            if (input == ResultID.Dx.E_TARGET_ALREADY_CONNECTED)
                return -1073477866;
            if (input == ResultID.Dx.E_UNKNOWN_SERVER_NAME)
                return -1073477865;
            if (input == ResultID.Dx.E_UNKNOWN_SOURCE_ITEM)
                return -1073477864;
            if (input == ResultID.Dx.E_INVALID_SOURCE_ITEM)
                return -1073477863;
            if (input == ResultID.Dx.E_INVALID_QUEUE_SIZE)
                return -1073477862;
            if (input == ResultID.Dx.E_INVALID_DEADBAND)
                return -1073477861;
            if (input == ResultID.Dx.E_INVALID_CONFIG_FILE)
                return -1073477860;
            if (input == ResultID.Dx.E_PERSIST_FAILED)
                return -1073477859;
            if (input == ResultID.Dx.E_TARGET_FAULT)
                return -1073477858;
            if (input == ResultID.Dx.E_TARGET_NO_ACCESSS)
                return -1073477857;
            if (input == ResultID.Dx.E_SOURCE_SERVER_FAULT)
                return -1073477856;
            if (input == ResultID.Dx.E_SOURCE_SERVER_NO_ACCESSS)
                return -1073477855;
            if (input == ResultID.Dx.E_SUBSCRIPTION_FAULT)
                return -1073477854;
            if (input == ResultID.Dx.E_SOURCE_ITEM_BADRIGHTS)
                return -1073477853;
            if (input == ResultID.Dx.E_SOURCE_ITEM_BAD_QUALITY)
                return -1073477852;
            if (input == ResultID.Dx.E_SOURCE_ITEM_BADTYPE)
                return -1073477851;
            if (input == ResultID.Dx.E_SOURCE_ITEM_RANGE)
                return -1073477850;
            if (input == ResultID.Dx.E_SOURCE_SERVER_NOT_CONNECTED)
                return -1073477849;
            if (input == ResultID.Dx.E_SOURCE_SERVER_TIMEOUT)
                return -1073477848;
            if (input == ResultID.Dx.E_TARGET_ITEM_DISCONNECTED)
                return -1073477847;
            if (input == ResultID.Dx.E_TARGET_NO_WRITES_ATTEMPTED)
                return -1073477846;
            if (input == ResultID.Dx.E_TARGET_ITEM_BADTYPE)
                return -1073477845;
            if (input == ResultID.Dx.E_TARGET_ITEM_RANGE)
                return -1073477844;
            if (input == ResultID.Dx.S_TARGET_SUBSTITUTED)
                return 264064;
            if (input == ResultID.Dx.S_TARGET_OVERRIDEN)
                return 264065;
            if (input == ResultID.Dx.S_CLAMP)
                return 264066;
        }
        else if (input.Code == -1)
            return input.Succeeded() ? 1 : -2147467259;
        return input.Code;
    }

    internal static VarEnum GetType(System.Type input)
    {
        if (input == null)
            return VarEnum.VT_EMPTY;
        if (input == typeof(sbyte))
            return VarEnum.VT_I1;
        if (input == typeof(byte))
            return VarEnum.VT_UI1;
        if (input == typeof(short))
            return VarEnum.VT_I2;
        if (input == typeof(ushort))
            return VarEnum.VT_UI2;
        if (input == typeof(int))
            return VarEnum.VT_I4;
        if (input == typeof(uint))
            return VarEnum.VT_UI4;
        if (input == typeof(long))
            return VarEnum.VT_I8;
        if (input == typeof(ulong))
            return VarEnum.VT_UI8;
        if (input == typeof(float))
            return VarEnum.VT_R4;
        if (input == typeof(double))
            return VarEnum.VT_R8;
        if (input == typeof(Decimal))
            return VarEnum.VT_CY;
        if (input == typeof(bool))
            return VarEnum.VT_BOOL;
        if (input == typeof(DateTime))
            return VarEnum.VT_DATE;
        if (input == typeof(string))
            return VarEnum.VT_BSTR;
        if (input == typeof(object))
            return VarEnum.VT_EMPTY;
        if (input == typeof(sbyte[]))
            return VarEnum.VT_I1 | VarEnum.VT_ARRAY;
        if (input == typeof(byte[]))
            return VarEnum.VT_UI1 | VarEnum.VT_ARRAY;
        if (input == typeof(short[]))
            return VarEnum.VT_I2 | VarEnum.VT_ARRAY;
        if (input == typeof(ushort[]))
            return VarEnum.VT_UI2 | VarEnum.VT_ARRAY;
        if (input == typeof(int[]))
            return VarEnum.VT_I4 | VarEnum.VT_ARRAY;
        if (input == typeof(uint[]))
            return VarEnum.VT_UI4 | VarEnum.VT_ARRAY;
        if (input == typeof(long[]))
            return VarEnum.VT_I8 | VarEnum.VT_ARRAY;
        if (input == typeof(ulong[]))
            return VarEnum.VT_UI8 | VarEnum.VT_ARRAY;
        if (input == typeof(float[]))
            return VarEnum.VT_R4 | VarEnum.VT_ARRAY;
        if (input == typeof(double[]))
            return VarEnum.VT_R8 | VarEnum.VT_ARRAY;
        if (input == typeof(Decimal[]))
            return VarEnum.VT_CY | VarEnum.VT_ARRAY;
        if (input == typeof(bool[]))
            return VarEnum.VT_BOOL | VarEnum.VT_ARRAY;
        if (input == typeof(DateTime[]))
            return VarEnum.VT_DATE | VarEnum.VT_ARRAY;
        if (input == typeof(string[]))
            return VarEnum.VT_BSTR | VarEnum.VT_ARRAY;
        if (input == typeof(object[]))
            return VarEnum.VT_VARIANT | VarEnum.VT_ARRAY;
        if (input == Type.ILLEGAL_TYPE)
            return (VarEnum)System.Enum.ToObject(typeof(VarEnum), (int)short.MaxValue);
        if (input == typeof(System.Type) || input == typeof(Quality))
            return VarEnum.VT_I2;
        return input == typeof(accessRights) || input == typeof(euType) ? VarEnum.VT_I4 : VarEnum.VT_EMPTY;
    }

    internal static System.Type GetType(VarEnum input)
    {
        switch (input)
        {
            case VarEnum.VT_EMPTY:
                return null;

            case VarEnum.VT_I2:
                return typeof(short);

            case VarEnum.VT_I4:
                return typeof(int);

            case VarEnum.VT_R4:
                return typeof(float);

            case VarEnum.VT_R8:
                return typeof(double);

            case VarEnum.VT_CY:
                return typeof(Decimal);

            case VarEnum.VT_DATE:
                return typeof(DateTime);

            case VarEnum.VT_BSTR:
                return typeof(string);

            case VarEnum.VT_BOOL:
                return typeof(bool);

            case VarEnum.VT_I1:
                return typeof(sbyte);

            case VarEnum.VT_UI1:
                return typeof(byte);

            case VarEnum.VT_UI2:
                return typeof(ushort);

            case VarEnum.VT_UI4:
                return typeof(uint);

            case VarEnum.VT_I8:
                return typeof(long);

            case VarEnum.VT_UI8:
                return typeof(ulong);

            case VarEnum.VT_I2 | VarEnum.VT_ARRAY:
                return typeof(short[]);

            case VarEnum.VT_I4 | VarEnum.VT_ARRAY:
                return typeof(int[]);

            case VarEnum.VT_R4 | VarEnum.VT_ARRAY:
                return typeof(float[]);

            case VarEnum.VT_R8 | VarEnum.VT_ARRAY:
                return typeof(double[]);

            case VarEnum.VT_CY | VarEnum.VT_ARRAY:
                return typeof(Decimal[]);

            case VarEnum.VT_DATE | VarEnum.VT_ARRAY:
                return typeof(DateTime[]);

            case VarEnum.VT_BSTR | VarEnum.VT_ARRAY:
                return typeof(string[]);

            case VarEnum.VT_BOOL | VarEnum.VT_ARRAY:
                return typeof(bool[]);

            case VarEnum.VT_VARIANT | VarEnum.VT_ARRAY:
                return typeof(object[]);

            case VarEnum.VT_I1 | VarEnum.VT_ARRAY:
                return typeof(sbyte[]);

            case VarEnum.VT_UI1 | VarEnum.VT_ARRAY:
                return typeof(byte[]);

            case VarEnum.VT_UI2 | VarEnum.VT_ARRAY:
                return typeof(ushort[]);

            case VarEnum.VT_UI4 | VarEnum.VT_ARRAY:
                return typeof(uint[]);

            case VarEnum.VT_I8 | VarEnum.VT_ARRAY:
                return typeof(long[]);

            case VarEnum.VT_UI8 | VarEnum.VT_ARRAY:
                return typeof(ulong[]);

            default:
                return Type.ILLEGAL_TYPE;
        }
    }

    internal static object MarshalPropertyValue(PropertyID propertyID, object input)
    {
        if (input == null)
            return null;
        try
        {
            if (propertyID == Property.DATATYPE)
                return (short)Interop.GetType((System.Type)input);
            if (propertyID == Property.ACCESSRIGHTS)
            {
                switch ((accessRights)input)
                {
                    case accessRights.readable:
                        return 1;

                    case accessRights.writable:
                        return 2;

                    case accessRights.readWritable:
                        return 3;

                    default:
                        return null;
                }
            }
            else if (propertyID == Property.EUTYPE)
            {
                switch ((euType)input)
                {
                    case euType.noEnum:
                        return OPCEUTYPE.OPC_NOENUM;

                    case euType.analog:
                        return OPCEUTYPE.OPC_ANALOG;

                    case euType.enumerated:
                        return OPCEUTYPE.OPC_ENUMERATED;

                    default:
                        return null;
                }
            }
            else
            {
                if (propertyID == Property.QUALITY)
                    return ((Quality)input).GetCode();
                if (propertyID == Property.TIMESTAMP)
                {
                    if (input.GetType() == typeof(DateTime))
                    {
                        DateTime dateTime = (DateTime)input;
                        return dateTime != DateTime.MinValue ? dateTime.ToLocalTime() : (object)dateTime;
                    }
                }
            }
        }
        catch
        {
        }
        return input;
    }

    internal static object UnmarshalPropertyValue(PropertyID propertyID, object input)
    {
        if (input == null)
            return null;
        try
        {
            if (propertyID == Property.DATATYPE)
                return Interop.GetType((VarEnum)System.Convert.ToUInt16(input));
            if (propertyID == Property.ACCESSRIGHTS)
            {
                switch (System.Convert.ToInt32(input))
                {
                    case 1:
                        return accessRights.readable;

                    case 2:
                        return accessRights.writable;

                    case 3:
                        return accessRights.readWritable;

                    default:
                        return null;
                }
            }
            else if (propertyID == Property.EUTYPE)
            {
                switch ((OPCEUTYPE)input)
                {
                    case OPCEUTYPE.OPC_NOENUM:
                        return euType.noEnum;

                    case OPCEUTYPE.OPC_ANALOG:
                        return euType.analog;

                    case OPCEUTYPE.OPC_ENUMERATED:
                        return euType.enumerated;

                    default:
                        return null;
                }
            }
            else
            {
                if (propertyID == Property.QUALITY)
                    return new Quality(System.Convert.ToInt16(input));
                if (propertyID == Property.TIMESTAMP)
                {
                    if (input.GetType() == typeof(DateTime))
                    {
                        DateTime dateTime = (DateTime)input;
                        return dateTime != DateTime.MinValue ? dateTime.ToLocalTime() : (object)dateTime;
                    }
                }
            }
        }
        catch
        {
        }
        return input;
    }
}