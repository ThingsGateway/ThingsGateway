#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion


namespace ThingsGateway.Web.Entry;

/// <summary>
/// ����
/// </summary>
public class Program
{
    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);


#if KINGVIEW //��ȡ��̬�����ܺ�̨������������������һ�������������
        SevicesExtension.KINGVIEWCONFIG();
#endif

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseWebRoot("wwwroot");
        builder.WebHost.UseStaticWebAssets();
        //��Ҫ�����ػ��ɰ�װ
        builder.Host.UseWindowsService();
        builder.Host.UseSystemd();
        //Furion��������
        builder.Inject();
        var app = builder.Build();
        app.Run();
    }
}