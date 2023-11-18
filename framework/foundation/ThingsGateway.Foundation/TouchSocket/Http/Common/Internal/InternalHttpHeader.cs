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

namespace ThingsGateway.Foundation.Http
{
    internal class InternalHttpHeader : Dictionary<string, string>, IHttpHeader
    {
        public InternalHttpHeader() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public new string this[string key]
        {
            get
            {
                if (key == null)
                {
                    return null;
                }
                if (this.TryGetValue(key, out var value))
                {
                    return value;
                }
                return null;
            }

            set
            {
                if (key == null)
                {
                    return;
                }

                this.AddOrUpdate(key, value);
            }
        }

        public string this[HttpHeaders headers]
        {
            get
            {
                if (this.TryGetValue(headers.GetDescription(), out var value))
                {
                    return value;
                }
                return null;
            }

            set
            {
                this.AddOrUpdate(headers.GetDescription(), value);
            }
        }

        public new void Add(string key, string value)
        {
            if (key == null)
            {
                return;
            }
            this.AddOrUpdate(key, value);
        }

        public void Add(HttpHeaders key, string value)
        {
            this.AddOrUpdate(key.GetDescription(), value);
        }

        public string Get(string key)
        {
            if (key == null)
            {
                return null;
            }
            if (this.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        public string Get(HttpHeaders key)
        {
            if (this.TryGetValue(key.GetDescription(), out var value))
            {
                return value;
            }
            return null;
        }
    }
}