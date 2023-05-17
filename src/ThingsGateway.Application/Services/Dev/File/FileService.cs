﻿using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Application
{
    /// <summary>
    /// <inheritdoc cref="IFileService"/>
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IConfigService _configService;

        /// <inheritdoc cref="IFileService"/>
        public FileService(IConfigService configService)
        {
            this._configService = configService;
        }


        /// <inheritdoc/>
        public void ImportVerification(IBrowserFile file, int maxSzie = 300, string[] allowTypes = null)
        {

            if (file == null) throw Oops.Bah("文件不能为空");
            if (file.Size > maxSzie * 1024 * 1024) throw Oops.Bah($"文件大小不允许超过{maxSzie}M");
            var fileSuffix = Path.GetExtension(file.Name).ToLower().Split(".")[1]; // 文件后缀
            string[] allowTypeS = allowTypes == null ? new string[] { "xlsx" } : allowTypes;//允许上传的文件类型
            if (!allowTypeS.Contains(fileSuffix)) throw Oops.Bah(errorMessage: "文件格式错误");

        }


        #region 方法

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="fileSizeKb"></param>
        /// <returns></returns>
        private string GetSizeInfo(long fileSizeKb)
        {

            var b = fileSizeKb * 1024;
            const int MB = 1024 * 1024;
            const int KB = 1024;
            if (b / MB >= 1)
            {
                return Math.Round(b / (float)MB, 2) + "MB";
            }

            if (b / KB >= 1)
            {
                return Math.Round(b / (float)KB, 2) + "KB";
            }
            if (b == 0)
            {
                return "0B";
            }
            return null;
        }

        /// <summary>
        /// 判断是否是图片
        /// </summary>
        /// <param name="suffix">后缀名</param>
        /// <returns></returns>
        private bool IsPic(string suffix)
        {
            //图片后缀名列表
            var pics = new string[]
            {
                ".png", ".bmp", ".gif", ".jpg", ".jpeg",".psd"
            };
            if (pics.Contains(suffix))
                return true;
            else
                return false;
        }

        #endregion
    }
}
