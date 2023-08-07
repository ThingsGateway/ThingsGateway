#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// ����/ͣ�� �ı���ʾ
/// </summary>
public partial class EnableChip
{
    /// <summary>
    /// Class
    /// </summary>
    [Parameter]
    public string Class { get; set; } = "";
    /// <summary>
    /// Style
    /// </summary>
    [Parameter]
    public string Style { get; set; } = "";
    /// <summary>
    /// Value
    /// </summary>
    [Parameter]
    public bool Value { get; set; }
    /// <summary>
    /// DisabledLabel
    /// </summary>
    [Parameter]
    public string DisabledLabel { get; set; }
    /// <summary>
    /// EnabledLabel
    /// </summary>
    [Parameter]
    public string EnabledLabel { get; set; }

    private string TextColor => Value ? "green" : "error";
    private string Color => Value ? "green-lighten" : "warning-lighten";
    private string Label => Value ? EnabledLabel ?? "����" : DisabledLabel ?? "ͣ��";
}