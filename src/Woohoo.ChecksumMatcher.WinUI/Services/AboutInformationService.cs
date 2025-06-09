// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;
using Windows.ApplicationModel;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Models;

internal sealed class AboutInformationService : IAboutInformationService
{
    public AboutInformation GetInformation()
    {
        return new AboutInformation
        {
            AppVersion = GetVersion(),
            DotNetFramework = RuntimeInformation.FrameworkDescription,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
            OperatingSystem = string.Format(CultureInfo.CurrentUICulture, "{0} {1}", RuntimeInformation.OSDescription, RuntimeInformation.OSArchitecture),
            WindowsRuntime = RuntimeInfo.AsString,
            WindowsSdk = ReleaseInfo.AsString,
        };
    }

    private static string GetVersion()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return string.Format(CultureInfo.CurrentUICulture, Localized.SettingsPageVersionFormat, version);
    }
}
