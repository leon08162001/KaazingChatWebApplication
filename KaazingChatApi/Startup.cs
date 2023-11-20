using Common;
namespace KaazingChatApi
{
    public static class Startup
    {
        public static IConfiguration? _appSettingManager;
        public static void Configure(IWebHostEnvironment env)
        {
            #region 系統Log啟動
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(env.ContentRootPath + @"\log4net.config"));
            #endregion
            FileInfo IniFile = new FileInfo(Path.Combine(env.ContentRootPath, "common.ini"));
            using (FileStream FS = IniFile.OpenRead())
            {
                Config.ConfigStream = FS;
                Config.ReadParameter();
                Common.LogHelper.Logger.logPath = Path.Combine(env.ContentRootPath, Config.logDir);
            }

            var objBuilder = new ConfigurationBuilder();
            objBuilder.SetBasePath(Directory.GetCurrentDirectory());
            objBuilder.AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);
            _appSettingManager = objBuilder.Build();
        }
        public static IConfiguration? AppSettingManager
        {
            get
            {
                return _appSettingManager;
            }
        }
    }
}
