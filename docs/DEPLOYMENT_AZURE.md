# Azure App Service Deployment Guide

## Backend API Deployment to Azure App Service

### Prerequisites
- Azure CLI installed (`az` command available)
- Azure account with subscription access
- Access to the existing Azure App Service at `naar-noor.runasp.net`

### Quick Deployment Steps

#### 1. Build the Backend
```powershell
cd api-server/src/NaarNoor.API
dotnet publish -c Release -o publish
```

#### 2. Deploy Using Azure CLI

**Option A: Using Azure CLI (Recommended)**

```powershell
# Login to Azure
az login

# Get your subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Deploy the published files
az webapp deployment source config-zip \
  --resource-group "naar-noor-rg" \
  --name "naar-noor" \
  --src "publish.zip"
```

**Option B: Using Visual Studio**

1. Right-click `NaarNoor.API` project
2. Select "Publish"
3. Choose "Azure App Service"
4. Select the existing app service `naar-noor`
5. Click "Publish"

**Option C: Using FTP**

1. Go to Azure Portal > App Service > Deployment credentials
2. Set your FTP username/password
3. Use FTP client to connect and upload contents of `publish/` folder

#### 3. Verify Deployment

Once deployed, verify the endpoints:

```bash
# Check API is running
curl https://naar-noor.runasp.net/api

# Test Reviews endpoint
curl https://naar-noor.runasp.net/api/reviews

# Test Chefs endpoint
curl https://naar-noor.runasp.net/api/chefs

# Test Menu endpoint
curl https://naar-noor.runasp.net/api/menu

# Check Swagger docs
open https://naar-noor.runasp.net/api/docs
```

### Environment Variables Required

Ensure these are set in Azure App Service Configuration:

- `ConnectionStrings__DefaultConnection`: Azure SQL connection string
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `AllowedOrigins`: `https://naar-noor.vercel.app`

### Troubleshooting

**Issue: 404 Not Found**
- Ensure all controllers have `[ApiController]` attribute
- Check routes are correct: `/api/[controller]`
- Verify web.config exists in publish folder

**Issue: 504 Gateway Timeout**
- Check Application Insights logs in Azure Portal
- Verify database connection string is correct
- Check if App Service plan has enough resources
- Increase timeout in Azure Application Gateway if needed

**Issue: CORS Errors**
- Verify `AllowedOrigins` is set to frontend URL
- Check CORS configuration in Program.cs

**Issue: Swagger not loading**
- Swagger is configured to serve at `/api/docs`
- Ensure App Service doesn't have URL rewrite rules blocking this path

### Automated Deployment (GitHub Actions)

For continuous deployment, create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches: [ main ]
    paths:
      - 'api-server/**'

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Build
        working-directory: ./api-server/src/NaarNoor.API
        run: dotnet publish -c Release -o ${{ runner.temp }}/publish
      
      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'naar-noor'
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: ${{ runner.temp }}/publish
```

To set up:
1. In Azure Portal, download publish profile from App Service overview
2. Add as GitHub secret: `AZURE_PUBLISH_PROFILE`
3. Commit the workflow file

### Database Migrations

If database schema has changed:

```powershell
# Apply pending migrations
dotnet ef database update --project NaarNoor.Infrastructure --startup-project NaarNoor.API
```

Or use Azure Portal:
1. Open App Service > Database connections
2. Run migrations through SQL command interface
