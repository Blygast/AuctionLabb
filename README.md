# AuctionLabb

> A full-stack online auction platform — ASP.NET Core 10 + React 19


[![Backend tests](https://img.shields.io/badge/backend-93%2F93%20passing-4c1?logo=dotnet)](https://github.com/USER/labb2/actions/workflows/test.yml)
[![Frontend lint](https://img.shields.io/badge/frontend-0%20errors-4c1?logo=react)](https://github.com/USER/labb2/actions/workflows/test.yml)
[![Coverage](https://img.shields.io/badge/coverage-93%25%20lines-4c1)](https://github.com/USER/labb2/actions/workflows/test.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://react.dev/)

---

## Table of contents

- [What it is](#what-it-is)
- [Features](#features)
- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Repository layout](#repository-layout)
- [Quick start](#quick-start)
- [Testing](#testing)
- [Coverage report](#coverage-report)
- [CI / CD](#ci--cd)
- [Docker](#docker)
- [Releases](#releases)
- [Dev container](#dev-container)
- [Pre-commit hooks](#pre-commit-hooks)
- [API reference](#api-reference)
- [Configuration](#configuration)
- [Admin account](#admin-account)
- [License](#license)

---

## What it is

AuctionLabb is a multi-user auction application. Registered users can list items
(including file attachments, images, videos, PDFs), place and cancel bids, and
edit their own listings. An admin role can activate or deactivate users and
auctions. The platform supports real-time auction status, search, and a
responsive dark-mode UI.

This is the second Webbutveckling med .NET assignment (`AuctionLabb`) in the IT-Högskolan (ITHS) Web Development course.

---

## Features

### For users
- Register, log in, change password
- Create auctions with title, description, start/end date, and attachments
- Upload images, videos, and documents (≤ 50 MB each)
- Browse open / closed / all auctions, with full-text title search
- Place bids, view bid history, **cancel your own latest bid**
- Edit your own listings (title, description, end date)
- View other users' auctions without being able to bid on your own

### For admins
- View every user and every auction in dedicated tabs
- Activate / deactivate users (deactivated users can't log in)
- Activate / deactivate individual auctions

### UI / UX
- Flowbite admin dashboard theme (light + dark mode)
- Responsive sidebar + top-nav layout
- Auction cards with thumbnail previews
- Image gallery, embedded video player, and document download list
- Real-time status badges (Open / Closed / Deactivated)
- In-app error boundary so render failures show a recoverable fallback

---

## Tech stack

| Layer | Technology | Version |
|---|---|---|
| Backend runtime | .NET | 10.0 |
| Backend framework | ASP.NET Core | 10.0 |
| ORM | Entity Framework Core + SQL Server LocalDB | 10.0 |
| Auth | JWT Bearer + BCrypt | — |
| File storage | Local filesystem (`Uploads/`) | — |
| API docs | Swagger / Swashbuckle | 6.6 |
| Frontend runtime | React | 19 |
| Build tool | Vite | 8 |
| Styling | Tailwind CSS + Flowbite | 4 / 4 |
| Routing | React Router | 7 |
| HTTP | Axios | 1.x |
| Testing (backend) | xUnit + in-memory fakes + EF integration tests | — |
| Coverage | coverlet.collector | 6.x |
| CI | GitHub Actions | — |

---

## Architecture

The backend is split into three projects following the **Clean Architecture /
Onion** pattern. Dependencies flow **inward** — `Core` knows nothing about EF
Core, JWT, or HTTP; `Data` implements the interfaces declared in `Core`; the
`Api` project is a thin presentation layer.

```
┌──────────────────────────────────────────────────────┐
│  AuctionApi (presentation)                           │
│  ┌─────────────────┐  ┌────────────────────────┐    │
│  │   Controllers   │  │  Middleware / Mappers  │    │
│  │  (thin shells)  │  │  (ExceptionFilter)     │    │
│  └────────┬────────┘  └────────────┬───────────┘    │
│           │  depends on             │               │
│           ▼                         ▼               │
│  ┌──────────────────────────────────────────────┐    │
│  │  AuctionApi.Core (domain)                    │    │
│  │  • Entities (User, Auction, Bid, Attachment) │    │
│  │  • Interfaces (IUnitOfWork, IFileStorage…)   │    │
│  │  • Services (Auth, Auction, Bid, Attachment) │    │
│  │    ← pure business rules, no I/O             │    │
│  └────────┬─────────────────────────────────────┘    │
│           │  implements                               │
│           ▼                                          │
│  ┌──────────────────────────────────────────────┐    │
│  │  AuctionApi.Data (infrastructure)            │    │
│  │  • AppDbContext + EF migrations              │    │
│  │  • Repository implementations                │    │
│  │  • BCrypt + JWT implementations              │    │
│  │  • LocalFileStorage                          │    │
│  └──────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────┘
```

The frontend has a thin page layer that composes reusable
components, which read from **custom hooks**, which call **typed service
modules** that wrap Axios.

```
┌────────────────────────────────────────────┐
│  pages/  →  components/  →  hooks/  →  services/  →  http/axios  →  API
│  (route)    (presentational)  (state)     (typed)        (infra)
└────────────────────────────────────────────┘
```

---

## Repository layout

```
labb2/
├── api/                                     # ASP.NET Core 10 backend
│   ├── AuctionApi.slnx
│   ├── AuctionApi/                          # Presentation (Controllers, DTOs, Mappers)
│   ├── AuctionApi.Core/                     # Domain (Entities, Interfaces, Services)
│   ├── AuctionApi.Data/                     # Infrastructure (DbContext, Repos, Migrations)
│   └── AuctionApi.Core.Tests/               # xUnit — unit + LocalDB integration tests
│       ├── Fakes/                           # In-memory repo / service doubles
│       └── Integration/                     # Real DbContext tests
│
├── client/                                  # React 19 + Vite frontend
│   ├── src/
│   │   ├── api/                             # (reserved — currently empty)
│   │   ├── components/                      # Reusable UI (Navbar, BidForm, AuctionTable…)
│   │   ├── context/                         # AuthContext + useAuth hook
│   │   ├── hooks/                           # useAuctions, useAuctionDetail, useAdminData
│   │   ├── pages/                           # Route components
│   │   ├── services/                        # authService, auctionService, adminService, http
│   │   ├── types/                           # Shared DTO / entity types
│   │   ├── utils/                           # formatPrice, formatDate, getErrorMessage
│   │   ├── App.tsx                          # Router + ErrorBoundary
│   │   ├── config.ts                        # SERVER_URL from import.meta.env
│   │   ├── routes.ts                        # Route paths + isPathActive helper
│   │   └── main.tsx
│   ├── .env.development                     # VITE_API_URL=...
│   └── package.json
│
├── .github/
│   ├── workflows/
│   │   ├── test.yml                         # CI on every push/PR — 3 jobs
│   │   ├── release.yml                      # Tag push → GitHub Release with build artifacts
│   │   ├── coverage.yml                     # Push to main → refresh docs/coverage/
│   │   └── pages.yml                        # Push to main → deploy coverage to GitHub Pages
│   └── dependabot.yml                       # Weekly NuGet + npm + GH Actions updates
│
├── .devcontainer/
│   └── devcontainer.json                    # VS Code / Codespaces one-click setup
│
├── .pre-commit-config.yaml                  # dotnet format + ESLint + tsc on every commit
│
├── docs/
│   └── coverage/                            # Committed HTML coverage report
│       └── index.html
│
├── Dockerfile                               # Multi-stage: node → dotnet → aspnet
├── .dockerignore
│
├── README.md         # ← you are here
└── PLAN.md                                  # Historical course notes and assignment requirements
```

---

## Quick start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- SQL Server LocalDB (included with the .NET SDK / Visual Studio)

### Backend

```bash
cd api/AuctionApi
dotnet run
```

The API starts at `https://localhost:5001`. Swagger UI at
`/swagger` lists every endpoint and lets you try requests with a JWT.

### Frontend

```bash
cd client
npm install
npm run dev
```

The dev server starts at `http://localhost:5173`. The frontend reads
`VITE_API_URL` from `.env.development`; the default points at the local
backend.

### Seed data

The first run auto-applies EF migrations, which create the schema and seed
three users plus three sample auctions (Dutch masterworks). See
[Admin account](#admin-account) for the seeded credentials.

---

### Auto-installer (Windows)

If you'd rather not run the backend and frontend in two separate terminals,
just double-click `run.bat` at the repo root. It will:

1. Check that `dotnet`, `node`, and `npm` are on `PATH`
2. `dotnet restore` + `npm install` (every time to make sure you're always up to date)
3. Run the backend tests and the frontend lint
4. Spawn the backend in one window and the frontend in another
5. Open `http://localhost:5173` in your default browser

Close the two spawned cmd windows to stop the services.

---

## Testing

The backend has a full test project at `api/AuctionApi.Core.Tests/`.

```bash
# All 93 tests (unit + LocalDB integration)
dotnet test api/AuctionApi.Core.Tests

# Unit tests only — no DB required, runs anywhere
dotnet test api/AuctionApi.Core.Tests --filter "Category!=Integration"

# Integration tests only — needs LocalDB
dotnet test api/AuctionApi.Core.Tests --filter "Category=Integration"

# With coverage (writes TestResults/**/coverage.cobertura.xml)
dotnet test api/AuctionApi.Core.Tests --collect:"XPlat Code Coverage"

# Convert the Cobertura report to HTML (one-time tool install)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:./TestResults/**/coverage.cobertura.xml \
  -targetdir:./TestResults/html \
  -reporttypes:Html
```

### What's tested

| Layer | Count | Notes |
|---|---|---|
| **Unit** (in-memory fakes) | 76 | Service logic only — fast, deterministic, no I/O |
| **Integration** (real LocalDB) | 17 | Real EF + migrations + transactional behavior |
| **Total** | **93** | Coverage: 97.9% Core, 92.5% Data, 93.6% overall |

The integration tests use `[Trait("Category", "Integration")]` so the unit
job in CI can filter them out cleanly. They get a fresh, uniquely-named
LocalDB per test class (auto-migrated and dropped on teardown).

---

## Coverage report

The full HTML coverage report is committed to the repo and browsable directly
on GitHub (or in your local checkout):

**→ [Open the coverage report](./docs/coverage/index.html)**

It is also published live to GitHub Pages by `.github/workflows/pages.yml`:

**→ [Live coverage site](https://github.com/Blygast/AuctionLabb/)**

The report gets regenerated automatically by `.github/workflows/coverage.yml`
on every push to `main`. That workflow runs the full
test suite, generates the Cobertura + HTML reports, and commits the updated
HTML back to the repo, which then triggers the Pages deployment.

If you want to regenerate index.html Coverage Summary locally:

```bash
dotnet test api/AuctionApi.Core.Tests --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:./api/TestResults/**/coverage.cobertura.xml \
  -targetdir:./docs/coverage \
  -reporttypes:Html
```

---

## CI / CD

Four workflows + Dependabot keep the project healthy:

| File | Trigger | What it does |
|---|---|---|
| `.github/workflows/test.yml` | every push + PR | 3-job matrix: backend unit / backend integration (LocalDB) / frontend build+lint |
| `.github/workflows/coverage.yml` | push to main | Regenerates `docs/coverage/` and commits it back so the README link is always fresh |
| `.github/workflows/release.yml` | push of tag `v*` (or manual) | Builds backend + frontend, zips, and attaches to a GitHub Release |
| `.github/dependabot.yml` | weekly / monthly | Opens grouped PRs for NuGet + npm + GitHub Actions updates |

`windows-latest` is used for integration because LocalDB is Windows-only.
The unit-tests job runs on Linux for speed.

---

## Docker

A single `Dockerfile` at the repo root builds a production image with the
ASP.NET Core API and the React frontend (served as static files by the API).

```bash
# Build
docker build -t auctionlabb:latest .

# Run (override the connection string + CORS as needed)
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=...;Database=AuctionDb;..." \
  -e Cors__AllowedOrigins__0="https://my-frontend.example.com" \
  auctionlabb:latest
```

The image:

- is **multi-stage** (Node → .NET SDK → ASP.NET runtime)
- runs as a **non-root** user
- exposes port **8080**
- has a **HEALTHCHECK** hitting `/health`
- bakes the React `dist/` into `wwwroot/` so the API also serves the SPA

`.dockerignore` keeps the build context clean (skips `bin/`, `obj/`,
`node_modules/`, `TestResults/`, `.git/`, etc.).

---

## Releases

Tag a commit with `v*` and the release workflow builds production artifacts
and attaches them to a GitHub Release with auto-generated notes:

```bash
git tag v1.0.0
git push origin v1.0.0
```

The release archive contains:

- `release/api/` — self-contained ASP.NET Core 10 publish
- `release/client/` — static Vite build

You can trigger a dry-run release from the Actions tab via
`workflow_dispatch`.

---

## Dev container

The repo ships a `.devcontainer/devcontainer.json` so contributors (or
Codespaces users) get a fully-configured environment simply. Open
the folder in VS Code with the Dev Containers extension installed and accept
the prompt, or click the **Code → Codespaces → Create codespace** button
on GitHub.

The container includes:

- **.NET 10 SDK** (from the Microsoft base image)
- **Node 20** (added via the Node devcontainer feature)
- **VS Code extensions**: C# Dev Kit, C#, Entity Framework Core, ESLint,
  Prettier, Tailwind CSS IntelliSense
- **Editor settings**: format-on-save, Prettier for JS/TS, C# formatter for
  C#, Tailwind CSS language support
- **Forwarded ports**: 5001 (backend), 5173 (Vite), 8080 (Docker)

Post-create script: `npm install` in the client + `dotnet restore` for the
backend, so the user is ready to code as soon as the container starts.

> **Note on databases:** the dev container does **not** include SQL Server.
> The backend uses Windows-only LocalDB by default. Unit tests
> (`--filter "Category!=Integration"`) run fine with no DB. For integration
> tests, either run them on a Windows host with LocalDB, or extend the
> devcontainer with a `docker-compose.yml` that brings up the
> `mcr.microsoft.com/mssql/server` image and swap the test connection
> string to point at it.

---

## Pre-commit hooks

`.pre-commit-config.yaml` runs lint + format on every commit so bad code
never lands. Install once:

```bash
pip install pre-commit          # or: brew install pre-commit
pre-commit install
```

After that, every `git commit` runs:

| Hook | What it does |
|---|---|
| `trailing-whitespace`, `end-of-file-fixer`, `check-yaml`, `check-json`, `check-merge-conflict` | standard hygiene |
| `dotnet format (verify)` | fails the commit if `dotnet format` would change anything; the fix is to run `dotnet format api/AuctionApi.slnx` and re-stage |
| `eslint (frontend)` | runs `npm run lint` in `client/`; auto-fix is not enabled on commit (intentional — review fixes) |
| `tsc --noEmit` | type-checks the frontend (catches the kind of breakage ESLint misses) |

To run all hooks on demand without committing:

```bash
pre-commit run --all-files
```

To skip on a single commit (e.g. trivial doc fix while iterating):

```bash
git commit --no-verify
```

---

## API reference

All endpoints under `/api`. JSON in / JSON out. Auth via `Authorization: Bearer <jwt>`.

### Auth

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | — | `{ name, email, password }` → `{ token, user }` |
| POST | `/api/auth/login` | — | `{ email, password }` → `{ token, user }` |
| GET  | `/api/auth/me` | user | Current user from the JWT |
| PUT  | `/api/auth/password` | user | `{ currentPassword, newPassword }` |

### Auctions

| Method | Path | Auth | Description |
|---|---|---|---|
| GET  | `/api/auctions?search=&status=open\|closed\|all` | — | List / search auctions |
| GET  | `/api/auctions/{id}` | — | Auction detail + bids + attachments + winning bid if closed |
| POST | `/api/auctions` (multipart) | user | Create with optional file attachments |
| PUT  | `/api/auctions/{id}` | owner / admin | Update title / description / end date |
| POST | `/api/auctions/{id}/bids` | user | `{ amount }` — must exceed current highest |
| DELETE | `/api/auctions/{id}/bids/{bidId}` | owner | Cancel your own latest bid |
| POST | `/api/auctions/{id}/attachments` | owner | Upload more attachments |
| DELETE | `/api/auctions/{id}/attachments/{attId}` | owner | Remove an attachment + file from disk |
| PUT  | `/api/auctions/{id}/activate` | admin | |
| PUT  | `/api/auctions/{id}/deactivate` | admin | |

### Admin

| Method | Path | Auth | Description |
|---|---|---|---|
| GET  | `/api/admin/users` | admin | All users + auction/bid counts |
| PUT  | `/api/admin/users/{id}/activate` | admin | |
| PUT  | `/api/admin/users/{id}/deactivate` | admin | (can't deactivate yourself) |
| GET  | `/api/admin/auctions` | admin | All auctions with summary stats |

### Files

| Method | Path | Auth | Description |
|---|---|---|---|
| GET  | `/api/files/{storedFileName}` | — | Streams an uploaded file with the right content type |

---

## Configuration

### Backend

Edit `api/AuctionApi/appsettings.json` (or override via env vars / `appsettings.Development.json`):

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AuctionDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "<at least 32 chars>",
    "Issuer": "AuctionApi",
    "Audience": "AuctionClient",
    "ExpiryInHours": 2
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:5173" ]
  }
}
```

### Frontend

Set in `client/.env.development` (or `.env.production`):

```
VITE_API_URL=https://localhost:5001
```

---

## Accounts seed data

The seed data inserts three users (all with password `Admin123!`):

| Email | Role | Notes |
|---|---|---|
| `admin@auctionlabb.com` | Admin | Can manage users & auctions |
| `curator@auctionlabb.com` | User | Has the three sample auction listings |
| `collector@auctionlabb.com` | User | Has seeded bids on the listings |

> **Note:** To reset the passwords during your own testing. drop the database and restart the API.
> The migrations will re-seed automatically.

---

## License

This project is a school assignment for **IT-Högskolan (ITHS)**. Treat it as
educational material — feel free to read, learn from, and borrow patterns,
but please don't redistribute it as your own course submission. That would be pretty dumb :P
