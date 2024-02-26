FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitNodes.FibonacciSvc/RabbitNodes.FibonacciSvc.csproj", "RabbitNodes.FibonacciSvc/"]
COPY ["RabbitNodes.Shared/RabbitNodes.Shared.csproj", "RabbitNodes.Shared/"]
COPY ["TrueCodeTest.RpcClient/TrueCodeTest.RpcClient.csproj", "TrueCodeTest.RpcClient/"]
RUN dotnet restore "RabbitNodes.FibonacciSvc/RabbitNodes.FibonacciSvc.csproj"
COPY . .
WORKDIR "/src/RabbitNodes.FibonacciSvc"
RUN dotnet build "RabbitNodes.FibonacciSvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RabbitNodes.FibonacciSvc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitNodes.FibonacciSvc.dll"]
