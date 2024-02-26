FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitNodes.Math/RabbitNodes.Math.csproj", "RabbitNodes.Math/"]
COPY ["TrueCodeTest.RpcClient/TrueCodeTest.RpcClient.csproj", "TrueCodeTest.RpcClient/"]
COPY ["RabbitNodes.Shared/RabbitNodes.Shared.csproj", "RabbitNodes.Shared/"]
RUN dotnet restore "RabbitNodes.Math/RabbitNodes.Math.csproj"
COPY . .
WORKDIR "/src/RabbitNodes.Math"
RUN dotnet build "RabbitNodes.Math.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RabbitNodes.Math.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitNodes.Math.dll"]
