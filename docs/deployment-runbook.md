# Deployment Runbook (Git tag → GitHub Release → LXC)

Denne runbook er en “agent-skill”, så fremtidige ændringer/deploys kan laves ensartet og hurtigt.

## Sikkerhedsregel (vigtigt)

Ved live deployment i Proxmox skal der altid være **eksplicit bruger-godkendelse** lige før kommandoen, der faktisk opdaterer (`indkob-update`) køres.

Kort praksis:

1. Gennemfør discovery/verifikation (node, CT, status).
2. Vis den konkrete update-kommando til brugeren.
3. Spørg: “Skal jeg køre den nu?”
4. Kør først efter tydeligt ja.

## Overblik

Deploy-flowet er:

1. Opret/push et tag der matcher `v*` (fx `v1.2.0`)
2. GitHub Actions workflow `.github/workflows/release.yml` bygger release
3. Workflow uploader asset: `indkob-release-linux-x64.tar.gz`
4. Server/LXC henter seneste release via `indkob-update`

## Trigger for automatisk build

Workflow trigger:

```yml
on:
  push:
    tags:
      - "v*"
```

Kun tags der starter med `v` starter release-workflowet.

## Release fra lokal maskine

```bash
git tag vX.Y.Z
git push origin vX.Y.Z
```

Verificér bagefter i GitHub Actions + Releases at workflow er grønt og asset er uploadet.

## Førstegangs deploy i LXC

Kør i repo checkout på server/container:

```bash
sudo bash deploy/lxc-bootstrap.sh <owner>/<repo>
```

Det installerer nginx + systemd service og kører første update.

## Opdatering i LXC

```bash
sudo /usr/local/bin/indkob-update <owner>/<repo>
```

Repo har også helper:

```bash
./update.sh <owner>/<repo>
```

## Proxmox cluster-procedure (denne opsætning)

Følgende er den praktiske “skill” til dette miljø (opdateres ved ændringer):

- Proxmox noder: `192.168.50.225`-`192.168.50.228`
- Kendt SSH nøgle: `/home/Casper/.ssh/codex_cluster`
- Indkøb container: `CT 110` (navn: `indkob`)
- Senest observeret host-node for CT 110: `balder` (`192.168.50.228`)

### 1) Bekræft SSH adgang til noder

```bash
for ip in 192.168.50.225 192.168.50.226 192.168.50.227 192.168.50.228; do
  echo "== $ip =="
  ssh -i /home/Casper/.ssh/codex_cluster -o IdentitiesOnly=yes -o BatchMode=yes -o ConnectTimeout=5 root@$ip 'hostname'
done
```

### 2) Find hvilken node der aktuelt hoster CT 110

Kør fx mod en vilkårlig node (cluster API returnerer korrekt node):

```bash
ssh -i /home/Casper/.ssh/codex_cluster -o IdentitiesOnly=yes root@192.168.50.225 \
  "pvesh get /cluster/resources --type vm --output-format json" \
  | jq -r '.[] | select(.vmid==110) | "vmid=\(.vmid) node=\(.node) type=\(.type) status=\(.status) name=\(.name)"'
```

### 3) Kør update i CT 110 (efter eksplicit godkendelse)

Hvis CT 110 fx er på `192.168.50.228`:

```bash
ssh -i /home/Casper/.ssh/codex_cluster -o IdentitiesOnly=yes root@192.168.50.228 \
  "pct exec 110 -- /usr/local/bin/indkob-update Meatgiffel/indkob"
```

### 4) Verificér service efter update

```bash
ssh -i /home/Casper/.ssh/codex_cluster -o IdentitiesOnly=yes root@192.168.50.228 \
  "pct exec 110 -- systemctl status indkob-api --no-pager"
```

Ekstra logs:

```bash
ssh -i /home/Casper/.ssh/codex_cluster -o IdentitiesOnly=yes root@192.168.50.228 \
  "pct exec 110 -- journalctl -u indkob-api -n 200 --no-pager"
```

> Bemærk: CT kan flytte node over tid. Find altid aktiv host-node i trin 2 før trin 3/4.

## Verifikation efter deploy

```bash
sudo systemctl status indkob-api --no-pager
sudo journalctl -u indkob-api -n 200 --no-pager
```

Kontrollér også at websitet svarer, og at `/api` virker via nginx.

## Kendte begrænsninger

- Release-asset er `linux-x64` (x86_64) og deploy-script afviser andre arkitekturer.
- Flowet bruger “latest release” i GitHub ved update.
