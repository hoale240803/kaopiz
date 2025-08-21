#!/bin/bash

# Test script for KaopizAuth
set -e

echo "🧪 Running KaopizAuth Tests..."

# Run .NET tests
echo "🔬 Running backend tests..."
dotnet test --configuration Release --verbosity normal

# Run frontend tests (when we have them)
echo "🌐 Running frontend tests..."
cd src/KaopizAuth.WebApp

# Type checking
echo "📝 Type checking..."
npm run type-check

# Linting
echo "🧹 Linting..."
npm run lint

cd ../..

echo "✅ All tests passed successfully!"