const fs = require("fs");

require("dotenv").config();

var environmentContent =
  `
var variables = {
  oauthIssuer: "` +
  process.env.SCRUFFY_OAUTH_ISSUER +
  `",
  oauthClientId: "` +
  process.env.SCRUFFY_OAUTH_CLIENT_ID +
  `",
  oauthClientSecret: "` +
  process.env.SCRUFFY_OAUTH_CLIENT_SECRET +
  `",
  webApiBaseUrl: "` +
  process.env.SCRUFFY_WEBAPI_BASE_URL +
  `",
};

function getEnvironmentString(name) {
  return variables[name];
}
`;

fs.writeFile("./src/assets/js/environment.js", environmentContent, (err) => {
  if (err) {
    console.error(err);
  }
});
