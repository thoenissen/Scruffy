import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from './environment';

export const authConfig: AuthConfig = {
  // Url des Authorization-Servers
  issuer: environment.oauthIssuer,

  // Url der Angular-Anwendung
  // An diese URL sendet der Authorization-Server den Access Code
  redirectUri: window.location.origin + '/',

  // Name der Angular-Anwendung
  clientId: environment.oauthClientId,
  dummyClientSecret: environment.oauthClientSecret,

  // Rechte des Benutzers, die die Angular-Anwendung wahrnehmen möchte
  scope: 'openid profile offline_access api_v1',

  // Code Flow (PKCE ist standardmäßig bei Nutzung von Code Flow aktiviert)
  responseType: 'code',
};
