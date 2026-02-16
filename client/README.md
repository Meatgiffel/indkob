# Indkøb (client)

Angular-frontend til Indkøb (PWA) bygget med PrimeNG/PrimeFlex.

## Ruter
- `/login`: Login med brugernavn/password (inkl. husk mig).
- `/list`: Opret og vedligehold indkøbsseddel (inkl. opret vare fra søgning).
- `/shop`: Tæt “handle”-visning med filter (Alle/Færdige/Mangler).
- `/mealplan`: Ugevis madplan med autosave pr. dag.
- `/items`: Varekatalog.
- `/users`: Brugeradministration (kun admin).

## Udvikling
```bash
cd client
npm install
npm start
```

`npm start` bruger `proxy.conf.json`, så `/api` proxys til API’et på `http://localhost:5046`.

## Build
```bash
cd client
npm run build
```

Output ligger i `client/dist/client/browser`.
