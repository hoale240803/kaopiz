#!/bin/bash

# Build script for KaopizAuth
set -e

echo "🏗️  Building KaopizAuth Solution..."

# Restore dependencies
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build solution
echo "🔨 Building solution..."
dotnet build --configuration Release --no-restore

# Build WebApp
echo "🌐 Building WebApp..."
cd src/KaopizAuth.WebApp
npm ci
npm run build
cd ../..

echo "✅ Build completed successfully!"