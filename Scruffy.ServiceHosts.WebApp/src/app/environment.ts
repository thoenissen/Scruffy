export const environment = {
  production: false,
  //@ts-ignore
  oauthIssuer: getEnvironmentString('oauthIssuer'),
  //@ts-ignore
  oauthClientId: getEnvironmentString('oauthClientId'),
  //@ts-ignore
  oauthClientSecret: getEnvironmentString('oauthClientSecret'),
  //@ts-ignore
  webApiBaseUrl: getEnvironmentString('webApiBaseUrl'),
};
