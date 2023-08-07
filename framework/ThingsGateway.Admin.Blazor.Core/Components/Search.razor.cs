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

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// Search
/// </summary>
public partial class Search
{
    private string _value;
    private string Value
    {
        get => _value;
        set
        {
            _value = value;
            if (!string.IsNullOrEmpty(value))
            {
                NavigationManager.NavigateTo(value);
            }
        }
    }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private List<SysResource> AvalidMenus;
    [Inject]
    private UserResoures UserResoures { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        AvalidMenus = UserResoures.SameLevelMenus.Where(it => it.Component != null).ToList();
        base.OnParametersSet();
    }
}