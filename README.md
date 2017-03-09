#o365api-as-apponly-webapp

Sample web app that uses client credential flow to access Users, Mail, Calendar, Contacts in Office 365 via Rest APIs.

For more information about how the protocols work in this scenario, see [Service to Service Calls Using Client Credentials] (http://msdn.microsoft.com/en-us/library/azure/dn645543.aspx)

For more information about "app-only" aka 'Service or Daemon applications' in Office 365, see the companion blog on: (https://blogs.msdn.microsoft.com/exchangedev/2015/01/21/building-daemon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow/)

## How To Run This Sample

To run this sample you will need:
- Visual Studio 2013
- An Internet connection
- An Office 365 Developer Subscription (a free trial is sufficient)

You can get a Office 365 Developer Subscription by signing up at (https://msdn.microsoft.com/en-us/library/office/fp179924(v=office.15).aspx)


### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/mattleib/o365api-as-apponly-webapp`


### Step 2  Register the sample with your Azure Active Directory tenant

####Prereq: Create a certificate for your app as described in the companion blog: (https://blogs.msdn.microsoft.com/exchangedev/2015/01/21/building-daemon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow/)

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the directory tenant where you wish to register the sample application.
4. Click the Applications tab.
5. In the drawer, click Add.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "O365AppOnlySample", select "Web Application and/or Web API", and click next.
8. For the sign-on URL, enter the base URL for the sample, e.g. `https://localhost:44321/Home`. 
  - *Note*: The sign-on URL must end with **"`Home`"** as the application code expects this. 
  - *Note*: As host component make sure that is the correct port for your IIS Express SSL that you later use for running/debugging the sample.
9. For the App ID URI, enter `https://<your_tenant_name>/O365AppOnlySample`, replacing `<your_tenant_name>` with the name of your Azure AD tenant.  Click OK to complete the registration.
10. While still in the Azure portal, click the Configure tab of your application.
11. Find the Client ID value and copy it aside, you will need this later when configuring your application.
12. Configure following application permissions for the web app:
  - In the section "permissions to other applications" select **"Windows Azure Active Directory"** 
  - From the "Application Permission" drop-down for "Windows Azure Active Directory" check: **"Read directory data"**
  - Select "Add Application" and add **"Office 365 Exchange Online"**
  - From the "Application Permission" drop-down for "Office 365 Exchange Online" check: **"Read users' mail"**
  - From the "Application Permission" drop-down for "Office 365 Exchange Online" check: **"Read users' calendar"**
  - From the "Application Permission" drop-down for "Office 365 Exchange Online" check: **"Read users' contacts"**
13. Save the configuration so you can view the key value.
14. Configure the X.509 public certificate as explained in the companion blog: (https://blogs.msdn.microsoft.com/exchangedev/2015/01/21/building-daemon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow/)


### Step 3  Configure the sample

1. Open the solution in Visual Studio 2013.
2. Open the `web.config` file.
3. Find the app key `RedirectUriLocalHost` and replace the value with the value in Step 2.
4. Find the app key `ClientId` and replace the value with client ID of Step 2.
5. Find the app key `ClientCertificatePfx` and replace the value with the location where your X.509 certificate with the private key is located
6. Find the app key `ClientCertificatePfxPassword` and replace the value with the password of your x.509 certificate with the private key
7. Configure the project to require SSL and make sure the Start URL is set to SSL by:
    a) Project Properties box: SSL Enabled, set to true
	b) Project Properties box: SSL URL: Make sure it matches the sign-on URL host component as specified in Step 2
	c) Project Settings Editor: Set the Start URL to the value as specified in previous step
	


### Step 4  Run the sample

Rebuild the solution, and run it.  You might want to go into the solution properties to check if the Startup matches your "https" sign-on URL as specified in Step 2.

Explore the sample by signing in with your Office 365 Developer Tenant, select a mailbox (you might want to create more mailboxes and fill with data in the Office 365 Admin Portal) and retrieve data.



