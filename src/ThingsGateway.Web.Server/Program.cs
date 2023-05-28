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


namespace ThingsGateway.Web.Entry
{
    /// <summary>
    /// ����
    /// </summary>
    public class Program
    {
        /// <summary>
        /// ��
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
#if KINGVIEW //��ȡ��̬�����ܺ�̨������������������һ�������������
            #region Windows�� ����������,����ͬ������ʱֱ���˳�
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                if (WindowsControl.RunningInstance(typeof(Program).Assembly.FullName).Count > 0)
                {
                    return;
                }
            }
            #endregion

            #region Windows�� ������ʾ�ı�ģʽ,����
            //SW_HIDE�����ش��ڲ������������ڡ�nCmdShow = 0��
            //SW_SHOWMINIMIZED������ڲ�������С����nCmdShow = 2��
            //SW_MAXIMIZE�����ָ���Ĵ��ڡ�nCmdShow = 3��
            //SW_SHOWMAXIMIZED������ڲ�������󻯡�nCmdShow = 3��
            //SW_SHOWNOACTIVATE���Դ������һ�εĴ�С��״̬��ʾ���ڡ��������Ȼά�ּ���״̬��nCmdShow = 4��
            //SW_SHOW���ڴ���ԭ����λ����ԭ���ĳߴ缤�����ʾ���ڡ�nCmdShow = 5��
            //SW_MINIMIZE����С��ָ���Ĵ��ڲ��Ҽ�����Z���е���һ�����㴰�ڡ�nCmdShow = 6��
            //SW_SHOWMINNOACTIVE��������С�����������Ȼά�ּ���״̬��nCmdShow = 7��
            //SW_SHOWNA���Դ���ԭ����״̬��ʾ���ڡ��������Ȼά�ּ���״̬��nCmdShow = 8��
            //SW_RESTORE�������ʾ���ڡ����������С������󻯣���ϵͳ�����ڻָ���ԭ���ĳߴ��λ�á��ڻָ���С������ʱ��Ӧ�ó���Ӧ��ָ�������־��nCmdShow = 9��
            //SW_SHOWDEFAULT��������STARTUPINFO�ṹ��ָ����SW_FLAG��־�趨��ʾ״̬��STARTUPINFO �ṹ��������Ӧ�ó���ĳ��򴫵ݸ�CreateProcess�����ġ�nCmdShow = 10��
            //SW_SHOWNORMAL�������ʾһ�����ڡ�������ڱ���С������󻯣�ϵͳ����ָ���ԭ���ĳߴ�ʹ�С��Ӧ�ó����ڵ�һ����ʾ���ڵ�ʱ��Ӧ��ָ���˱�־��nCmdShow = 1��
            //SW_FORCEMINIMIZE����WindowNT5.0����С�����ڣ���ʹӵ�д��ڵ��̱߳�����Ҳ����С�����ڴ������߳���С������ʱ��ʹ�����������nCmdShow = 11��
            //������ʾ�ı�ģʽ
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Console.Title=typeof(Program).Assembly.FullName;
                WindowsControl.LocalBringToFront(Console.Title, 0);
            }

            #endregion
#endif


            System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseWebRoot("wwwroot");
            builder.WebHost.UseStaticWebAssets();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //��Ҫ�����ػ��ɰ�װ
            builder.Host.UseWindowsService();
            builder.Host.UseSystemd();

            builder.Inject();
            var app = builder.Build();
            app.Run();

        }
    }
}