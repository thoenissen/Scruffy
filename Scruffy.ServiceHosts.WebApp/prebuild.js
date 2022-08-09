const fs = require("fs");

var environmentContent = `
function getEnvironmentString(name) {
  throw "Invalid configuration";
}
`;

fs.writeFile("./src/assets/js/environment.js", environmentContent, (err) => {
  if (err) {
    console.error(err);
  }
});

var environmentTemplateContent = `
var variables = {
  oauthIssuer: "\${SCRUFFY_OAUTH_ISSUER}",
  oauthClientId: "\${SCRUFFY_OAUTH_CLIENT_ID}",
  oauthClientSecret: "\${SCRUFFY_OAUTH_CLIENT_SECRET}",
  webApiBaseUrl: "\${SCRUFFY_WEBAPI_BASE_URL}",
};

function getEnvironmentString(name) {
  return variables[name];
}
`;

fs.writeFile(
  "./src/assets/js/environment.template.js",
  environmentTemplateContent,
  (err) => {
    if (err) {
      console.error(err);
    }
  }
);
