# Stage 1 - Build app

FROM node:14-alpine AS build-stage
WORKDIR /app
COPY . .
RUN npm ci && npm run build

# Stage 2 - Build image

# Nginx web service - https://hub.docker.com/_/nginx
FROM nginx:alpine

# copy the distribution files of the web app
COPY --from=build-stage /app/dist/WebApp /usr/share/nginx/html

# exposing port 80
EXPOSE 80
