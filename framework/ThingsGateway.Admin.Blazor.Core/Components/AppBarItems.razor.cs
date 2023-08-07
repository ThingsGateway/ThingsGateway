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
/// AppBarItems
/// </summary>
public partial class AppBarItems
{
    /// <summary>
    /// ����
    /// </summary>
    [Parameter]
    public string CONFIG_COPYRIGHT_URL { get; set; }

    /// <summary>
    /// ��Ȩ
    /// </summary>
    [Parameter]
    public string CONFIG_COPYRIGHT { get; set; }

    /// <summary>
    /// ����
    /// </summary>
    [Parameter]
    public string CONFIG_TITLE { get; set; }
}