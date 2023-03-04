# 脚本来自furion

Write-Warning "正在发布 Nuget 包......";

# 查找所有 *.nupkg 文件
$template_nupkgs = Get-ChildItem -Filter *.nupkg;

# 遍历所有 *.nupkg 文件
for ($i = 0; $i -le $template_nupkgs.Length-1; $i++){

    $item = $template_nupkgs[$i];

    $nupkg = $item.FullName;

    Write-Output "-----------------";
    $nupkg;

    # nuget setApiKey <apikey> 使用默认apikey
    # 发布到 nuget.org 平台
    dotnet nuget push $nupkg --skip-duplicate  --source https://api.nuget.org/v3/index.json;
    Write-Output "-----------------";
}

Write-Warning "发布成功";