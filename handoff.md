# Handoff — Website mua bán bất động sản (Đồ án ASP.NET)

> State snapshot for next session/agent. Written 2026-08-19 after full live-test matrix.

## 1. Project

ASP.NET Core MVC (.NET 8) real-estate marketplace, Vietnamese UI, course project (Chuyên đề ASP.NET).
EF Core + SQLite, ASP.NET Identity, Bootstrap 5, Leaflet maps, ML.NET price prediction.
Spec: `docs/superpowers/specs/2026-08-19-real-estate-website-design.md`. Report template: `220064_MauBaoCao FINAL.docx` (+ pdf/txt).

## 2. Repo state

- **All 15 tasks done, committed.** Latest commits (newest first):
  `e1cfdd7` README live-test guide · `6d8e511` ML metrics.json fix + report v2 · `4d67af9` DeleteUser cascade fix · `2944c63` T15 README+E2E · `d838604` T14 ML.NET · `ae38327` T13 types · `2944a68` T12 users · `356d44a` T11 moderation · `46932db` T10 dashboard · `926cac2` T9 seller · `68db733` T8 favs/contact · `496750e` T7 auth · `2224ca5` T6 details · `6c9909e` T5 search (TDD) · `0e352c2` T4 home · `d631f96` T3 seeder · `3791917` T2 entities · `30127ba` T1 bootstrap
- Working tree clean (git status empty). Not a git repo problem: `.superpowers/`, `.playwright-mcp/`, `.omo/` are gitignored agent dirs — safe to delete before publishing, referenced nowhere in app code.
- Git on Windows: prefix `$env:Path = "C:\Program Files\Git\cmd;" + $env:Path` in PowerShell.

## 3. Run it

```powershell
dotnet watch run --non-interactive --urls http://localhost:5000   # live reload
```

- First run: creates `realestate.db` + seed, trains ML (~30-60s → `ML/model.zip` + `ML/metrics.json`).
- Reset: kill dotnet, `Remove-Item realestate.db`, rerun (reseed + no retrain if model.zip exists).
- Tests: `dotnet test Tests/Tests.csproj` (5 search/filter tests).
- **Known flakiness:** `dotnet watch` dies silently mid-session (no error log). Before each test round:
  `Invoke-WebRequest http://localhost:5000` must return 200, else restart. Plain `dotnet run` also observed dying; restart pattern used throughout.

### Browser (Playwright + Brave CDP)

```powershell
Start-Process "C:\Users\erikthetwin\AppData\Local\BraveSoftware\Brave-Browser\Application\brave.exe" `
  -ArgumentList "--remote-debugging-port=9222","--user-data-dir=C:\Users\erikthetwin\AppData\Local\Temp\brave-cdp-test","--no-first-run","--no-default-browser-check","about:blank"
```

- Playwright MCP via `skill_mcp` (mcp_name=playwright, cdp_url=http://localhost:9222, tool_name=browser_run_code_unsafe). Direct `playwright_*` tools fail ("Chromium distribution 'chrome' is not found") — **always use skill_mcp**.
- `connect ECONNREFUSED ::1:9222` = Brave closed; relaunch command above.

## 4. Accounts & data (current DB)

| Role | Email | Password | Notes |
|---|---|---|---|
| Admin | admin@demo.com | Admin@123 | Protected row: no Xóa/Khóa buttons |
| Seller | seller@demo.com | Seller@123 | Owns ALL 23 seed listings (id `65aaad97-0edc-4550-8a93-f9ae8ac2e634`) |
| Buyer | anhnha@demo.com | Test@12345 | Test-created, name "Anh Nhà Mua 2", phone 0911112222 |

DB state (verified final): 26 Properties (21 Approved, 3 Pending seed, 1 Rejected id=26 "…- B", 1 Banned id=27 "…- C"; id=25 "…- A (đã sửa)" Approved), 26 images, 7 PropertyTypes all active (6 seed + "Nhà phố thương mại"), 1 Favorite + 1 Contact (anhnha → prop 2). Users: only the 3 above, all unlocked.

**Status enum (`Models/PropertyStatus.cs`):** 0 Pending, 1 Approved, 2 Rejected, 3 Banned.

## 5. Testing knowledge (hard-won)

- Confirm dialogs (delete listing/user): register `page.on('dialog', d => d.accept())` INSIDE run_code_unsafe BEFORE the click. MCP may still report "Modal state" pending afterwards — ignore it, verify via DB.
- MyListings "Sửa" is an `<a>` link (`/MyListings/Edit/{id}`); "Xóa" is form submit w/ confirm. Moderation page & Types page use `div.card`, NOT tables. Users page IS a table (select role + "Đổi", "Khóa"/"Mở khóa", "Xóa"; admin row has only Đổi).
- Types create field: `input[name=name]`, form action `/Admin/CreateType`. Careful: navbar logout is a `form[method=post]` that precedes content forms — never `.first()` it.
- ML form (`/Ml/Predict`): `select#District`, `select#PropertyType`, `input[name=Area|FacadeWidth|Bedrooms|Bathrooms|Floors]`, checkbox `input[name=IsForRent]`. NO Price field.
- `/MyListings/Create` district option is `'Thủ Đức'` (not "Thành phố Thủ Đức").
- Python one-liners with Vietnamese chars fail (cp1252 console) — write script files with `sys.stdout.reconfigure(encoding='utf-8')`.
- DB verify pattern (script file): sqlite3 select users/props-by-status/favs/contacts/types.
- Dashboard numbers matched DB 100% in v2 test (26/3/21/1/1 + 3 users + 1 contact).

## 6. ML internals (`Services/ML/PricePredictionService.cs`)

- FastTree, seed=1 (deterministic), DatasetGenerator.Generate(1200), 80/20 split.
- `TrainIfNeededAsync`: `if model.zip exists → LoadAsync() else Train()` (called at startup).
- **Fixed 2026-08-19 (6d8e511):** R²/RMSE/MAE were only computed in `Train()`; LoadAsync showed zeros after restart. Now persisted to `ML/metrics.json` at train time, loaded in LoadAsync. Expected: R² 0.963 / RMSE 2,428,848,273 / MAE 1,638,812,770; sample predict (Q1, Căn hộ, 75m², 2BR/1BA/1F) = 11,268,080,000₫ buy / 11,200,080,000₫ rent.
- Deleting `ML/model.zip` + `metrics.json` triggers retrain on next start (~60s).

## 7. Known issues / open items

1. **Unexplained data-loss incident (earlier):** during delete testing, all listings + seller + a buyer vanished in one step (SQLite, likely a session/agent tool accident during `dotnet watch` flakiness). Not reproducible; mitigated by DB restore; documented in LIVE-USAGE-TEST.md appendix A. No action unless it recurs.
2. **dotnet watch silent deaths** (see §3) — operational, not a code bug.
3. Ledger lives in `.superpowers/sdd/2026-08-19-real-estate-website/progress.md` (gitignored, not committed).
4. DeleteUser fix (4d67af9) only handles favorites/contacts/images/listings cascade — if new FK'd tables are added, extend `AdminController.DeleteUser`.

## 8. Docs inventory

- `README.md` — run + live-test instructions for others (§1-6).
- `LIVE-USAGE-TEST.md` — v1 report (5 scenarios, appendix A bugfix writeup) + **Phụ lục B: 21-group multi-account matrix** (the checklist to re-run), environment quirks.
- Report PDFs: `220064_MauBaoCao FINAL.pdf/txt/docx` (template), `Phieu_danh_gia_ASP.NET.pdf/txt` (grading rubric).

## 9. Agent-tooling layout

- `.omo/run-continuation/*.json`, `.playwright-mcp/*.log`, `.superpowers/` — all agent artifacts, gitignored, safe to delete for publish.
- Skills available: superpowers suite (brainstorming, TDD, verification, debugging, etc.) + caveman mode (user prefers terse responses; code/commits written normal).
- User directives standing: live-test with real browsers (Playwright+Brave), verify against DB, keep reports in markdown, commit per task.
