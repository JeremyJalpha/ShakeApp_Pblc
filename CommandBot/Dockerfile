# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY CommandBot/CommandBot.csproj CommandBot/
COPY CbTsSa_Shared/CbTsSa_Shared.csproj CbTsSa_Shared/
COPY . ./
RUN dotnet restore "CommandBot/CommandBot.csproj"

# Build and publish
WORKDIR /src/CommandBot
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Simple entrypoint - just wait for SQL then start
RUN echo '#!/bin/bash\n\
echo "Waiting for dependencies..."\n\
sleep 30\n\
echo "Starting application..."\n\
exec dotnet CommandBot.dll' > /app/entrypoint.sh \
    && chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]