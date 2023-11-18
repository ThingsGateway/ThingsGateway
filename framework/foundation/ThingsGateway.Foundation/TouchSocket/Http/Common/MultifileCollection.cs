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
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// 多文件集合
    /// </summary>
    public class MultifileCollection : IEnumerable<IFormFile>
    {
        private readonly HttpRequest m_request;

        /// <summary>
        /// 多文件集合
        /// </summary>
        /// <param name="request"></param>
        public MultifileCollection(HttpRequest request)
        {
            this.m_request = request;
        }

        /// <summary>
        /// 获取一个迭代器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IFormFile> GetEnumerator()
        {
            if (this.m_request.ContentComplated == null || this.m_request.ContentComplated == true)
            {
                if (this.m_request.TryGetContent(out var context))
                {
                    var boundary = $"--{this.m_request.GetBoundary()}".ToUTF8Bytes();
                    var indexs = context.IndexOfInclude(0, context.Length, boundary);
                    if (indexs.Count <= 0)
                    {
                        throw new Exception("没有发现由Boundary包裹的数据。");
                    }
                    var files = new List<IFormFile>();
                    for (var i = 0; i < indexs.Count; i++)
                    {
                        if (i + 1 < indexs.Count)
                        {
                            var internalFormFile = new InternalFormFile();
                            files.Add(internalFormFile);
                            var index = context.IndexOfFirst(indexs[i] + 3, indexs[i + 1], Encoding.UTF8.GetBytes("\r\n\r\n"));
                            var line = Encoding.UTF8.GetString(context, indexs[i] + 3, index - indexs[i] - 6);
                            var lines = line.Split(new string[] { ";", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            internalFormFile.DataPair = new NameValueCollection();
                            foreach (var item in lines)
                            {
                                var kv = item.Split(new char[] { ':', '=' });
                                if (kv.Length == 2)
                                {
                                    internalFormFile.DataPair.Add(kv[0].Trim(), kv[1].Replace("\"", String.Empty).Trim());
                                }
                            }

                            var length = indexs[i + 1] - (index + 2) - boundary.Length;
                            var data = new byte[length];
                            Array.Copy(context, index + 1, data, 0, length);
                            //string ssss = Encoding.UTF8.GetString(data);
                            internalFormFile.Data = data;
                        }
                    }

                    return files.GetEnumerator();
                }
                throw new Exception("管道状态异常");
            }
            else
            {
                throw new Exception("管道状态异常");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}