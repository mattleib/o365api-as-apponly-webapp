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
using System.Security.Cryptography;
using System.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Claims;

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
            Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext authenticationContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(
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


        /**
         * Below is only to better understand how a client assertion can be build and an access token request be submitted via straight HTTP.
         * It is by no means meant for production or to replace what ADAL provides. If you develop in .Net you should use ADAL.
         */

        // Copied from:
        // https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/blob/master/src/ADAL.NET/CryptographyHelper.cs
        // This method returns an AsymmetricSignatureFormatter capable of supporting Sha256 signatures. 
        private static RSACryptoServiceProvider GetCryptoProviderForSha256(RSACryptoServiceProvider rsaProvider)
        {
            const int PROV_RSA_AES = 24;    // CryptoApi provider type for an RSA provider supporting sha-256 digital signatures
            if (rsaProvider.CspKeyContainerInfo.ProviderType == PROV_RSA_AES)
            {
                return rsaProvider;
            }

            CspParameters csp = new CspParameters
            {
                ProviderType = PROV_RSA_AES,
                KeyContainerName = rsaProvider.CspKeyContainerInfo.KeyContainerName,
                KeyNumber = (int)rsaProvider.CspKeyContainerInfo.KeyNumber
            };

            if (rsaProvider.CspKeyContainerInfo.MachineKeyStore)
            {
                csp.Flags = CspProviderFlags.UseMachineKeyStore;
            }

            //
            // If UseExistingKey is not specified, the CLR will generate a key for a non-existent group.
            // With this flag, a CryptographicException is thrown instead.
            //
            csp.Flags |= CspProviderFlags.UseExistingKey;
            return new RSACryptoServiceProvider(csp);
        }

        public class AADClientCredentialSuccessResponse
        {
            //{
            //  "token_type": "Bearer",
            //  "expires_in": "3600",
            //  "expires_on": "1423336547",
            //  "not_before": "1423332647",
            //  "resource": "https://outlook.office365.com/",
            //  "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciO....ZVvynkUXjZPNg1oJWDKBymPL-U0WA"
            //}
            public string token_type;
            public string expires_in;
            public string expires_on;
            public string not_before;
            public string resource;
            public string access_token;
        };

        public class AADClientCredentialErrorResponse
        { 
            //{
            //  "error": "invalid_client",
            //  "error_description": "AADSTS70002: Error ...",
            //  "error_codes": [
            //    70002,
            //    50012
            //  ],
            //  "timestamp": "2015-02-07 18:44:09Z",
            //  "trace_id": "dabcfa26-ea8d-46c5-81bc-ff57a0895629",
            //  "correlation_id": "8e270f2d-ba05-42fb-a7ab-e819d142c843",
            //  "submit_url": null,
            //  "context": null
            //}
            public string error;
            public string error_description;
            public string[] error_codes;
            public string timestamp;
            public string trace_id;
            public string correlation_id;
            public string submit_url;
            public string context;
        }

        // Get the access token via straight http post request doing client credential flow
        private async Task<String> GetAppOnlyAccessTokenWithHttpRequest(string resource, string tenantId)
        {
            /**
             * use the tenant specific endpoint for requesting the app-only access token
             */
            string tokenIssueEndpoint = appConfig.TokenIssueingUri.Replace("common", tenantId);

            /**
             * sign the assertion with the private key
             */
            string certfile = Server.MapPath(appConfig.ClientCertificatePfx);
            X509Certificate2 cert = new X509Certificate2(
                certfile,
                appConfig.ClientCertificatePfxPassword,
                X509KeyStorageFlags.MachineKeySet);

            /**
             * Example building assertion using Json Tokenhandler. 
             * Sort of cheating, but just if someone wonders ... there are always more ways to do something :-)
             */
            Dictionary<string, string> claims = new Dictionary<string, string>()
            {
                { "sub", appConfig.ClientId },
                { "jti", Guid.NewGuid().ToString() },
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            X509SigningCredentials signingCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest);

            JwtSecurityToken selfSignedToken = new JwtSecurityToken(
                appConfig.ClientId,
                tokenIssueEndpoint,
                claims.Select(c => new Claim(c.Key, c.Value)),
                DateTime.UtcNow, 
                DateTime.UtcNow.Add(TimeSpan.FromMinutes(15)),
                signingCredentials);

            string signedAssertion = tokenHandler.WriteToken(selfSignedToken);

            //---- End example with Json Tokenhandler... now to the fun part doing it all ourselves ...

            /**
              * Example building assertion from scratch with Crypto APIs
            */
            JObject clientAssertion = new JObject();
            clientAssertion.Add("aud", tokenIssueEndpoint);
            clientAssertion.Add("iss", appConfig.ClientId);
            clientAssertion.Add("sub", appConfig.ClientId);
            clientAssertion.Add("jti", Guid.NewGuid().ToString());
            clientAssertion.Add("nbf", WebConvert.EpocTime(DateTime.UtcNow + TimeSpan.FromMinutes(-5)));
            clientAssertion.Add("exp", WebConvert.EpocTime(DateTime.UtcNow + TimeSpan.FromMinutes(15)));

            string assertionPayload = clientAssertion.ToString(Newtonsoft.Json.Formatting.None);

            X509AsymmetricSecurityKey x509Key = new X509AsymmetricSecurityKey(cert);
            RSACryptoServiceProvider rsa = x509Key.GetAsymmetricAlgorithm(SecurityAlgorithms.RsaSha256Signature, true) as RSACryptoServiceProvider;
            RSACryptoServiceProvider newRsa = GetCryptoProviderForSha256(rsa);
            SHA256Cng sha = new SHA256Cng();

            JObject header = new JObject(new JProperty("alg", "RS256"));
            string thumbprint = WebConvert.Base64UrlEncoded(WebConvert.HexStringToBytes(cert.Thumbprint));
            header.Add(new JProperty("x5t", thumbprint));

            string encodedHeader = WebConvert.Base64UrlEncoded(header.ToString());
            string encodedPayload = WebConvert.Base64UrlEncoded(assertionPayload);

            string signingInput = String.Concat(encodedHeader, ".", encodedPayload);

            byte[] signature = newRsa.SignData(Encoding.UTF8.GetBytes(signingInput), sha);
  
            signedAssertion = string.Format("{0}.{1}.{2}",
                encodedHeader,
                encodedPayload,
                WebConvert.Base64UrlEncoded(signature));

            /**
             * build the request payload
             */
            FormUrlEncodedContent tokenRequestForm;
            tokenRequestForm = new FormUrlEncodedContent(
                new[] { 
                new KeyValuePair<string,string>("resource", appConfig.ExchangeResourceUri),
                new KeyValuePair<string,string>("client_id", appConfig.ClientId),
                new KeyValuePair<string,string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                new KeyValuePair<string,string>("client_assertion", signedAssertion),
                new KeyValuePair<string,string>("grant_type","client_credentials"),
                }
                );

            /*
             * Do the web request
             */
            HttpClient client = new HttpClient();

            Task<string> requestString = tokenRequestForm.ReadAsStringAsync();
            StringContent requestContent = new StringContent(requestString.Result);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            requestContent.Headers.Add("client-request-id", System.Guid.NewGuid().ToString());
            requestContent.Headers.Add("return-client-request-id", "true");
            requestContent.Headers.Add("UserAgent", "MatthiasLeibmannsAppOnlyAppSampleBeta/0.1");

            HttpResponseMessage response = client.PostAsync(tokenIssueEndpoint, requestContent).Result;
            JObject jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            JsonSerializer jsonSerializer = new JsonSerializer();

            if(response.IsSuccessStatusCode == true)
            { 
                AADClientCredentialSuccessResponse s = (AADClientCredentialSuccessResponse)jsonSerializer.Deserialize(new JTokenReader(jsonResponse), typeof(AADClientCredentialSuccessResponse));
                return s.access_token;
            }

            AADClientCredentialErrorResponse e = (AADClientCredentialErrorResponse)jsonSerializer.Deserialize(new JTokenReader(jsonResponse), typeof(AADClientCredentialErrorResponse));
            throw new Exception(e.error_description);
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