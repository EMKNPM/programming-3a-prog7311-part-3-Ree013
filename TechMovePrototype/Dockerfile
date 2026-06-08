# TechMovePrototype/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TechMovePrototype.csproj", "TechMovePrototype/"]
RUN dotnet restore "TechMovePrototype/TechMovePrototype.csproj"
COPY . ./TechMovePrototype/
WORKDIR "/src/TechMovePrototype"
RUN dotnet publish "TechMovePrototype.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads/contracts
EXPOSE 80
ENTRYPOINT ["dotnet", "TechMovePrototype.dll"]