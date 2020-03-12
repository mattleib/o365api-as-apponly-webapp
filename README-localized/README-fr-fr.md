#o365api-as-apponly-webapp

Exemple d’application web qui utilise le flux des informations d’identification du client pour accéder aux utilisateurs, au courrier, au calendrier et aux contacts dans Office 365 via les API REST.

Pour plus d’informations sur le fonctionnement des protocoles dans ce scénario, voir \[Appels de service aux appels de service à l’aide des informations d’identification du client] (http://msdn.microsoft.com/en-us/library/azure/dn645543.aspx)

Pour plus d’informations sur « app-only », alias « applications service ou daemon » dans Office 365, voir le blog associé à l’article suivant : (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

## Comment exécuter cet exemple

Pour exécuter cet exemple, vous avez besoin des éléments suivants :
-Visual Studio 2013
-Une connexion Internet
-Un abonnement Office 365 Developer (une version d’évaluation gratuite est suffisante)

Vous pouvez obtenir un abonnement Office 365 Developer en vous inscrivant à (https://msdn.microsoft.com/en-us/library/office/fp179924(v=office.15).aspx)


### Étape 1 : Clonage ou téléchargement de ce référentiel

À partir de votre shell ou de la ligne de commande :

`git clone https://github.com/mattleib/o365api-as-apponly-webapp`


### Étape 2 inscrire l’exemple auprès de votre client Azure Active Directory

####Condition préalable : Créez un certificat pour votre application, comme décrit dans le blog associé : (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

1. Connectez-vous au [portail de gestion Azure](https://manage.windowsazure.com).
2. Cliquez sur Active Directory dans le menu de navigation de gauche.
3. Cliquez sur le client de l’annuaire dans lequel vous voulez inscrire l’exemple d’application.
4. Cliquez sur l’onglet Applications.
5. Dans le tiroir, cliquez sur Ajouter.
6. Sélectionnez «Ajouter une application développée par mon organisation».
7. Entrez un nom pour l’application (par exemple, «AppExempleO365»), sélectionnez «Application Web et/ou API Web», puis cliquez sur Suivant.
8. Pour l’URL d’authentification, entrez l’URL de base pour l’exemple, par exemple `https://localhost:44321/Home`. 
  - *Remarque* : L’URL de connexion doit se terminer par **«`Accueil`»** comme prévu par le code d’application. 
  - *Remarque* : En tant que composant hôte, assurez-vous que est le port approprié pour votre SSL IIS Express que vous utiliserez par la suite pour l’exécution/le débogage de l’exemple.
9. Pour l’URI ID d’application, entrez `https://< nom_du_client >/AppExempleO365`, en remplaçant `< nom_du_client >` par le nom de votre client Azure AD. Cliquez sur OK pour terminer l’inscription.
10. Dans le portail Azure, cliquez sur l’onglet configurer de votre application.
11. Trouver la valeur ID client et la copier, vous en aurez besoin plus tard lors de la configuration de votre application.
12. Configurez les autorisations d’application suivantes pour l’application Web :
  - Dans la section « autorisations pour les autres applications », sélectionnez **« Windows Azure Active Directory »** 
  - Dans la liste déroulante « Autorisation application » pour « Windows Azure Active Directory », vérifiez : **« Lire les données d’annuaire »**
  - Sélectionnez « Ajouter une application », puis ajoutez **« Office 365 Exchange Online »**
  - Dans la liste déroulante « Autorisation application » pour « Office 365 Exchange Online », vérifiez : **« Lire le courrier des utilisateurs »**
  - Dans la liste déroulante « Autorisation application » pour « Office 365 Exchange Online », vérifiez : **« Lire le calendrier des utilisateurs »**
  - Dans la liste déroulante « Autorisation application » pour « Office 365 Exchange Online », vérifiez : **« Lire les contacts des utilisateurs »**
13. Enregistrez la configuration afin de pouvoir afficher la valeur de la clé.
14. Configurez le certificat public X.509 comme expliqué dans le blog associé : (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)


### Étape 3 configurer l’exemple

1. Ouvrez la solution dans Visual Studio 2013.
2. Ouvrez le fichier `web.config`.
3. Recherchez la clé d’application `RedirectUriLocalHost` et remplacez la valeur par la valeur à l’étape 2.
4. Recherchez la clé d’application `ClientId` et remplacez la valeur par l’ID client de l’étape 2.
5. Recherchez la clé d’application `ClientCertificatePfx` et remplacez la valeur par l’emplacement où se trouve votre certificat X.509 contenant la clé privée.
6. Recherchez la clé d’application `ClientCertificatePfxPassword` et remplacez la valeur par le mot de passe de votre certificat X.509 contenant la clé privée.
7. Configurez le projet pour qu’il requiert SSL et vérifiez que la zone URL de démarrage est configurée sur SSL par :
a) Zone Propriétés du projet : SSL activé, configuré sur true
b) Zone Propriétés du projet : URL SSL : Assurez-vous qu’il correspond au composant hôte d’URL d’authentification, comme spécifié à l’étape 2
c) Éditeur de paramètres de projet : Définit l’URL de début sur la valeur spécifiée dans l’étape précédente
	


### Étape 4 : exécuter l’exemple

Reconstituez la solution et exécutez-la. Vous pouvez accéder aux propriétés de la solution pour vérifier si le démarrage correspond à votre URL d’authentification « HTTPS », comme spécifié à l’étape 2.

Explorez l’exemple en vous connectant à l’aide de votre client Office 365 pour les développeurs, sélectionnez une boîte aux lettres (vous pouvez créer d’autres boîtes aux lettres et remplir les données du portail d’administration 365 Office) et récupérer des données.



