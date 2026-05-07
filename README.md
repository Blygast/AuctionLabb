# AuctionLabb

A full-stack auction platform with an ASP.NET Web API backend and a React (Vite + TypeScript + Tailwind) frontend.

> **School assignment** for the Web Development course at IT-Högskolan (ITHS).

---

## About

AuctionLabb is a full-featured online auction application where users can register, create auctions, place bids, upload file attachments, and manage their listings. An admin panel allows managing users and auctions (activate/deactivate). The platform supports real-time bidding, file uploads (images, videos, documents), search and filtering, and a responsive dark-mode UI built with Flowbite.

---

## Features

### Authentication

- **Register** a new account with name, email, and password
- **Login / Logout** — JWT-based authentication persisted across page reloads
- **Profile page** — change your password
- Role-based access control (User / Admin)

### Auctions

- **Create auctions** with title, description, starting price, end date, and file attachments
- **Edit auctions** — update title, description, and end date (owner only)
- **Search** auctions by title with status filtering (Open / Closed / All)
- **Auction cards** with thumbnail preview from first image attachment
- **Closed auctions** display the winning bid and disable bidding

### Bidding

- Place bids on open auctions (must be higher than current highest bid)
- **Cancel your latest bid** on an open auction
- Can't bid on your own auctions
- Can't bid on closed auctions
- Real-time current price display (highest bid or starting price)

### File Attachments

- Upload multiple files when creating an auction (images, videos, documents)
- Drag-and-drop upload interface
- **Image gallery** with grid display
- **Video player** embedded in auction detail
- **Document list** with download links
- Supported formats: images (jpg, png, gif, webp, svg), videos (mp4, webm, avi, mov), documents (pdf, doc/x, xls/x, txt, zip)
- Max file size: 50 MB per file

### Admin Panel

- **Users tab** — view all users, activate/deactivate accounts
- **Auctions tab** — view all auctions, activate/deactivate listings
- Deactivated users cannot log in
- Deactivated auctions are hidden from public views
- Role-based route guard (AdminRoute)

### UI & Design

- Flowbite admin dashboard theme
- Responsive sidebar + navbar layout
- Dark mode support
- Loading spinners and error handling
- Status badges (Open / Closed / Deactivated)

---

## Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Web API (.NET 10) | Backend REST API |
| Entity Framework Core | ORM and database access |
| SQL Server (LocalDB) | Database |
| JWT Bearer | Authentication |
| BCrypt | Password hashing |
| React 19 + TypeScript | Frontend framework |
| Vite | Build tool and dev server |
| Tailwind CSS v4 | Styling |
| Flowbite | UI component library |
| React Router v7 | Client-side routing |
| Axios | HTTP client |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/)
- SQL Server LocalDB (included with Visual Studio / .NET SDK)

### Backend (API)

```bash
cd api/AuctionApi

# Install dependencies
dotnet restore

# Run the API
dotnet run
```

- API runs at: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### Frontend (Client)

```bash
cd client

# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build
```

- Client runs at: `http://localhost:5173`

### Admin Account

| Field | Value |
|---|---|
| Email | `admin@auctionlabb.com` |
| Password | `Admin123!` |

---

## Project Structure

```
labb2/
├── api/
│   └── AuctionApi/
│       ├── Program.cs                  # App configuration, DI, middleware
│       ├── Controllers/
│       │   ├── AuthController.cs       # Register, Login, Me, UpdatePassword
│       │   ├── AuctionsController.cs   # CRUD, bids, search, attachments, cancel bid
│       │   ├── AdminController.cs      # Admin user/auction management
│       │   └── FilesController.cs      # Serve uploaded files
│       ├── Models/
│       │   ├── User.cs                 # User with Role, IsActive
│       │   ├── Auction.cs             # Auction with IsActive, IsOpen computed
│       │   ├── Bid.cs                  # Bid model
│       │   └── Attachment.cs           # File attachment model
│       ├── Data/
│       │   └── AppDbContext.cs         # EF Core DbContext + seed data
│       ├── DTOs/
│       │   ├── RegisterDto.cs
│       │   ├── LoginDto.cs
│       │   ├── AuthResponseDto.cs
│       │   ├── CreateAuctionDto.cs
│       │   ├── UpdateAuctionDto.cs
│       │   ├── AuctionResponseDto.cs
│       │   ├── CreateBidDto.cs
│       │   ├── BidResponseDto.cs
│       │   ├── AttachmentResponseDto.cs
│       │   └── UpdatePasswordDto.cs
│       ├── Services/
│       │   └── JwtService.cs           # JWT token generation with role claims
│       └── Migrations/                 # EF Core migrations
│
└── client/
    └── src/
        ├── App.tsx                     # Routes and layout
        ├── main.tsx                    # Entry point
        ├── index.css                   # Tailwind + Flowbite theme
        ├── api/
        │   └── axios.ts               # Axios instance with JWT interceptor
        ├── context/
        │   └── AuthContext.tsx         # Auth state, login, register, logout
        ├── components/
        │   ├── Navbar.tsx              # Top navigation bar
        │   ├── Sidebar.tsx             # Side navigation (responsive)
        │   ├── PrivateRoute.tsx        # Auth guard
        │   ├── AdminRoute.tsx          # Admin role guard
        │   ├── AuctionCard.tsx         # Auction card with thumbnail
        │   ├── BidList.tsx             # Bids table
        │   └── AttachmentViewer.tsx    # Image gallery, video, documents
        └── pages/
            ├── LoginPage.tsx
            ├── RegisterPage.tsx
            ├── AuctionsPage.tsx        # List with search + status tabs
            ├── AuctionDetailPage.tsx   # Detail with bids, attachments, cancel bid
            ├── CreateAuctionPage.tsx   # Create with file upload
            ├── EditAuctionPage.tsx     # Edit auction form
            ├── ProfilePage.tsx         # Change password
            └── AdminPage.tsx           # Admin panel (users/auctions)
```

---

## API Endpoints

### Auth

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and get JWT token |
| GET | `/api/auth/me` | Get current user info |
| PUT | `/api/auth/password` | Update password |

### Auctions

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/auctions` | List auctions (`?search=&status=open\|closed\|all`) |
| GET | `/api/auctions/{id}` | Get auction detail |
| POST | `/api/auctions` | Create auction (multipart/form-data with files) |
| PUT | `/api/auctions/{id}` | Update auction (title, description, endDate) |
| POST | `/api/auctions/{id}/bids` | Place a bid |
| DELETE | `/api/auctions/{auctionId}/bids/{bidId}` | Cancel your latest bid |
| POST | `/api/auctions/{id}/attachments` | Upload attachments |
| DELETE | `/api/auctions/{id}/attachments/{attachmentId}` | Delete attachment |

### Admin

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/admin/users` | List all users |
| PUT | `/api/admin/users/{id}/activate` | Activate a user |
| PUT | `/api/admin/users/{id}/deactivate` | Deactivate a user |
| GET | `/api/admin/auctions` | List all auctions |
| PUT | `/api/auctions/{id}/activate` | Activate an auction |
| PUT | `/api/auctions/{id}/deactivate` | Deactivate an auction |
