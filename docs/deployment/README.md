# Deploy Documentation — L2SLedger

> Documentação completa de configuração, deploy e operação do L2SLedger.

---

## 🌍 Ambientes

| Ambiente | Propósito | URL Base | Isolamento |
|----------|-----------|----------|------------|
| **DEV** | Desenvolvimento local | `http://localhost:3000` / `http://localhost:8080` | Total |
| **DEMO** | Demonstração e testes | Definido por projeto | Total |
| **PROD** | Produção (OCI VM) | `https://yourdomain.com` | Total |

> Bancos de dados, configurações e credenciais são **completamente isolados** entre ambientes.

---

## 📚 Guias Disponíveis

| # | Documento | Descrição |
|---|-----------|-----------|
| 1 | [Pré-requisitos](1-prerequisites.md) | Ferramentas, serviços e infraestrutura necessários |
| 2 | [Desenvolvimento Local](2-local-development.md) | Rodar backend e frontend sem Docker |
| 3 | [Docker Local](3-docker-local.md) | Stack completa via Docker Compose |
| 4 | [Setup de Produção](4-production-setup.md) | Configuração inicial do servidor (one-time) |
| 5 | [Deploy em Produção](5-production-deploy.md) | Deploy via GitHub Actions ou SSH manual |
| 6 | [Variáveis de Ambiente](6-environment-variables.md) | Referência completa de todas as variáveis |
| 7 | [Configuração do Caddy](7-caddy-configuration.md) | Reverse proxy, TLS e roteamento |
| 8 | [Monitoramento e Health](8-monitoring-health.md) | Health checks, logs e métricas |
| 9 | [Troubleshooting](9-troubleshooting.md) | Resolução de problemas comuns |
| 10 | [Rollback e Recuperação](10-rollback-recovery.md) | Rollback e disaster recovery |

---

## 🧭 Decisões Rápidas

### O que você quer fazer?

| Situação | Vá para |
|----------|---------|
| 🆕 **Primeira vez no projeto** | [1-prerequisites.md](1-prerequisites.md) → [2-local-development.md](2-local-development.md) |
| 💻 **Desenvolver sem Docker** | [2-local-development.md](2-local-development.md) |
| 🐳 **Rodar com Docker localmente** | [3-docker-local.md](3-docker-local.md) |
| 🏗️ **Preparar servidor novo** | [4-production-setup.md](4-production-setup.md) |
| 🚀 **Fazer deploy em produção** | [5-production-deploy.md](5-production-deploy.md) |
| 🔧 **Verificar variáveis de ambiente** | [6-environment-variables.md](6-environment-variables.md) |
| 🌐 **Configurar reverse proxy** | [7-caddy-configuration.md](7-caddy-configuration.md) |
| 📊 **Monitorar saúde do sistema** | [8-monitoring-health.md](8-monitoring-health.md) |
| 🐛 **Resolver um problema** | [9-troubleshooting.md](9-troubleshooting.md) |
| ⏪ **Reverter um deploy** | [10-rollback-recovery.md](10-rollback-recovery.md) |

---

## 🔄 Fluxo de Trabalho Típico

### Novo Desenvolvedor

```
1-prerequisites.md → 2-local-development.md → Desenvolver!
```

### Deploy para Produção

```
5-production-deploy.md → 8-monitoring-health.md → Verificar!
```

### Primeiro Deploy (Servidor Novo)

```
1-prerequisites.md → 4-production-setup.md → 7-caddy-configuration.md → 5-production-deploy.md
```

### Problema em Produção

```
9-troubleshooting.md → (se necessário) → 10-rollback-recovery.md
```

---

## 👥 Mapa de Responsabilidades

| Área | Responsável | Documentos Relevantes |
|------|-------------|----------------------|
| Setup Local | Desenvolvedor | 1, 2, 3 |
| Infraestrutura | DevOps / SRE | 1, 4, 7 |
| Deploy | DevOps | 5, 8 |
| Configuração | DevOps + Devs | 6 |
| Troubleshooting | Todos | 9, 10 |

---

## 📐 Convenções

### Terminologia

| Termo | Definição |
|-------|-----------|
| **VM** | Virtual Machine (OCI Compute Instance) |
| **GHCR** | GitHub Container Registry |
| **Caddy** | Reverse proxy com TLS automático (roda na VM) |
| **Health Check** | Verificação automática de saúde do container |
| **Rollback** | Reverter para versão anterior de imagem Docker |
| **Image Tag** | Versão de imagem Docker (ex: `v1.2.3`, `sha-abc1234`) |
| **`serve`** | Servidor estático leve usado no frontend (substitui nginx) |

### Padrões de Nomenclatura

| Recurso | Padrão | Exemplo |
|---------|--------|---------|
| Container backend | `l2sledger-backend` | — |
| Container frontend | `l2sledger-frontend` | — |
| Rede Caddy | `caddy-network` | — |
| Rede Database | `shared-db-network` | — |
| Diretório deploy (VM) | `/opt/l2sledger` | — |
| Diretório secrets (VM) | `/opt/l2sledger/secrets` | — |
| Image tag | `v{major}.{minor}.{patch}` | `v1.2.3` |
| Image tag (commit) | `sha-{short}` | `sha-abc1234` |

---

## 📖 Referências

- [Architecture.md](../../Architecture.md) — Visão arquitetural
- [DevOps Strategy](../devops-strategy.md) — Decisões DevOps e segurança
- [ADR Index](../adr/adr-index.md) — Decisões arquiteturais
- [Backend RUNNING.md](../../backend/RUNNING.md) — Guia original do backend
- [Frontend README.md](../../frontend/README.md) — Quick start do frontend
