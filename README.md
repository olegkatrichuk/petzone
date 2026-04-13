# PetZone

Platform for pet adoption and volunteer coordination in Ukraine. Shelters and volunteers can post animals for adoption, manage listings, and communicate with potential adopters.

**Live site:** [getpetzone.com](https://getpetzone.com)

---

## What it does

- Browse animals available for adoption with filtering by species, breed, city
- Shelters and volunteers register and immediately post their animals
- Adoption listings with contact forms
- Volunteer profiles with animal management cabinet
- Multilingual UI: Ukrainian, English, Polish, German, French, Russian
- Interactive map of animals and shelters
- Email notifications (approval, status updates)

## Tech stack

| Layer | Technologies |
|---|---|
| Frontend | React 19, TypeScript, Vite, Tailwind CSS 4, MUI 7, Redux Toolkit, TanStack Query v5 |
| Backend | ASP.NET 10, EF Core 10, Clean Architecture + DDD, CQRS (MediatR) |
| Database | PostgreSQL 17 |
| Cache | Redis 7 |
| Queue | RabbitMQ 3 + MassTransit |
| Storage | MinIO (S3-compatible) |
| Notifications | Separate ASP.NET 10 consumer service |
| Logs | Serilog → Seq + Elasticsearch/Kibana |
| Monitoring | Prometheus + Grafana |
| Proxy | Nginx |

## Project structure

```
petzone/
├── backend/          # ASP.NET 10 main API
│   ├── src/
│   │   ├── Accounts/          # Auth, users, JWT
│   │   ├── Species/           # Animal species catalog
│   │   ├── Listings/          # Adoption listings
│   │   ├── Volunteers/        # Volunteer profiles & pets
│   │   ├── VolunteerRequests/ # Volunteer registration flow
│   │   └── Shared/            # Core, Framework, SharedKernel
│   └── compose.yaml           # Full infrastructure (Docker)
├── frontend/         # React SPA
│   └── src/
│       ├── pages/      # Route-level components
│       ├── components/ # Reusable UI
│       ├── services/   # RTK Query API slices
│       ├── store/      # Redux slices
│       ├── types/      # TypeScript interfaces
│       └── i18n/       # Translations (6 locales)
└── notification-service/  # RabbitMQ consumer, sends emails
```

## Getting started

### Prerequisites

- Docker & Docker Compose
- .NET 10 SDK
- Node.js 20+

### Run everything with Docker

```bash
cd backend
cp .env.example .env   # fill in secrets
docker compose up -d
```

Services will be available at:
- API: `http://localhost:8080`
- Frontend (prod build via Nginx): `http://localhost:80`
- RabbitMQ management: `http://localhost:15672`
- MinIO console: `http://localhost:9001`
- Seq logs: `http://localhost:8081`
- Grafana: `http://localhost:3000`

### Frontend dev server

```bash
cd frontend
npm install
npm run dev   # http://localhost:5173
```

API requests are proxied from `:5173` → `http://localhost:5183`.

### Backend only

```bash
cd backend
dotnet build
dotnet run --project src/PetZone.API
```

### Database migrations

```bash
cd backend
dotnet ef migrations add <Name> -p src/PetZone.Infrastructure -s src/PetZone.API
dotnet ef database update -p src/PetZone.Infrastructure -s src/PetZone.API
```

### Run tests

```bash
cd backend
dotnet test
```

## Environment variables

Copy `backend/.env.example` to `backend/.env` and fill in:

```
POSTGRES_PASSWORD=
JWT_SECRET=
MINIO_ROOT_PASSWORD=
RABBITMQ_DEFAULT_PASS=
SMTP_PASSWORD=
```

Never commit `.env` — it is gitignored.

## Deployment

The project is deployed on [Coolify](https://coolify.io) with Traefik as the reverse proxy. Each service (backend, frontend, notification) is deployed as a separate Docker container pulling from this repository.

## License

MIT
