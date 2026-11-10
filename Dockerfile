FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Note the paths include the folder name now
COPY ["MaunoWeb/MaunoWeb.csproj", "MaunoWeb/"]
RUN dotnet restore "MaunoWeb/MaunoWeb.csproj"

COPY . .
WORKDIR "/src/MaunoWeb"
RUN dotnet publish "MaunoWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "MaunoWeb.dll"]