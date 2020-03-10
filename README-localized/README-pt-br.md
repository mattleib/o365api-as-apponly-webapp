\#o365api-as-apponly-webapp

Exemplo de aplicativo Web que usa o fluxo de credenciais do cliente para acessar usuários, email, calendário, contatos no Office 365 por meio de APIs Rest.

Para obter mais informações sobre como os protocolos funcionam nesse cenário, confira \[Chamadas de serviço para serviço usando as credenciais do cliente] (http://msdn.microsoft.com/pt-be/library/azure/dn645543.aspx)

Para obter mais informações sobre “apenas aplicativos”, também conhecido como “Aplicativos de serviço ou Daemon” no Office 365, confira o blog de complemento em: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

## Como executar esse exemplo

Para executar este exemplo, você precisará do seguinte: – Visual Studio 2013 – Uma conexão com a Internet – Uma assinatura do Office 365 Developer (uma versão de avaliação gratuita é suficiente)

Você pode obter uma assinatura do Office 365 Developer se inscrevendo em (https://msdn.microsoft.com/pt-br/library/office/fp179924(v=office.15).aspx)


### Etapa 1: Clone ou baixe este repositório

A partir do seu shell ou linha de comando:

`git clone https://github.com/mattleib/o365api-as-apponly-webapp`


### Etapa 2: registrar seu aplicativo de exemplo com o locatário do Azure Active Directory

\####Pré-requisitos: criar um certificado para seu aplicativo conforme descrito no blog de complemento: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

1. Entre no [portal de gerenciamento do Azure](https://manage.windowsazure.com).
2. Clique no Active Directory na barra de navegação à esquerda.
3. Clique no locatário do diretório no qual você deseja registrar o aplicativo de exemplo.
4. Clique na guia Aplicativos.
5. Na gaveta, clique em Adicionar.
6. Clique em “Adicionar um aplicativo que minha organização esteja desenvolvendo”.
7. Insira um nome amigável para o aplicativo, por exemplo, “O365AppOnlySample”, selecione “Aplicativo Web e/ou API Web” e clique em Avançar.
8. Para a URL de logon, insira a URL base do exemplo, como `https://localhost:44321/Home`. 
  - *Observação*: a URL de logon deve terminar com **“`Home`”**, uma vez que isso é esperado pelo código do aplicativo. 
  - *Observação*: como componente de host, certifique-se de que seja a porta correta para o seu SSL do IIS Express, que você usará mais tarde para executar/depurar o exemplo.
9. Para a URI da ID do aplicativo, insira `https://<your_tenant_name>/O365AppOnlySample`, substituindo `<your_tenant_name>` pelo nome do seu locatário do Azure AD. Clique em OK para concluir o registro.
10. Ainda no portal do Azure, clique na guia Configurar do seu aplicativo.
11. Localize o valor da ID do cliente e copie-o, pois você precisará dele posteriormente ao configurar seu aplicativo.
12. Configure as permissões de aplicativo a seguir para o aplicativo Web:
  - Na seção “permissões para outros aplicativos”, selecione **“Microsoft Azure Active Directory”** 
  - Na lista suspensa “Permissão do aplicativo” do “Microsoft Azure Active Directory”, marque: **“Ler dados do diretório”**
  - Selecione “Adicionar aplicativo” e adicionar **“Office 365 Exchange Online”**
  - Na lista suspensa “Permissão do aplicativo” do “Office 365 Exchange Online”, marque: **“Ler email de usuários”**
  - Na lista suspensa “Permissão do aplicativo” do “Office 365 Exchange Online”, marque: **“Ler calendário de usuários”**
  - Na lista suspensa “Permissão do aplicativo” do “Office 365 Exchange Online”, marque: **“Ler contatos de usuários”**
13. Salve as configurações para que você possa exibir o valor da chave.
14. Configure o certificado público X.509 conforme a explicação do blog de complemento: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)


### Etapa 3: configurar o exemplo

1. Abra a solução no Visual Studio 2013.
2. Abra o arquivo `web.config`.
3. Localize a chave do aplicativo `RedirectUriLocalHost` e substitua o valor pelo valor na Etapa 2.
4. Localize a chave do aplicativo `ClientId` e substitua o valor pela ID do cliente da Etapa 2.
5. Localize a chave do aplicativo `ClientCertificatePfx` e substitua o valor pelo local em que o certificado X.509 com a chave privada está localizado
6. Localize a chave do aplicativo `ClientCertificatePfxPassword` e substitua o valor pela senha do certificado x.509 com a chave privada
7. Configure o projeto para requerer o SSL e verifique se a URL inicial está definida como SSL por: a) Caixa de propriedades do projeto: SSL habilitado, definido como verdadeiro; b) Caixa de propriedades do projeto: URL do SSL: verifique se ela corresponde ao componente de host da URL de logon conforme especificado na Etapa 2; c) Editor de configurações do projeto: definir a URL inicial conforme o valor especificado na etapa anterior
	


### Etapa 4: executar o exemplo

Recompile a solução e execute-a. Você pode acessar as propriedades da solução para verificar se a inicialização corresponde à URL de entrada "https" conforme especificado na Etapa 2.

Explore o exemplo entrando usando seu locatário do Office 365 Developer, selecione uma caixa de correio (você pode criar mais caixas de correio e preencher com dados no Portal de administração do Office 365) e recupere dados.



