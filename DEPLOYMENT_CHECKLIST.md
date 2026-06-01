# Naar & Noor - Complete Deployment Checklist

## ✅ What's Ready for Deployment

### Backend API
- ✅ All 7 controllers implemented and discoverable
- ✅ Swagger configuration cleaned and simplified  
- ✅ No OpenAPI complexity - lean, fast configuration
- ✅ Release build created and ready to deploy
- ✅ Database migrations complete
- ✅ All endpoints have proper error handling

### Frontend
- ✅ Angular production build optimized
- ✅ Vercel routing fixed (filesystem handler before SPA fallback)
- ✅ Environment variables correctly configured
- ✅ All API service methods functional
- ✅ Error handling on all data loads
- ✅ Deployed and running at https://naar-noor.vercel.app

### Documentation
- ✅ API integration guide created (API_INTEGRATION.md)
- ✅ Swagger endpoints reference created (SWAGGER_ENDPOINTS.md)
- ✅ Azure deployment guide created (DEPLOYMENT_AZURE.md)
- ✅ Deployment script provided (deploy-backend.ps1)

---

## 🚀 Deploy Backend to Azure (URGENT - Required for Frontend)

### Step 1: Build Release Version
```powershell
cd "c:\Users\Memo\Downloads\Deeper-Insight\New folder (2)\New folder (2)\Naar-Noor"
cd api-server/src/NaarNoor.API
dotnet publish -c Release -o publish
```
✓ Already done - `publish` folder is ready

### Step 2: Deploy to Azure App Service

**Option A: Using PowerShell Script (EASIEST)**
```powershell
cd "c:\Users\Memo\Downloads\Deeper-Insight\New folder (2)\New folder (2)\Naar-Noor"

# First time only - login to Azure
.\deploy-backend.ps1 -Login

# Subsequent deployments
.\deploy-backend.ps1
```

**Option B: Manual Azure CLI**
```powershell
# Go to project root
cd "c:\Users\Memo\Downloads\Deeper-Insight\New folder (2)\New folder (2)\Naar-Noor"

# Create ZIP of published files
Compress-Archive -Path "api-server/src/NaarNoor.API/publish/*" -DestinationPath "deploy.zip" -Force

# Deploy
az webapp deployment source config-zip `
  --resource-group "naar-noor-rg" `
  --name "naar-noor" `
  --src "deploy.zip"
```

**Option C: Using Visual Studio**
1. Right-click `NaarNoor.API` project
2. Select "Publish"
3. Choose Azure App Service
4. Select existing app service named `naar-noor`
5. Click "Publish"

### Step 3: Verify Deployment (Wait 2-3 minutes for restart)

```powershell
# Test Reviews endpoint
curl https://naar-noor.runasp.net/api/reviews

# Test Chefs endpoint
curl https://naar-noor.runasp.net/api/chefs

# Test Menu endpoint
curl https://naar-noor.runasp.net/api/menu

# View Swagger documentation
start https://naar-noor.runasp.net/api/docs
```

Expected Results:
- ✓ All endpoints return JSON data (not errors)
- ✓ Swagger UI loads with all 7 endpoints visible
- ✓ No 404, 503, or 504 errors

---

## ✅ Frontend Verification (After Backend Deployed)

Visit: https://naar-noor.vercel.app

Verify these sections load without errors:

### Section 1: "What Our Guests Say" (Reviews)
- [ ] Section displays without "Unable to load" error
- [ ] Shows customer reviews with ratings
- [ ] Reviews come from backend (not hardcoded)

### Section 2: "Meet Our Chefs" (Chefs)
- [ ] Section displays without "Unable to load" error
- [ ] Shows chef names, titles, and bios
- [ ] Data comes from backend

### Section 3: "Crafted With Passion" (Menu)
- [ ] Section displays without error
- [ ] Shows menu items with prices
- [ ] Categories are populated
- [ ] Data comes from backend

### Section 4: "Book a Table" (Reservations)
- [ ] Form displays completely
- [ ] Can select date and time
- [ ] Submit button works without errors

### Section 5: Contact Section
- [ ] Form displays completely
- [ ] Can fill and submit without errors

### Additional Checks
- [ ] No console errors (F12 → Console tab)
- [ ] No 504 Gateway Timeout errors
- [ ] All page sections load smoothly
- [ ] No "shimmer effect stuck" on initial load

---

## 📊 Swagger Documentation Access

### For Development
```
http://localhost:8080/api/docs
```

### For Production
```
https://naar-noor.runasp.net/api/docs
```

### What You Should See

All 7 endpoints visible:
1. ✓ GET /api/reviews
2. ✓ GET /api/chefs
3. ✓ GET /api/menu
4. ✓ POST /api/reservations
5. ✓ POST /api/contact
6. ✓ POST /api/orders
7. ✓ GET /health

Each endpoint shows:
- Description of what it does
- Request/response schemas
- Status codes
- Example data

---

## 🔧 Endpoint Testing

### Get Reviews
```bash
curl -X GET https://naar-noor.runasp.net/api/reviews
```

### Get Chefs
```bash
curl -X GET https://naar-noor.runasp.net/api/chefs
```

### Get Menu Items
```bash
curl -X GET https://naar-noor.runasp.net/api/menu
curl -X GET "https://naar-noor.runasp.net/api/menu?category=Starters"
```

### Create Reservation
```bash
curl -X POST https://naar-noor.runasp.net/api/reservations \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "+44123456789",
    "reservationDate": "2026-06-15",
    "reservationTime": "19:00",
    "partySize": 4,
    "specialRequests": "Window seat if possible"
  }'
```

### Create Contact
```bash
curl -X POST https://naar-noor.runasp.net/api/contact \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane Smith",
    "email": "jane@example.com",
    "phoneNumber": "+44987654321",
    "subject": "Catering Inquiry",
    "message": "We would like to inquire about catering services"
  }'
```

### Check Health
```bash
curl https://naar-noor.runasp.net/health
```

---

## ⚠️ Troubleshooting

### Issue: 504 Gateway Timeout
**Cause**: Backend API not responding (crashed, not deployed, or slow restart)
**Fix**:
1. Check Azure Portal > App Service > Overview
2. Verify App Service is "Running"
3. Check Application Insights for errors
4. Redeploy the backend using deploy-backend.ps1
5. Wait 2-3 minutes for app to start

### Issue: Swagger Shows Blank Page
**Cause**: Swagger UI assets not loading
**Fix**:
1. Clear browser cache (Ctrl+Shift+Delete)
2. Try incognito/private window
3. Check browser console for errors (F12)
4. Verify `/api/docs` route is accessible

### Issue: Reviews/Chefs/Menu Still Not Loading
**Cause**: Frontend still has stale production build
**Fix**:
1. Hard refresh frontend (Ctrl+Shift+R or Cmd+Shift+R)
2. Clear Vercel cache: Delete `.next` folder and redeploy
3. Wait 5 minutes for Vercel to rebuild and deploy

### Issue: CORS Errors in Console
**Cause**: Backend CORS not configured for frontend URL
**Fix**:
1. Check Program.cs CORS configuration
2. Verify `https://naar-noor.vercel.app` is in AllowedOrigins
3. Redeploy backend after fixing

### Issue: Database Connection Fails
**Cause**: Connection string not set in Azure App Service
**Fix**:
1. Go to Azure Portal > App Service > Configuration
2. Add Connection String:
   - Name: `ConnectionStrings__DefaultConnection`
   - Value: Azure SQL connection string
   - Type: SQLAzure
3. Click Save and Restart app

---

## 📝 Files Changed in This Update

### Swagger Configuration (Simplified)
- `api-server/src/NaarNoor.API/Middleware/SwaggerMiddleware.cs` - Removed OpenAPI complexity
- `api-server/src/NaarNoor.API/Configuration/SwaggerServiceConfiguration.cs` - Minimal configuration

### Documentation (New)
- `docs/API_INTEGRATION.md` - Complete endpoint mapping
- `docs/SWAGGER_ENDPOINTS.md` - All 7 endpoints reference
- `docs/DEPLOYMENT_AZURE.md` - Azure deployment steps

### Deployment (New)
- `deploy-backend.ps1` - Automated deployment script
- `api-server/src/NaarNoor.API/publish/` - Release build artifacts

---

## ✨ After Successful Deployment

### User Experience
- ✅ Frontend loads fast at https://naar-noor.vercel.app
- ✅ All sections (Reviews, Chefs, Menu) load without errors
- ✅ Forms (Reservation, Contact, Order) work smoothly
- ✅ No 504 errors or timeouts
- ✅ Data is real, not fallback

### Developer Experience
- ✅ Swagger docs accessible at https://naar-noor.runasp.net/api/docs
- ✅ All 7 endpoints documented
- ✅ Can test endpoints directly from Swagger UI
- ✅ Can curl endpoints from command line
- ✅ Clear error responses with status codes

### Monitoring
- ✅ Backend API responding within 200ms
- ✅ Database queries performant
- ✅ No error logs in Application Insights
- ✅ Health check endpoint accessible

---

## 🎯 Next Steps

1. **Deploy Backend** (using one of 3 methods above)
2. **Verify Endpoints** (test with curl or Swagger UI)
3. **Test Frontend** (visit https://naar-noor.vercel.app)
4. **Verify All Sections** (check Reviews, Chefs, Menu, Forms)
5. **Monitor** (check Application Insights for errors)

**Estimated Time**: 10-15 minutes for deployment + 3 min restart = 15-20 minutes total

---

## 📞 Quick Reference

**Swagger UI**: https://naar-noor.runasp.net/api/docs
**Swagger JSON**: https://naar-noor.runasp.net/swagger/v1/swagger.json
**Health Check**: https://naar-noor.runasp.net/health
**Frontend**: https://naar-noor.vercel.app

**Database**: Azure SQL at `db54355.public.databaseasp.net`
**App Service**: `naar-noor.azurewebsites.net`
**Resource Group**: `naar-noor-rg`
