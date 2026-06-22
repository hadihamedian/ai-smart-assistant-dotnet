FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["src/SmartAssistant.API/SmartAssistant.API.csproj", "src/SmartAssistant.API/"]
COPY ["src/SmartAssistant.Application/SmartAssistant.Application.csproj", "src/SmartAssistant.Application/"]
COPY ["src/SmartAssistant.Domain/SmartAssistant.Domain.csproj", "src/SmartAssistant.Domain/"]
COPY ["src/SmartAssistant.Infrastructure/SmartAssistant.Infrastructure.csproj", "src/SmartAssistant.Infrastructure/"]
RUN dotnet restore "src/SmartAssistant.API/SmartAssistant.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/SmartAssistant.API"
RUN dotnet build "SmartAssistant.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartAssistant.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartAssistant.API.dll"]