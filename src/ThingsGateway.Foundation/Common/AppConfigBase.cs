//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace ThingsGateway.Foundation;

/// <summary>
/// 运行配置类
/// </summary>
public abstract class AppConfigBase
{
    private static readonly Dictionary<Type, object> m_list = new Dictionary<Type, object>();
    private readonly string m_fullPath;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fullPath"></param>
    public AppConfigBase(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            throw new ArgumentException(nameof(fullPath));
        }

        this.m_fullPath = fullPath;
    }

    /// <summary>
    /// 获取默认配置。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetDefault<T>() where T : AppConfigBase, new()
    {
        var type = typeof(T);
        if (m_list.TryGetValue(type, out var value))
        {
            return (T)value;
        }
        var _default = ((T)Activator.CreateInstance(typeof(T)));
        _default.Load(out _);
        m_list.Add(type, _default);
        return _default;
    }

    /// <summary>
    /// 获取默认配置，每次调用该方法时，都会重新加载配置。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetNewDefault<T>() where T : AppConfigBase, new()
    {
        var _default = ((T)Activator.CreateInstance(typeof(T)));
        _default.Load(out _);
        if (m_list.ContainsKey(_default.GetType()))
        {
            m_list[_default.GetType()] = _default;
        }
        else
        {
            m_list.Add(_default.GetType(), _default);
        }
        return _default;
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool Load(out string msg)
    {
        try
        {
            if (!File.Exists(this.m_fullPath))
            {
                this.Save(false, out _);
            }
            var obj = SerializeConvert.FromJsonString(File.ReadAllText(this.m_fullPath), this.GetType());
            var ps = this.GetType().GetProperties();

            foreach (var item in ps)
            {
                item.SetValue(this, item.GetValue(obj));
            }
            msg = null;
            return true;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="overwrite"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool Save(bool overwrite, out string msg)
    {
        if (overwrite == false && File.Exists(this.m_fullPath))
        {
            msg = null;
            return true;
        }
        try
        {
            File.WriteAllText(this.m_fullPath, SerializeConvert.ToJsonString(this));
            msg = null;
            return true;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            return false;
        }
    }
}
