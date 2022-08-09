import { AuthConfig } from 'angular-oauth2-oidc';
import { env } from 'process';

export const authConfig: AuthConfig = {
  // Url des Authorization-Servers
  issuer: env.SCRUFFY_OAUTH_ISSUER,

  // Url der Angular-Anwendung
  // An diese URL sendet der Authorization-Server den Access Code
  redirectUri: window.location.origin + '/',

  // Name der Angular-Anwendung
  clientId: env.SCRUFFY_OAUTH_CLIEND_ID,
  dummyClientSecret: env.SCRUFFY_OAUTH_CLIEND_SECRET,

  // Rechte des Benutzers, die die Angular-Anwendung wahrnehmen möchte
  scope: 'openid profile offline_access api_v1',

  // Code Flow (PKCE ist standardmäßig bei Nutzung von Code Flow aktiviert)
  responseType: 'code',
};
