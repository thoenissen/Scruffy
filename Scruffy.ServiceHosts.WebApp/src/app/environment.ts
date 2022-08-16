export const environment = {
  //@ts-ignore
  production: isProduction(),
  //@ts-ignore
  oauthIssuer: getEnvironmentString('oauthIssuer'),
  //@ts-ignore
  oauthClientId: getEnvironmentString('oauthClientId'),
  //@ts-ignore
  oauthClientSecret: getEnvironmentString('oauthClientSecret'),
  //@ts-ignore
  webApiBaseUrl: getEnvironmentString('webApiBaseUrl'),
};
