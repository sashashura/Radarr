using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using Radarr.Http;

namespace Radarr.Api.V3.Logs
{
    [V3ApiController("log/file")]
    public class LogFileController : LogFileControllerBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public LogFileController(IAppFolderInfo appFolderInfo,
                             IDiskProvider diskProvider,
                             IOptionsMonitor<ConfigFileOptions> configFileOptions)
            : base(diskProvider, configFileOptions, "")
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
        }

        protected override IEnumerable<string> GetLogFiles()
        {
            return _diskProvider.GetFiles(_appFolderInfo.GetLogFolder(), SearchOption.TopDirectoryOnly);
        }

        protected override string GetLogFilePath(string filename)
        {
            return Path.Combine(_appFolderInfo.GetLogFolder(), filename);
        }

        protected override string DownloadUrlRoot
        {
            get
            {
                return "logfile";
            }
        }
    }
}
