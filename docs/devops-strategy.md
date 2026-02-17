# DevOps Strategy — Technical Decisions & Security Checklist

> **Date**: 2026-02-16 (revised)
> **Author**: AI Agent (DevOps Engineer Senior)
> **Scope**: Build, containerization, CI/CD, and deployment strategy

---

## 0. Critical Corrections from Previous Implementation

The previous implementation introduced **nginx** as both an internal SPA server and a reverse proxy. This was incorrect for the following reasons:

| Problem | Risk | Resolution |
| --- | --- | --- |
| **nginx reverse proxy in docker-compose.prod.yml** | Conflicts with the existing Caddy on the VM. Two proxies competing for port 80 causes binding errors or request routing conflicts. | Removed entirely. Caddy already handles TLS termination, security headers, and reverse proxying. |
| **nginx inside the frontend container** | Unnecessarily large attack surface (~25MB nginx + config files). nginx master process requires root for port 80, breaking true non-root enforcement. | Replaced with `serve` (~2MB), which runs entirely as non-root on port 3000. |
| **`ports:` directives in prod compose** | Exposes containers directly on the VM host network, bypassing Caddy. Anyone with VM IP access could hit the raw backend on :8080. | Changed to `expose:` only. Caddy routes via Docker DNS on the shared `caddy-network`. |
| **nginx config files (`docker/nginx/`, `frontend/docker/nginx-frontend.conf`)** | Maintenance burden for config that duplicates what Caddy already provides (security headers, TLS, proxying). | Deleted completely. |

---

## 1. Architecture Decisions & Justifications

### 1.1 Frontend Static Server: `serve` vs nginx vs Caddy file_server

| Option | Pros | Cons | Verdict |
| --- | --- | --- | --- |
| **`serve` (Node.js)** | ~2MB, native SPA fallback (`-s` flag), runs as non-root on any port, zero config, same base image as build stage | Slightly higher memory than C-based servers (~30MB RSS) | **Chosen** |
| **nginx** | Battle-tested, efficient, low memory | Requires root for ports <1024, separate base image (nginx:alpine), config management overhead, duplicates Caddy responsibility | Rejected |
| **Caddy `file_server`** | Could eliminate the frontend container entirely | Requires mounting build artifacts on the VM Caddy instance, couples frontend deployment to Caddy config, breaks container isolation | Rejected |

**Justification**: `serve` provides the best balance of simplicity, security, and container isolation. The frontend container remains a self-contained unit that Caddy proxies to via `caddy-network`. No config files needed — SPA fallback is a single CLI flag.

### 1.2 Dockerfiles — Multi-Stage Builds

| Decision | Justification |
| --- | --- |
| **3-stage frontend build (deps → build → runtime)** | Dependencies are cached independently. The runtime stage contains only `serve` + static assets — no `node_modules`, no source code, no devDependencies. |
| **`--ignore-scripts` on npm ci** | Prevents execution of arbitrary post-install scripts from dependencies (event-stream, ua-parser-js attack vectors). |
| **Non-root user in both containers** | Backend: `appuser`. Frontend: `appuser`. Neither container has root access. CIS Docker Benchmark 4.1. |
| **Alpine-based images** | ~5x smaller than Debian-based. Smaller attack surface, faster pulls. |
| **OCI labels** | Enables image provenance tracking in GHCR. Required for SBOM and supply chain security. |
| **Build args for version (backend)** | Embeds `InformationalVersion` in the .NET assembly for runtime version reporting. |

### 1.3 Docker Compose Strategy

| Decision | Justification |
| --- | --- |
| **`docker-compose.yml` (local) — self-contained, no proxy** | Includes Postgres + Redis. Developers access services directly on `:8080` (backend) and `:3000` (frontend). No proxy layer needed locally. |
| **`docker-compose.prod.yml` — no ports, only expose** | Containers are reachable only via Docker DNS on `caddy-network`. No direct host port exposure. Caddy handles all external traffic. |
| **`caddy-network` (external)** | Shared Docker network where Caddy already runs. Backend and frontend join this network so Caddy can route to them by container name. |
| **`shared-db-network` (external)** | Backend connects to existing PostgreSQL/Redis containers without managing their lifecycle. |
| **`read_only: true` + `tmpfs`** | Prevents runtime filesystem writes. `tmpfs` provides required writable areas (`/tmp`, env-config.js generation). |
| **Resource limits** | Backend: 512MB/1 CPU. Frontend: 128MB/0.5 CPU. Prevents resource exhaustion on single-VM deployment. |
| **`no-new-privileges`** | Blocks privilege escalation via setuid/setgid binaries inside containers. |

### 1.4 CI/CD Pipeline Design

| Decision | Justification |
| --- | --- |
| **Path-filtered triggers** | Avoids running backend CI for frontend-only changes and vice versa. Saves ~60% of CI minutes. |
| **`dotnet format --verify-no-changes`** | Enforces consistent code style without modifying files. |
| **`dotnet list package --vulnerable`** | Built-in NuGet vulnerability scanning for transitive dependencies. |
| **Trivy for container scanning** | Industry-standard, free, SARIF output for GitHub Security tab. |
| **CodeQL for both stacks** | GitHub-native SAST. Catches SQL injection, XSS, path traversal, etc. |
| **Docker build only on `main` push** | PRs run tests only. Image builds happen on merge, avoiding registry bloat. |
| **Manual deploy trigger (`workflow_dispatch`)** | Production deploys require explicit human action. No accidental deploys from CI. |
| **SBOM + Provenance** | Supply chain attestation on every image build. |

### 1.5 Versioning Strategy

| Tag Pattern | Purpose | Example |
| --- | --- | --- |
| `latest` | Latest successful build on main | Always points to newest |
| `v{major}.{minor}.{patch}` | Release versions (from git tags) | `v1.2.3` |
| `v{major}.{minor}` | Minor version tracking | `v1.2` |
| `sha-{short}` | Every commit build | `sha-abc1234` |

**Rule**: Semver tags are immutable. `latest` and `sha-*` are mutable by definition.

### 1.6 Deploy Strategy

| Decision | Justification |
| --- | --- |
| **Pull → Stop → Start (per service)** | Simple and idempotent. Caddy is never touched during deploys. |
| **Health checks for both services after deploy** | Backend: `GET /api/v1/health`. Frontend: `GET /` on port 3000. Logs container output on failure. |
| **No proxy restarted during deploy** | Caddy runs independently. Container restarts are invisible to Caddy — Docker DNS updates automatically. |
| **Image pruning on deploy** | Prevents disk exhaustion. Keeps images < 7 days old. |

---

## 2. Security Checklist

### 2.1 Container Security

- [x] Non-root user in backend container (`appuser`)
- [x] Non-root user in frontend container (`appuser`)
- [x] `read_only: true` filesystem in production
- [x] `no-new-privileges` security option
- [x] Resource limits (memory/CPU) defined
- [x] Alpine minimal base images
- [x] Security headers via `serve.json` (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy)
- [x] HEALTHCHECK defined in all Dockerfiles
- [x] No secrets in Dockerfiles or image layers
- [x] `.dockerignore` excludes tests, logs, IDE files
- [x] No nginx in any container
- [x] No container exposes ports directly (`expose:` only in prod)

### 2.2 CI/CD Security

- [x] Secrets via GitHub Secrets (never hardcoded)
- [x] `dotnet list package --vulnerable` — NuGet vulnerability scan
- [x] `npm audit` — npm vulnerability scan
- [x] Trivy container image scanning
- [x] CodeQL SAST for C# and TypeScript
- [x] ESLint enforced (fail on error)
- [x] `dotnet format` validation
- [x] SBOM generation on Docker build
- [x] Provenance attestation on Docker build
- [x] SARIF upload to GitHub Security tab

### 2.3 Runtime Security

- [x] Firebase ID tokens validated server-side only
- [x] HttpOnly + Secure + SameSite=Lax cookies (per ADR)
- [x] No tokens stored in frontend (per ADR)
- [x] Environment variables for all secrets
- [x] Firebase credential mounted as read-only volume
- [x] `.env` files excluded from version control
- [x] CORS restricted to allowed origins
- [x] TLS termination at Caddy level (not in containers)

### 2.4 Network Security (VM/OCI)

- [x] Caddy handles all external traffic (ports 80/443)
- [x] Containers communicate only via internal Docker networks
- [x] No container binds to host ports in production
- [ ] Configure OCI Security Lists: allow only ports 80, 443, 22 (restricted)
- [ ] Enable iptables/nftables firewall on VM
- [ ] Configure HSTS header in Caddy after SSL confirmed
- [ ] Restrict SSH to key-based auth only
- [ ] Set up fail2ban for SSH brute-force protection
- [ ] Regular OS security updates (`unattended-upgrades`)

---

## 3. Files Created/Modified

| File | Action | Purpose |
| --- | --- | --- |
| `backend/Dockerfile` | **Modified** | Non-root user, build args, OCI labels, hardening envs |
| `frontend/Dockerfile` | **Rewritten** | 3-stage build, `serve` replaces nginx, non-root user, port 3000 |
| `frontend/docker/env.sh` | **Modified** | Updated paths from `/usr/share/nginx/html` to `/app/dist` |
| `frontend/docker/serve.json` | **Created** | Security headers and cache control for `serve` |
| `docker-compose.yml` | **Modified** | Local-only, no proxy, direct port access, Postgres + Redis |
| `docker-compose.prod.yml` | **Rewritten** | No nginx, no ports, expose-only, caddy-network, hardened |
| `.env.example` | **Modified** | Updated port references |
| `.github/workflows/backend-ci.yml` | **Created** | Full backend CI pipeline |
| `.github/workflows/frontend-ci.yml` | **Modified** | Docker build/push, Trivy, CodeQL, npm audit |
| `.github/workflows/deploy.yml` | **Modified** | Removed proxy restart, added frontend health check |
| `frontend/docker/nginx-frontend.conf` | **Deleted** | nginx removed |
| `docker/nginx/default.conf` | **Deleted** | nginx proxy removed |

---

## 4. Integration with Caddy (VM)

The existing Caddy on the VM should have a configuration similar to:

```caddyfile
# Example Caddyfile — managed separately on the VM
yourdomain.com {
    # Frontend SPA
    handle {
        reverse_proxy l2sledger-frontend:3000
    }

    # Backend API
    handle /api/* {
        reverse_proxy l2sledger-backend:8080
    }

    # Caddy automatically provisions and renews TLS certificates
    # Security headers are handled by Caddy globally
}
```

**Important**: This Caddyfile is NOT managed by this repository. It lives on the VM and is maintained by the infrastructure team. The `handle /api/*` block must be placed **before** the catch-all `handle` block or use `handle_path` as needed.

**Prerequisites on VM**:
1. `caddy-network` Docker network must exist: `docker network create caddy-network`
2. Caddy must be connected to `caddy-network`
3. Caddy must be configured to route to `l2sledger-backend:8080` and `l2sledger-frontend:3000`

---

## 5. Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `VM_HOST` | Production VM IP or hostname |
| `VM_USER` | SSH user on the VM |
| `VM_SSH_KEY` | Private SSH key for the VM |
| `VM_SSH_PORT` | SSH port (default: 22) |
| `VM_DEPLOY_PATH` | Path to compose files on VM (default: `/opt/l2sledger`) |
| `VITE_API_BASE_URL` | Production API URL |
| `VITE_FIREBASE_API_KEY` | Firebase API key |
| `VITE_FIREBASE_AUTH_DOMAIN` | Firebase auth domain |
| `VITE_FIREBASE_PROJECT_ID` | Firebase project ID |
| `VITE_FIREBASE_STORAGE_BUCKET` | Firebase storage bucket |
| `VITE_FIREBASE_MESSAGING_SENDER_ID` | Firebase messaging sender ID |
| `VITE_FIREBASE_APP_ID` | Firebase app ID |

### Environment: `production`
- **Required reviewers**: At least 1 team member must approve deploys
- **Deployment branches**: Restrict to `main` only

---

## 6. VM Setup Checklist

```bash
# 1. Create deploy directory
sudo mkdir -p /opt/l2sledger
sudo chown $USER:$USER /opt/l2sledger

# 2. Copy production compose file
scp docker-compose.prod.yml user@vm:/opt/l2sledger/

# 3. Create .env with production values
nano /opt/l2sledger/.env

# 4. Place Firebase credential
mkdir -p /opt/l2sledger/secrets
chmod 600 /opt/l2sledger/secrets/firebase-credential.json

# 5. Authenticate to GHCR (one-time)
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# 6. Ensure Docker networks exist
docker network create caddy-network 2>/dev/null || true
docker network create shared-db-network 2>/dev/null || true

# 7. Ensure Caddy is on caddy-network
docker network connect caddy-network caddy 2>/dev/null || true

# 8. First deploy
cd /opt/l2sledger
export IMAGE_TAG=latest
docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d
```

---

## 7. Future Improvements

### Short-term
1. **Content-Security-Policy**: Define strict CSP in Caddy config
2. **HSTS**: Enable in Caddy after TLS confirmed
3. **Pre-commit hooks**: `dotnet format` + `eslint`

### Medium-term
1. **Blue-Green deploys**: Two container instances per service, Caddy routes to healthy one
2. **Container log aggregation**: Loki/Grafana stack
3. **Automated rollback**: Detect health failures and revert to previous image tag
4. **Dependabot/Renovate**: Automated dependency updates

### Long-term
1. **Kubernetes migration** (OKE)
2. **Terraform IaC** for OCI infrastructure
3. **Image signing** with cosign
4. **OIDC for deploy**: Replace SSH with GitHub OIDC tokens
