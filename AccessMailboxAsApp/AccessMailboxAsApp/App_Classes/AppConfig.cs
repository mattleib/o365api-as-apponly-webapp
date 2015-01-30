//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license at the bottom of this file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace AccessMailboxAsApp.App_Classes
{
    public static class AppConfigSettings
    {
        public const string ClientId = "ClientId";
        public const string ClientSecret = "ClientSecret";
        public const string ClientCertificatePfx = "ClientCertificatePfx";
        public const string ClientCertificatePfxPassword = "ClientCertificatePfxPassword";
        public const string AuthorizationUri = "AuthorizationUri";
        public const string TokenIssueingUri = "TokenIssueingUri";
        public const string RedirectUri = "RedirectUri";
        public const string RedirectUriLocalHost = "RedirectUriLocalHost";
        public const string ExchangeResourceUri = "ExchangeResourceUri";
        public const string GraphResourceUri = "GraphResourceUri";
        public const string SignoutUri = "SignoutUri";
        public const string DebugOffice365User = "DebugOffice365User";
    }

    public class AppConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientCertificatePfx { get; set; }
        public string ClientCertificatePfxPassword { get; set; }
        public string AuthorizationUri { get; set; }
        public string TokenIssueingUri { get; set; }
        public string SignoutUri { get; set; }
        public string RedirectUri { get; set; }
        public string ExchangeResourceUri { get; set; }
        public string GraphResourceUri { get; set; }
        public string DebugOffice365User { get; set; }

        public AppConfig()
        {
            this.ClientId = this.Read(AppConfigSettings.ClientId);
            this.ClientSecret = this.Read(AppConfigSettings.ClientSecret);
            this.AuthorizationUri = this.Read(AppConfigSettings.AuthorizationUri);
            this.TokenIssueingUri = this.Read(AppConfigSettings.TokenIssueingUri);
            this.SignoutUri = this.Read(AppConfigSettings.SignoutUri);
            this.RedirectUri = this.Read(
#if DEBUG                
                AppConfigSettings.RedirectUriLocalHost
#else
                AppConfigSettings.RedirectUri
#endif
            );
            this.ExchangeResourceUri = this.Read(AppConfigSettings.ExchangeResourceUri);
            this.GraphResourceUri = this.Read(AppConfigSettings.GraphResourceUri);
            this.ClientCertificatePfx = this.Read(AppConfigSettings.ClientCertificatePfx);
            this.ClientCertificatePfxPassword = this.Read(AppConfigSettings.ClientCertificatePfxPassword);
            this.DebugOffice365User = this.Read(AppConfigSettings.DebugOffice365User);
        }

        private string Read(string appSettingName)
        {
            string res = String.Empty;

            try
            {   // fails if null or empty
                string appSetting = ConfigurationManager.AppSettings[appSettingName];
                res = appSetting;
            }
            catch { }

            return res;
        }


    }
}

// MIT License: 

// Permission is hereby granted, free of charge, to any person obtaining 
// a copy of this software and associated documentation files (the 
// ""Software""), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions: 

// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software. 

// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.