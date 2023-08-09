console.log("prebuild start");

const fs = require("fs");

if (fs.existsSync("./src/assets/js") == false) {
  fs.mkdirSync("./src/assets/js/");
}

var environmentContent = `
function getEnvironmentString(name) {
  throw "Invalid configuration";
}

function isProduction() {
  return true;
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

function isProduction() {
  return true;
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

console.log("prebuild end");
