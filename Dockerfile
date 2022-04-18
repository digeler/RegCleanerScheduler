#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RegCleanerScheduler.csproj", "."]
RUN dotnet restore "./RegCleanerScheduler.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "RegCleanerScheduler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RegCleanerScheduler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RegCleanerScheduler.dll"]
RUN 	apt-get update ; \
		apt-get install -y curl procps zip unzip htop wget ; \
		curl -sSL https://aka.ms/getvsdbgsh -o /tmp/getvsdbg.sh ; \
		bash /tmp/getvsdbg.sh -v latest -l /vsdbg ; \
		rm /tmp/getvsdbg.sh ; \
		apt-get purge -y zip unzip ; \
