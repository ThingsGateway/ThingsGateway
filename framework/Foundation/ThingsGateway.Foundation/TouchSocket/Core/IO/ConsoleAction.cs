#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 控制台行为
    /// </summary>

    internal struct VAction
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="description"></param>
        /// <param name="fullOrder"></param>
        public VAction(string description, string fullOrder, Action action)
        {
            this.FullOrder = fullOrder;
            this.Action = action ?? throw new ArgumentNullException(nameof(action));
            this.Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public Action Action { get; }

        public string Description { get; }
        public string FullOrder { get; }
    }

    /// <summary>
    /// 控制台行为
    /// </summary>
    public class ConsoleAction
    {
        private readonly Dictionary<string, VAction> m_actions = new Dictionary<string, VAction>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="helpOrder">帮助信息指令，如："h|help|?"</param>
        public ConsoleAction(string helpOrder = "h|help|?")
        {
            this.HelpOrder = helpOrder;

            this.Add(helpOrder, "帮助信息", this.ShowAll);

            var title = $@"

  _______                   _       _____               _          _
 |__   __|                 | |     / ____|             | |        | |
    | |  ___   _   _   ___ | |__  | (___    ___    ___ | | __ ___ | |_
    | | / _ \ | | | | / __|| '_ \  \___ \  / _ \  / __|| |/ // _ \| __|
    | || (_) || |_| || (__ | | | | ____) || (_) || (__ |   <|  __/| |_
    |_| \___/  \__,_| \___||_| |_||_____/  \___/  \___||_|\_\\___| \__|

 -------------------------------------------------------------------
     Author     :   若汝棋茗
     Version    :   {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}
     Gitee      :   https://gitee.com/rrqm_home
     Github     :   https://github.com/rrqm
     API        :   http://rrqm_home.gitee.io/touchsocket/
 -------------------------------------------------------------------
";
            Console.WriteLine(title);
        }

        /// <summary>
        /// 执行异常
        /// </summary>
        public event Action<Exception> OnException;

        /// <summary>
        /// 帮助信息指令
        /// </summary>
        public string HelpOrder { get; private set; }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="order">指令，多个指令用“|”分割</param>
        /// <param name="description">描述</param>
        /// <param name="action"></param>
        public void Add(string order, string description, Action action)
        {
            var orders = order.ToLower().Split('|');
            foreach (var item in orders)
            {
                this.m_actions.Add(item, new VAction(description, order, action));
            }
        }

        /// <summary>
        /// 运行
        /// </summary>
        public void RunCommandLine()
        {
            while (true)
            {
                var str = Console.ReadLine();
                if (!this.Run(str))
                {
                    Console.WriteLine($"没有这个指令。");
                }
            }
        }

        /// <summary>
        /// 执行，返回值仅表示是否有这个指令，异常获取请使用<see cref="OnException"/>
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool Run(string order)
        {
            if (this.m_actions.TryGetValue(order.ToLower(), out var vAction))
            {
                try
                {
                    vAction.Action.Invoke();
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(ex);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 显示所有注册指令
        /// </summary>
        public void ShowAll()
        {
            var max = this.m_actions.Values.Max(a => a.FullOrder.Length) + 8;

            var s = new List<string>();
            foreach (var item in this.m_actions)
            {
                if (!s.Contains(item.Value.FullOrder.ToLower()))
                {
                    s.Add(item.Value.FullOrder.ToLower());
                    Console.Write($"[{item.Value.FullOrder}]");
                    for (var i = 0; i < max - item.Value.FullOrder.Length; i++)
                    {
                        Console.Write("-");
                    }
                    Console.WriteLine(item.Value.Description);
                }
            }
        }
    }
}