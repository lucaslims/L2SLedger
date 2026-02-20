# Plano de Correção de CI/CD — L2SLedger

> **Data:** 2026-02-20 
> **Versão:** 2.0  
> **Status:** Aprovado  
> **Prioridade:** P0 — Crítica (pipelines bloqueados + inconsistências de deploy)

---

## 📋 Índice

1. [Visão Geral](#1-visão-geral)
2. [Erro 1 — Frontend CI: Cache de Dependências](#2-erro-1--frontend-ci-cache-de-dependências)
3. [Erro 2 — Storybook Deploy: Cache de Dependências](#3-erro-2--storybook-deploy-cache-de-dependências)
4. [Erro 3 — Backend CI: Docker Build (NuGet Version)](#4-erro-3--backend-ci-docker-build-nuget-version)
5. [Erro 4 — Backend CI: Trivy Scan (SARIF não encontrado)](#5-erro-4--backend-ci-trivy-scan-sarif-não-encontrado)
6. [Erro 5 — Backend CI: CodeQL Analysis (Permissões)](#6-erro-5--backend-ci-codeql-analysis-permissões)
7. [Erro 6 — SHA Mismatch: Trivy vs Docker Tags](#7-erro-6--sha-mismatch-trivy-vs-docker-tags)
8. [Erro 7 — Deploy DEMO sem Dependência de CI](#8-erro-7--deploy-demo-sem-dependência-de-ci)
9. [Erro 8 — Storybook Deploy sem Dependência de CI](#9-erro-8--storybook-deploy-sem-dependência-de-ci)
10. [Erro 9 — Frontend CI: Trivy/SARIF Cascade](#10-erro-9--frontend-ci-trivysarif-cascade)
11. [Erro 10 — Frontend CI: CodeQL Analysis (Permissões)](#11-erro-10--frontend-ci-codeql-analysis-permissões)
12. [Resumo de Ações](#12-resumo-de-ações)
13. [Ordem de Execução](#13-ordem-de-execução)
14. [Critérios de Aceite](#14-critérios-de-aceite)

---

## 1. Visão Geral

Todos os pipelines de CI/CD do L2SLedger estão falhando. Foram identificados **10 erros distintos** em **5 workflows** que precisam ser corrigidos para desbloquear o ciclo de entrega.

### Resumo dos Erros

| # | Workflow | Job | Erro | Severidade |
|---|----------|-----|------|-----------|
| 1 | `frontend-ci.yml` | Setup Node | `Some specified paths were not resolved, unable to cache dependencies` | Bloqueante |
| 2 | `storybook-deploy.yml` | Setup Node | `Some specified paths were not resolved, unable to cache dependencies` | Bloqueante |
| 3 | `backend-ci.yml` | Docker Build | `NETSDK1018: Invalid NuGet version string: 'main'` | Bloqueante |
| 4 | `backend-ci.yml` | Trivy Scan | `Path does not exist: trivy-backend.sarif` | Bloqueante (cascata) |
| 5 | `backend-ci.yml` | CodeQL Analysis | `Resource not accessible by integration` | Bloqueante |
| 6 | `backend-ci.yml` / `frontend-ci.yml` | Trivy Scan | SHA longo no `image-ref` vs SHA curto nas tags Docker | Bloqueante |
| 7 | `deploy-demo.yml` | Deploy | Sem dependência dos CIs — race condition com build | Crítico |
| 8 | `storybook-deploy.yml` | Deploy | Sem dependência do `frontend-ci.yml` — pode publicar código quebrado | Moderado |
| 9 | `frontend-ci.yml` | Trivy/SARIF | Mesmos problemas de cascata do Erro 4 (sem `id`, sem condições) | Bloqueante |
| 10 | `frontend-ci.yml` | CodeQL | Mesmos problemas de permissão do Erro 5 (`actions: read` ausente) | Bloqueante |

---

## 2. Erro 1 — Frontend CI: Cache de Dependências

**Workflow:** `.github/workflows/frontend-ci.yml`  
**Job:** `build-and-test` → step `Setup Node.js`

### Log de Erro

```
Error: Some specified paths were not resolved, unable to cache dependencies.
```

### Causa Raiz

O workflow configura `actions/setup-node@v4` com:

```yaml
cache: 'npm'
cache-dependency-path: frontend/package-lock.json
```

Porém, o arquivo `package-lock.json` está listado no `.gitignore` global do repositório (linha 98):

```gitignore
package-lock.json
```

Isso significa que o `package-lock.json` **nunca é comitado no repositório**. Quando o CI faz checkout, o arquivo não existe, e o `actions/setup-node` falha ao tentar resolver o caminho para configurar o cache.

### Impacto

- **Pipeline completamente bloqueado** — falha antes de `npm ci`
- Sem `package-lock.json`, o `npm ci` também falharia (exige lockfile)

### Correção

| # | Task | Descrição |
|---|------|-----------|
| 2.1 | Remover `package-lock.json` do `.gitignore` | Remover a linha `package-lock.json` do arquivo `.gitignore` na raiz do repositório |
| 2.2 | Comitar `package-lock.json` | Executar `git add frontend/package-lock.json --force` e comitar |
| 2.3 | Considerar manter `yarn.lock` no `.gitignore` | Se não usa yarn, manter. Se usa, remover também |
| 2.4 | Validar pipeline | Fazer push e verificar que o job `build-and-test` passa |

### Referência (Context7 — actions/setup-node)

Conforme documentação oficial do `actions/setup-node`, o `cache-dependency-path` deve apontar para um lockfile **presente no repositório**:

```yaml
# Configuração correta para monorepo
- uses: actions/setup-node@v4
  with:
    node-version: 20
    cache: 'npm'
    cache-dependency-path: frontend/package-lock.json
```

> O `package-lock.json` é **essencial** para reproducibilidade de builds. Deve estar versionado.

---

## 3. Erro 2 — Storybook Deploy: Cache de Dependências

**Workflow:** `.github/workflows/storybook-deploy.yml`  
**Job:** `deploy` → step `Setup Node.js`

### Log de Erro

```
Error: Some specified paths were not resolved, unable to cache dependencies.
```

### Causa Raiz

**Idêntica ao Erro 1.** O workflow usa a mesma configuração:

```yaml
- uses: actions/setup-node@v4
  with:
    node-version: '20'
    cache: 'npm'
    cache-dependency-path: frontend/package-lock.json
```

E o `package-lock.json` não está no repositório por causa do `.gitignore`.

### Correção

A correção do Erro 1 (comitar `package-lock.json`) resolve automaticamente este erro também. Nenhuma ação adicional necessária no workflow.

| # | Task | Descrição |
|---|------|-----------|
| 3.1 | Corrigida pelo Erro 1 | Comitar `package-lock.json` resolve ambos os workflows |
| 3.2 | Validar pipeline | Após push, verificar que o Storybook deploy executa até o fim |

---

## 4. Erro 3 — Backend CI: Docker Build (NuGet Version)

**Workflow:** `.github/workflows/backend-ci.yml`  
**Job:** `docker` → step `Build and push Docker image`

### Log de Erro

```
error NETSDK1018: Invalid NuGet version string: 'main'.
[/src/src/L2SLedger.Domain/L2SLedger.Domain.csproj]
```

### Causa Raiz

O workflow passa `github.ref_name` como `BUILD_VERSION`:

```yaml
build-args: |
  BUILD_VERSION=${{ github.ref_name }}
  BUILD_SHA=${{ github.sha }}
```

Quando executado na branch `main`, `github.ref_name` = `"main"`. Este valor é usado no Dockerfile como:

```dockerfile
RUN dotnet publish -c Release -o /app/publish --no-restore \
    /p:Version="${BUILD_VERSION}" \
    /p:InformationalVersion="${BUILD_VERSION}+${BUILD_SHA}"
```

O `/p:Version` espera uma **versão SemVer válida** (ex: `1.0.0`), mas recebe `"main"`, que é inválido para NuGet.

### Análise

O Dockerfile já define um default seguro (`ARG BUILD_VERSION=0.0.0-local`), mas o CI sobrescreve com o valor inválido.

### Opções de Correção

| Opção | Descrição | Recomendação |
|-------|-----------|--------------|
| **A — Versão fixa + SHA** | Usar versão fixa (ex: `0.0.0-dev`) para builds de branch, SemVer real apenas para tags | **Recomendada** |
| **B — Separar Version de InformationalVersion** | Usar `Version` fixo e apenas `InformationalVersion` com ref_name | Boa alternativa |
| **C — Extrair versão do .csproj** | Ler versão do `.csproj` e usar no build | Mais complexa |

### Correção Recomendada (Opção A)

| # | Task | Descrição |
|---|------|-----------|
| 4.1 | Ajustar `BUILD_VERSION` no workflow | Condicionar o valor: se é tag `v*`, extrair versão SemVer; senão, usar `0.0.0-dev` |
| 4.2 | Manter `InformationalVersion` com ref | `InformationalVersion` aceita strings livres, manter `ref_name+SHA` |
| 4.3 | Testar build local | `docker build --build-arg BUILD_VERSION=0.0.0-dev ...` para validar |
| 4.4 | Validar pipeline | Push e verificar que Docker build passa |

**Código sugerido para o workflow:**

```yaml
- name: Resolve build version
  id: version
  run: |
    REF="${{ github.ref }}"
    if [[ "$REF" == refs/tags/v* ]]; then
      # Extract SemVer from tag (e.g., v1.2.3 → 1.2.3)
      VERSION="${REF#refs/tags/v}"
    else
      VERSION="0.0.0-dev"
    fi
    echo "version=${VERSION}" >> "$GITHUB_OUTPUT"

- name: Build and push Docker image
  uses: docker/build-push-action@v6
  with:
    # ...
    build-args: |
      BUILD_VERSION=${{ steps.version.outputs.version }}
      BUILD_SHA=${{ github.sha }}
```

---

## 5. Erro 4 — Backend CI: Trivy Scan (SARIF Não Encontrado)

**Workflow:** `.github/workflows/backend-ci.yml`  
**Job:** `docker` → step `Upload Trivy scan results`

### Log de Erro

```
Error: Path does not exist: trivy-backend.sarif
```

### Causa Raiz

Este é um **erro em cascata** do Erro 3. A sequência de eventos é:

1. Step `Build and push Docker image` **falha** (NuGet version inválida)
2. A imagem Docker **não é construída nem publicada**
3. Step `Scan Docker image with Trivy` tenta escanear uma imagem que **não existe**
4. Trivy falha silenciosamente (ou nem executa), não gerando o arquivo `trivy-backend.sarif`
5. Step `Upload Trivy scan results` (com `if: always()`) tenta fazer upload do SARIF inexistente → **falha**

### Correção

| # | Task | Descrição |
|---|------|-----------|
| 5.1 | Corrigir Erro 3 primeiro | Com o Docker build funcional, Trivy conseguirá escanear a imagem |
| 5.2 | Adicionar condição ao Trivy | Adicionar `if: success()` (ou `if: steps.docker-build.outcome == 'success'`) ao step de Trivy para não executar se build falhou |
| 5.3 | Condicionar upload SARIF | Alterar `if: always()` para `if: always() && steps.trivy.outcome != 'skipped'` para não falhar quando Trivy não executou |
| 5.4 | Atualizar CodeQL action | O log indica `Warning: CodeQL Action v3 will be deprecated in December 2026` — o workflow já usa `@v4` no código mas o log mostra `@v3`. Validar que está realmente usando `@v4` |

**Código sugerido:**

```yaml
- name: Build and push Docker image
  id: docker-build
  uses: docker/build-push-action@v6
  # ...

- name: Scan Docker image with Trivy
  id: trivy
  if: steps.docker-build.outcome == 'success'
  uses: aquasecurity/trivy-action@0.28.0
  # ...

- name: Upload Trivy scan results
  uses: github/codeql-action/upload-sarif@v4
  if: always() && steps.trivy.outcome == 'success'
  # ...
```

---

## 6. Erro 5 — Backend CI: CodeQL Analysis (Permissões)

**Workflow:** `.github/workflows/backend-ci.yml`  
**Job:** `codeql` → step `Perform CodeQL Analysis`

### Log de Erro

```
Error: Resource not accessible by integration - 
https://docs.github.com/rest/actions/workflow-runs#get-a-workflow-run
```

### Causa Raiz

O token `GITHUB_TOKEN` não possui permissões suficientes para o CodeQL fazer upload dos resultados SARIF e acessar a API de workflow runs. O job `codeql` declara:

```yaml
permissions:
  security-events: write
  contents: read
```

Porém, o token automático do GitHub Actions pode estar limitado por configurações do repositório. Possíveis causas:

1. **Repositório com actions permissions restritivas** — `Settings → Actions → General → Workflow permissions` pode estar configurado como "Read repository contents and packages permissions" ao invés de "Read and write permissions"
2. **Fork** — Se o workflow roda em um fork, o `GITHUB_TOKEN` tem permissões reduzidas
3. **Falta de `actions: read`** — O erro menciona especificamente a API de workflow runs, que requer `actions: read`

### Correção

| # | Task | Descrição |
|---|------|-----------|
| 6.1 | Verificar configuração do repositório | Em `Settings → Actions → General → Workflow permissions`, garantir que está como "Read and write permissions" |
| 6.2 | Adicionar `actions: read` ao job | Adicionar permissão `actions: read` ao job `codeql` para permitir acesso à API de workflow runs |
| 6.3 | Validar em PR e push | CodeQL roda tanto em `push` quanto em `pull_request`, validar ambos |

**Código sugerido:**

```yaml
codeql:
  name: CodeQL Analysis
  needs: build-and-test
  runs-on: ubuntu-latest
  if: github.event_name == 'push' || github.event_name == 'pull_request'
  permissions:
    security-events: write
    contents: read
    actions: read       # ← Necessário para telemetria do CodeQL
```

---

## 7. Erro 6 — SHA Mismatch: Trivy vs Docker Tags

**Workflows:** `.github/workflows/backend-ci.yml` e `.github/workflows/frontend-ci.yml`  
**Job:** `docker` → step `Scan Docker image with Trivy`

### Evidência

Em ambos os workflows, o `docker/metadata-action` gera tags com SHA **curto** (7 caracteres):

```yaml
# metadata-action
tags: |
  type=sha,prefix=sha-,format=short
```

Isso produz uma tag como: `sha-6d2b3f3`

Porém, o step do Trivy referencia a imagem com SHA **completo** (40 caracteres):

```yaml
# Backend CI
image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}
# Resolve para: ghcr.io/.../l2sledger-backend:sha-6d2b3f354067dff204c8ba0ee26c926c65ec5fc7
```

### Causa Raiz

`github.sha` é sempre o SHA completo de 40 caracteres. A tag `format=short` do `metadata-action` usa apenas os 7 primeiros. O Trivy tenta escanear uma tag que **não existe** no registry.

### Fluxo do Problema

```
metadata-action → gera tag sha-6d2b3f3 (7 chars)
docker push    → publica com sha-6d2b3f3
trivy          → procura sha-6d2b3f354067dff204c8ba0ee26c926c65ec5fc7 (40 chars) → NOT FOUND
```

### Impacto

- Trivy **nunca** consegue escanear a imagem mesmo quando o build é bem-sucedido
- Vulnerabilidades Docker não são detectadas em nenhum pipeline
- O `deploy-demo.yml` usa `cut -c1-7` corretamente, provando que o padrão esperado é SHA curto

### Correção

| # | Task | Descrição |
|---|------|-----------|
| 7.1 | Corrigir `image-ref` no backend-ci | Usar output do metadata-action ou calcular SHA curto |
| 7.2 | Corrigir `image-ref` no frontend-ci | Mesma correção |
| 7.3 | Validar scan Trivy | Confirmar que Trivy encontra e escaneia a imagem |

**Código sugerido (ambos os workflows):**

```yaml
- name: Extract version metadata
  id: meta
  # ...

- name: Build and push Docker image
  id: docker-build
  # ...

- name: Scan Docker image with Trivy
  id: trivy
  if: steps.docker-build.outcome == 'success'
  uses: aquasecurity/trivy-action@0.28.0
  with:
    # ✅ Usar SHA curto, consistente com format=short do metadata-action
    image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ steps.meta.outputs.version }}
    # Alternativa: calcular manualmente
    # image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-$(echo ${{ github.sha }} | cut -c1-7)
    format: 'sarif'
    output: 'trivy-backend.sarif'  # ou trivy-frontend.sarif
    severity: 'CRITICAL,HIGH'
    exit-code: '0'
```

> **Nota:** `steps.meta.outputs.version` já contém o SHA curto com prefixo. Alternativamente, a forma mais segura é referenciar diretamente uma das tags do output:
>
> ```yaml
> image-ref: ${{ fromJSON(steps.meta.outputs.json).tags[0] }}
> ```
>
> Isso garante que usamos exatamente a mesma tag que foi publicada.

---

## 8. Erro 7 — Deploy DEMO sem Dependência de CI (Race Condition)

**Workflow:** `.github/workflows/deploy-demo.yml`  
**Tipo:** Falha de design — ausência de guard

### Problema

O `deploy-demo.yml` e os CIs (`backend-ci.yml`, `frontend-ci.yml`) são disparados pelo **mesmo evento** (`push` para `main` com paths `backend/**` ou `frontend/**`), mas executam como **workflows independentes em paralelo**.

```
push to main (backend/**)
  ├─ backend-ci.yml    → build-and-test → docker (build+push) → ~5-10 min
  └─ deploy-demo.yml   → detect-changes → validate → deploy    → ~2-3 min
```

### Cenários de Falha

| Cenário | Resultado |
|---------|-----------|
| CI está buildando, deploy-demo chega no `validate` antes | `docker manifest inspect` falha → deploy falha (por sorte, não por design) |
| CI falha no build, deploy-demo tenta validar | `docker manifest inspect` falha → deploy falha (correto, mas acidental) |
| CI é lento, deploy-demo retenta antes | Imagem antiga (ou inexistente) pode ser encontrada → **deploy de versão errada** |

### Causa Raiz

Não existe um `workflow_run` trigger nem qualquer mecanismo que garanta que o CI **completou com sucesso** antes do deploy começar.

### Impacto

- **Deploy pode executar sem build prévio** — viola o princípio "build antes de deploy"
- **Race condition** — resultado depende de timing, não de lógica
- Em caso raro, pode deployar uma imagem com tag SHA antiga que coincida

### Correção Aprovada — workflow_run

| # | Task | Descrição |
|---|------|-----------|
| 8.1 | Alterar trigger do deploy-demo | Substituir `push` por `workflow_run` para esperar CI completar |
| 8.2 | Garantir que ambos CIs passaram | Condicionar deploy ao sucesso de ambos os CIs relevantes |
| 8.3 | Manter validate como fallback | Manter `docker manifest inspect` como dupla validação |

**Código aprovado:**

```yaml
name: Deploy to DEMO (Auto)

on:
  workflow_run:
    workflows: ["Backend CI", "Frontend CI"]
    branches: [main]
    types: [completed]

jobs:
  deploy:
    # Só deploya se o CI que disparou completou com sucesso
    if: github.event.workflow_run.conclusion == 'success'
    # ...
```

> **Nota:** `workflow_run` roda após o workflow referenciado **completar** (não quando inicia). Isso elimina a race condition.

---

## 9. Erro 8 — Storybook Deploy sem Dependência de CI

**Workflow:** `.github/workflows/storybook-deploy.yml`  
**Tipo:** Falha de design — ausência de guard

### Problema

O `storybook-deploy.yml` é acionado por `push` para `main` em paths de stories/storybook, mas **não depende do `frontend-ci.yml`**. Isso significa que:

1. Pode deployar Storybook mesmo que lint, type-check ou testes do frontend falhem
2. Não há garantia de que o código que gerou as stories é válido

### Análise

O Storybook faz seu próprio `npm ci` + `npm run build-storybook`, então erros de **build** são pegos. Mas:

- Erros de **lint** → não detectados
- Erros de **type-check** → não detectados
- Erros de **testes unitários** → não detectados

### Impacto

- **Moderado** — Storybook é documentação visual, não produção
- Mas pode gerar confusão se stories deployadas referenciam código com bugs

### Correção Aprovada — workflow_run

| # | Task | Descrição |
|---|------|-----------|
| 9.1 | Alterar trigger para `workflow_run` | Disparar Storybook deploy **somente após** `Frontend CI` completar com sucesso |
| 9.2 | Condicionar deploy ao sucesso | `if: github.event.workflow_run.conclusion == 'success'` |

**Código aprovado:**

```yaml
name: Deploy Storybook

on:
  workflow_run:
    workflows: ["Frontend CI"]
    branches: [main]
    types: [completed]

jobs:
  deploy:
    if: github.event.workflow_run.conclusion == 'success'
    # ... resto igual
```

---

## 10. Erro 9 — Frontend CI: Trivy/SARIF Cascade

**Workflow:** `.github/workflows/frontend-ci.yml`  
**Job:** `docker` → steps Trivy + Upload SARIF

### Problema

O `frontend-ci.yml` tem **exatamente os mesmos problemas** do `backend-ci.yml` (Erro 4):

1. Step `Build and push Docker image` **não tem `id`** — impossível referenciar seu outcome
2. Step `Scan Docker image with Trivy` **não tem condição** — executa mesmo se build falhou
3. Step `Upload Trivy scan results` usa `if: always()` — tenta upload mesmo sem SARIF

### Correção

| # | Task | Descrição |
|---|------|-----------|
| 10.1 | Adicionar `id: docker-build` ao build step | Permitir referência ao outcome |
| 10.2 | Condicionar Trivy | `if: steps.docker-build.outcome == 'success'` |
| 10.3 | Condicionar upload SARIF | `if: always() && steps.trivy.outcome == 'success'` |

**Código sugerido:**

```yaml
- name: Build and push Docker image
  id: docker-build
  uses: docker/build-push-action@v6
  # ...

- name: Scan Docker image with Trivy
  id: trivy
  if: steps.docker-build.outcome == 'success'
  uses: aquasecurity/trivy-action@0.28.0
  with:
    image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-$(echo ${{ github.sha }} | cut -c1-7)
    format: 'sarif'
    output: 'trivy-frontend.sarif'
    severity: 'CRITICAL,HIGH'
    exit-code: '0'

- name: Upload Trivy scan results
  uses: github/codeql-action/upload-sarif@v4
  if: always() && steps.trivy.outcome == 'success'
  with:
    sarif_file: trivy-frontend.sarif
    category: trivy-frontend
```

---

## 11. Erro 10 — Frontend CI: CodeQL Analysis (Permissões)

**Workflow:** `.github/workflows/frontend-ci.yml`  
**Job:** `codeql`

### Problema

O job `codeql` do frontend tem as mesmas permissões insuficientes do backend (Erro 5):

```yaml
permissions:
  security-events: write
  contents: read
  # ← Falta actions: read
```

### Correção

| # | Task | Descrição |
|---|------|-----------|
| 11.1 | Adicionar `actions: read` | Adicionar ao bloco de permissões do job `codeql` |

**Código sugerido:**

```yaml
codeql:
  name: CodeQL Analysis
  needs: build-and-test
  runs-on: ubuntu-latest
  if: github.event_name == 'push' || github.event_name == 'pull_request'
  permissions:
    security-events: write
    contents: read
    actions: read       # ← Necessário para telemetria do CodeQL
```

---

## 12. Resumo de Ações

### Arquivo `.gitignore` (raiz)

| Ação | Linha | Mudança |
|------|-------|---------|
| Remover `package-lock.json` | 98 | Deletar a linha |
| Opcional: remover `yarn.lock` | 99 | Deletar se necessário |

### Arquivo `.github/workflows/backend-ci.yml`

| Ação | Seção | Mudança | Erro |
|------|-------|---------| -----|
| Adicionar step de resolução de versão | Job `docker` | Novo step antes do Docker build | 3 |
| Ajustar `build-args` | Step `Build and push` | Usar output do step de versão | 3 |
| Adicionar `id` ao Docker build step | Step `Build and push` | `id: docker-build` | 4 |
| Condicionar Trivy ao sucesso do build | Step `Scan Docker image` | `if: steps.docker-build.outcome == 'success'` | 4 |
| Corrigir SHA no `image-ref` do Trivy | Step `Scan Docker image` | Usar SHA curto (7 chars) ao invés de `github.sha` completo | 6 |
| Condicionar upload SARIF | Step `Upload Trivy scan` | `if: always() && steps.trivy.outcome == 'success'` | 4 |
| Adicionar `actions: read` | Job `codeql.permissions` | Nova permissão | 5 |

### Arquivo `.github/workflows/frontend-ci.yml`

| Ação | Seção | Mudança | Erro |
|------|-------|---------| -----|
| Adicionar `id` ao Docker build step | Step `Build and push` | `id: docker-build` | 9 |
| Condicionar Trivy ao sucesso do build | Step `Scan Docker image` | `if: steps.docker-build.outcome == 'success'` | 9 |
| Corrigir SHA no `image-ref` do Trivy | Step `Scan Docker image` | Usar SHA curto (7 chars) ao invés de `github.sha` completo | 6 |
| Condicionar upload SARIF | Step `Upload Trivy scan` | `if: always() && steps.trivy.outcome == 'success'` | 9 |
| Adicionar `actions: read` | Job `codeql.permissions` | Nova permissão | 10 |

### Arquivo `.github/workflows/deploy-demo.yml`

| Ação | Seção | Mudança | Erro |
|------|-------|---------| -----|
| Alterar trigger para `workflow_run` | `on:` | Substituir `push` por `workflow_run` após CIs | 7 |
| Condicionar deploy ao sucesso do CI | Job `deploy` | `if: github.event.workflow_run.conclusion == 'success'` | 7 |
| Manter validate como fallback | Job `validate` | `docker manifest inspect` continua como dupla validação | 7 |

### Arquivo `.github/workflows/storybook-deploy.yml`

| Ação | Seção | Mudança | Erro |
|------|-------|---------| -----|
| Corrigida cache pelo Erro 1 | — | Comitar `package-lock.json` resolve | 2 |
| Alterar trigger para `workflow_run` OU adicionar lint/type-check | `on:` ou steps | Garantir que código é válido antes de deployar | 8 |

### Git (repositório)

| Ação | Comando |
|------|---------|
| Comitar `package-lock.json` | `git add frontend/package-lock.json --force && git commit -m "chore: track package-lock.json for CI cache"` |

---

## 13. Ordem de Execução

```
 1. [GITIGNORE]       Remover package-lock.json do .gitignore
 2. [GIT]             Comitar frontend/package-lock.json
 3. [BACKEND-CI]      Adicionar step de resolução de versão SemVer
 4. [BACKEND-CI]      Adicionar id + condições ao Trivy/SARIF
 5. [BACKEND-CI]      Corrigir SHA curto no image-ref do Trivy
 6. [BACKEND-CI]      Adicionar actions:read ao job codeql
 7. [FRONTEND-CI]     Adicionar id + condições ao Trivy/SARIF
 8. [FRONTEND-CI]     Corrigir SHA curto no image-ref do Trivy
 9. [FRONTEND-CI]     Adicionar actions:read ao job codeql
10. [DEPLOY-DEMO]     Alterar trigger para workflow_run (após CIs)
11. [STORYBOOK]       Alterar trigger para workflow_run OU adicionar lint/type-check
12. [REPO SETTINGS]   Verificar Workflow permissions (Settings → Actions → General)
13. [COMMIT]          Comitar todas as alterações de workflows
14. [PUSH]            Push para branch, validar CIs
15. [VALIDAR]         Confirmar que todos os workflows passam
```

> **Recomendação:** Fazer todas as correções em um único commit/PR para validar tudo de uma vez.
> **IMPORTANTE:** Nenhum deploy deve prosseguir se o build anterior falhou. Toda cadeia deve ser: Build → Sucesso → Deploy.

---

## 14. Critérios de Aceite

### Pipelines de CI

- [ ] `frontend-ci.yml` — Job `build-and-test` executa com sucesso (Setup Node + npm ci + lint + type-check + tests + build)
- [ ] `frontend-ci.yml` — Job `docker` constrói e publica imagem com tags corretas (SHA curto)
- [ ] `frontend-ci.yml` — Trivy scan executa e gera SARIF quando Docker build é bem-sucedido
- [ ] `frontend-ci.yml` — Trivy scan é ignorado graciosamente quando Docker build falha
- [ ] `frontend-ci.yml` — CodeQL Analysis completa e faz upload de resultados
- [ ] `backend-ci.yml` — Job `build-and-test` passa
- [ ] `backend-ci.yml` — Job `docker` constrói e publica imagem com versão SemVer válida
- [ ] `backend-ci.yml` — Trivy scan executa e gera SARIF quando Docker build é bem-sucedido
- [ ] `backend-ci.yml` — Trivy scan é ignorado graciosamente quando Docker build falha
- [ ] `backend-ci.yml` — CodeQL Analysis completa e faz upload de resultados

### Pipelines de Deploy

- [ ] `deploy-demo.yml` — Só executa **após** CI completar com sucesso (sem race condition)
- [ ] `deploy-demo.yml` — Não deploya quando CI falha
- [ ] `deploy-demo.yml` — `docker manifest inspect` valida existência da imagem
- [ ] `deploy.yml` — Deploy manual continua funcional (sem regressão)
- [ ] `storybook-deploy.yml` — Só deploya código que passou por validação (lint/type-check ou workflow_run)

### Consistência de SHA

- [ ] SHA curto (7 chars) é usado **consistentemente** em todos os workflows: metadata-action, Trivy, deploy-demo
- [ ] Nenhum workflow usa `github.sha` completo (40 chars) como tag Docker

### Infraestrutura

- [ ] `package-lock.json` está versionado no repositório
- [ ] Configurações de permissão do repositório permitem `actions: read`
- [ ] Nenhuma regressão nos pipelines existentes

---

## ⚠️ Considerações ADRs e Governança

- Nenhum ADR é impactado diretamente (mudanças são infraestruturais/CI)
- A adição de `package-lock.json` ao git é uma **boa prática** obrigatória para reproducibilidade
- A estratégia de versionamento SemVer no Docker build deve ser documentada como padrão do projeto
- A mudança de trigger do `deploy-demo.yml` para `workflow_run` é uma mudança de comportamento — **builds** e **deploys** passam a ser sequenciais ao invés de paralelos
- O princípio **"build antes de deploy"** deve ser regra obrigatória: nenhum deploy pode iniciar sem que o CI correspondente tenha completado com sucesso

---

**Estimativa Total:** ~6 horas  
**Status:** Aprovado  
**Próximo passo:** Executar correções em um único PR
