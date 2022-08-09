# Stage 1 - Build app

# Node server - https://hub.docker.com/_/node
FROM node:alpine AS build-stage

# Working directory
WORKDIR /app

# Copying all sources
COPY . .

# Building the app
RUN npm ci && npm run build

# Stage 2 - Build image

# Nginx web service - https://hub.docker.com/_/nginx
FROM nginx:alpine

# copy the distribution files of the web app
COPY --from=build-stage /app/dist/WebApp /usr/share/nginx/html

# exposing port 80
EXPOSE 80

# Replacing the environment variables and then starting the nginx server
CMD ["/bin/sh",  "-c",  "envsubst < /usr/share/nginx/html/assets/js/environment.template.js > /usr/share/nginx/html/assets/js/environment.js && exec nginx -g 'daemon off;'"]
