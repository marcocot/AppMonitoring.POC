FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["AppMonitoring.POC.csproj", "./"]
RUN dotnet restore "AppMonitoring.POC.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "AppMonitoring.POC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AppMonitoring.POC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppMonitoring.POC.dll"]
