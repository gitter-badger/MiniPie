﻿using System;
using System.Deployment.Application;
using System.IO;

namespace MiniPie.Core
{
    public sealed class AutorunService
    {
        private readonly ILog _Logger;
        private readonly AppSettings _Settings;
        private readonly AppContracts _Contracts;

        private const string AutorunSettingsName = "StartWithWindows";
        private const string Extention = ".appref-ms";

        public AutorunService(ILog logger, AppSettings settings, AppContracts contracts)
        {
            _Logger = logger;
            _Settings = settings;
            _Contracts = contracts;

            ValidateAutorun();
        }

        public void ValidateAutorun()
        {
            try
            {
                string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                startupPath = Path.Combine(startupPath, _Contracts.ProductName) + Extention;
                if (_Settings.StartWithWindows)
                {
                    // Add/Update autorun
                    if (ApplicationDeployment.IsNetworkDeployed)
                    {
                        if (!File.Exists(startupPath))
                        {
                            string allProgramsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                            string shortcutPath = Path.Combine(allProgramsPath, _Contracts.PublisherName);
                            shortcutPath = Path.Combine(shortcutPath, _Contracts.ProductName) + Extention;

                            File.Copy(shortcutPath, startupPath);
                        }
                    }
                }
                else
                {
                    //Remove autorun
                    if (File.Exists(startupPath))
                    {
                        File.Delete(startupPath);
                    }
                }
            }
            catch (Exception exc)
            {
                _Logger.FatalException("Failed to update or remove autorun", exc);
            }
        }
    }
}
