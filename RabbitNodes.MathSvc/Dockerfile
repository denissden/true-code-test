FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitNodes.MathSvc/RabbitNodes.MathSvc.csproj", "RabbitNodes.MathSvc/"]
COPY ["RabbitNodes.Shared/RabbitNodes.Shared.csproj", "RabbitNodes.Shared/"]
COPY ["TrueCodeTest.RpcClient/TrueCodeTest.RpcClient.csproj", "TrueCodeTest.RpcClient/"]
RUN dotnet restore "RabbitNodes.MathSvc/RabbitNodes.MathSvc.csproj"
COPY . .
WORKDIR "/src/RabbitNodes.MathSvc"
RUN dotnet build "RabbitNodes.MathSvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RabbitNodes.MathSvc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitNodes.MathSvc.dll"]
