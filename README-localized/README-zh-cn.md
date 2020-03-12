#o365api-as-apponly-webapp

示例 Web 应用，使用客户端凭据流通过 Rest API 访问 Office 365 中的用户、邮件、日历、联系人。

有关此情形中的协议工作方式的详细信息，请参阅 [Service to Service Calls Using Client Credentials] (http://msdn.microsoft.com/en-us/library/azure/dn645543.aspx)

有关 Office 365 中的“仅限应用”（也称为“服务或守护程序应用程序”）的详细信息，请参阅以下位置的配套博客：(http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

## 如何运行此示例应用

若要运行此示例应用，你将需要：- Visual Studio 2013 - Internet 连接 - Office 365 开发人员订阅（免费试用版已足够）

可在以下位置注册获取 Office 365 开发人员订阅：(https://msdn.microsoft.com/en-us/library/office/fp179924(v=office.15).aspx)


### 步骤 1：克隆或下载此存储库

在 shell 或命令行中键入：

`git clone https://github.com/mattleib/o365api-as-apponly-webapp`


### 步骤 2：将示例应用注册到 Azure Active Directory 租户

####先决条件：如以下配套博客中所述为你的应用创建证书：(http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

1. 登录 [Azure 管理门户](https://manage.windowsazure.com)。
2. 单击左侧导航栏中的 Active Directory。
3. 单击要在其中注册示例应用程序的目录租户。
4. 单击“应用程序”选项卡。
5. 在弹窗中，单击“添加”。
6. 单击“添加我的组织正在开发的应用程序”。
7. 为应用程序输入一个友好的名称（例如“O365AppOnlySample”），选择“Web 应用程序和/或 Web API”，然后单击“下一步”。
8. 对于登录 URL，输入示例应用的基 URL，例如 `https://localhost:44321/Home`。 
  - *注意*：登录 URL 必须以**“`Home`”**结尾，因为应用程序代码需要此字符串。 
  - *注意*：确保主机 (host) 部分的端口是 IIS Express SSL 的正确端口，稍后将用于运行/调试示例应用。
9. 对于“应用程序 ID URI”，输入 `https://<your_tenant_name>/O365AppOnlySample`，并将 `<your_tenant_name>` 替换为 Azure AD 租户的名称。单击“确定”完成注册。
10. 继续在 Azure 门户中单击应用程序的“配置”选项卡。
11. 查找“客户端 ID”值并将其复制到一旁，稍后在配置应用程序时将需要此值。
12. 配置 Web 应用的以下应用程序权限：
  - 在“针对其他应用程序的权限”部分中，选择“**Microsoft Azure Active Directory**” 
  - 在“Windows Azure Active Directory”的“应用程序权限”下拉列表中，选中：“**读取目录数据**”
  - 选择“添加应用程序”并添加“**Office 365 Exchange Online**”
  - 在“Office 365 Exchange Online”的“应用程序权限”下拉列表中，选中：“**读取用户的邮件**”
  - 在“Office 365 Exchange Online”的“应用程序权限”下拉列表中，选中：“**读取用户的日历**”
  - 在“Office 365 Exchange Online”的“应用程序权限”下拉列表中，选中：“**读取用户的联系人**”
13. 保存配置，以便可以查看键值。
14. 按照以下配套博客中的说明配置 X.509 公共证书：(http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)


### 步骤 3：配置示例应用

1. 在 Visual Studio 2013 中打开该解决方案。
2. 打开 `web.config` 文件。
3. 查找应用键值 `RedirectUriLocalHost`，并将该值替换为步骤 2 中的值。
4. 查找应用键值 `ClientId`，并将该值替换为步骤 2 中的客户端 ID。
5. 查找应用键值 `ClientCertificatePfx`，并将该值替换为包含私钥的 X.509 证书所在的位置
6. 查找应用键值 `ClientCertificatePfxPassword`，并将该值替换为包含私钥的 X.509 证书的密码。
7. 通过以下设置将项目配置为需要 SSL 并确保将“起始 URL”设置为 SSL：a)“项目属性”框：“已启用 SSL”，设置为 True；b)“项目属性”框：“SSL URL”：确保该值与步骤 2 中指定的登录 URL 主机部分匹配；c) 项目设置编辑器：将“起始 URL”设置为上一步中指定的值
	


### 步骤 4：运行示例应用

重新生成解决方案，然后运行解决方案。你可能希望通过查看解决方案属性来检查“启动”值是否与步骤 2 中指定的“https”登录 URL 匹配。

请登录 Office 365 开发人员租户来探索示例应用，选择邮箱（你可能希望在 Office 365 管理门户中创建更多邮箱并填充数据），并检索数据。



