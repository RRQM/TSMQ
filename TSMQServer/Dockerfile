# Stage 1: Build Vue frontend
FROM node:22-alpine AS frontend-build

WORKDIR /workspace

# Create directory structure matching vite outDir: ../TSMQServer/TSMQServer.Web/build
RUN mkdir -p TSMQServer/TSMQServer.Web/build

WORKDIR /workspace/TSMQWeb

# Install dependencies first for better layer caching
COPY TSMQWeb/package.json ./
RUN npm install --ignore-scripts

COPY TSMQWeb/ ./
RUN npm run build

# Stage 2: Build .NET application with AOT
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# linux-x64 for amd64, linux-arm64 for arm64
ARG RUNTIME_ID=linux-x64

# Install native AOT prerequisites (clang + zlib)
RUN apt-get update && apt-get install -y --no-install-recommends \
    clang \
    zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

COPY Directory.Build.props ./
COPY TSMQServer/ ./TSMQServer/

# Copy frontend build output to embedded resource location
COPY --from=frontend-build /workspace/TSMQServer/TSMQServer.Web/build ./TSMQServer/TSMQServer.Web/build/

WORKDIR /src/TSMQServer
RUN dotnet publish TSMQServer.App/TSMQServer.App.csproj \
    -c Release \
    -r ${RUNTIME_ID} \
    --self-contained true \
    /p:PublishAot=true \
    /p:DebugType=None \
    /p:DebugSymbols=false \
    -o /app/publish

# Stage 3: Minimal runtime dependencies for AOT (no .NET runtime needed)
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 7789

ENTRYPOINT ["./TSMQServer.App"]
