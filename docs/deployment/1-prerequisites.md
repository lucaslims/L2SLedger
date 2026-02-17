# 1. Pré-requisitos — L2SLedger

> Garanta que todas as dependências estejam instaladas antes de prosseguir com qualquer configuração.

---

## 🖥️ Ferramentas Locais (Desenvolvimento)

### Obrigatórias

| Ferramenta | Versão Mínima | Verificação | Instalação |
|------------|---------------|-------------|------------|
| **Node.js** | 20+ | `node --version` | [nodejs.org](https://nodejs.org/) |
| **.NET SDK** | 9.0 | `dotnet --version` | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| **Docker Desktop** | 24+ (Engine) | `docker --version` | [docker.com](https://www.docker.com/products/docker-desktop/) |
| **Docker Compose** | 2.x (plugin) | `docker compose version` | Incluído no Docker Desktop |
| **Git** | 2.x | `git --version` | [git-scm.com](https://git-scm.com/) |

### Opcionais (Desenvolvimento Local sem Docker)

| Ferramenta | Versão Mínima | Verificação | Nota |
|------------|---------------|-------------|------|
| **PostgreSQL** | 17 | `psql --version` | Pode usar container via `backend/docker-compose.dev.yml` |
| **Redis** | 7+ | `redis-cli --version` | Obrigatório apenas em produção (futuro) |
| **EF Core CLI** | 9.x | `dotnet ef --version` | Instalar: `dotnet tool install --global dotnet-ef` |

### Verificação Rápida

```bash
# Executar todos os checks de uma vez
node --version        # v20.x.x
dotnet --version      # 9.x.x
docker --version      # Docker version 24+
docker compose version # Docker Compose version v2.x
git --version         # git version 2.x
```

---

## 🔥 Serviços Externos — Firebase Authentication

O L2SLedger usa Firebase como **único IdP (Identity Provider)**.

### Criar Projeto Firebase

1. Acesse o [Firebase Console](https://console.firebase.google.com/)
2. Clique em **Add project** (ou use um projeto existente)
3. Siga o assistente de criação

### Configurar Authentication

1. No projeto Firebase, vá para **Authentication** → **Sign-in method**
2. Ative o provedor **Email/Password**
3. Salve

### Gerar Service Account Key

1. Vá para **Project Settings** → **Service Accounts**
2. Clique em **Generate New Private Key**
3. **Download** o arquivo JSON
4. **Guarde com segurança** — ele contém credenciais sensíveis
5. **NUNCA** commite este arquivo no Git

### Obter Web API Key

1. Vá para **Project Settings** → **General**
2. Copie a **Web API Key**
3. Esta será usada como `VITE_FIREBASE_API_KEY`

### Obter Configuração do Firebase (Frontend)

1. Em **Project Settings** → **General** → **Your apps**
2. Clique em **Add app** → **Web** (se ainda não tiver)
3. Copie as credenciais:
   - `apiKey` → `VITE_FIREBASE_API_KEY`
   - `authDomain` → `VITE_FIREBASE_AUTH_DOMAIN`
   - `projectId` → `VITE_FIREBASE_PROJECT_ID`
   - `storageBucket` → `VITE_FIREBASE_STORAGE_BUCKET`
   - `messagingSenderId` → `VITE_FIREBASE_MESSAGING_SENDER_ID`
   - `appId` → `VITE_FIREBASE_APP_ID`

---

## 📦 GitHub Container Registry (GHCR)

Necessário apenas para **deploy em produção** (pull de imagens pré-buildadas).

### Autenticação

1. Crie um **Personal Access Token (Classic)** no GitHub:
   - Settings → Developer settings → Personal access tokens
   - Escopo requerido: `read:packages`
2. Autentique-se:

```bash
echo YOUR_TOKEN | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin
```

---

## 🏗️ Infraestrutura para Produção (OCI VM)

> Esta seção é relevante apenas para quem vai configurar o servidor de produção.

### Requisitos da VM

| Recurso | Mínimo | Recomendado |
|---------|--------|-------------|
| **OS** | Ubuntu 22.04 LTS | Ubuntu 24.04 LTS |
| **vCPUs** | 2 | 4 |
| **RAM** | 4 GB | 8 GB |
| **Storage** | 50 GB | 100 GB |
| **IP Público** | Sim | Sim |
| **SSH** | Configurado | Com chave pública |

### Portas Necessárias

| Porta | Protocolo | Uso |
|-------|-----------|-----|
| 22 | TCP | SSH |
| 80 | TCP | HTTP (Caddy → redirect para HTTPS) |
| 443 | TCP | HTTPS (Caddy → TLS termination) |

> **Nota**: Portas 8080 e 3000 **NÃO** são expostas externamente. Caddy roteia via Docker DNS interno.

### Software na VM

| Software | Versão | Propósito |
|----------|--------|-----------|
| **Docker Engine** | 24+ | Containers de aplicação |
| **Docker Compose Plugin** | 2.x | Orquestração de serviços |
| **Caddy** | 2.x | Reverse proxy + TLS automático |
| **PostgreSQL** | 17 | Banco de dados (container ou gerenciado) |

### Redes Docker (Obrigatórias)

```bash
# Criar redes compartilhadas (executar uma vez)
docker network create caddy-network
docker network create shared-db-network
```

| Rede | Propósito |
|------|-----------|
| `caddy-network` | Comunicação Caddy ↔ Backend/Frontend |
| `shared-db-network` | Comunicação Backend ↔ PostgreSQL |

---

## 🔑 Credenciais Necessárias

### Desenvolvimento Local

| Credencial | Onde Obter | Uso |
|-----------|------------|-----|
| Firebase Service Account JSON | Firebase Console | Backend — autenticação |
| Firebase Web API Key | Firebase Console | Frontend + Backend |
| Firebase App Config | Firebase Console | Frontend — SDK |

### Produção

| Credencial | Onde Obter | Uso |
|-----------|------------|-----|
| Todas as de DEV | Firebase Console | Mesmo propósito |
| JWT Secret | Gerar (mín. 64 chars) | Assinatura de tokens |
| PostgreSQL Password | Definir (forte) | Acesso ao banco |
| GitHub Token (`read:packages`) | GitHub Settings | Pull de imagens GHCR |
| SSH Key | Gerar | Acesso à VM |

### Gerar JWT Secret Seguro

```bash
# Linux/macOS
openssl rand -base64 64

# PowerShell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
```

---

## ✅ Checklist de Verificação

### Para Desenvolvimento Local

```markdown
- [ ] Node.js 20+ instalado
- [ ] .NET 9 SDK instalado
- [ ] Docker Desktop rodando
- [ ] Git instalado
- [ ] Firebase projeto criado
- [ ] Firebase Email/Password provider ativado
- [ ] Firebase Service Account Key baixado
- [ ] Firebase Web API Key copiada
- [ ] Firebase App Config copiada (para frontend)
```

### Para Produção (Adicional)

```markdown
- [ ] VM provisionada com Ubuntu 22.04+
- [ ] Docker Engine instalado na VM
- [ ] Caddy instalado na VM
- [ ] PostgreSQL acessível
- [ ] Redes Docker criadas (caddy-network, shared-db-network)
- [ ] GitHub Token com read:packages criado
- [ ] JWT Secret gerado (mín. 64 chars)
- [ ] Senha forte do PostgreSQL definida
- [ ] SSH configurado com chave pública
- [ ] Firewall configurado (portas 22, 80, 443)
```

---

## ➡️ Próximos Passos

- **Desenvolvedor**: Vá para [2-local-development.md](2-local-development.md)
- **Docker local**: Vá para [3-docker-local.md](3-docker-local.md)
- **Produção**: Vá para [4-production-setup.md](4-production-setup.md)
