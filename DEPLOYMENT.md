# LabTrack Lite Deployment Guide

This guide outlines the steps to deploy the LabTrack Lite platform to a production environment.

## 🏗️ Architecture Overview
- **Backend**: ASP.NET Core 8 Web API.
- **Frontend**: React (Vite) Single Page Application.
- **Database**: SQLite (Development) -> Recommend PostgreSQL/SQL Server (Production).
- **Communication**: REST API with JWT Authentication.

---

## 1. Backend Deployment (ASP.NET Core)

### Step 1: Preparation
Ensure your `appsettings.json` has production values or uses environment variables.

### Step 2: Publish the API
Run the following command in the `backend/LabTrackApi` directory:
```powershell
dotnet publish -c Release -o ./publish
```

### Step 3: Hosting Options
- **Azure App Service**: Excellent for .NET. Use "Web App" with Linux or Windows.
- **AWS Elastic Beanstalk**: Good for auto-scaling.
- **Docker**: Containerize the app for any cloud provider.
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet publish -c Release -o /app
  FROM mcr.microsoft.com/dotnet/aspnet:8.0
  WORKDIR /app
  COPY --from=build /app .
  ENTRYPOINT ["dotnet", "LabTrackApi.dll"]
  ```

---

## 2. Frontend Deployment (React + Vite)

### Step 1: Environment Variables
Create a `.env.production` file in the `frontend` directory:
```env
VITE_API_URL=https://your-api-domain.com/api
```

### Step 2: Build the Assets
Run the build command:
```bash
npm run build
```
This generates a `dist` folder containing optimized HTML, JS, and CSS.

### Step 3: Hosting Options
- **Static Hosting**: Vercel, Netlify, or GitHub Pages.
- **Azure Static Web Apps**: Integrates well with GitHub/Azure DevOps.
- **Nginx**: If self-hosting, serve the `dist` folder using Nginx.

---

## 3. Database Migration (PostgreSQL Recommended)
SQLite is great for development, but for production, you should migrate to a robust provider.

1. **Update Connection String**: Point to your production DB in `appsettings.Production.json`.
2. **Entity Framework**:
   ```powershell
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

---

## 4. CI/CD Pipeline (GitHub Actions)
Automate your deployment every time you push to `main`.

```yaml
name: Deploy LabTrack
on:
  push:
    branches: [ main ]
jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      - name: Build & Publish Backend
        run: dotnet publish backend/LabTrackApi -c Release
      - name: Build Frontend
        run: |
          cd frontend
          npm install
          npm run build
```

---

## 🛡️ Production Security Checklist
- [ ] **HTTPS**: Enforce TLS/SSL for all traffic.
- [ ] **JWT Key**: Ensure `Jwt:Key` is a long, random secret stored in a Key Vault (not in source control).
- [ ] **CORS**: Update the policy in `Program.cs` to only allow your frontend domain.
- [ ] **Rate Limiting**: Monitor the Rate Limiter logs to prevent abuse.
- [ ] **Headers**: Ensure `X-Content-Type-Options` and other security headers are active (already implemented in `Program.cs`).
