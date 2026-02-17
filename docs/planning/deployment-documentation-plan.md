# Plano de Documentação: Configuração e Deploy do L2SLedger

> **Data**: 2026-02-16  
> **Tipo**: Planejamento de Documentação  
> **Status**: Aguardando Aprovação

---

## 📋 Sumário Executivo

Este documento define o plano completo para criação de documentação de **configuração e deploy** do L2SLedger, cobrindo desde o ambiente de desenvolvimento local até produção em OCI.

### Objetivos
- Consolidar informações dispersas em guias coerentes
- Criar documentação completa de setup e deploy
- Facilitar onboarding de novos desenvolvedores
- Documentar procedimentos de troubleshooting e recuperação

---

## 1. Análise do Estado Atual

### 1.1 Documentação Existente

| Documento | Escopo | Cobertura | Status |
|-----------|--------|-----------|--------|
| [docs/devops-strategy.md](../devops-strategy.md) | Decisões DevOps, segurança, CI/CD | Técnico profundo | ✅ Completo |
| [backend/RUNNING.md](../../backend/RUNNING.md) | Execução local do backend | Parcial | ✅ Existe |
| [frontend/README.md](../../frontend/README.md) | Quick start frontend | Básico | ✅ Existe |
| [README.md](../../README.md) | Visão geral do projeto | Alto nível | ✅ Existe |
| [.env.example](../../.env.example) | Template de variáveis | Completo | ✅ Existe |
| [docker-compose.yml](../../docker-compose.yml) | Stack local | Configurado | ✅ Existe |
| [docker-compose.prod.yml](../../docker-compose.prod.yml) | Stack produção | Configurado | ✅ Existe |

### 1.2 Lacunas Identificadas

#### ❌ Ausências Críticas
- Não existe guia consolidado de **configuração inicial completa**
- Falta guia de **deploy para produção passo a passo**
- Ausência de **troubleshooting guide** estruturado
- Sem documentação de **pré-requisitos de infraestrutura**
- Falta guia de **rollback e recuperação de desastres**
- Documentação de **variáveis de ambiente** dispersa

#### ⚠️ Problemas Identificados
- Informações sobre configuração estão em múltiplos arquivos
- Desenvolvedores precisam "caçar" informações
- Falta clareza sobre diferenças entre ambientes
- Procedimentos de emergência não documentados

---

## 2. Estrutura Proposta da Documentação

### 2.1 Hierarquia de Diretórios

```
docs/
└── deployment/
    ├── README.md                    # Índice e visão geral
    ├── 1-prerequisites.md           # Pré-requisitos de infraestrutura
    ├── 2-local-development.md       # Configuração ambiente local (sem Docker)
    ├── 3-docker-local.md            # Execução com Docker Compose local
    ├── 4-production-setup.md        # Configuração inicial do servidor (one-time)
    ├── 5-production-deploy.md       # Deploy em produção
    ├── 6-environment-variables.md   # Referência completa de variáveis
    ├── 7-caddy-configuration.md     # Configuração do Caddy (reverse proxy)
    ├── 8-monitoring-health.md       # Monitoramento e health checks
    ├── 9-troubleshooting.md         # Guia de resolução de problemas
    └── 10-rollback-recovery.md      # Rollback e recuperação
```

### 2.2 Princípios de Organização

| Princípio | Justificativa |
|-----------|---------------|
| **Sequencial** | Documentos numerados seguem ordem natural de execução |
| **Separação de Contextos** | Local vs Produção claramente separados |
| **Referência Centralizada** | Variáveis de ambiente em documento único |
| **Troubleshooting Isolado** | Problemas comuns em guia dedicado |
| **Recuperação Dedicada** | Procedimentos de emergência destacados |

---

## 3. Detalhamento de Cada Documento

### 3.1 `deployment/README.md` — Índice e Visão Geral

**Objetivo**: Ponto de entrada para toda documentação de deploy.

**Público-Alvo**: 
- Desenvolvedores iniciantes no projeto
- DevOps realizando deploy
- Equipe de suporte

**Conteúdo**:
- Visão geral dos ambientes (DEV/DEMO/PROD)
- Tabela de decisão: "O que você quer fazer?" → documento correto
- Links para cada guia em ordem de execução
- Mapa de responsabilidades (quem faz o quê)
- Fluxo de trabalho típico
- Convenções e terminologia

**Estrutura**:
```markdown
# Deploy Documentation — L2SLedger

## Ambientes
- DEV / DEMO / PROD

## Guias Disponíveis
1. Prerequisites
2. Local Development
...

## Decisões Rápidas
- Primeira vez? → Prerequisites
- Desenvolvedor? → Local Development
- Deploy? → Production Deploy

## Convenções
- Terminologia
- Padrões de nomenclatura
```

---

### 3.2 `1-prerequisites.md` — Pré-requisitos

**Objetivo**: Garantir que o ambiente base está pronto antes de qualquer configuração.

**Público-Alvo**: 
- Novos desenvolvedores
- DevOps preparando infraestrutura

**Conteúdo**:

#### Ferramentas Locais (Desenvolvimento)
- **Node.js 20+** (verificação: `node --version`)
- **.NET 9 SDK** (verificação: `dotnet --version`)
- **Docker Desktop** ou Docker Engine 24+ (verificação: `docker --version`)
- **Git** (verificação: `git --version`)
- **PostgreSQL 17** (opcional local, obrigatório produção)
- **Redis 7+** (opcional local, obrigatório produção)

#### Serviços Externos
- **Firebase Authentication**:
  - Como criar projeto
  - Configurar Email/Password provider
  - Gerar Service Account Key
  - Obter Web API Key
- **GitHub Container Registry (GHCR)**:
  - Autenticação
  - Personal Access Token

#### Infraestrutura para PROD
- **OCI VM**:
  - Ubuntu 22.04 LTS ou superior
  - Mínimo 2 vCPUs, 4GB RAM
  - 50GB storage
  - IP público
  - Portas abertas: 80, 443, 22
- **Docker** instalado na VM
- **Caddy** instalado e rodando
- **PostgreSQL 17** (container ou gerenciado)
- **Redis 7+** (container ou gerenciado)

#### Credenciais Necessárias
- Firebase Service Account JSON
- GitHub Token (read:packages)
- JWT Secret (mínimo 32 caracteres)
- Senhas de banco de dados

#### Redes Docker (Produção)
```bash
docker network create caddy-network
docker network create shared-db-network
```

**Checklist de Verificação**:
```markdown
- [ ] Node.js 20+ instalado
- [ ] .NET 9 SDK instalado
- [ ] Docker rodando
- [ ] Firebase projeto criado
- [ ] Service Account baixado
- [ ] PostgreSQL acessível
- [ ] Redes Docker criadas (prod)
```

---

### 3.3 `2-local-development.md` — Desenvolvimento Local (Sem Docker)

**Objetivo**: Desenvolvedores que querem rodar diretamente no sistema operacional.

**Público-Alvo**: 
- Desenvolvedores backend
- Desenvolvedores frontend
- Debug e desenvolvimento ativo

**Conteúdo**:

#### Backend (.NET)

**1. Configuração do PostgreSQL**
```sql
CREATE DATABASE l2sledger;
CREATE USER l2sledger WITH ENCRYPTED PASSWORD 'l2sledger';
GRANT ALL PRIVILEGES ON DATABASE l2sledger TO l2sledger;
```

**2. Arquivo `appsettings.Development.json`**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=l2sledger;Username=l2sledger;Password=l2sledger"
  },
  "Firebase": {
    "CredentialPath": "C:\\path\\to\\firebase-credential.json",
    "WebApiKey": "YOUR_FIREBASE_WEB_API_KEY"
  },
  "JWT": {
    "Secret": "your-local-jwt-secret-min-32-chars",
    "Issuer": "https://localhost",
    "Audience": "l2sledger"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

**3. Executar Backend**
```bash
cd backend
dotnet restore
dotnet ef database update --project src/L2SLedger.Infrastructure --startup-project src/L2SLedger.API
cd src/L2SLedger.API
dotnet run
```

**4. Verificar**
- Swagger: http://localhost:8080/swagger
- Health: http://localhost:8080/api/v1/health

#### Frontend (React)

**1. Arquivo `.env.development`**
```env
VITE_API_BASE_URL=http://localhost:8080/api
VITE_FIREBASE_API_KEY=your-api-key
VITE_FIREBASE_AUTH_DOMAIN=your-project.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=your-project-id
VITE_FIREBASE_STORAGE_BUCKET=your-project.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
VITE_FIREBASE_APP_ID=1:123456789:web:abc
VITE_ENABLE_DEVTOOLS=true
```

**2. Executar Frontend**
```bash
cd frontend
npm install
npm run dev
```

**3. Verificar**
- App: http://localhost:3000

#### Integração
- Backend deve estar rodando antes do frontend
- CORS configurado para `http://localhost:3000`
- Firebase deve estar configurado em ambos

---

### 3.4 `3-docker-local.md` — Desenvolvimento com Docker Compose

**Objetivo**: Rodar stack completa localmente via containers (ambiente mais próximo da produção).

**Público-Alvo**: 
- Desenvolvedores testando integração
- QA local
- Testes de integração

**Conteúdo**:

#### 1. Preparação

**Copiar e Configurar `.env`**
```bash
cp .env.example .env
```

**Editar `.env`** com valores reais:
```env
# Firebase
FIREBASE_CREDENTIAL_PATH=./secrets/firebase-credential.json
VITE_FIREBASE_API_KEY=your-actual-key
VITE_FIREBASE_AUTH_DOMAIN=your-project.firebaseapp.com
# ... outras variáveis
```

**Colocar Firebase Credential**
```bash
mkdir -p secrets/
cp ~/Downloads/firebase-credential.json secrets/
```

#### 2. Executar Stack

**Build e Start**
```bash
docker compose up -d
```

**Verificar Status**
```bash
docker compose ps
```

#### 3. Acessar Serviços

| Serviço | URL | Container |
|---------|-----|-----------|
| Backend API | http://localhost:8080 | l2sledger-backend |
| Frontend | http://localhost:3000 | l2sledger-frontend |
| PostgreSQL | localhost:5432 | postgres |

#### 4. Logs e Debug

**Ver Logs**
```bash
docker compose logs -f backend
docker compose logs -f frontend
```

**Executar Comandos no Container**
```bash
docker exec -it l2sledger-backend sh
docker exec -it l2sledger-frontend sh
```

#### 5. Rebuild Após Mudanças

**Backend**
```bash
docker compose build backend
docker compose up -d backend
```

**Frontend**
```bash
docker compose build frontend
docker compose up -d frontend
```

#### 6. Parar e Limpar

```bash
docker compose down
docker compose down -v  # com volumes
```

---

### 3.5 `4-production-setup.md` — Configuração Inicial do Servidor

**Objetivo**: Preparar OCI VM para primeiro deploy (procedimento único, one-time setup).

**Público-Alvo**: 
- DevOps
- Administradores de sistema

**Conteúdo**:

#### 1. Pré-requisitos da VM

**Sistema Operacional**
- Ubuntu 22.04 LTS ou superior
- Acesso root ou sudo
- SSH configurado

**Atualizar Sistema**
```bash
sudo apt update && sudo apt upgrade -y
```

#### 2. Instalar Docker

```bash
# Remover versões antigas
sudo apt remove docker docker-engine docker.io containerd runc

# Instalar dependências
sudo apt install -y ca-certificates curl gnupg lsb-release

# Adicionar GPG key oficial do Docker
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Adicionar repositório
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Instalar Docker Engine
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Verificar
docker --version
```

#### 3. Instalar Caddy

```bash
sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
sudo apt update
sudo apt install caddy

# Verificar
caddy version
```

#### 4. Criar Estrutura de Diretórios

```bash
sudo mkdir -p /opt/l2sledger
sudo mkdir -p /opt/l2sledger/secrets
sudo chown -R $USER:$USER /opt/l2sledger
chmod 700 /opt/l2sledger/secrets
```

#### 5. Criar Redes Docker

```bash
docker network create caddy-network
docker network create shared-db-network
```

#### 6. Autenticar no GHCR

```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin
```

#### 7. Configurar Arquivo `.env` de Produção

```bash
cd /opt/l2sledger
nano .env
```

**Conteúdo** (valores reais de produção):
```env
# Environment
ASPNETCORE_ENVIRONMENT=Production
NODE_ENV=production

# PostgreSQL
POSTGRES_HOST=postgres-container-or-ip
POSTGRES_PORT=5432
POSTGRES_USER=l2sledger_prod
POSTGRES_PASSWORD=STRONG_PASSWORD_HERE

# JWT
JWT_SECRET=PRODUCTION_SECRET_MIN_64_CHARS_RECOMMENDED
JWT_ISSUER=https://yourdomain.com
JWT_AUDIENCE=l2sledger-prod

# Firebase
FIREBASE_CREDENTIAL_PATH=/opt/l2sledger/secrets/firebase-credential.json
VITE_FIREBASE_API_KEY=actual-prod-key
VITE_FIREBASE_AUTH_DOMAIN=prod-project.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=prod-project-id
VITE_FIREBASE_STORAGE_BUCKET=prod-project.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
VITE_FIREBASE_APP_ID=1:123456789:web:abc

# CORS
CORS_ALLOWED_ORIGINS=https://yourdomain.com

# Frontend
VITE_API_BASE_URL=https://yourdomain.com/api
VITE_ENABLE_DEVTOOLS=false

# Image Registry
GHCR_OWNER=your-github-org
IMAGE_TAG=latest
```

#### 8. Upload do Firebase Credential

```bash
scp firebase-credential.json user@vm-ip:/opt/l2sledger/secrets/
chmod 600 /opt/l2sledger/secrets/firebase-credential.json
```

#### 9. Copiar `docker-compose.prod.yml`

```bash
scp docker-compose.prod.yml user@vm-ip:/opt/l2sledger/
```

#### 10. Configuração de Segurança

**Firewall (UFW)**
```bash
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

**SSH Hardening**
```bash
# Editar /etc/ssh/sshd_config
sudo nano /etc/ssh/sshd_config

# Configurações recomendadas:
PermitRootLogin no
PasswordAuthentication no
PubkeyAuthentication yes
Port 22  # ou porta customizada

sudo systemctl restart sshd
```

**Fail2Ban**
```bash
sudo apt install -y fail2ban
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
```

#### 11. Verificação Final

```bash
# Docker rodando
docker ps

# Redes criadas
docker network ls | grep -E 'caddy-network|shared-db-network'

# Caddy rodando
systemctl status caddy

# Diretórios existem
ls -la /opt/l2sledger

# .env configurado
cat /opt/l2sledger/.env

# Firebase credential presente
ls -l /opt/l2sledger/secrets/
```

---

### 3.6 `5-production-deploy.md` — Deploy em Produção

**Objetivo**: Guia de deploy via GitHub Actions e manual.

**Público-Alvo**: 
- DevOps executando deploy
- Engenheiros em operações de emergência

**Conteúdo**:

#### Método 1: GitHub Actions (Recomendado)

**1. Preparação**
- Garantir que imagens foram buildadas e pushadas no GHCR
- Verificar tag disponível (ex: `v1.2.3`, `sha-abc1234`)

**2. Trigger do Workflow**

Via GitHub UI:
1. Ir para **Actions** → **Deploy to Production**
2. Clicar em **Run workflow**
3. Preencher:
   - `image_tag`: `v1.2.3` ou `sha-abc1234`
   - `service`: `all`, `backend`, ou `frontend`
4. Confirmar

**3. Acompanhar Execução**
- Verificar logs em tempo real
- Aguardar health checks
- Confirmar sucesso

**4. Verificação Pós-Deploy**
```bash
# Via SSH na VM
docker compose -f /opt/l2sledger/docker-compose.prod.yml ps

# Verificar logs
docker logs l2sledger-backend --tail 100
docker logs l2sledger-frontend --tail 100
```

#### Método 2: Deploy Manual SSH (Emergência)

**1. Conectar na VM**
```bash
ssh user@vm-ip
cd /opt/l2sledger
```

**2. Autenticar no GHCR**
```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin
```

**3. Definir Tag**
```bash
export IMAGE_TAG=v1.2.3
export GHCR_OWNER=your-org
```

**4. Pull de Imagens**

**Todos os serviços:**
```bash
docker compose -f docker-compose.prod.yml pull
```

**Apenas backend:**
```bash
docker compose -f docker-compose.prod.yml pull backend
```

**Apenas frontend:**
```bash
docker compose -f docker-compose.prod.yml pull frontend
```

**5. Deploy**

**Todos os serviços:**
```bash
docker compose -f docker-compose.prod.yml up -d
```

**Apenas backend:**
```bash
docker compose -f docker-compose.prod.yml up -d --no-deps backend
```

**Apenas frontend:**
```bash
docker compose -f docker-compose.prod.yml up -d --no-deps frontend
```

**6. Verificar Status**
```bash
docker compose -f docker-compose.prod.yml ps
```

**7. Limpar Imagens Antigas**
```bash
docker image prune -f --filter "until=168h"
```

#### Health Check Manual

**Backend**
```bash
# Dentro da VM
curl -f http://localhost:8080/api/v1/health

# Externo
curl https://yourdomain.com/api/v1/health
```

**Frontend**
```bash
# Dentro da VM
curl -f http://localhost:3000/

# Externo
curl https://yourdomain.com/
```

#### Verificação de Containers

```bash
# Status
docker ps -a | grep l2sledger

# Logs recentes
docker logs l2sledger-backend --tail 50 --timestamps
docker logs l2sledger-frontend --tail 50 --timestamps

# Resource usage
docker stats l2sledger-backend l2sledger-frontend --no-stream
```

#### Troubleshooting Deploy

**Container não inicia**
```bash
docker logs l2sledger-backend
docker inspect l2sledger-backend
```

**Health check falhando**
```bash
# Verificar conectividade interna
docker exec l2sledger-backend wget -O- http://localhost:8080/api/v1/health

# Verificar variáveis de ambiente
docker exec l2sledger-backend env | grep -E 'ASPNETCORE|Firebase|ConnectionStrings'
```

---

### 3.7 `6-environment-variables.md` — Referência de Variáveis

**Objetivo**: Documentação completa e consolidada de todas as variáveis de ambiente.

**Público-Alvo**: 
- Desenvolvedores
- DevOps
- Referência rápida

**Conteúdo**:

#### Estrutura

Para cada variável:
- **Nome**: Nome da variável
- **Descrição**: O que ela faz
- **Obrigatório?**: Sim/Não
- **Padrão**: Valor padrão (se houver)
- **Exemplo**: Exemplo de valor
- **Contexto**: Backend/Frontend/Ambos
- **Ambiente**: DEV/PROD/Ambos

#### Categorias

##### PostgreSQL
| Variável | Descrição | Obrigatório | Contexto | Exemplo |
|----------|-----------|-------------|----------|---------|
| `POSTGRES_HOST` | Host do PostgreSQL | Sim | Backend | `postgres` (local), `10.0.1.5` (prod) |
| `POSTGRES_PORT` | Porta do PostgreSQL | Não (padrão: 5432) | Backend | `5432` |
| `POSTGRES_USER` | Usuário do banco | Sim | Backend | `l2sledger` |
| `POSTGRES_PASSWORD` | Senha do banco | Sim | Backend | `SecureP@ssw0rd` |
| `ConnectionStrings__DefaultConnection` | String completa de conexão | Sim | Backend | `Host=postgres;Port=5432;Database=l2sledger;Username=l2sledger;Password=xxx` |

##### Firebase
| Variável | Descrição | Obrigatório | Contexto | Exemplo |
|----------|-----------|-------------|----------|---------|
| `FIREBASE_CREDENTIAL_PATH` | Caminho para Service Account JSON | Sim | Backend | `./secrets/firebase-credential.json` |
| `VITE_FIREBASE_API_KEY` | Firebase Web API Key | Sim | Frontend/Backend | `AIzaSyXXXXXXXXXXXXXXXXXXXXXX` |
| `VITE_FIREBASE_AUTH_DOMAIN` | Firebase Auth Domain | Sim | Frontend | `my-project.firebaseapp.com` |
| `VITE_FIREBASE_PROJECT_ID` | Firebase Project ID | Sim | Frontend | `my-project-id` |
| `VITE_FIREBASE_STORAGE_BUCKET` | Firebase Storage Bucket | Sim | Frontend | `my-project.appspot.com` |
| `VITE_FIREBASE_MESSAGING_SENDER_ID` | Firebase Messaging ID | Sim | Frontend | `123456789` |
| `VITE_FIREBASE_APP_ID` | Firebase App ID | Sim | Frontend | `1:123456789:web:abc123` |

##### JWT
| Variável | Descrição | Obrigatório | Contexto | Exemplo |
|----------|-----------|-------------|----------|---------|
| `JWT_SECRET` | Chave secreta para assinar tokens | Sim | Backend | `your-secret-min-32-chars-here` |
| `JWT_ISSUER` | Emissor dos tokens | Sim | Backend | `https://yourdomain.com` |
| `JWT_AUDIENCE` | Audiência dos tokens | Sim | Backend | `l2sledger` |

##### CORS
| Variável | Descrição | Obrigatório | Contexto | Exemplo |
|----------|-----------|-------------|----------|---------|
| `CORS_ALLOWED_ORIGINS` | Origens permitidas (separadas por vírgula) | Sim | Backend | `http://localhost` (dev), `https://yourdomain.com` (prod) |

##### Application
| Variável | Descrição | Obrigatório | Contexto | Exemplo |
|----------|-----------|-------------|----------|---------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente do .NET | Sim | Backend | `Development`, `Production` |
| `NODE_ENV` | Ambiente do Node | Não | Frontend | `development`, `production` |
| `VITE_API_BASE_URL` | URL base da API | Sim | Frontend | `http://localhost:8080/api` (dev), `https://yourdomain.com/api` (prod) |
| `VITE_ENABLE_DEVTOOLS` | Habilitar devtools no frontend | Não | Frontend | `true` (dev), `false` (prod) |

##### Docker/Deploy
| Variável | Descrição | Obrigatório | Contexto | Exemplo |
|----------|-----------|-------------|----------|---------|
| `GHCR_OWNER` | Owner do repositório no GHCR | Sim (prod) | Docker Compose | `your-github-org` |
| `IMAGE_TAG` | Tag da imagem a ser deployada | Sim (prod) | Docker Compose | `v1.2.3`, `sha-abc1234`, `latest` |

#### Diferenças entre Ambientes

**Desenvolvimento Local**:
```env
ASPNETCORE_ENVIRONMENT=Development
VITE_API_BASE_URL=http://localhost:8080/api
VITE_ENABLE_DEVTOOLS=true
CORS_ALLOWED_ORIGINS=http://localhost
JWT_SECRET=local-dev-secret-not-for-production
```

**Produção**:
```env
ASPNETCORE_ENVIRONMENT=Production
VITE_API_BASE_URL=https://yourdomain.com/api
VITE_ENABLE_DEVTOOLS=false
CORS_ALLOWED_ORIGINS=https://yourdomain.com
JWT_SECRET=STRONG_RANDOM_PRODUCTION_SECRET_64_CHARS
```

#### Variáveis Sensíveis (Secrets)

**NUNCA** commitar:
- `JWT_SECRET`
- `POSTGRES_PASSWORD`
- Qualquer conteúdo de `firebase-credential.json`
- Tokens de API

**Armazenamento**:
- Local: `.env` (gitignored)
- Produção: Variáveis de ambiente ou arquivo `.env` com permissões restritas
- CI/CD: GitHub Secrets

---

### 3.8 `7-caddy-configuration.md` — Configuração do Caddy

**Objetivo**: Configurar reverse proxy na VM para rotear tráfego externo aos containers.

**Público-Alvo**: 
- DevOps
- Administradores de sistema

**Conteúdo**:

#### 1. Conceito

Caddy age como:
- **Reverse Proxy**: Roteia requests externos para containers internos
- **TLS Termination**: Gerencia certificados SSL/TLS automaticamente (Let's Encrypt)
- **Security Headers**: Adiciona headers de segurança
- **Load Balancer**: Pode fazer load balancing (futuro)

#### 2. Arquitetura de Rede

```
Internet
    ↓
Caddy :80/:443
    ↓
Docker Network (caddy-network)
    ├─→ l2sledger-backend:8080
    └─→ l2sledger-frontend:3000
```

#### 3. Caddyfile Básico

**Local**: `/etc/caddy/Caddyfile`

```caddyfile
# L2SLedger Production
yourdomain.com {
    # Logs
    log {
        output file /var/log/caddy/access.log {
            roll_size 100mb
            roll_keep 5
        }
        level INFO
    }

    # Backend API
    handle /api/* {
        reverse_proxy l2sledger-backend:8080 {
            header_up X-Real-IP {remote_host}
            header_up X-Forwarded-For {remote_host}
            header_up X-Forwarded-Proto {scheme}
        }
    }

    # Frontend SPA
    handle {
        reverse_proxy l2sledger-frontend:3000
    }

    # Security Headers
    header {
        # XSS Protection
        X-Content-Type-Options "nosniff"
        X-Frame-Options "DENY"
        X-XSS-Protection "1; mode=block"
        
        # Referrer Policy
        Referrer-Policy "strict-origin-when-cross-origin"
        
        # HSTS (enable after SSL confirmed)
        # Strict-Transport-Security "max-age=31536000; includeSubDomains; preload"
        
        # CSP (ajustar conforme necessário)
        Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;"
    }
}
```

#### 4. Conectar Caddy à Rede Docker

```bash
# Verificar se Caddy está rodando
systemctl status caddy

# Conectar Caddy à rede (se não estiver)
# Primeiro, encontrar o container/serviço do Caddy
docker ps | grep caddy

# Se Caddy roda como container
docker network connect caddy-network caddy-container-name

# Se Caddy roda como systemd service, ele já acessa via Docker DNS
# desde que a rede seja 'external: true' no compose
```

#### 5. Ordem de Handles

**IMPORTANTE**: A ordem importa!

```caddyfile
# ✅ CORRETO - específico antes de genérico
handle /api/* {
    reverse_proxy backend:8080
}
handle {
    reverse_proxy frontend:3000
}

# ❌ INCORRETO - catch-all captura tudo
handle {
    reverse_proxy frontend:3000
}
handle /api/* {
    reverse_proxy backend:8080  # nunca será alcançado
}
```

#### 6. Comandos Úteis

**Validar Configuração**
```bash
sudo caddy validate --config /etc/caddy/Caddyfile
```

**Reload (sem downtime)**
```bash
sudo caddy reload --config /etc/caddy/Caddyfile
```

**Restart**
```bash
sudo systemctl restart caddy
```

**Ver Logs**
```bash
sudo journalctl -u caddy -f
tail -f /var/log/caddy/access.log
```

**Testar Roteamento**
```bash
# Backend
curl -H "Host: yourdomain.com" http://localhost/api/v1/health

# Frontend
curl -H "Host: yourdomain.com" http://localhost/
```

#### 7. TLS Automático

Caddy provisiona certificados automaticamente via Let's Encrypt:

**Requisitos**:
- Domínio válido apontando para o IP da VM
- Portas 80 e 443 abertas
- Caddy rodando com permissão de bind em portas privilegiadas

**Verificar Certificado**
```bash
# Lista certificados gerenciados pelo Caddy
sudo caddy list-certificates

# Verificar expiry
echo | openssl s_client -showcerts -servername yourdomain.com -connect yourdomain.com:443 2>/dev/null | openssl x509 -noout -dates
```

#### 8. Segurança Adicional

**Rate Limiting (futuro)**
```caddyfile
rate_limit {
    zone dynamic {
        key {remote_host}
        events 100
        window 1m
    }
}
```

**IP Whitelist (admin routes)**
```caddyfile
@admin {
    path /api/admin/*
}
handle @admin {
    @allowed remote_ip 10.0.0.0/8 192.168.0.0/16
    abort @allowed
    reverse_proxy backend:8080
}
```

#### 9. Troubleshooting Caddy

**Erro: cannot bind to port 80/443**
```bash
# Verificar se outra coisa está usando a porta
sudo lsof -i :80
sudo lsof -i :443

# Garantir que Caddy tem permissão
sudo setcap cap_net_bind_service=+ep $(which caddy)
```

**Erro: certificate obtaining failed**
```bash
# Verificar DNS
dig yourdomain.com

# Verificar firewall
sudo ufw status

# Ver logs detalhados
sudo journalctl -u caddy -f
```

---

### 3.9 `8-monitoring-health.md` — Monitoramento e Health Checks

**Objetivo**: Verificar saúde e status do sistema em produção.

**Público-Alvo**: 
- DevOps
- SRE
- Equipe de suporte

**Conteúdo**:

#### 1. Health Endpoints

##### Backend
**URL**: `GET /api/v1/health`

**Resposta Saudável** (HTTP 200):
```json
{
  "status": "Healthy",
  "timestamp": "2026-02-16T10:00:00Z"
}
```

**Verificação**:
```bash
# Interno (na VM)
curl http://localhost:8080/api/v1/health

# Externo
curl https://yourdomain.com/api/v1/health
```

##### Frontend
**URL**: `GET /`

**Resposta**: HTML da SPA (HTTP 200)

**Verificação**:
```bash
# Interno (na VM)
curl -I http://localhost:3000/

# Externo
curl -I https://yourdomain.com/
```

#### 2. Docker Health Checks

**Built-in no Dockerfile**:

**Backend**:
```dockerfile
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/api/v1/health || exit 1
```

**Frontend**:
```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:3000/ || exit 1
```

**Verificar Status**:
```bash
docker ps
# Coluna "STATUS" mostra estado do health check
# "Up 5 minutes (healthy)"
# "Up 2 minutes (unhealthy)"
```

**Detalhes do Health Check**:
```bash
docker inspect l2sledger-backend | jq '.[0].State.Health'
```

#### 3. Logs

**Ver Logs em Tempo Real**:
```bash
# Backend
docker logs -f l2sledger-backend

# Frontend
docker logs -f l2sledger-frontend

# Ambos
docker logs -f l2sledger-backend & docker logs -f l2sledger-frontend
```

**Logs com Timestamp**:
```bash
docker logs --timestamps l2sledger-backend
```

**Últimas N Linhas**:
```bash
docker logs --tail 100 l2sledger-backend
```

**Filtrar por Nível**:
```bash
docker logs l2sledger-backend 2>&1 | grep "ERROR"
docker logs l2sledger-backend 2>&1 | grep "Exception"
```

#### 4. Status de Containers

**Lista Rápida**:
```bash
docker ps -a | grep l2sledger
```

**Formato Customizado**:
```bash
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep l2sledger
```

**Com Docker Compose**:
```bash
cd /opt/l2sledger
docker compose -f docker-compose.prod.yml ps
```

#### 5. Resource Usage

**Stats em Tempo Real**:
```bash
docker stats l2sledger-backend l2sledger-frontend
```

**Snapshot**:
```bash
docker stats --no-stream l2sledger-backend l2sledger-frontend
```

**Verificar Limites Configurados**:
```bash
docker inspect l2sledger-backend | jq '.[0].HostConfig.Memory'
docker inspect l2sledger-backend | jq '.[0].HostConfig.NanoCpus'
```

#### 6. Network Connectivity

**Verificar Conectividade Interna**:
```bash
# Backend pode acessar database?
docker exec l2sledger-backend ping -c 3 postgres

# Frontend pode resolver backend?
docker exec l2sledger-frontend nslookup l2sledger-backend
```

**Verificar Redes**:
```bash
# Listar redes conectadas ao backend
docker inspect l2sledger-backend | jq '.[0].NetworkSettings.Networks'

# Listar containers na rede
docker network inspect caddy-network | jq '.[0].Containers'
```

#### 7. Database Connection

**Teste de Conexão**:
```bash
# Via container backend
docker exec l2sledger-backend dotnet ef database update --help

# Direct PostgreSQL
docker exec -it postgres-container psql -U l2sledger -d l2sledger -c "SELECT 1;"
```

#### 8. Automated Monitoring (Futuro)

**Prometheus + Grafana**:
- Expor métricas: `/api/v1/metrics`
- Dashboard de performance
- Alertas automáticos

**Loki + Promtail**:
- Agregação de logs
- Queries centralizadas

#### 9. Alert Checklist

| Condição | Ação |
|----------|------|
| Container unhealthy | Verificar logs, restart se necessário |
| Health endpoint down | Verificar application logs, database connection |
| CPU > 90% | Investigar leak ou load, considerar scaling |
| Memory > 80% | Verificar leaks, restart preventivo |
| Disk > 85% | Limpar images antigas, logs rotativos |
| 5xx errors | Analisar logs de exceção, verificar dependencies |

---

### 3.10 `9-troubleshooting.md` — Resolução de Problemas

**Objetivo**: Guia para problemas comuns e suas soluções.

**Público-Alvo**: 
- Desenvolvedores
- DevOps
- Suporte

**Conteúdo**:

#### Estrutura

Para cada problema:
- **Sintoma**: Como o problema se manifesta
- **Causas Possíveis**: Lista de causas comuns
- **Diagnóstico**: Como confirmar a causa
- **Solução**: Passos para resolver

---

#### Problema 1: Container Não Inicia

**Sintomas**:
- `docker ps` não mostra o container
- Container aparece e desaparece imediatamente
- Status `Exited (1)` ou similar

**Causas Possíveis**:
1. Erro na aplicação durante startup
2. Variável de ambiente faltando ou inválida
3. Arquivo de configuração ausente
4. Porta já em uso

**Diagnóstico**:
```bash
# Ver logs do container (mesmo se stopped)
docker logs l2sledger-backend

# Inspecionar configuração
docker inspect l2sledger-backend

# Verificar portas em uso
sudo lsof -i :8080
sudo lsof -i :3000
```

**Solução**:
```bash
# 1. Corrigir variáveis no .env
nano /opt/l2sledger/.env

# 2. Verificar arquivo firebase-credential.json existe
ls -l /opt/l2sledger/secrets/

# 3. Liberar porta se ocupada
docker stop <container-using-port>

# 4. Tentar iniciar novamente
docker compose -f docker-compose.prod.yml up -d backend

# 5. Monitorar logs durante startup
docker logs -f l2sledger-backend
```

---

#### Problema 2: Health Check Falhando

**Sintomas**:
- Docker mostra `(unhealthy)` no status
- Deploy falha na verificação de saúde
- Aplicação parece estar rodando mas health endpoint não responde

**Causas Possíveis**:
1. Aplicação não terminou de inicializar
2. Health endpoint retornando erro
3. Timeout muito curto
4. Network issue interno

**Diagnóstico**:
```bash
# Tentar acessar health endpoint manualmente
docker exec l2sledger-backend wget -O- http://localhost:8080/api/v1/health

# Ver detalhes do health check
docker inspect l2sledger-backend | jq '.[0].State.Health'

# Verificar logs da aplicação
docker logs l2sledger-backend | grep -i health
```

**Solução**:
```bash
# 1. Dar mais tempo para startup (aguardar 30s-1min)
sleep 60
docker ps

# 2. Se persistir, verificar configuração da aplicação
docker logs l2sledger-backend | tail -50

# 3. Verificar conectividade com database
docker exec l2sledger-backend ping postgres

# 4. Restart forçado
docker compose -f docker-compose.prod.yml restart backend
```

---

#### Problema 3: Erro de Autenticação Firebase

**Sintomas**:
- Login retorna erro 500 ou 401
- Logs mostram "Firebase authentication failed"
- "Could not load Firebase credential"

**Causas Possíveis**:
1. `firebase-credential.json` não existe ou path incorreto
2. Permissões do arquivo incorretas
3. JSON malformado
4. Service Account desabilitado no Firebase

**Diagnóstico**:
```bash
# Verificar se arquivo existe
ls -l /opt/l2sledger/secrets/firebase-credential.json

# Verificar permissões
stat /opt/l2sledger/secrets/firebase-credential.json

# Verificar se é um JSON válido
cat /opt/l2sledger/secrets/firebase-credential.json | jq .

# Ver variável de ambiente no container
docker exec l2sledger-backend env | grep FIREBASE_CREDENTIAL_PATH
```

**Solução**:
```bash
# 1. Re-download do Service Account do Firebase Console
# 2. Re-upload para o servidor
scp firebase-credential.json user@vm:/opt/l2sledger/secrets/

# 3. Corrigir permissões
chmod 600 /opt/l2sledger/secrets/firebase-credential.json
chown $USER:$USER /opt/l2sledger/secrets/firebase-credential.json

# 4. Verificar path no docker-compose.prod.yml
grep FIREBASE_CREDENTIAL_PATH docker-compose.prod.yml

# 5. Restart do backend
docker compose -f docker-compose.prod.yml restart backend
```

---

#### Problema 4: Frontend Não Conecta ao Backend

**Sintomas**:
- Frontend carrega mas API calls falham
- Console mostra CORS errors
- Network tab mostra 404 ou timeout

**Causas Possíveis**:
1. `VITE_API_BASE_URL` incorreto
2. CORS não configurado no backend
3. Backend não está acessível
4. Caddy não está roteando corretamente

**Diagnóstico**:
```bash
# 1. Verificar variável no frontend
docker exec l2sledger-frontend cat /app/dist/env-config.js

# 2. Testar backend diretamente
curl http://localhost:8080/api/v1/health

# 3. Testar via Caddy
curl https://yourdomain.com/api/v1/health

# 4. Ver logs do Caddy
sudo journalctl -u caddy -f

# 5. Ver network do frontend (dentro do browser)
# Abrir DevTools → Network → verificar URL e response
```

**Solução**:
```bash
# 1. Corrigir VITE_API_BASE_URL no .env
# Deve ser: https://yourdomain.com/api (sem /v1)
nano /opt/l2sledger/.env

# 2. Verificar CORS no backend
docker logs l2sledger-backend | grep CORS

# 3. Corrigir CORS_ALLOWED_ORIGINS no .env
# Deve incluir o domínio do frontend
CORS_ALLOWED_ORIGINS=https://yourdomain.com

# 4. Restart frontend e backend
docker compose -f docker-compose.prod.yml restart
```

---

#### Problema 5: Caddy Não Roteia Tráfego

**Sintomas**:
- `curl localhost:80` não responde
- HTTPS não funciona
- 502 Bad Gateway
- Containers estão healthy mas site inacessível

**Causas Possíveis**:
1. Caddy não está rodando
2. Containers não estão na rede `caddy-network`
3. Caddyfile mal configurado
4. Firewall bloqueando portas

**Diagnóstico**:
```bash
# 1. Verificar Caddy
systemctl status caddy

# 2. Verificar configuração
sudo caddy validate --config /etc/caddy/Caddyfile

# 3. Verificar portas
sudo lsof -i :80
sudo lsof -i :443

# 4. Verificar redes Docker
docker network inspect caddy-network | jq '.[0].Containers'

# 5. Verificar logs
sudo journalctl -u caddy -n 50
```

**Solução**:
```bash
# 1. Restart Caddy
sudo systemctl restart caddy

# 2. Conectar containers à rede (se não estiverem)
docker network connect caddy-network l2sledger-backend
docker network connect caddy-network l2sledger-frontend

# 3. Verificar Caddyfile (ver seção 7)
sudo nano /etc/caddy/Caddyfile

# 4. Reload configuração
sudo caddy reload --config /etc/caddy/Caddyfile

# 5. Verificar firewall
sudo ufw status
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
```

---

#### Problema 6: Database Connection Failed

**Sintomas**:
- Backend não inicia ou crashloop
- Logs mostram "Could not connect to database"
- Migrations falham

**Causas Possíveis**:
1. PostgreSQL não está rodando
2. Connection string incorreta
3. Rede `shared-db-network` não configurada
4. Credenciais inválidas

**Diagnóstico**:
```bash
# 1. Verificar PostgreSQL
docker ps | grep postgres

# 2. Testar conexão
docker exec -it postgres psql -U l2sledger -d l2sledger

# 3. Verificar variáveis no backend
docker exec l2sledger-backend env | grep ConnectionStrings

# 4. Verificar rede
docker network inspect shared-db-network
```

**Solução**:
```bash
# 1. Iniciar PostgreSQL (se não estiver rodando)
docker start postgres-container

# 2. Corrigir connection string no .env
ConnectionStrings__DefaultConnection=Host=CORRECT_HOST;Port=5432;Database=l2sledger;Username=USER;Password=PASS

# 3. Garantir que backend está na rede
docker network connect shared-db-network l2sledger-backend

# 4. Restart backend
docker compose -f docker-compose.prod.yml restart backend
```

---

#### Problema 7: Image Pull Failed

**Sintomas**:
- Deploy falha com "image not found"
- "authentication required"
- "manifest unknown"

**Causas Possíveis**:
1. Não autenticado no GHCR
2. Tag não existe
3. Repository privado sem permissão

**Diagnóstico**:
```bash
# 1. Verificar autenticação
docker login ghcr.io

# 2. Listar tags disponíveis (via GitHub UI ou API)
# 3. Tentar pull manual
docker pull ghcr.io/your-org/l2sledger-backend:v1.2.3
```

**Solução**:
```bash
# 1. Re-autenticar
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# 2. Verificar tag correto
# Ir para GitHub Actions → workflow que fez build → ver tag

# 3. Usar tag correto no deploy
export IMAGE_TAG=correct-tag
docker compose -f docker-compose.prod.yml pull
```

---

#### Comandos Úteis de Diagnóstico

```bash
# Status geral do sistema
docker ps -a
docker compose -f docker-compose.prod.yml ps
systemctl status caddy

# Logs importantes
docker logs l2sledger-backend --tail 100
docker logs l2sledger-frontend --tail 100
sudo journalctl -u caddy -n 100

# Network debugging
docker network ls
docker network inspect caddy-network
docker network inspect shared-db-network

# Resource monitoring
docker stats --no-stream
df -h
free -m

# Process check
ps aux | grep -E 'caddy|docker'
ss -tulpn | grep -E ':80|:443|:8080|:3000'
```

---

### 3.11 `10-rollback-recovery.md` — Rollback e Recuperação

**Objetivo**: Procedimentos de rollback e recuperação de desastres.

**Público-Alvo**: 
- DevOps
- Engenheiros em situações de emergência

**Conteúdo**:

#### 1. Rollback para Versão Anterior

**Cenário**: Deploy da versão nova causou problema crítico.

**Pré-requisito**: Saber a tag da versão anterior (ex: `v1.2.2` ou `sha-abc1234`)

**Procedimento**:

```bash
# 1. Conectar na VM
ssh user@vm-ip
cd /opt/l2sledger

# 2. Definir tag da versão anterior
export IMAGE_TAG=v1.2.2  # substituir pela última versão estável
export GHCR_OWNER=your-org

# 3. Pull da versão anterior
docker compose -f docker-compose.prod.yml pull

# 4. Deploy da versão anterior
docker compose -f docker-compose.prod.yml up -d

# 5. Verificar saúde
sleep 15
docker compose -f docker-compose.prod.yml ps
curl http://localhost:8080/api/v1/health
curl http://localhost:3000/

# 6. Verificar logs
docker logs l2sledger-backend --tail 50
docker logs l2sledger-frontend --tail 50
```

**Rollback Específico**:

**Apenas Backend**:
```bash
export IMAGE_TAG=v1.2.2
docker compose -f docker-compose.prod.yml pull backend
docker compose -f docker-compose.prod.yml up -d --no-deps backend
```

**Apenas Frontend**:
```bash
export IMAGE_TAG=v1.2.2
docker compose -f docker-compose.prod.yml pull frontend
docker compose -f docker-compose.prod.yml up -d --no-deps frontend
```

#### 2. Recuperação de Container Crashed

**Cenário**: Container parou inesperadamente.

**Diagnóstico**:
```bash
# Ver estado
docker ps -a | grep l2sledger

# Ver logs do crash
docker logs l2sledger-backend --tail 200

# Ver eventos Docker
docker events --since '10m' --filter 'container=l2sledger-backend'
```

**Recuperação**:
```bash
# 1. Tentar restart simples
docker compose -f /opt/l2sledger/docker-compose.prod.yml restart backend

# 2. Se não funcionar, forçar recreação
docker compose -f /opt/l2sledger/docker-compose.prod.yml up -d --force-recreate backend

# 3. Se ainda falhar, verificar recursos
docker stats --no-stream
df -h
free -m

# 4. Cleanup e restart
docker system prune -f
docker compose -f /opt/l2sledger/docker-compose.prod.yml down backend
docker compose -f /opt/l2sledger/docker-compose.prod.yml up -d backend
```

#### 3. Recuperação de Ambiente Completo

**Cenário**: Múltiplos problemas, ambiente instável, necessita reset completo.

**⚠️ ATENÇÃO**: Isso vai recriar TODOS os containers da stack L2SLedger (exceto database/redis se forem externos).

**Procedimento**:
```bash
cd /opt/l2sledger

# 1. Backup de configuração atual
cp .env .env.backup.$(date +%Y%m%d_%H%M%S)

# 2. Parar e remover containers
docker compose -f docker-compose.prod.yml down

# 3. Limpar imagens antigas (opcional)
docker image prune -f

# 4. Re-pull das imagens
export IMAGE_TAG=latest  # ou versão específica
docker compose -f docker-compose.prod.yml pull

# 5. Recriar ambiente
docker compose -f docker-compose.prod.yml up -d

# 6. Monitorar startup
docker compose -f docker-compose.prod.yml logs -f

# 7. Verificar saúde
docker compose -f docker-compose.prod.yml ps
curl http://localhost:8080/api/v1/health
curl http://localhost:3000/
```

#### 4. Recuperação de Rede Docker

**Cenário**: Problemas de conectividade entre containers ou Caddy.

**Diagnóstico**:
```bash
# Listar redes
docker network ls

# Inspecionar rede
docker network inspect caddy-network
docker network inspect shared-db-network

# Ver conectividade dos containers
docker inspect l2sledger-backend | jq '.[0].NetworkSettings.Networks'
```

**Recuperação**:
```bash
# 1. Recriar redes (se necessário)
docker network rm caddy-network shared-db-network
docker network create caddy-network
docker network create shared-db-network

# 2. Conectar Caddy (se for container)
docker network connect caddy-network caddy-container

# 3. Recriar containers da aplicação
cd /opt/l2sledger
docker compose -f docker-compose.prod.yml down
docker compose -f docker-compose.prod.yml up -d

# 4. Verificar conectividade
docker exec l2sledger-backend ping -c 3 postgres
docker exec l2sledger-frontend nslookup l2sledger-backend
```

#### 5. Recuperação de Database (Referência)

**⚠️ NOTA**: Este procedimento assume backup prévio existente. 
Ver ADRs específicos para estratégia completa de backup/restore.

**Conceito**:
```bash
# Backup (executar periodicamente)
docker exec postgres pg_dump -U l2sledger -d l2sledger > backup_$(date +%Y%m%d).sql

# Restore
cat backup_20260216.sql | docker exec -i postgres psql -U l2sledger -d l2sledger
```

#### 6. Recuperação de Secrets Perdidos

**Cenário**: Arquivo `firebase-credential.json` foi deletado ou corrompido.

**Solução**:
```bash
# 1. Re-download do Firebase Console
# Firebase Console → Project Settings → Service Accounts → Generate New Key

# 2. Upload para VM
scp firebase-credential.json user@vm:/opt/l2sledger/secrets/

# 3. Permissões corretas
chmod 600 /opt/l2sledger/secrets/firebase-credential.json

# 4. Restart backend
docker compose -f /opt/l2sledger/docker-compose.prod.yml restart backend
```

#### 7. Disaster Recovery: VM Perdida

**Cenário**: VM foi destruída ou inacessível, necessita recriar ambiente do zero.

**Pré-requisitos**:
- Backups de database (externo à VM)
- `.env` com configurações (backup externo ou GitHub Secrets)
- `firebase-credential.json` (backup seguro)
- Código no GitHub (sempre atualizado)

**Procedimento**:
1. Provisionar nova VM (seguir `4-production-setup.md`)
2. Restaurar database backup
3. Configurar `.env` e secrets
4. Deploy da última versão estável (seguir `5-production-deploy.md`)
5. Verificar tudo
6. Atualizar DNS se IP mudou

#### 8. Checklist Pós-Recuperação

```markdown
- [ ] Todos os containers healthy
- [ ] Health endpoints respondem OK
- [ ] Logs não mostram erros críticos
- [ ] Frontend acessível externamente
- [ ] Backend API acessível via frontend
- [ ] Autenticação funcionando
- [ ] Database conectado
- [ ] Caddy roteando corretamente
- [ ] HTTPS funcionando (certificado válido)
- [ ] Monitoramento restaurado (se houver)
- [ ] Equipe notificada
- [ ] Post-mortem agendado (se aplicável)
```

#### 9. Contatos de Emergência

| Responsável | Área | Contato |
|-------------|------|---------|
| DevOps Lead | Infraestrutura | ... |
| Backend Lead | API/Database | ... |
| Frontend Lead | UI/Client | ... |

#### 10. Escalation Path

1. **Primeiro Responder**: Tentar rollback (Seção 1)
2. **Se Persistir**: Recuperação de container (Seção 2)
3. **Se Crítico**: Recuperação completa (Seção 3)
4. **Se Falhar**: Contatar Lead DevOps
5. **Desastre Total**: Contatar CTO + iniciar DR completo

---

## 4. Plano de Execução

### 4.1 Ordem de Criação

| Fase | Documento | Prioridade | Dependências | Tempo Estimado |
|------|-----------|------------|--------------|----------------|
| 1 | `deployment/README.md` | 🔴 Alta | - | 1h |
| 2 | `1-prerequisites.md` | 🔴 Alta | README | 2h |
| 3 | `6-environment-variables.md` | 🔴 Alta | prerequisites | 2h |
| 4 | `2-local-development.md` | 🔴 Alta | prerequisites, env-vars | 3h |
| 5 | `3-docker-local.md` | 🔴 Alta | prerequisites, env-vars | 2h |
| 6 | `4-production-setup.md` | 🔴 Alta | prerequisites, env-vars | 3h |
| 7 | `7-caddy-configuration.md` | 🟡 Média | prod-setup | 2h |
| 8 | `5-production-deploy.md` | 🔴 Alta | prod-setup, caddy | 3h |
| 9 | `8-monitoring-health.md` | 🟡 Média | deploy | 2h |
| 10 | `9-troubleshooting.md` | 🟡 Média | todos anteriores | 4h |
| 11 | `10-rollback-recovery.md` | 🟢 Baixa | deploy | 2h |

**Total Estimado**: 26 horas

### 4.2 Fases de Execução

#### Fase 1: Fundação (Documentos 1-3)
- Criar estrutura base
- Definir referências centralizadas
- Estabelecer vocabulário comum

#### Fase 2: Desenvolvimento Local (Documentos 4-5)
- Guias para desenvolvedores
- Ambientes locais funcionais

#### Fase 3: Produção (Documentos 6-8)
- Setup de infraestrutura
- Deploy procedures
- Monitoramento básico

#### Fase 4: Manutenção (Documentos 9-11)
- Troubleshooting
- Recuperação
- Procedimentos de emergência

### 4.3 Responsáveis

| Documento | Responsável Sugerido | Revisor Sugerido |
|-----------|---------------------|------------------|
| README + prerequisites | DevOps / Tech Lead | Arquiteto |
| Local Development | Backend Dev + Frontend Dev | Tech Lead |
| Docker Local | DevOps + Backend Dev | Arquiteto |
| Production Setup | DevOps / SRE | Arquiteto |
| Caddy Configuration | DevOps / SRE | Security Lead |
| Production Deploy | DevOps / SRE | Tech Lead |
| Environment Variables | Backend Dev + DevOps | Arquiteto |
| Monitoring | DevOps / SRE | Tech Lead |
| Troubleshooting | DevOps + Devs | Todos |
| Rollback | DevOps / SRE | Arquiteto |

---

## 5. Considerações Arquiteturais

### 5.1 Conformidade com ADRs

Esta documentação:
- **Não contradiz** nenhum ADR existente
- **Referencia** ADRs relevantes onde aplicável
- **Complementa** decisões técnicas documentadas
- **Respeita** princípios arquiteturais (Clean Architecture, DDD, Fail-Fast)

### 5.2 Alinhamento com DevOps Strategy

O plano está alinhado com [devops-strategy.md](../devops-strategy.md):
- Respeita decisões de containerização (Dockerfile, multi-stage builds)
- Documenta arquitetura de rede (Caddy, expose-only)
- Reflete segurança (non-root users, read-only filesystem)
- Explica CI/CD workflows existentes

### 5.3 Integração com Documentação Existente

| Documento Existente | Relação com Nova Documentação |
|---------------------|------------------------------|
| [backend/RUNNING.md](../../backend/RUNNING.md) | Consolidado em `2-local-development.md` |
| [frontend/README.md](../../frontend/README.md) | Referenciado e expandido |
| [devops-strategy.md](../devops-strategy.md) | Usado como base técnica, nova doc foca em "how-to" |
| [README.md](../../README.md) | Permanece como overview, nova doc detalha operação |

### 5.4 Manutenibilidade

**Quando Atualizar Documentação**:
- Workflows CI/CD mudarem → atualizar `5-production-deploy.md`
- Variáveis de ambiente adicionadas/removidas → atualizar `6-environment-variables.md`
- Infraestrutura mudar (novo serviço) → atualizar `1-prerequisites.md`, `4-production-setup.md`
- Novo problema comum descoberto → adicionar em `9-troubleshooting.md`

**Responsabilidade de Atualização**:
- Mudanças de código que afetam deploy → Dev que fez a mudança
- Mudanças de infraestrutura → DevOps
- Novos problemas → Quem resolveu o problema

---

## 6. Registro de Mudanças e Auditoria

### 6.1 Registro em ai-driven/changelog.md

Após execução, registrar em `ai-driven/changelog.md`:

```markdown
## 2026-02-16 — Criação de Documentação de Deploy

**Tipo**: Documentação  
**Impacto**: Estrutura  
**ADRs Relacionados**: -

**Mudanças**:
- Criados 11 documentos em `docs/deployment/`
- Consolidação de informações dispersas
- Guias completos de setup local e produção
- Troubleshooting e recovery procedures

**Arquivos Criados**:
- docs/deployment/README.md
- docs/deployment/1-prerequisites.md
- docs/deployment/2-local-development.md
- docs/deployment/3-docker-local.md
- docs/deployment/4-production-setup.md
- docs/deployment/5-production-deploy.md
- docs/deployment/6-environment-variables.md
- docs/deployment/7-caddy-configuration.md
- docs/deployment/8-monitoring-health.md
- docs/deployment/9-troubleshooting.md
- docs/deployment/10-rollback-recovery.md

**Próximos Passos**:
- Revisar documentos com equipe
- Testar procedimentos na prática
- Ajustar com feedback real
```

### 6.2 Governança

Este plano segue o fluxo **Planejar → Aprovar → Executar**:

- ✅ **Planejar**: Este documento (completo)
- ⏳ **Aprovar**: Aguardando revisão e aprovação
- ⏳ **Executar**: A ser executado após aprovação

---

## 7. Próximos Passos

1. **Revisão deste Plano**: Equipe técnica + stakeholders
2. **Ajustes**: Baseado em feedback
3. **Aprovação Formal**: Tech Lead / Arquiteto
4. **Execução**: Criação sequencial dos documentos
5. **Revisão Técnica**: Cada documento revisado antes do próximo
6. **Validação Prática**: Testar procedimentos em ambiente real
7. **Publicação**: Merge para branch principal
8. **Socialização**: Comunicar equipe sobre nova documentação
9. **Iteração**: Ajustar baseado em uso real

---

## 8. Critérios de Sucesso

Esta documentação será considerada bem-sucedida quando:

- ✅ Desenvolvedor novo consegue setup local em < 30 min
- ✅ DevOps consegue fazer deploy sem consultar pessoa
- ✅ 80% dos problemas comuns resolvidos via troubleshooting guide
- ✅ Zero ambiguidade sobre configuração de variáveis
- ✅ Rollback pode ser executado em < 5 min
- ✅ Feedback da equipe predominantemente positivo

---

## 9. Anexos

### 9.1 Referências

- [Architecture.md](../../Architecture.md)
- [docs/adr/adr-index.md](../adr/adr-index.md)
- [docs/devops-strategy.md](../devops-strategy.md)
- [backend/RUNNING.md](../../backend/RUNNING.md)
- [ai-driven/agent-rules.md](../../ai-driven/agent-rules.md)

### 9.2 Terminologia

| Termo | Definição |
|-------|-----------|
| **VM** | Virtual Machine (OCI Compute Instance) |
| **GHCR** | GitHub Container Registry |
| **Caddy** | Reverse proxy com TLS automático |
| **Health Check** | Verificação automática de saúde do container |
| **Rollback** | Reverter para versão anterior |
| **Image Tag** | Versão específica de imagem Docker (ex: v1.2.3) |

---

**Fim do Plano de Documentação**

---

> 📝 **Status**: Aguardando aprovação  
> 📅 **Data de Criação**: 2026-02-16  
> 👤 **Autor**: GitHub Copilot (AI Agent)  
> 🔄 **Próxima Revisão**: Após aprovação e execução inicial
