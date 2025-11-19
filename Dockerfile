# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Indkob.sln ./
COPY src/Indkob/Indkob.csproj src/Indkob/
RUN dotnet restore src/Indkob/Indkob.csproj
COPY . .
RUN dotnet publish src/Indkob/Indkob.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
VOLUME ["/data"]
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/data/app.db;Cache=Shared"
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Indkob.dll"]
