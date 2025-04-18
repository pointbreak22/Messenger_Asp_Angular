# Этап 1: Сборка Angular-приложения
FROM node:18 AS build_client
WORKDIR /app

# Копируем package.json и устанавливаем зависимости
COPY ChatClient/package*.json ./ 
RUN npm install

# Копируем весь проект и собираем его для продакшн
COPY ChatClient ./ 
RUN npm run build --prod  # Строим проект с флагом --prod

# Этап 2: Сборка .NET API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build_backend
WORKDIR /src

# Копируем .NET проект и восстанавливаем зависимости
COPY ChatApi/ChatApi.csproj ./ 
RUN dotnet restore "ChatApi.csproj"
COPY ChatApi/ ./ 
RUN dotnet publish "ChatApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 3: Финальный образ (на базе dotnet runtime) + nginx + supervisor
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Устанавливаем nginx и supervisor
RUN apt-get update && \
    apt-get install -y nginx supervisor && \
    rm -rf /var/lib/apt/lists/*

# Копируем опубликованное .NET-приложение в финальный образ
COPY --from=build_backend /app/publish /app/publish

# Копируем собранное Angular-приложение (статические файлы)
COPY --from=build_client /app/dist/chat-client/browser /usr/share/nginx/html

# Копируем конфигурацию для Nginx и Supervisor
COPY ./nginx/default.conf /etc/nginx/conf.d/default.conf
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

# Логи Supervisor
RUN mkdir -p /var/log/supervisor

# Открываем порты
EXPOSE 80  
# для Nginx (фронтенд)
EXPOSE 5000
# для API .NET

# Запуск supervisor
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
