# ========== build ==========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# копируем только csproj для быстрого кешируемого restore
COPY src/InternationalShopper.Bot/InternationalShopper.Bot.csproj src/InternationalShopper.Bot/
RUN dotnet restore src/InternationalShopper.Bot/InternationalShopper.Bot.csproj

# теперь всё остальное
COPY . .
RUN dotnet publish src/InternationalShopper.Bot/InternationalShopper.Bot.csproj -c Release -o /app

# ========== run ==========
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# PaaS (Koyeb и др.) задаёт PORT; слушаем его
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

COPY --from=build /app .
ENTRYPOINT ["dotnet","InternationalShopper.Bot.dll"]
