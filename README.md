# Indkøb

> Disclaimer: Alt kode i dette repository er lavet af AI.

Indkøb er en lille PWA til en delt indkøbsliste. Du vedligeholder et varekatalog (navn + område i butikken), opretter linjer til indkøbssedlen og kan krydse af på mobilen, når du står i butikken.

## Funktioner
- Varekatalog med hurtig oprettelse og inline redigering.
- Indkøbsseddel der grupperer efter område.
- “Handle”-visning der prioriterer tæt liste og touch.
- Live synkronisering mellem klienter (SignalR) for indkøbsseddel.
- Ugentlig madplan (aftensmad) med autosave.
- PWA (offline-klar), PrimeNG UI.
- Simpelt login (brugernavn + password) og bruger-administration.

## Teknologi
- Frontend: Angular (standalone) + PrimeNG/PrimeFlex (`client/`).
- Backend: ASP.NET Core (.NET 10) + EF Core + SQLite (`Api/`).

## Dokumentation
- API contract: `docs/api-contract.md`
- Prioriteret hardening-plan: `docs/hardening-plan.md`
- Agent/repo guidelines: `AGENTS.md`

## Kør lokalt
### API
```bash
cd Api
dotnet restore
dotnet tool install --global dotnet-ef   # hvis du ikke allerede har den
dotnet ef database update
dotnet run
```

Kræver .NET 10 SDK.

### Client
```bash
cd client
npm install
npm start
# http://localhost:4200
```

Anbefalet Node.js: LTS (samme major som i CI er 20).

## Deployment til LXC (anbefalet)
Setup’et her gør, at din LXC ikke behøver Node eller .NET. GitHub Actions bygger en release, og LXC downloader bare seneste bundle og genstarter services.

### 1) Lav en release
Workflowet i `.github/workflows/release.yml` kører på tags der matcher `v*`.

```bash
git tag v1.0.0
git push origin v1.0.0
```

Efter workflowet er færdigt, ligger der assets i din GitHub Release:
- `indkob-release-linux-x64.tar.gz`

### 2) Installér i en Debian/Ubuntu LXC
Krav: systemd i containeren.
Krav: x86_64 (64-bit).

Fra en checkout i containeren:
```bash
sudo bash deploy/lxc-bootstrap.sh <owner>/<repo>
```

Det installerer nginx + en systemd service (`indkob-api`) og kører første deploy.
Alternativt (nemmere at huske):
```bash
./bootstrap.sh <owner>/<repo>
```

### 3) Opdatér senere
```bash
sudo /usr/local/bin/indkob-update <owner>/<repo>
```
Alternativt (nemmere at huske):
```bash
./update.sh <owner>/<repo>
```

### Data og ports
- SQLite DB: `/var/lib/indkob/grocery.db` (bevares ved opdatering)
- Data Protection keys (login-cookie krypteringsnøgler): `/var/lib/indkob/dataprotection-keys`
- Web: nginx på port 80
- API: lytter på `127.0.0.1:5046` og proxys via nginx `/api`

## Login
Første gang API’et starter på en helt ny database, opretter den automatisk en admin-bruger:
- Brugernavn: `admin` (kan ændres)
- Password: `changeme` (skift det med det samme i “Brugere”)

Sæt selv bootstrap-login (valgfrit) ved at sætte environment variables på `indkob-api` servicen:
- `Auth__BootstrapUser`
- `Auth__BootstrapPassword`

## Fejlsøgning
Hvis frontend virker men API-kald fejler (fx `502 Bad Gateway`), så er det typisk fordi API-servicen ikke kører:

```bash
sudo systemctl status indkob-api --no-pager
sudo journalctl -u indkob-api -n 200 --no-pager
```
