namespace ThingsGateway.Upgrade;

public class FileConst
{
    public const string FilePathKey = "Path";
    public static string UpgradePath = Path.Combine(AppContext.BaseDirectory, "Upgrade.zip");
    public static string BackupPath = Path.Combine(AppContext.BaseDirectory, "..", "Backup.zip");
    public static string BackupDirPath = Path.Combine(AppContext.BaseDirectory, "..", "Backup");
    public const string ServerDir = "UpdateZipFile";
}
