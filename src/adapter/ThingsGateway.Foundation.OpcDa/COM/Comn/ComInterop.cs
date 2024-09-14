//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;

using ThingsGateway.Foundation.OpcDa.Rcw;

namespace ThingsGateway.Foundation.OpcDa.Comn;
#pragma warning disable CA1416 // 验证平台兼容性
#pragma warning disable IDE0090
#pragma warning disable IDE0051

internal class ComInterop
{
    private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

    #region const

    private const uint EOAC_ACCESS_CONTROL = 0x04;
    private const uint EOAC_APPID = 0x08;
    private const uint EOAC_CLOAKING = 0x10;
    private const uint EOAC_DYNAMIC_CLOAKING = 0x40;
    private const uint EOAC_MUTUAL_AUTH = 0x01;
    private const uint EOAC_NONE = 0x00;
    private const uint EOAC_SECURE_REFS = 0x02;
    private const uint EOAC_STATIC_CLOAKING = 0x20;

    /// <summary>
    /// The WIN32 system default locale.
    /// </summary>
    private const int LOCALE_SYSTEM_DEFAULT = 0x800;

    /// <summary>
    /// The WIN32 user default locale.
    /// </summary>
    private const int LOCALE_USER_DEFAULT = 0x400;

    private const uint RPC_C_AUTHN_DCE_PRIVATE = 1;
    private const uint RPC_C_AUTHN_DCE_PUBLIC = 2;
    private const uint RPC_C_AUTHN_DEC_PUBLIC = 4;
    private const uint RPC_C_AUTHN_DEFAULT = 0xFFFFFFFF;
    private const uint RPC_C_AUTHN_DIGEST = 21;
    private const uint RPC_C_AUTHN_DPA = 17;
    private const uint RPC_C_AUTHN_GSS_KERBEROS = 16;
    private const uint RPC_C_AUTHN_GSS_NEGOTIATE = 9;
    private const uint RPC_C_AUTHN_GSS_SCHANNEL = 14;
    private const uint RPC_C_AUTHN_LEVEL_CALL = 3;
    private const uint RPC_C_AUTHN_LEVEL_CONNECT = 2;
    private const uint RPC_C_AUTHN_LEVEL_DEFAULT = 0;
    private const uint RPC_C_AUTHN_LEVEL_NONE = 1;
    private const uint RPC_C_AUTHN_LEVEL_PKT = 4;
    private const uint RPC_C_AUTHN_LEVEL_PKT_INTEGRITY = 5;
    private const uint RPC_C_AUTHN_LEVEL_PKT_PRIVACY = 6;
    private const uint RPC_C_AUTHN_MQ = 100;
    private const uint RPC_C_AUTHN_MSN = 18;
    private const uint RPC_C_AUTHN_NONE = 0;
    private const uint RPC_C_AUTHN_WINNT = 10;
    private const uint RPC_C_AUTHZ_DCE = 2;
    private const uint RPC_C_AUTHZ_DEFAULT = 0xffffffff;
    private const uint RPC_C_AUTHZ_NAME = 1;
    private const uint RPC_C_AUTHZ_NONE = 0;
    private const uint RPC_C_IMP_LEVEL_ANONYMOUS = 1;
    private const uint RPC_C_IMP_LEVEL_DELEGATE = 4;
    private const uint RPC_C_IMP_LEVEL_IDENTIFY = 2;
    private const uint RPC_C_IMP_LEVEL_IMPERSONATE = 3;

    #endregion const

    #region struct

    public struct COSERVERINFO
    {
        public uint dwReserved1;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszName;

        public IntPtr pAuthInfo;
        public uint dwReserved2;
    };

    public struct MULTI_QI
    {
        public IntPtr iid;

        [MarshalAs(UnmanagedType.IUnknown)]
        public object pItf;

        public uint hr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SOLE_AUTHENTICATION_SERVICE
    {
        public uint dwAuthnSvc;
        public uint dwAuthzSvc;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pPrincipalName;

        public int hr;
    }

    #endregion struct

    #region win32 api

    [DllImport("ole32.dll")]
    private static extern void CoCreateInstanceEx(ref Guid clsid,
                                        [MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
                                        uint dwClsCtx,
                                        [In] ref COSERVERINFO pServerInfo,
                                        uint dwCount,
                                        [In, Out] MULTI_QI[] pResults);

    [DllImport("ole32.dll")]
    private static extern int CoInitializeSecurity(
                                                IntPtr pSecDesc,
                                                int cAuthSvc,
                                                SOLE_AUTHENTICATION_SERVICE[] asAuthSvc,
                                                IntPtr pReserved1,
                                                uint dwAuthnLevel,
                                                uint dwImpLevel,
                                                IntPtr pAuthList,
                                                uint dwCapabilities,
                                                IntPtr pReserved3);

    [DllImport("Kernel32.dll")]
    private static extern int FormatMessageW(
                                                int dwFlags,
                                                IntPtr lpSource,
                                                int dwMessageId,
                                                int dwLanguageId,
                                                IntPtr lpBuffer,
                                                int nSize,
                                                IntPtr Arguments);

    [DllImport("Kernel32.dll")]
    private static extern int GetSystemDefaultLangID();

    [DllImport("Kernel32.dll")]
    private static extern int GetUserDefaultLangID();

    #endregion win32 api

    /// <summary>
    /// 创建一个COM服务器的实例。
    /// </summary>
    public static object CreateInstance(Guid clsid, string hostName)
    {
        COSERVERINFO coserverInfo = new()
        {
            pwszName = hostName,
            pAuthInfo = IntPtr.Zero,
            dwReserved1 = 0,
            dwReserved2 = 0
        };

        GCHandle hIID = GCHandle.Alloc(IID_IUnknown, GCHandleType.Pinned);

        MULTI_QI[] results = new MULTI_QI[1];

        results[0].iid = hIID.AddrOfPinnedObject();
        results[0].pItf = null;
        results[0].hr = 0;

        try
        {
            // 检查是否在本地或远程连接。
            uint clsctx = 0x01 | 0x04;

            if (hostName != null && hostName.Length > 0 && hostName.ToLower() != "localhost" && hostName != "127.0.0.1")
            {
                clsctx = 0x04 | 0x10;
            }

            // create an instance.
            CoCreateInstanceEx(
                ref clsid,
                null,
                clsctx,
                ref coserverInfo,
                1,
                results);
        }
        catch (Exception ex)
        {
            throw new ExternalException("CoCreateInstanceEx: " + ex.Message);
        }
        finally
        {
            if (hIID.IsAllocated) hIID.Free();
        }

        if (results[0].hr != 0)
        {
            throw new ExternalException("CoCreateInstanceEx: " + GetSystemMessage((int)results[0].hr));
        }
        return results[0].pItf;
    }

    /// <summary>
    /// 指定错误消息文本检索系统。
    /// </summary>
    internal static string GetSystemMessage(int error)
    {
        const int MAX_MESSAGE_LENGTH = 1024;
        const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        IntPtr buffer = Marshal.AllocCoTaskMem(MAX_MESSAGE_LENGTH);

        _ = FormatMessageW(
             (int)(FORMAT_MESSAGE_IGNORE_INSERTS | FORMAT_MESSAGE_FROM_SYSTEM),
             IntPtr.Zero,
             error,
             0,
             buffer,
             MAX_MESSAGE_LENGTH - 1,
             IntPtr.Zero);

        string msg = Marshal.PtrToStringUni(buffer);
        Marshal.FreeCoTaskMem(buffer);

        if (!string.IsNullOrEmpty(msg))
        {
            return msg;
        }
        return string.Format("0x{0,0:X}", error);
    }

    /// <summary>
    /// 初始化COM安全。
    /// </summary>
    internal static void InitializeSecurity()
    {
        int error = CoInitializeSecurity(
            IntPtr.Zero,
            -1,
            null,
            IntPtr.Zero,
            RPC_C_AUTHN_LEVEL_CONNECT,
            RPC_C_IMP_LEVEL_IMPERSONATE,
            IntPtr.Zero,
            EOAC_NONE,
            IntPtr.Zero);

        if (error != 0)
        {
            throw new ExternalException("InitializeSecurity fail: " + GetSystemMessage(error), error);
        }
    }

    /// <summary>
    /// 从枚举器读取guid。
    /// </summary>
    internal static Guid[] ReadClasses(IOPCEnumGUID enumerator)
    {
        List<Guid> guids = new List<Guid>();

        int fetched = 0;
        Guid[] buffer = new Guid[10];
        do
        {
            try
            {
                IntPtr pGuids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Guid)) * buffer.Length);

                try
                {
                    enumerator.Next(buffer.Length, pGuids, out fetched);

                    if (fetched > 0)
                    {
                        IntPtr pos = pGuids;

                        for (int ii = 0; ii < fetched; ii++)
                        {
                            object o = Marshal.PtrToStructure(pos, typeof(Guid));
                            if (o != null)
                            {
                                buffer[ii] = (Guid)o;
                            }
                            pos = IntPtr.Add(pos, Marshal.SizeOf(typeof(Guid)));
                            guids.Add(buffer[ii]);
                        }
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pGuids);
                }
            }
            catch (Exception)
            {
                break;
            }
        }
        while (fetched > 0);

        return guids.ToArray();
    }

    /// <summary>
    /// 从枚举器读取guid。
    /// </summary>
    internal static Guid[] ReadClasses(IEnumGUID enumerator)
    {
        List<Guid> guids = new List<Guid>();

        int fetched = 0;
        Guid[] buffer = new Guid[10];

        do
        {
            try
            {
                IntPtr pGuids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Guid)) * buffer.Length);

                try
                {
                    enumerator.Next(buffer.Length, pGuids, out fetched);

                    if (fetched > 0)
                    {
                        IntPtr pos = pGuids;

                        for (int ii = 0; ii < fetched; ii++)
                        {
                            object o = Marshal.PtrToStructure(pos, typeof(Guid));
                            if (o != null)
                            {
                                buffer[ii] = (Guid)o;
                            }
                            pos = IntPtr.Add(pos, Marshal.SizeOf(typeof(Guid)));
                            guids.Add(buffer[ii]);
                        }
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pGuids);
                }
            }
            catch (Exception)
            {
                break;
            }
        }
        while (fetched > 0);

        return guids.ToArray();
    }

    /// <summary>
    /// 释放 COM 对象
    /// </summary>
    /// <param name="m_server"></param>
    internal static void RealseComServer(object m_server)
    {
        if (m_server != null && m_server.GetType().IsCOMObject)
        {
            Marshal.ReleaseComObject(m_server);
        }
    }
}
