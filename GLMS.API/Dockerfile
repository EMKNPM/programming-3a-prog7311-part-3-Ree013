# GLMS.API/Dockerfile
# Stage 1: Build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GLMS.API.csproj", "GLMS.API/"]
RUN dotnet restore "GLMS.API/GLMS.API.csproj"
COPY . ./GLMS.API/
WORKDIR "/src/GLMS.API"
RUN dotnet publish "GLMS.API.csproj" -c Release -o /app/publish

# Stage 2: Runtime image (smaller, no SDK)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "GLMS.API.dll"]