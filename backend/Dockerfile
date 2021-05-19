FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src/
COPY . .
RUN dotnet publish -o /app/

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app/
EXPOSE 80
ENTRYPOINT ["dotnet", "backend.dll"]
COPY --from=build /app/ .