# Indkøb (client)

Angular-frontend til Indkøb (PWA) bygget med PrimeNG/PrimeFlex.

## Ruter
- `/list`: Opret og vedligehold indkøbsseddel (inkl. opret vare fra søgning).
- `/shop`: Tæt “handle”-visning med filter (Alle/Færdige/Mangler).
- `/items`: Varekatalog.

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
