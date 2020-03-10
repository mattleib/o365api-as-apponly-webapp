\#o365api-as-apponly-webapp

Aplicación web de ejemplo que usa el flujo de credenciales de cliente para obtener acceso a usuarios, correos, calendario y contactos en Office 365 con API de REST.

Para obtener más información sobre cómo funcionan los protocolos en este escenario, vea \[Llamadas entre servicios mediante las credenciales del cliente] (http://msdn.microsoft.com/en-us/library/azure/dn645543.aspx)

Para obtener más información sobre "solo para la aplicación" también denominado “Aplicaciones de servicio o Daemon” en Office 365, consulte el blog complementario de la dirección: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

## Cómo ejecutar este ejemplo

Para ejecutar este ejemplo, necesitará:
- Visual Studio 2013
- Una conexión a Internet
- Una suscripción Office 365 para desarrolladores (una evaluación gratuita es suficiente)

Puede obtener una suscripción de Office 365 para desarrolladores registrándose en (https://msdn.microsoft.com/en-us/library/office/fp179924(v=office.15).aspx)


### Paso 1: Clonar o descargar el repositorio

Desde la línea de comandos o shell:

`git clone https://github.com/mattleib/o365api-as-apponly-webapp`


### Paso 2: Registrar el ejemplo con el inquilino de Azure Active Directory

####Requisitos previos: Cree un certificado para la aplicación tal y como se describe en el blog complementario de la dirección: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

1. Inicie sesión en el [Portal de administración de Microsoft Azure](https://manage.windowsazure.com).
2. Haga clic en Active Directory en el panel de navegación izquierdo.
3. Haga clic en el inquilino del directorio en el que desea registrar la aplicación de ejemplo.
4. Haga clic en la pestaña Aplicaciones.
5. En el cajón, haga clic en Agregar.
6. Haga clic en “Agregar una aplicación desarrollada por mi organización”.
7. Escriba un nombre descriptivo para la aplicación (por ejemplo, "O365AppOnlySample"), seleccione “Aplicación web y/o API web” y haga clic en Siguiente.
8. Para la URL de inicio de sesión, escriba la URL base para el ejemplo. Por ejemplo: `https://localhost:44321/Home`. 
  - *Nota*: La dirección URL de inicio de sesión debe finalizar con **"`Home`"**, ya que es lo que espera el código de la aplicación. 
  - *Nota*: Como componente de host, asegúrese de que sea el puerto correcto para el SSL de IIS Express que use más adelante para ejecutar o depurar el ejemplo.
9. Para el URI de Id. de aplicación, escriba `https://<your_tenant_name>/O365AppOnlySample`, y reemplace `<your_tenant_name>` por el nombre de su inquilino de Azure AD. Haga clic en Aceptar para completar el registro.
10. Sin salir de Azure Portal, haga clic en la pestaña Configurar de la aplicación.
11. Busque el valor de Id. de cliente y cópielo aparte, lo necesitará más adelante para configurar la aplicación.
12. Configure los siguientes permisos de aplicación para la aplicación web:
  - En la sección "permisos para otras aplicaciones" seleccione **"Windows Azure Active Directory"** 
  - En la lista desplegable "Permisos de aplicación" de "Windows Azure Active Directory", marque: **"Leer datos de directorio"**
  - Seleccione "Agregar aplicación" y agregue**"Office 365 Exchange Online"**
  - En la lista desplegable "Permisos de aplicación" de "Office 365 Exchange Online", marque: **"Leer correo de los usuarios"**
  - En la lista desplegable "Permisos de aplicación" de "Office 365 Exchange Online", marque: **"Leer calendario de los usuarios"**
  - En la lista desplegable "Permisos de aplicación" de "Office 365 Exchange Online", marque: **"Leer contactos de los usuarios"**
13. Guarde la configuración para poder ver el valor clave.
14. Configure el certificado público X.509 tal como se describe en el blog complementario de la dirección: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)


### Paso 3: Configurar el ejemplo

1. Abra la solución en Visual Studio 2013.
2. Abra el archivo `web.config`.
3. Busque la clave de aplicación `RedirectUriLocalHost` y reemplace el valor por el valor del paso 2.
4. Busque la clave de aplicación `ClientId` y reemplace el valor por el Id. de cliente del paso 2.
5. Busque la clave de aplicación `ClientCertificatePfx` y reemplace el valor por la ubicación en la que se encuentra el certificado X.509 con la clave privada.
6. Busque la clave de aplicación `ClientCertificatePfxPassword` y reemplace el valor por la contraseña del certificado X.509 con la clave privada.
7. Configure el proyecto para que requiera SSL y asegúrese de que la URL de inicio está establecida en SSL mediante:
a) Cuadro de propiedades del proyecto: SSL habilitado, establecido en true.
b) Cuadro de propiedades del proyecto: SSL. URL: Asegúrese de que coincide con el componente de host de URL de inicio de sesión que se especificó en el paso 2.
c) Editor de configuración del proyecto: Establezca la URL de inicio con el valor que se especificó en el paso anterior.
	


### Paso 4: Ejecutar el ejemplo

Recompile la solución y ejecútela. Es recomendable que vaya a las propiedades de la solución para comprobar si la URL de inicio coincide con la URL "https" de inicio de sesión que se especificó en el paso 2.

Para explorar el ejemplo, inicie sesión con su inquilino de Office 365 para desarrolladores, seleccione un buzón de correo (es recomendable que cree más buzones y rellene con datos en el portal de administración de Office 365) y recupere los datos.



