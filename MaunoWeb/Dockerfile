# Build stage: use .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj file (adjust based on your actual project filename)
COPY ["MaunoWeb/MaunoWeb.csproj", "MaunoWeb/"]
RUN dotnet restore "MaunoWeb/MaunoWeb.csproj"

# Copy all source code
COPY . .
WORKDIR "/src/MaunoWeb"
RUN dotnet publish "MaunoWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage: use .NET 10 ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render requires the app to listen on port 10000
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "MaunoWeb.dll"]
