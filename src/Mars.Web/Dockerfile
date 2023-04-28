#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN apt-get update && apt-get install -y curl
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Mars.Web/Mars.Web.csproj", "Mars.Web/"]
COPY ["Mars.MissionControl/Mars.MissionControl.csproj", "Mars.MissionControl/"]
COPY ["Mars.Web.Types/Mars.Web.Types.csproj", "Mars.Web.Types/"]
RUN dotnet restore "Mars.Web/Mars.Web.csproj"
COPY . .
WORKDIR "/src/Mars.Web"
RUN dotnet build "Mars.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mars.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mars.Web.dll"]