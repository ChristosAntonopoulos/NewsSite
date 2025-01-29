# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["NewsSite.Server/NewsSite.Server.csproj", "NewsSite.Server/"]
RUN dotnet restore "NewsSite.Server/NewsSite.Server.csproj"

# Copy the server project only
COPY ["NewsSite.Server/", "NewsSite.Server/"]

# Build and publish
WORKDIR "/src/NewsSite.Server"
RUN dotnet build "NewsSite.Server.csproj" -c Release -o /app/build
RUN dotnet publish "NewsSite.Server.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "NewsSite.Server.dll"] 