//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.

Runtime.InteropServices;

namespace ThingsGateway.Foundation.OpcDa.Rcw;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

/// <exclude />
[ComImport]
[GuidAttribute("B196B286-BAB4-101A-B69C-00AA00341D07")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IConnectionPoint
{
    void GetConnectionInterface(
        [Out]
        out Guid pIID);

    void GetConnectionPointContainer(
        [Out]
        out IConnectionPointContainer ppCPC);

    void Advise(
        [MarshalAs(UnmanagedType.IUnknown)]
        object pUnkSink,
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pdwCookie);

    void Unadvise(
        [MarshalAs(UnmanagedType.I4)]
        int dwCookie);

    void EnumConnections(
        [Out]
        out IEnumConnections ppEnum);
}

/// <exclude />
[ComImport]
[GuidAttribute("B196B284-BAB4-101A-B69C-00AA00341D07")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IConnectionPointContainer
{
    void EnumConnectionPoints(
        [Out]
        out IEnumConnectionPoints ppEnum);

    void FindConnectionPoint(
        ref Guid riid,
        [Out]
        out IConnectionPoint ppCP);
}

/// <exclude />
[ComImport]
[GuidAttribute("B196B285-BAB4-101A-B69C-00AA00341D07")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IEnumConnectionPoints
{
    void RemoteNext(
        [MarshalAs(UnmanagedType.I4)]
        int cConnections,
        [Out]
        IntPtr ppCP,
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pcFetched);

    void Skip(
        [MarshalAs(UnmanagedType.I4)]
        int cConnections);

    void Reset();

    void Clone(
        [Out]
        out IEnumConnectionPoints ppEnum);
}

/// <exclude />
[ComImport]
[GuidAttribute("B196B287-BAB4-101A-B69C-00AA00341D07")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IEnumConnections
{
    void RemoteNext(
        [MarshalAs(UnmanagedType.I4)]
        int cConnections,
        [Out]
        IntPtr rgcd,
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pcFetched);

    void Skip(
        [MarshalAs(UnmanagedType.I4)]
        int cConnections);

    void Reset();

    void Clone(
        [Out]
        out IEnumConnections ppEnum);
}

/// <exclude />
[ComImport]
[GuidAttribute("0002E000-0000-0000-C000-000000000046")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IEnumGUID
{
    void Next(
        [MarshalAs(UnmanagedType.I4)]
        int celt,
        [Out]
        IntPtr rgelt,
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pceltFetched);

    void Skip(
        [MarshalAs(UnmanagedType.I4)]
        int celt);

    void Reset();

    void Clone(
        [Out]
        out IEnumGUID ppenum);
}

/// <exclude />
[ComImport]
[GuidAttribute("00000101-0000-0000-C000-000000000046")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IEnumString
{
    [PreserveSig]
    int RemoteNext(
        [MarshalAs(UnmanagedType.I4)]
        int celt,
        IntPtr rgelt,
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pceltFetched);

    void Skip(
        [MarshalAs(UnmanagedType.I4)]
        int celt);

    void Reset();

    void Clone(
        [Out]
        out IEnumString ppenum);
}

/// <exclude />
[ComImport]
[GuidAttribute("00000100-0000-0000-C000-000000000046")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IEnumUnknown
{
    void RemoteNext(
        [MarshalAs(UnmanagedType.I4)]
        int celt,
        [Out]
        IntPtr rgelt,
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pceltFetched);

    void Skip(
        [MarshalAs(UnmanagedType.I4)]
        int celt);

    void Reset();

    void Clone(
        [Out]
        out IEnumUnknown ppenum);
}

/// <exclude />
[ComImport]
[GuidAttribute("F31DFDE2-07B6-11d2-B2D8-0060083BA1FB")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IOPCCommon
{
    void SetLocaleID(
        [MarshalAs(UnmanagedType.I4)]
        int dwLcid);

    void GetLocaleID(
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pdwLcid);

    void QueryAvailableLocaleIDs(
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pdwCount,
        [Out]
        out IntPtr pdwLcid);

    void GetErrorString(
        [MarshalAs(UnmanagedType.I4)]
        int dwError,
        [Out][MarshalAs(UnmanagedType.LPWStr)]
        out String ppString);

    void SetClientName(
        [MarshalAs(UnmanagedType.LPWStr)]
        String szName);
}

/// <exclude />
[ComImport]
[GuidAttribute("55C382C8-21C7-4e88-96C1-BECFB1E3F483")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IOPCEnumGUID
{
    void Next(
        [MarshalAs(UnmanagedType.I4)]
        int celt,
        [Out]
        IntPtr rgelt,
        [Out][MarshalAs(UnmanagedType.I4)]
        out int pceltFetched);

    void Skip(
        [MarshalAs(UnmanagedType.I4)]
        int celt);

    void Reset();

    void Clone(
        [Out]
        out IOPCEnumGUID ppenum);
}

/// <exclude />
[ComImport]
[GuidAttribute("13486D50-4821-11D2-A494-3CB306C10000")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IOPCServerList
{
    void EnumClassesOfCategories(
        [MarshalAs(UnmanagedType.I4)]
        int cImplemented,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStruct, SizeParamIndex=0)]
        Guid[] rgcatidImpl,
        [MarshalAs(UnmanagedType.I4)]
        int cRequired,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStruct, SizeParamIndex=2)]
        Guid[] rgcatidReq,
        [Out][MarshalAs(UnmanagedType.IUnknown)]
        out object ppenumClsid);

    void GetClassDetails(
        ref Guid clsid,
        [Out][MarshalAs(UnmanagedType.LPWStr)]
        out string ppszProgID,
        [Out][MarshalAs(UnmanagedType.LPWStr)]
        out string ppszUserType,
        [Out][MarshalAs(UnmanagedType.LPWStr)]
        out string ppszVerIndProgID);

    void CLSIDFromProgID(
        [MarshalAs(UnmanagedType.LPWStr)]
        string szProgId,
        [Out]
        out Guid clsid);
}

/// <exclude />
[ComImport]
[GuidAttribute("9DD0B56C-AD9E-43ee-8305-487F3188BF7A")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IOPCServerList2
{
    void EnumClassesOfCategories(
        [MarshalAs(UnmanagedType.I4)]
        int cImplemented,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStruct, SizeParamIndex=0)]
        Guid[] rgcatidImpl,
        [MarshalAs(UnmanagedType.I4)]
        int cRequired,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStruct, SizeParamIndex=0)]
        Guid[] rgcatidReq,
        [Out]
        out IOPCEnumGUID ppenumClsid);

    void GetClassDetails(
        ref Guid clsid,
        [Out][MarshalAs(UnmanagedType.LPWStr)]
        out string ppszProgID,
        [Out][MarshalAs(UnmanagedType.LPWStr)]
        out string ppszUserType,
        [Out][MarshalAs(UnmanagedType.LPWStr)]
        out string ppszVerIndProgID);

    void CLSIDFromProgID(
        [MarshalAs(UnmanagedType.LPWStr)]
        string szProgId,
        [Out]
        out Guid clsid);
}

/// <exclude />
[ComImport]
[GuidAttribute("F31DFDE1-07B6-11d2-B2D8-0060083BA1FB")]
[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
public interface IOPCShutdown
{
    void ShutdownRequest(
        [MarshalAs(UnmanagedType.LPWStr)]
        string szReason);
}

/// <exclude />
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct CONNECTDATA
{
    [MarshalAs(UnmanagedType.IUnknown)]
    private object pUnk;

    [MarshalAs(UnmanagedType.I4)]
    private int dwCookie;
}