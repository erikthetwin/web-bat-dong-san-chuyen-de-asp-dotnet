# Design: Real Estate Trading Website (ASP.NET)

**Date:** 2026-08-19
**Course:** Chuyên đề ASP.NET — Đại học Trà Vinh
**Student:** Ngô Ngọc Hưng (470124093, VX24TTK4)
**Advisor:** TS. Nguyễn Nhứt Lam

## 1. Goal

Build full stack real estate trading website per report "Xây dựng website mua bán bất động sản" and grading sheet. Website connects buyers, sellers, and admins. Bonus feature: machine learning price prediction.

## 2. Tech Stack (approved)

- ASP.NET Core MVC (.NET 8), Razor Views
- EF Core + SQLite (zero install, runs on school machines)
- ASP.NET Identity with roles: `Admin`, `Seller`, `Buyer`
- Bootstrap 5 UI, Leaflet + OpenStreetMap for map
- ML.NET in-process regression model (`model.zip`), no external service

## 3. Roles & Permissions

| Role | Permissions |
|---|---|
| Guest | browse listings, search, filter, view detail, ML predict page |
| Buyer (registered) | guest + save/unsave favorites, contact seller (creates ContactRequest), manage own profile |
| Seller | buyer + post listings, edit/delete own listings, upload images, manage own posts |
| Admin | moderate listings (approve/reject/ban), manage users (block/unblock), manage property types, view reports/statistics |

Registration form has role selector (Buyer/Seller) — new account gets selected role directly. Admin can change roles later via `/Admin/Users`. Admin account seeded.

## 4. Database Model (EF Core, SQLite)

Entities:

- **AppUser** (extends IdentityUser): `FullName`, `Phone`, `Address`, `AvatarUrl`
- **PropertyType**: `Id`, `Name` (Căn hộ, Nhà riêng, Đất, Nhà mặt tiền, Mặt bằng kinh doanh, Biệt thự), `IsActive`
- **Property**: `Id`, `Title`, `Description`, `Price` (decimal, CHECK > 0), `Area` (decimal m², CHECK > 0), `Bedrooms`, `Bathrooms`, `Floors`, `FacadeWidth` (mặt tiền), `District` (quận/huyện), `Ward`, `Street`, `Address`, `Latitude`, `Longitude`, `PropertyTypeId` FK, `OwnerId` FK (AppUser), `Status` enum (Pending / Approved / Rejected / Banned / Sold), `IsForRent` (mua/thuê), `ContactPhone`, `CreatedAt`, `UpdatedAt`
- **PropertyImage**: `Id`, `PropertyId` FK (cascade delete), `ImageUrl`, `IsPrimary`
- **Favorite**: `Id`, `UserId` FK, `PropertyId` FK, `SavedAt` — unique constraint (UserId, PropertyId)
- **ContactRequest**: `Id`, `PropertyId` FK, `UserId` FK nullable, `Name`, `Phone`, `Message`, `CreatedAt`

Integrity constraints: FKs on all relations; unique (Favorite.UserId, Favorite.PropertyId); CHECK Price > 0, Area > 0; required string fields non-null; cascade delete Property → Images/Favorites/ContactRequests.

Seed data: admin account (`admin@demo.com` / `Admin@123`), 6 property types, ~20 approved sample listings across HCMC districts (Quận 1, 2, 7, 9, Bình Thạnh, Thủ Đức, Gò Vấp, Nhà Bè…) with realistic prices, coordinates (lat/lng) for map, 1-3 images each (generated placeholder images or SVG/stock), 2-3 pending listings for moderation demo.

## 5. Pages / Routes

Public (no auth):
- `/` Home — hero search box, featured approved listings, property type quick links
- `/Listings` — listing grid + sidebar filters: keyword (LIKE on title/address/description), district, property type, price range (min/max), area range (min/max), bedrooms, buy/rent toggle, sort (newest/price asc/price desc). Server-side paging (10/page). Relative search = `EF.Functions.Like`.
- `/Listings/Details/{id}` — gallery (primary + thumbs), key facts, description, seller contact card, Leaflet map marker, favorite toggle, contact seller form
- `/ML/Predict` — form: district, area, bedrooms, bathrooms, floors, property type → predicted price + confidence (R²/RMSE/MAE of chosen model displayed)

Auth (Buyer/Seller):
- `/Account/Register`, `/Account/Login`, `/Account/Logout` (ASP.NET Identity + cookie auth)
- `/Account/Profile` — edit full name, phone, address, avatar; change password
- `/Favorites` — saved listings, remove from favorites

Auth (Seller):
- `/MyListings` — CRUD own listings: create (form + multi-image upload), edit, delete (soft via status or hard delete — decision: hard delete images cascade), list with status badge

Auth (Admin):
- `/Admin/` dashboard — stats: total listings, by status, by type, by district (charts via Chart.js or simple Bootstrap tables/badges)
- `/Admin/Moderation` — pending listings queue: approve / reject (with note) / ban
- `/Admin/Users` — list users, block/unblock, change role, delete non-admin users
- `/Admin/Types` — CRUD property types

Images: uploaded to `wwwroot/uploads/`, validated (extension jpg/png/webp, max 5MB, max 5 per listing), first image = primary.

## 6. ML Price Prediction

- Dataset: generated synthetic ~1000 rows resembling HCMC real estate (district base price per m² + area + rooms + floors + type adjustments + noise), realistic ranges
- Models (ML.NET): FastTree (Gradient Boosting) vs LightGbm vs LBFGS (linear). Train/test split 80/20, evaluate MAE/RMSE/R², pick best by R²
- Persist winner to `ML/model.zip` at build/seed time (console: `dotnet run -- train` or startup task that trains if model missing)
- Predict page loads model, predicts price, shows model metrics
- Input features: District (one-hot/key-value), Area, Bedrooms, Bathrooms, Floors, FacadeWidth, PropertyType (key-value), IsForRent

## 7. Error Handling & Security

- ASP.NET Identity: hashed passwords, cookie auth, role-based authorization attributes (`[Authorize(Roles=...)]`)
- Input validation: DataAnnotations + server-side validation, antiforgery tokens
- Upload validation: extension + size checks, unique filenames (GUID)
- 404 page for missing listing, error handling middleware
- No secrets in code; connection string local file path

## 8. Testing / Verification

- `dotnet build` clean
- Browser test all flows: register → login → seller posts listing → admin approves → appears public → buyer saves favorite + contacts seller → admin sees contact request → reports render
- Test search filters, paging, ML predict page output
- Responsive check (Bootstrap)

## 9. Deliverables

- Working ASP.NET Core MVC app in repo root
- Design spec (this file), implementation plan
- README with setup + default accounts
- Report (220064_MauBaoCao FINAL.docx/pdf) is external artifact — not regenerated unless requested

## 10. Out of Scope

- Legal processes (notary, payment, document verification)
- Real scraped dataset (synthetic generated)
- Email confirmation, password reset via email (demo flow: local only)
- Deployment to hosting