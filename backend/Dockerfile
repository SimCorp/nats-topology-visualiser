FROM mcr.microsoft.com/dotnet/aspnet:5.0

COPY ./publish /publish 
COPY ./.env /publish/.env 

WORKDIR /publish
EXPOSE 5000
ENTRYPOINT ["dotnet", "backend.dll"]

