#o365api-as-apponly-webapp

Rest API 経由で Office 365 のユーザー、メール、予定表、連絡先にアクセスするためにクライアント認証フローを使用する簡易 Web アプリです。

このシナリオでプロトコルがどのように機能するかの詳細については、[クライアントの資格情報を使用したサービス間での呼び出し] (http://msdn.microsoft.com/en-us/library/azure/dn645543.aspx) を参照してください。

Office 365 の "アプリ専用"、別名 'サービスまたはデーモン アプリケーション' の詳細については、次の関連ブログを参照してください: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

## このサンプルの実行方法

このサンプルを実行するには、次のものが必要です。
- Visual Studio 2013
- インターネット接続
- Office 365 Developer サブスクリプション (無料トライアルで十分です)

(https://msdn.microsoft.com/en-us/library/office/fp179924(v=office.15).aspx) でサインアップすることにより、Office 365 Developer サブスクリプションを取得できます。


### 手順 1: このリポジトリを複製またはダウンロードする

シェルまたはコマンド ラインから:

`git clone https://github.com/mattleib/o365api-as-apponly-webapp`


### 手順 2 サンプルを Azure Active Directory テナントに登録する

####条件:関連ブログの説明に従って、アプリの証明書を作成します: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)

1. [Azure 管理ポータル](https://manage.windowsazure.com)にサインインします。
2. 左側のナビゲーションで Active Directory をクリックします。
3. サンプル アプリケーションを登録するディレクトリ テナントをクリックします。
4. [アプリケーション] タブをクリックします。
5. ドロワーで、[追加] をクリックします。
6. [所属組織が開発しているアプリケーションの追加] をクリックします。
7. たとえば "O365AppOnlySample" など、アプリケーションのわかりやすい名前を入力し、[Web アプリケーションや Web API] を選択して、[次へ] をクリックします。
8. サインオン URL には、サンプルのベース URL を入力します (例: `https://localhost:44321/Home`)。 
  - *注*: アプリケーション コードが予期しているため、サインオン URL は **"`Home`"** で終わる必要があります。 
  - *注*: ホスト コンポーネントとして、後でサンプルの実行/デバッグに使用する IIS Express SSL 用の正しいポートであることを確認します。
9. アプリID URI に `https://<your_tenant_name>/O365AppOnlySample` を入力し、`<your_tenant_name>` を Azure AD テナントの名前に置き換えます。[OK] をクリックして登録を完了します。
10. 引き続き Azure ポータルでアプリケーションの [構成] タブをクリックします。
11. クライアント ID の値を見つけて任意の場所にコピーします。これは、後でアプリケーションを構成するときに必要になります。
12. Web アプリの次のアプリケーション アクセス許可を構成します。
  - [他のアプリケーションへのアクセス許可] セクションで、[**Windows Azure Active Directory**] を選択します。 
  - [Windows Azure Active Directory] の [アプリケーションのアクセス許可] ドロップダウンから次のものをチェックします:[**ディレクトリ データの読み取り**]
  - [アプリケーションの追加] を選択し、[**Office 365 Exchange Online**] を追加します
  - [Office 365 Exchange Online] の [アプリケーションのアクセス許可] ドロップダウンから次のものをチェックします:[**ユーザーのメールの読み取り**]
  - [Office 365 Exchange Online] の [アプリケーションのアクセス許可] ドロップダウンから次のものをチェックします:[**ユーザーのカレンダーを読み取る**]
  - [Office 365 Exchange Online] の [アプリケーションのアクセス許可] ドロップダウンから次のものをチェックします:[**ユーザーの連絡先を読み取る**]
13. キー値を表示できるように、構成を保存します。
14. 関連ブログで説明されているように、X.509 パブリック証明書を構成します: (http://blogs.msdn.com/b/exchangedev/archive/2015/01/22/building-demon-or-service-apps-with-office-365-mail-calendar-and-contacts-apis-oauth2-client-credential-flow.aspx)


### 手順 3 サンプルを構成する

1. Visual Studio 2013 でソリューションを開きます。
2. `web.config` ファイルを開きます。
3. アプリ キー `RedirectUriLocalHost` を見つけて、値を手順 2 の値に置き換えます。
4. アプリ キー `ClientId` を見つけて、値を手順 2 のクライアント ID に置き換えます。
5. アプリ キー `ClientCertificatePfx` を見つけて、値を X.509 証明書とプライベート キーがある場所に置き換えます
6. アプリ キー `ClientCertificatePfxPassword` を見つけて、値を X.509 証明書のパスワードとプライベート キーに置き換えます
7. SSL を要求するようにプロジェクトを構成し、次の方法で開始 URL が SSL に設定されていることを確認します。
a) プロジェクト プロパティ ボックス:SSL 有効、true に設定
b) プロジェクト プロパティ ボックス:SSL URL:手順 2 で指定したサインオン URL ホスト コンポーネントと一致することを確認します
c) プロジェクト設定エディター:前の手順で指定した値に開始 URL を設定します
	


### 手順 4 サンプルを実行する

ソリューションを再構築し、実行します。ソリューション プロパティにアクセスして、スタートアップが手順 2 で指定した "https" のサインオン URL と一致するかどうかを確認します。

Office 365 Developer テナントでサインインしてサンプルを探索し、メールボックスを選択して (メールボックスをさらに作成して、Office 365 管理ポータルでデータを入力することもできます)、データを取得します。



