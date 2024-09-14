//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;

using ThingsGateway.NewLife.X.Reflection;

namespace ThingsGateway.NewLife.X.Extension;

internal class SpeakProvider
{
    private static readonly String typeName = "System.Speech.Synthesis.SpeechSynthesizer";
    private Type? _type;

    private Object? synth;

    public SpeakProvider()
    {
        try
        {
            //_type = typeName.GetTypeEx(true);
            _type = Type.GetType(typeName);
            if (_type == null)
            {
                Assembly? asm = null;
                try
                {
                    // 新版系统内置
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        asm ??= Assembly.Load("System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                    }
                }
                catch { }
                try
                {
                    asm ??= Assembly.Load("System.Speech");
                }
                catch { }
                _type = asm?.GetType(typeName);
            }
        }
        catch
        {
        }

        if (_type == null) throw new("找不到语音库System.Speech，需要从nuget引用");
    }

    public void Speak(String value)
    {
        if (_type == null) return;

        EnsureSynth();
        synth?.Invoke("Speak", value);
    }

    public void SpeakAsync(String value)
    {
        if (_type == null) return;

        EnsureSynth();
        synth?.Invoke("SpeakAsync", value);
    }

    /// <summary>
    /// 停止话音播报
    /// </summary>
    public void SpeakAsyncCancelAll()
    {
        if (_type == null) return;

        EnsureSynth();
        synth?.Invoke("SpeakAsyncCancelAll");
    }

    private void EnsureSynth()
    {
        if (synth == null && _type != null)
        {
            try
            {
                synth = _type.CreateInstance([]);
                synth?.Invoke("SetOutputToDefaultAudioDevice", []);
            }
            catch
            {
                _type = null;
            }
        }
    }
}
