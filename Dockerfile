FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY publish .
ENTRYPOINT ["dotnet", "tower-admin-portal.dll"]
