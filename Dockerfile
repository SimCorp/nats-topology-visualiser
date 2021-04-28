FROM mcr.microsoft.com/dotnet/core/sdk:5.0 as build

RUN curl -sL https://deb.nodesource.com/setup_14.x | bash
RUN apt-get update && apt-get install -y nodejs

WORKDIR /workspace
COPY . .
RUN dotnet tool restore

RUN npm ci && npm run serve
RUN dotnet publish -c release -o ../../deploy

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
COPY --from=build /workspace/deploy /app
WORKDIR /app
EXPOSE 5050
ENTRYPOINT [ "dotnet", "Server.dll" ]