# Implementações Futuras do MVP API — L2SLedger

> **Data:** 2026-01-23  
> **Status:** 📋 Planejado  
> **Versão:** 1.1

Este documento consolida os itens pendentes e planejamento de implementações futuras do MVP da API L2SLedger.

---

## 1. Resumo Executivo

Com base na análise do [api-planning.md](api-planning/api-planning.md) e [STATUS.md](../../backend/STATUS.md), o MVP da API L2SLedger está **~95% completo**. As funcionalidades core estão implementadas e testadas.

### Status por Fase do MVP

| Fase | Descrição | Status | Observações |
|------|-----------|--------|-------------|
| Fase 1 | Estrutura Base | ✅ 100% | Clean Architecture configurada |
| Fase 2 | Autenticação | ✅ 100% | Firebase + Cookies HttpOnly |
| Fase 3 | Categorias | ✅ 100% | CRUD completo |
| Fase 3.1 | Firebase Login (DEV) | ✅ 100% | Endpoint auxiliar funcional |
| Fase 4 | Transações | ✅ 100% | CRUD + regras de período |
| Fase 5 | Períodos Financeiros | ✅ 100% | Fechamento/reabertura |
| Fase 6 | Saldos e Relatórios | ✅ 100% | Consolidação implementada |
| Fase 7 | Health & Observabilidade | ✅ 100% | Endpoints /health, /metrics |
| Fase 8 | Exportação | ✅ 100% | CSV funcional, PDF (HTML) |
| Fase 9 | Auditoria | ✅ 100% | Logs imutáveis |
| Fase 10 | Usuários e Permissões | ✅ 100% | RBAC Admin-only |

---

## 2. Itens Pendentes do MVP

### 2.1 Itens MENORES (Quick Wins)

| Item | Origem | Prioridade | Esforço | Descrição |
|------|--------|------------|---------|-----------|
| Swagger grupo "dev" | Fase 3.1 | Baixa | ~2h | Documentar `/api/v1/auth/firebase/login` com grupo separado no Swagger |
| Polly para Database | ADR-007 | Média | ~4h | Adicionar políticas Polly para conexões PostgreSQL |

### 2.2 Itens ADIADOS para Pós-MVP

| Item | Fase | Motivo | Planejamento |
|------|------|--------|--------------|
| Dockerfile/docker-compose produção | Fase 7 | Foco no core | Ver seção 3.1 |
| PDF real (PdfSharpCore) | Fase 8 | PDF via HTML funcional | Ver seção 3.2 |
| Testes de Integração Health | Fase 7 | Requerem ambiente completo | Executar em CI/CD |
| Testes unitários Fase 10 | Fase 10 | ~29 testes planejados | Ver seção 3.3 |
| Controle de Planos (Comercial) | Fase 8 | Pós-MVP | Ver seção 3.4 |
| Notificações e Alertas | Fase 11 | Pós-MVP | A planejar |

---

## 3. Plano de Implementação Futura (Pós-MVP)

### 3.1 Dockerfile e Docker-Compose Produção

**Objetivo:** Containerização completa para deploy em OCI.

**Arquivos a Criar:**
```
backend/
├── Dockerfile
├── docker-compose.yml        # Produção
├── docker-compose.demo.yml   # Demonstração
└── .dockerignore
```

**Especificações Técnicas:**
- Base image: `mcr.microsoft.com/dotnet/aspnet:9.0-alpine`
- Build image: `mcr.microsoft.com/dotnet/sdk:9.0`
- Multi-stage build para imagem otimizada
- Health check via `/health/live`
- Non-root user para segurança
- Environment variables para configuração

**Tarefas:**
1. Criar `Dockerfile` multi-stage
2. Criar `docker-compose.yml` para produção (API + PostgreSQL + volumes)
3. Criar `docker-compose.demo.yml` para ambiente DEMO
4. Criar `.dockerignore` 
5. Documentar processo de build e deploy
6. Testar em ambiente local

**ADRs Relacionados:** ADR-032 (Docker), ADR-033 (OCI)

**Esforço Estimado:** 8-12 horas

---

### 3.2 Exportação PDF Real

**Objetivo:** Substituir geração HTML por PDF real.

**Status Atual:** PdfExportService gera HTML formatado (funcional para PoC).

#### ❌ QuestPDF Descartado

A biblioteca QuestPDF foi descartada por **incompatibilidade de licença comercial**:
- Licença "Community" exige pagamento para projetos comerciais
- Não é MIT/Apache 2.0 como indicado anteriormente

#### ✅ Alternativas Compatíveis com Uso Comercial

| Biblioteca | Licença | Prós | Contras |
|------------|---------|------|---------|
| **PdfSharpCore** | MIT | Leve, .NET Core nativo, sem dependências | API mais baixo nível |
| **iTextSharp (iText 5.x)** | AGPL/LGPL | Maduro, muita documentação | Versões novas são pagas |
| **Puppeteer Sharp** | MIT | Renderiza HTML→PDF via Chromium | Requer Chromium instalado |
| **wkhtmltopdf + wrapper** | LGPL | HTML→PDF de alta qualidade | Binário externo |
| **DinkToPdf** | MIT | Wrapper .NET para wkhtmltopdf | Binário nativo |

#### Recomendação: PdfSharpCore + MigraDocCore

**Justificativa:**
- **Licença MIT** - 100% compatível com comercialização
- Suporte nativo a .NET Core/.NET 9
- Não requer binários externos
- API de alto nível com MigraDocCore para documentos
- Ativa na comunidade

**Pacotes NuGet:**
```xml
<PackageReference Include="PdfSharpCore" Version="1.3.65" />
<PackageReference Include="MigraDocCore" Version="1.3.65" />
```

**Especificações:**
- Layout profissional com cabeçalho/rodapé
- Tabela paginada de transações
- Totalizadores e resumo financeiro
- Logo e branding
- Fontes embutidas (sem dependência do sistema)

**Tarefas:**
1. Adicionar pacotes `PdfSharpCore` e `MigraDocCore` ao Infrastructure
2. Criar `MigraDocPdfBuilder` com template financeiro
3. Atualizar `PdfExportService` para usar MigraDocCore
4. Implementar estilos (cores, fontes, margens)
5. Testes de geração PDF
6. Validar tamanho de arquivo e performance

**Esforço Estimado:** 10-14 horas (ligeiramente maior devido à API mais verbosa)

---

### 3.3 Testes Unitários Fase 10 (Usuários e Permissões)

**Testes Planejados:** ~29 testes

| Classe de Teste | Testes | Cobertura |
|-----------------|--------|-----------|
| GetUsersUseCaseTests | 4 | Paginação, filtros, vazios |
| GetUserByIdUseCaseTests | 3 | Encontrado, não encontrado, inativo |
| GetUserRolesUseCaseTests | 3 | Roles, disponíveis, admin |
| UpdateUserRolesUseCaseTests | 6 | Sucesso, auto-proteção, último admin, invalid |
| RoleTests | 4 | FromString, IsValid, GetAll, equality |
| UsersControllerTests | 9 | Endpoints, autorização, erros |

**Esforço Estimado:** 6-8 horas

---

### 3.4 Módulo Comercial (Planos e Assinaturas)

**Referência:** ADR-042, ADR-042-a

**Endpoints Planejados:**
| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/v1/me/commercial-context` | GET | Contexto comercial do tenant |
| `/api/v1/me/plan` | GET | Plano ativo e features |
| `/api/v1/me/usage` | GET | Limites e uso atual |
| `/api/v1/me/ads-permission` | GET | Permissão para anúncios |
| `/api/v1/me/upgrade` | POST | Solicitar upgrade |
| `/api/v1/me/downgrade` | POST | Solicitar downgrade |
| `/api/v1/me/cancel` | POST | Cancelar assinatura |

**Entidades de Domínio:**
- `Plan` (Id, Name, Features, Limits, Price)
- `Subscription` (TenantId, PlanId, StartDate, EndDate, Status)
- `Usage` (TenantId, TransactionCount, ExportCount, UserCount)

**Esforço Estimado:** 20-30 horas

---

### 3.5 Módulo de Notificações e Alertas

**Objetivo:** Sistema de notificações para eventos importantes.

**Funcionalidades Planejadas:**
- Alertas de período próximo do fechamento
- Notificação de exportação concluída
- Avisos de limite de uso (planos)
- Notificações de auditoria (admin)

**Tecnologias:**
- SignalR para real-time (opcional)
- Email via SendGrid/SMTP
- Push notifications (futuro)

**Esforço Estimado:** 15-20 horas

---

## 4. Validação de Licenças para Comercialização

> ⚠️ **Importante:** Todas as bibliotecas devem ser compatíveis com uso comercial.

### 4.1 Bibliotecas de Produção — ✅ Validadas

| Biblioteca | Versão | Licença | Status |
|------------|--------|---------|--------|
| Microsoft.* (EF Core, OpenApi, Health, Polly) | 9.0.x | MIT | ✅ Permitido |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.2 | PostgreSQL License | ✅ Permitido |
| FirebaseAdmin | 3.4.0 | Apache 2.0 | ✅ Permitido |
| Serilog.* | 9.0.0+ | Apache 2.0 | ✅ Permitido |
| FluentValidation | 12.1.1 | Apache 2.0 | ✅ Permitido |
| AutoMapper | 13.0.1 | MIT | ✅ Permitido |
| Swashbuckle.AspNetCore | 7.2.0 | MIT | ✅ Permitido |
| OpenTelemetry.* | 1.9.0+ | Apache 2.0 | ✅ Permitido |
| AspNetCore.HealthChecks.NpgSql | 8.0.2 | Apache 2.0 | ✅ Permitido |
| PdfSharpCore (planejado) | 1.3.65 | MIT | ✅ Permitido |
| MigraDocCore (planejado) | 1.3.65 | MIT | ✅ Permitido |

### 4.2 Bibliotecas de Teste — ✅ Validadas

| Biblioteca | Versão | Licença | Status |
|------------|--------|---------|--------|
| xUnit | 2.9.3 | Apache 2.0 | ✅ Permitido |
| FluentAssertions | 7.0.0 | Apache 2.0 | ✅ Permitido |
| Moq | 4.20.72 | BSD-3-Clause | ✅ Permitido |
| coverlet.collector | 6.0.4 | MIT | ✅ Permitido |

### 4.3 Bibliotecas Descartadas — ❌ Não Usar

| Biblioteca | Licença | Motivo |
|------------|---------|--------|
| QuestPDF | Community (paga comercial) | Exige licença paga para comercialização |
| iText 7+ | AGPL | Requer código aberto ou licença comercial paga |
| PDFsharp (original) | - | Usar PdfSharpCore (fork MIT) |

### 4.4 Licenças Permitidas

Para novas bibliotecas, apenas estas licenças são aceitas:
- ✅ **MIT** - Sem restrições
- ✅ **Apache 2.0** - Sem restrições (manter NOTICE se existir)
- ✅ **BSD-2-Clause / BSD-3-Clause** - Sem restrições
- ✅ **PostgreSQL License** - Sem restrições
- ✅ **MS-PL** - Sem restrições
- ⚠️ **LGPL** - Permitido com cuidado (não modificar biblioteca)

**NÃO permitidas:**
- ❌ GPL / AGPL - Exige código aberto
- ❌ Licenças "Community" com cláusula comercial
- ❌ Licenças proprietárias

---

## 5. Priorização Recomendada (Pós-MVP)

| Ordem | Item | Justificativa | Esforço |
|-------|------|---------------|---------|
| 1 | Dockerfile | Necessário para deploy em produção | 8-12h |
| 2 | Testes Fase 10 | Aumentar cobertura de código | 6-8h |
| 3 | PDF Real (PdfSharpCore) | Melhorar experiência de exportação | 10-14h |
| 4 | Swagger dev group | Baixo esforço, documenta endpoint DEV | 2h |
| 5 | Polly Database | Resiliência adicional | 4h |
| 6 | Módulo Comercial | Funcionalidade pós-MVP | 20-30h |
| 7 | Notificações | Funcionalidade pós-MVP | 15-20h |

**Total Estimado Pós-MVP:** 67-94 horas

---

## 6. Roadmap Visual

```
MVP Completo (Atual)
│
├─► Sprint 1 (Pós-MVP)
│   ├── Dockerfile/docker-compose
│   └── Testes Fase 10
│
├─► Sprint 2
│   ├── PdfSharpCore (PDF real)
│   ├── Swagger grupo "dev"
│   └── Polly Database
│
├─► Sprint 3-4
│   └── Módulo Comercial (Planos)
│
└─► Sprint 5+
    └── Notificações e Alertas
```

---

## 7. Dependências e Riscos

### Dependências Técnicas

| Item | Dependência | Impacto |
|------|-------------|---------|
| Dockerfile | Ambiente OCI configurado | Bloqueante para produção |
| PdfSharpCore | Pacote NuGet MIT, .NET 9 compatível | Validado ✅ |
| Módulo Comercial | Definição de planos pelo negócio | Requer input do PO |

### Riscos Identificados

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Limitações PdfSharpCore | Baixa | Baixo | API estável, MIT license |
| Complexidade Comercial | Média | Alto | Implementação incremental |
| Performance Notificações | Baixa | Médio | Load testing antes de produção |

---

## 8. Critérios de Aceitação

### Para considerar item como CONCLUÍDO:

1. ✅ Código implementado e revisado
2. ✅ Testes unitários passando
3. ✅ Documentação atualizada
4. ✅ Build sem erros/warnings
5. ✅ Changelog atualizado
6. ✅ PR aprovado e mergeado

---

## 9. Histórico de Revisões

| Data | Versão | Descrição |
|------|--------|-----------|
| 2026-01-23 | 1.0 | Versão inicial do planejamento de implementações futuras |
| 2026-01-23 | 1.1 | Validação de licenças; QuestPDF substituído por PdfSharpCore |

---

> **Nota:** Este planejamento segue os ADRs do L2SLedger e o fluxo oficial `Planejar → Aprovar → Executar`.
