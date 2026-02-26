# Deploy — L2SLedger

Documentação de configuração, deploy e operação do sistema.

---

## Ambientes (ADR-028, ADR-030)

| Ambiente | Propósito | Isolamento |
|----------|-----------|------------|
| **DEV** | Desenvolvimento local | Total |
| **DEMO** | Demonstração e testes | Total |
| **PROD** | Produção (OCI ARM64) | Total |

Bancos, configurações e credentials são **completamente isolados** entre ambientes.

---

## Guias

| # | Documento | Descrição |
|---|-----------|-----------|
| 1 | [Pré-requisitos](1-prerequisites.md) | Ferramentas e serviços necessários |
| 2 | [Desenvolvimento Local](2-local-development.md) | Backend e frontend sem Docker |
| 3 | [Docker Local](3-docker-local.md) | Stack completa via Docker Compose |
| 4 | [Setup de Produção](4-production-setup.md) | Configuração inicial do servidor |
| 5 | [Deploy em Produção](5-production-deploy.md) | Deploy via GitHub Actions ou SSH |
| 6 | [Variáveis de Ambiente](6-environment-variables.md) | Referência completa |
| 7 | [Configuração do Caddy](7-caddy-configuration.md) | Reverse proxy e TLS |
| 8 | [Monitoramento e Health](8-monitoring-health.md) | Health checks, logs e métricas |
| 9 | [Troubleshooting](9-troubleshooting.md) | Problemas comuns |
| 10 | [Rollback e Recuperação](10-rollback-recovery.md) | Rollback e disaster recovery |

---

## Fluxos Rápidos

| Situação | Caminho |
|----------|---------|
| Primeira vez no projeto | 1 → 2 |
| Rodar com Docker local | 3 |
| Preparar servidor novo | 1 → 4 → 7 → 5 |
| Deploy em produção | 5 → 8 |
| Problema em produção | 9 → 10 |

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

## Convenções

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

| Recurso | Padrão |
|---------|--------|
| Container backend | `l2sledger-backend` |
| Container frontend | `l2sledger-frontend` |
| Rede Caddy | `caddy-network` |
| Rede Database | `shared-db-network` |
| Diretório deploy | `/opt/l2sledger` |
| Image tag | `v{major}.{minor}.{patch}` ou `sha-{short}` |

---

## Referências

- [Architecture.md](../../Architecture.md) — Visão arquitetural
- [DevOps Strategy](../devops-strategy.md) — Decisões DevOps
- [ADR Index](../adr/adr-index.md) — Decisões arquiteturais
- [Backend README](../../backend/README.md) — Documentação do backend
- [Frontend README](../../frontend/README.md) — Documentação do frontend
