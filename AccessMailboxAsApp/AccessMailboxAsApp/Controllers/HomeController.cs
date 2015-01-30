//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license at the bottom of this file.
//
using AccessMailboxAsApp.App_Classes;
using AccessMailboxAsApp.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Globalization;

namespace AccessMailboxAsApp.Controllers
{
    // From: Jason Johnston@https://github.com/jasonjoh/office365-azure-guides/blob/master/code/parse-token.cs
    static class Base64UrlEncoder
    {
        static char Base64PadCharacter = '=';
        static string DoubleBase64PadCharacter = String.Format(CultureInfo.InvariantCulture, "{0}{0}", Base64PadCharacter);
        static char Base64Character62 = '+';
        static char Base64Character63 = '/';
        static char Base64UrlCharacter62 = '-';
        static char Base64UrlCharacter63 = '_';

        public static byte[] DecodeBytes(string arg)
        {
            string s = arg;
            s = s.Replace(Base64UrlCharacter62, Base64Character62); // 62nd char of encoding
            s = s.Replace(Base64UrlCharacter63, Base64Character63); // 63rd char of encoding
            switch (s.Length % 4) // Pad 
            {
                case 0:
                    break; // No pad chars in this case
                case 2:
                    s += DoubleBase64PadCharacter; break; // Two pad chars
                case 3:
                    s += Base64PadCharacter; break; // One pad char
                default:
                    throw new ArgumentException("Illegal base64url string!", arg);
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }

        public static string Decode(string arg)
        {
            return Encoding.UTF8.GetString(DecodeBytes(arg));
        }
    }

    public class HomeController : Controller
    {
        private static AppConfig appConfig = new AppConfig();

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class MultipleButtonAttribute : ActionNameSelectorAttribute
        {
            public string Name { get; set; }
            public string Argument { get; set; }

            public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
            {
                var isValidName = false;
                var keyValue = string.Format("{0}:{1}", Name, Argument);
                var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

                if (value != null)
                {
                    controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
                    isValidName = true;
                }

                return isValidName;
            }
        }

        //
        // GET: /Home/
        public async Task<ActionResult> Index()
        {
            // Force SSL
            if (!Request.IsSecureConnection)
            {
                string httplength = "http";
                string nonsecureurl = Request.Url.AbsoluteUri.Substring(httplength.Length);
                string secureurl = String.Format("https{0}", nonsecureurl);
                RedirectResult result = Redirect(secureurl);
                result.ExecuteResult(this.ControllerContext);
            }

            // This is where state of the app is maintained and data passed between view and controller
            AppState appState = new AppState();

            // Authorization back from AAD in a form post as requested in the authorize request
            if(Request.Form != null) 
            { 
                // Did it return with an error?
                if (!String.IsNullOrEmpty(Request.Form["error"]))
                {
                    appState.ErrorMessage = Request.Form["error"];                
                    appState.AppIsAuthorized = false;

                    return View(appState);
                }

                // Authorized without error: Check to see if we have an ID token
                if (String.IsNullOrEmpty(Request.Form["id_token"]))
                {
                    appState.AppIsAuthorized = false;
                }
                else
                {
                    // Was it correlated with authorize request
                    var authstate = Session[AppSessionVariables.AuthState] as String;
                    Session[AppSessionVariables.AuthState] = null;
                    if (String.IsNullOrEmpty(authstate))
                    {
                        appState.ErrorMessage = "Oops. Something went wrong with the authorization state (No auth state). Please retry.";
                        appState.AppIsAuthorized = false;

                        return View(appState);
                    }
                    if (!Request.Form["state"].Equals(authstate))
                    {
                        appState.ErrorMessage = "Oops. Something went wrong with the authorization state (Invalid auth state). Please retry.";
                        appState.AppIsAuthorized = false;

                        return View(appState);
                    }

                    // Get the TenantId out of the ID Token to address tenant specific token endpoint.
                    // No validation of ID Token as the only info we need is the tenantID
                    // If for any case your app wants to use the ID Token to authenticate 
                    // it must be validated.
                    JwtToken openIDToken = GetTenantId(Request.Form["id_token"]);
                    appState.TenantId = openIDToken.tid;
                    appState.TenantDomain = openIDToken.domain;
                    appState.LoggedOnUser = openIDToken.upn;

                    appState.AppIsAuthorized = true;
                }
            }

            if(appState.AppIsAuthorized)
            {
                // Get app-only access tokens ....
                try
                {
                    // Get an app-only access token for the AAD Graph Rest APIs
                    var authResult = await GetAppOnlyAccessToken(appConfig.GraphResourceUri, appState.TenantId);

                    // Get list of users in this Tenant from AAD Graph to fill drop down with smtp addresses
                    List<GraphUser> users = GetUsers(appState.TenantId, authResult.AccessToken);

                    appState.MailboxList = this.BuildMailboxDropDownList(string.Empty, users);
                    appState.MailboxSmtpAddress = appState.MailboxList.SelectedValue as String;

                    // For convenience maintain this list as a session
                    Session[AppSessionVariables.MailboxList] = users;

                    // Get app-only access tokens for Exchange Rest APIs
                    authResult = await GetAppOnlyAccessToken(appConfig.ExchangeResourceUri, appState.TenantId);

                    appState.AccessToken = authResult.AccessToken;
                    appState.AccessTokenAquiredWithoutError = true;
                    
                    SetSessionInProgress();
                }
                catch(Exception ex)
                {
                    appState.ErrorMessage = ex.Message;
                }
            }

            return View(appState);
        }

        private void SetSessionInProgress()
        {
            Session[AppSessionVariables.IsAuthorized] = true;
        }

        private bool IsSessionInProgress()
        {
            bool? inprogress = Session[AppSessionVariables.IsAuthorized] as bool?;
            if (null == inprogress)
                return false;

            return (bool) inprogress;
        }

        private ViewResult RedirectHome()
        {
            RedirectResult result = Redirect(appConfig.RedirectUri);
            result.ExecuteResult(this.ControllerContext);

            return View("Index", new AppState());
        }

        private SelectList BuildMailboxDropDownList(string selectedSmtp, List<GraphUser> users)
        {
            string selectedValue = "";
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (GraphUser u in users)
            {
                if (!string.IsNullOrEmpty(selectedSmtp))
                {
                    if (selectedSmtp.Equals(u.mail))
                    {
                        selectedValue = u.mail;
                    }
                }
                else
                {
                    selectedValue = selectedSmtp = u.mail;
                }
                if (IsValidSmtp(u.mail))
                {
                    items.Add(
                        new SelectListItem
                        {
                            Text = u.mail,
                            Value = u.mail,
                        }
                    );
                }
            }

            SelectList selectList = new SelectList(
                items, "Value", "Text", selectedValue);

            return selectList;
        }

        public class GraphUsers
        {
            public List<GraphUser> data { get; set; } 
        }

        public class GraphUser 
        {
            public string objectId;
            public string mail;
        }

        private List<GraphUser> GetUsers(string tenantId, string accessToken)
        {
             string api = String.Format("{0}{1}/users?api-version=2013-04-05", appConfig.GraphResourceUri, tenantId);

            Func<HttpRequestMessage> requestCreator = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, api);
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                return request;
            };

            string json = HttpRequestHelper.MakeHttpRequest(requestCreator, accessToken);
            var data = JObject.Parse(json).SelectToken("value");
            List<GraphUser> users = JsonConvert.DeserializeObject<List<GraphUser>>(data.ToString());

            return users;
        }

        private bool IsValidSmtp(string smtp)
        {
            if(string.IsNullOrEmpty(smtp))
                return false;

            // Copied from: http://www.regular-expressions.info/email.html
            string smtpRegexPattern = @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$";
            Regex rgx = new Regex(smtpRegexPattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(smtp);
            if (matches.Count == 0)
                return false;

            return true;
        }

        private void MakeExchangeRestApiRequest(string api, ref AppState passedAppState)
        {
            if (IsValidSmtp(passedAppState.MailboxSmtpAddress))
            {
                passedAppState.Request = api;

                Func<HttpRequestMessage> requestCreator = () =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, api);
                    request.Headers.Add("Accept", "application/json; odata.metadata=none");
                    return request;
                };

                string json = HttpRequestHelper.MakeHttpRequest(requestCreator, passedAppState.AccessToken);
                passedAppState.Response = JObject.Parse(json).ToString(/*Newtonsoft.Json.Formatting.Indented*/);
            }
            else
            {
                passedAppState.ErrorMessage = "Invalid SMTP";
            }

            List<GraphUser> users = Session[AppSessionVariables.MailboxList] as List<GraphUser>;
            passedAppState.MailboxList = BuildMailboxDropDownList(passedAppState.MailboxSmtpAddress, users);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Messages")]
        public ActionResult GetMessages(AppState passedAppState)
        {
            if (!IsSessionInProgress())
            {
                return RedirectHome();
            }

            string api = String.Format("{0}api/v1.0/users('{1}')/folders/inbox/messages?$top=50", appConfig.ExchangeResourceUri, passedAppState.MailboxSmtpAddress);

            MakeExchangeRestApiRequest(api, ref passedAppState);

            return View("Index", passedAppState);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Calendar")]
        public ActionResult GetEvents(AppState passedAppState)
        {
            if (!IsSessionInProgress())
            {
                return RedirectHome();
            }

            string api = String.Format("{0}api/v1.0/users('{1}')/events?$top=50", appConfig.ExchangeResourceUri, passedAppState.MailboxSmtpAddress);

            MakeExchangeRestApiRequest(api, ref passedAppState);

            return View("Index", passedAppState);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Contacts")]
        public ActionResult GetContacts(AppState passedAppState)
        {
            if (!IsSessionInProgress())
            {
                return RedirectHome();
            }

            string api = String.Format("{0}api/v1.0/users('{1}')/contacts?$top=50", appConfig.ExchangeResourceUri, passedAppState.MailboxSmtpAddress);

            MakeExchangeRestApiRequest(api, ref passedAppState);

            return View("Index", passedAppState);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "StartOver")]
        public ActionResult StartOver(AppState passedAppState)
        {
            if (!IsSessionInProgress())
            {
                return RedirectHome();
            }

            AppState appState = new AppState();

            Session.Clear();

            UriBuilder signOutRequest = new UriBuilder(appConfig.SignoutUri.Replace("common", passedAppState.TenantId));

            signOutRequest.Query = "post_logout_redirect_uri=" + HttpUtility.UrlEncode(appConfig.RedirectUri);
            
            RedirectResult result = Redirect(signOutRequest.Uri.ToString());
            result.ExecuteResult(this.ControllerContext);
        
            return View("Index", appState);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Authorize")]
        public ActionResult Auhorize(AppState passedAppState)
        {
            passedAppState.AppIsAuthorized = false;

            // hit the common endpoint for authorization, 
            // after authorization we will use the tenant specific endpoint for getting app-only tokens
            UriBuilder authorizeRequest = new UriBuilder(appConfig.AuthorizationUri); 

            // Maintain state for authorize request to prvenet cross forgery attacks
            var authstate = Guid.NewGuid().ToString();
            Session[AppSessionVariables.AuthState] = authstate;

            authorizeRequest.Query =
                    "state=" + authstate +
                    "&response_type=code+id_token" +
                    "&scope=openid" +
                    "&nonce=" + Guid.NewGuid().ToString() +
                    "&client_id=" + appConfig.ClientId +
                    "&redirect_uri=" + HttpUtility.UrlEncode(appConfig.RedirectUri) +
                    "&resource=" + HttpUtility.UrlEncode(appConfig.GraphResourceUri) +
#if DEBUG
                    "&login_hint=" + appConfig.DebugOffice365User +
#endif
                    "&prompt=admin_consent" +
                    "&response_mode=form_post";

            RedirectResult result = Redirect(authorizeRequest.Uri.ToString());
            result.ExecuteResult(this.ControllerContext);

            return View("Index", passedAppState);
        }

        private string Base64UrlDecodeJwtTokenPayload(string base64UrlEncodedJwtToken)
        {
            string payload = base64UrlEncodedJwtToken.Split('.')[1];

            return Base64UrlEncoder.Decode(payload);
        }

        public class JwtToken
        {
            public string tid { get; set; }
            public string upn { get; set; }
            public string domain { get { return (string.IsNullOrEmpty(upn)) ? "string.Empty" : upn.Split('@')[1]; } }
        }

        private JwtToken GetTenantId(string id_token)
        {
            string encodedOpenIdToken = id_token;

            string decodedToken = Base64UrlDecodeJwtTokenPayload(encodedOpenIdToken);

            JwtToken token = JsonConvert.DeserializeObject<JwtToken>(decodedToken);

            return token;
        }

        private async Task<AuthenticationResult> GetAppOnlyAccessToken(string resource, string tenantId)
        {
            string authority = appConfig.AuthorizationUri.Replace("common", tenantId);
            AuthenticationContext authenticationContext = new AuthenticationContext(
                authority,
                false);

            string certfile = Server.MapPath(appConfig.ClientCertificatePfx);

            X509Certificate2 cert = new X509Certificate2(
                certfile,
                appConfig.ClientCertificatePfxPassword,
                X509KeyStorageFlags.MachineKeySet);

            // ADAL new ... (in Beta, might change)
            // ClientAssertionCertificate cac = new ClientAssertionCertificate(
            //    appConfig.ClientId,
            //    cert.GetRawCertData(),
            //    appConfig.ClientCertificatePfxPassword);

            // ADAL current released (2.12.111071459)
            ClientAssertionCertificate cac = new ClientAssertionCertificate(
                appConfig.ClientId, cert);
                    
            var authenticationResult = await authenticationContext.AcquireTokenAsync(
                resource, 
                cac);

            return authenticationResult;
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