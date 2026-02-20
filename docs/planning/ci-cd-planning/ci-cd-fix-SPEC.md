# SPEC — Correção de CI/CD Pipelines — L2SLedger

> **Data:** 2026-02-20  
> **Versão:** 1.0  
> **Status:** Pronto para Execução  
> **Plano de Origem:** `docs/planning/ci-cd-fix-plan.md` v2.0  
> **Prioridade:** P0 — Crítica  
> **Agente Executor:** CI/CD Agent (`L2SLedger-CI-CD.prompt.md`)

---

## 📋 Índice

1. [Objetivo](#1-objetivo)
2. [Escopo](#2-escopo)
3. [Arquivos Impactados](#3-arquivos-impactados)
4. [Pré-condições](#4-pré-condições)
5. [Especificações de Mudança](#5-especificações-de-mudança)
   - [5.1 .gitignore — Remover package-lock.json](#51-gitignore--remover-package-lockjson)
   - [5.2 backend-ci.yml — Resolução de Versão SemVer](#52-backend-ciyml--resolução-de-versão-semver)
   - [5.3 backend-ci.yml — Trivy/SARIF Cascade Guards](#53-backend-ciyml--trivysarif-cascade-guards)
   - [5.4 backend-ci.yml — SHA Mismatch Fix](#54-backend-ciyml--sha-mismatch-fix)
   - [5.5 backend-ci.yml — CodeQL Permissions](#55-backend-ciyml--codeql-permissions)
   - [5.6 frontend-ci.yml — Trivy/SARIF Cascade Guards](#56-frontend-ciyml--trivysarif-cascade-guards)
   - [5.7 frontend-ci.yml — SHA Mismatch Fix](#57-frontend-ciyml--sha-mismatch-fix)
   - [5.8 frontend-ci.yml — CodeQL Permissions](#58-frontend-ciyml--codeql-permissions)
   - [5.9 deploy-demo.yml — workflow_run Trigger](#59-deploy-demoyml--workflow_run-trigger)
   - [5.10 storybook-deploy.yml — workflow_run Trigger](#510-storybook-deployyml--workflow_run-trigger)
6. [Referência de Documentação Oficial (Context7)](#6-referência-de-documentação-oficial-context7)
7. [Validação & Testes](#7-validação--testes)
8. [Rollback](#8-rollback)
9. [Checklist de Aceite](#9-checklist-de-aceite)

---

## 1. Objetivo

Especificar de forma **exata e implementável** todas as mudanças necessárias para corrigir os 10 erros identificados nos pipelines de CI/CD do L2SLedger, conforme aprovado no plano `ci-cd-fix-plan.md` v2.0.

Este documento serve como **contrato de execução** para o Agente CI/CD. Cada seção contém o **código exato** (before/after) que deve ser aplicado.

---

## 2. Escopo

### Incluído

- 5 workflow files do GitHub Actions
- 1 arquivo `.gitignore`
- 1 operação Git (commit de `package-lock.json`)
- 1 verificação manual de configuração do repositório

### Excluído

- Alterações em código de aplicação (backend/frontend)
- Criação de novos ADRs (mudanças são infraestruturais)
- Alterações no `deploy.yml` (deploy manual para PROD — não impactado)

### ADRs Relevantes

- Nenhum ADR é violado por estas mudanças
- Nenhum novo ADR é necessário (escopo infraestrutural)

---

## 3. Arquivos Impactados

| Arquivo | Tipo de Mudança | Erros Resolvidos |
|---------|----------------|-----------------|
| `.gitignore` | Remoção de linha | #1, #2 |
| `.github/workflows/backend-ci.yml` | Edição (4 blocos) | #3, #4, #5, #6 |
| `.github/workflows/frontend-ci.yml` | Edição (3 blocos) | #6, #9, #10 |
| `.github/workflows/deploy-demo.yml` | Reescrita do `on:` trigger + condição | #7 |
| `.github/workflows/storybook-deploy.yml` | Reescrita do `on:` trigger + condição | #8 |
| `frontend/package-lock.json` | Git add (já existe local) | #1, #2 |

---

## 4. Pré-condições

1. O plano `ci-cd-fix-plan.md` v2.0 está **aprovado**
2. Branch de trabalho criada a partir de `main`
3. `frontend/package-lock.json` existe localmente (gerado por `npm install`)
4. Acesso de escrita ao repositório

---

## 5. Especificações de Mudança

### 5.1 .gitignore — Remover package-lock.json

**Erro(s):** #1 (Frontend CI cache), #2 (Storybook deploy cache)

**Arquivo:** `.gitignore` (raiz do repositório)

**Justificativa (Context7 — actions/setup-node):**  
Conforme documentação oficial do `actions/setup-node`, o parâmetro `cache-dependency-path` exige que o lockfile **exista no repositório** após o checkout. Sem o `package-lock.json` versionado:
- O `actions/setup-node` falha com `"Some specified paths were not resolved, unable to cache dependencies"`
- O `npm ci` também falharia, pois **exige** o lockfile

**BEFORE:**

```gitignore
# Linha 98 do .gitignore
package-lock.json
```

**AFTER:**

```gitignore
# Linha 98 removida — package-lock.json DEVE ser versionado para CI (npm ci + cache)
```

**Ação Git complementar:**

```bash
git add frontend/package-lock.json --force
git commit -m "chore: track package-lock.json for CI reproducibility"
```

> **Nota:** Se `yarn.lock` (linha 99) existir e não for utilizado, pode ser mantido no `.gitignore`. O projeto usa `npm`.

---

### 5.2 backend-ci.yml — Resolução de Versão SemVer

**Erro:** #3 (Docker Build — `NETSDK1018: Invalid NuGet version string: 'main'`)

**Arquivo:** `.github/workflows/backend-ci.yml`  
**Job:** `docker`  
**Localização:** Antes do step `Build and push Docker image`

**Justificativa (Context7 — docker/build-push-action):**  
O `build-args` do `docker/build-push-action` passa valores diretamente ao Dockerfile. O `ARG BUILD_VERSION` é usado em `/p:Version=`, que exige SemVer válido segundo a especificação NuGet. O `github.ref_name` retorna `"main"` para pushes na branch main, que é inválido para NuGet.

**BEFORE:**

```yaml
      - name: Build and push Docker image
        uses: docker/build-push-action@v6
        with:
          context: ./backend
          file: ./backend/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          build-args: |
            BUILD_VERSION=${{ github.ref_name }}
            BUILD_SHA=${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
          provenance: true
          sbom: true
```

**AFTER:**

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
        id: docker-build
        uses: docker/build-push-action@v6
        with:
          context: ./backend
          file: ./backend/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          build-args: |
            BUILD_VERSION=${{ steps.version.outputs.version }}
            BUILD_SHA=${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
          provenance: true
          sbom: true
```

**Mudanças neste bloco:**

1. **Novo step** `Resolve build version` com `id: version` — calcula versão SemVer válida
2. **`id: docker-build`** adicionado ao step de build (necessário para 5.3)
3. **`BUILD_VERSION`** alterado de `${{ github.ref_name }}` para `${{ steps.version.outputs.version }}`

**Comportamento esperado:**

| Contexto | `github.ref` | `VERSION` resultante |
|----------|--------------|---------------------|
| Push para `main` | `refs/heads/main` | `0.0.0-dev` |
| Push para `develop` | `refs/heads/develop` | `0.0.0-dev` |
| Tag `v1.2.3` | `refs/tags/v1.2.3` | `1.2.3` |
| Tag `v2.0.0-rc.1` | `refs/tags/v2.0.0-rc.1` | `2.0.0-rc.1` |

---

### 5.3 backend-ci.yml — Trivy/SARIF Cascade Guards

**Erro(s):** #4 (SARIF não encontrado — erro em cascata)

**Arquivo:** `.github/workflows/backend-ci.yml`  
**Job:** `docker`  
**Localização:** Steps de Trivy scan e upload SARIF

**Justificativa (Context7 — aquasecurity/trivy):**  
O Trivy requer que a imagem Docker exista para executar o scan. Se o build falhou, a imagem não existe e o Trivy falha silenciosamente, não gerando o arquivo `.sarif`. O upload subsequente com `if: always()` tenta enviar um arquivo inexistente.

**BEFORE:**

```yaml
      - name: Scan Docker image with Trivy
        uses: aquasecurity/trivy-action@0.28.0
        with:
          image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}
          format: 'sarif'
          output: 'trivy-backend.sarif'
          severity: 'CRITICAL,HIGH'
          exit-code: '0'

      - name: Upload Trivy scan results
        uses: github/codeql-action/upload-sarif@v4
        if: always()
        with:
          sarif_file: trivy-backend.sarif
          category: trivy-backend
```

**AFTER:**

```yaml
      - name: Scan Docker image with Trivy
        id: trivy
        if: steps.docker-build.outcome == 'success'
        uses: aquasecurity/trivy-action@0.28.0
        with:
          image-ref: ${{ fromJSON(steps.meta.outputs.json).tags[0] }}
          format: 'sarif'
          output: 'trivy-backend.sarif'
          severity: 'CRITICAL,HIGH'
          exit-code: '0'

      - name: Upload Trivy scan results
        uses: github/codeql-action/upload-sarif@v4
        if: always() && steps.trivy.outcome == 'success'
        with:
          sarif_file: trivy-backend.sarif
          category: trivy-backend
```

**Mudanças neste bloco:**

1. **`id: trivy`** adicionado ao step de scan
2. **`if: steps.docker-build.outcome == 'success'`** — Trivy só executa se Docker build passou
3. **`image-ref`** corrigido (ver seção 5.4 para detalhes do SHA fix)
4. **Upload SARIF** condição alterada de `if: always()` para `if: always() && steps.trivy.outcome == 'success'`

---

### 5.4 backend-ci.yml — SHA Mismatch Fix

**Erro:** #6 (SHA longo 40 chars vs SHA curto 7 chars nas tags Docker)

**Arquivo:** `.github/workflows/backend-ci.yml`  
**Job:** `docker` → step `Scan Docker image with Trivy`

**Justificativa (Context7 — docker/metadata-action):**  
Conforme documentação oficial do `docker/metadata-action`, o tipo `type=sha,format=short` gera tags com SHA de **7 caracteres** (padrão configurável via `DOCKER_METADATA_SHORT_SHA_LENGTH`). O `github.sha` contém sempre 40 caracteres. A referência no Trivy deve usar a **mesma tag** gerada pelo metadata-action.

A abordagem mais segura é usar `${{ fromJSON(steps.meta.outputs.json).tags[0] }}`, que referencia diretamente a primeira tag publicada (garantindo consistência exata).

**BEFORE:**

```yaml
          image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}
```

**AFTER:**

```yaml
          image-ref: ${{ fromJSON(steps.meta.outputs.json).tags[0] }}
```

> **Nota:** Esta mudança já está incorporada no bloco AFTER da seção 5.3. Documentada separadamente para rastreabilidade do Erro #6.

**Alternativa válida (menos recomendada):**

```yaml
          image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ steps.meta.outputs.version }}
```

> O output `version` do metadata-action contém o valor da tag de maior prioridade. Para `type=sha,prefix=sha-,format=short`, o valor seria `sha-6d2b3f3`.

---

### 5.5 backend-ci.yml — CodeQL Permissions

**Erro:** #5 (`Resource not accessible by integration`)

**Arquivo:** `.github/workflows/backend-ci.yml`  
**Job:** `codeql`

**Justificativa (Context7 — github/codeql):**  
O CodeQL Action requer `actions: read` para acessar a API de workflow runs (telemetria). Sem essa permissão, o upload de resultados SARIF falha com `Resource not accessible by integration`.

**BEFORE:**

```yaml
  codeql:
    name: CodeQL Analysis
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push' || github.event_name == 'pull_request'
    permissions:
      security-events: write
      contents: read
```

**AFTER:**

```yaml
  codeql:
    name: CodeQL Analysis
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push' || github.event_name == 'pull_request'
    permissions:
      security-events: write
      contents: read
      actions: read
```

**Mudança:** Adição de `actions: read` ao bloco `permissions`.

**Ação manual complementar (Settings do repositório):**

- Navegar para `Settings → Actions → General → Workflow permissions`
- Garantir que está configurado como **"Read and write permissions"**
- Esta configuração é **pré-requisito** para o `GITHUB_TOKEN` ter as permissões solicitadas

---

### 5.6 frontend-ci.yml — Trivy/SARIF Cascade Guards

**Erro:** #9 (Mesmos problemas de cascata do backend)

**Arquivo:** `.github/workflows/frontend-ci.yml`  
**Job:** `docker`

**BEFORE:**

```yaml
      - name: Build and push Docker image
        uses: docker/build-push-action@v6
        with:
          context: ./frontend
          file: ./frontend/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
          provenance: true
          sbom: true

      - name: Scan Docker image with Trivy
        uses: aquasecurity/trivy-action@0.28.0
        with:
          image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}
          format: 'sarif'
          output: 'trivy-frontend.sarif'
          severity: 'CRITICAL,HIGH'
          exit-code: '0'

      - name: Upload Trivy scan results
        uses: github/codeql-action/upload-sarif@v4
        if: always()
        with:
          sarif_file: trivy-frontend.sarif
          category: trivy-frontend
```

**AFTER:**

```yaml
      - name: Build and push Docker image
        id: docker-build
        uses: docker/build-push-action@v6
        with:
          context: ./frontend
          file: ./frontend/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
          provenance: true
          sbom: true

      - name: Scan Docker image with Trivy
        id: trivy
        if: steps.docker-build.outcome == 'success'
        uses: aquasecurity/trivy-action@0.28.0
        with:
          image-ref: ${{ fromJSON(steps.meta.outputs.json).tags[0] }}
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

**Mudanças:**

1. **`id: docker-build`** adicionado ao step de build
2. **`id: trivy`** + **`if: steps.docker-build.outcome == 'success'`** no Trivy
3. **`image-ref`** corrigido para usar `fromJSON(steps.meta.outputs.json).tags[0]` (resolve Erro #6)
4. **Upload SARIF** condição: `if: always() && steps.trivy.outcome == 'success'`

---

### 5.7 frontend-ci.yml — SHA Mismatch Fix

**Erro:** #6 (SHA longo vs curto — frontend)

**Arquivo:** `.github/workflows/frontend-ci.yml`  
**Job:** `docker` → step `Scan Docker image with Trivy`

Mesma correção da seção 5.4 (backend). Já incorporada no bloco AFTER da seção 5.6.

**BEFORE:**

```yaml
          image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}
```

**AFTER:**

```yaml
          image-ref: ${{ fromJSON(steps.meta.outputs.json).tags[0] }}
```

---

### 5.8 frontend-ci.yml — CodeQL Permissions

**Erro:** #10 (`actions: read` ausente)

**Arquivo:** `.github/workflows/frontend-ci.yml`  
**Job:** `codeql`

**BEFORE:**

```yaml
  codeql:
    name: CodeQL Analysis
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push' || github.event_name == 'pull_request'
    permissions:
      security-events: write
      contents: read
```

**AFTER:**

```yaml
  codeql:
    name: CodeQL Analysis
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push' || github.event_name == 'pull_request'
    permissions:
      security-events: write
      contents: read
      actions: read
```

---

### 5.9 deploy-demo.yml — workflow_run Trigger

**Erro:** #7 (Race condition — deploy sem dependência de CI)

**Arquivo:** `.github/workflows/deploy-demo.yml`

**Justificativa (Context7 — github/codeql / workflow_run):**  
Conforme padrão documentado na documentação do CodeQL Action (e das GitHub Actions em geral), o trigger `workflow_run` executa **após** o workflow referenciado completar. Com `types: [completed]` e `conclusion == 'success'`, garante-se que o deploy só acontece após CI bem-sucedido.

**BEFORE (bloco `on:` completo):**

```yaml
name: Deploy to DEMO (Auto)

on:
  push:
    branches:
      - main
      - 'release/**'
    paths:
      - 'backend/**'
      - 'frontend/**'
      - 'docker-compose.prod.yml'
      - '.github/workflows/deploy-demo.yml'
```

**AFTER:**

```yaml
name: Deploy to DEMO (Auto)

on:
  workflow_run:
    workflows: ["Backend CI", "Frontend CI"]
    branches: [main]
    types: [completed]
```

**Mudanças no bloco de jobs:**

Adicionar condição ao primeiro job que executa lógica:

**BEFORE (job `detect-changes`):**

```yaml
jobs:
  # ── Determine what to deploy ──────────────────────────────────
  detect-changes:
    name: Detect Changes
    runs-on: ubuntu-latest
```

**AFTER (job `detect-changes`):**

```yaml
jobs:
  # ── Determine what to deploy ──────────────────────────────────
  detect-changes:
    name: Detect Changes
    runs-on: ubuntu-latest
    if: github.event.workflow_run.conclusion == 'success'
```

**Comportamento esperado:**

| Cenário | Resultado |
|---------|-----------|
| Backend CI completa com sucesso | Deploy inicia, detecta mudanças, faz deploy se necessário |
| Frontend CI completa com sucesso | Deploy inicia, detecta mudanças, faz deploy se necessário |
| Backend CI falha | Deploy **não inicia** (`conclusion != 'success'`) |
| Frontend CI falha | Deploy **não inicia** |
| Ambos completam | Deploy inicia **duas vezes** (uma para cada), mas `concurrency` garante serialização |

> **Nota:** `workflow_run` dispara uma vez para **cada** workflow referenciado que completa. O grupo de `concurrency` existente (`demo-deploy-${{ github.ref }}`) garante que não há deploys simultâneos.

**Consideração sobre `detect-changes`:**

Com `workflow_run`, o `github.sha` corresponde ao commit que disparou o CI. O step `Determine deploy tag` já usa `github.sha` com `cut -c1-7`, que produz SHA curto consistente com as tags Docker geradas pelo metadata-action (ambos 7 chars). Nenhuma mudança adicional necessária.

---

### 5.10 storybook-deploy.yml — workflow_run Trigger

**Erro:** #8 (Deploy sem dependência de CI — pode publicar código não validado)

**Arquivo:** `.github/workflows/storybook-deploy.yml`

**BEFORE (bloco `on:` completo):**

```yaml
name: Deploy Storybook

on:
  push:
    branches: [main]
    paths:
      - 'frontend/src/**/*.stories.tsx'
      - 'frontend/.storybook/**'
      - '.github/workflows/storybook-deploy.yml'
```

**AFTER:**

```yaml
name: Deploy Storybook

on:
  workflow_run:
    workflows: ["Frontend CI"]
    branches: [main]
    types: [completed]
```

**Mudança no job `deploy`:**

**BEFORE:**

```yaml
jobs:
  deploy:
    name: Build & Deploy Storybook
    runs-on: ubuntu-latest
```

**AFTER:**

```yaml
jobs:
  deploy:
    name: Build & Deploy Storybook
    runs-on: ubuntu-latest
    if: github.event.workflow_run.conclusion == 'success'
```

**Trade-off documentado:**

- **Antes:** Storybook era deployado apenas quando arquivos de stories/storybook mudavam (via `paths`)
- **Depois:** Storybook será deployado **sempre** que o Frontend CI completar com sucesso em `main`
- **Justificativa:** Garantir que stories deployadas sempre correspondem a código validado. O custo adicional (build Storybook em pushes sem mudanças de stories) é aceitável dado que o build é rápido (~1-2 min) e o risco de publishar stories com código quebrado é eliminado.

> **Nota:** Se o Frontend CI não for disparado (ex: mudança apenas em `backend/**`), o Storybook deploy também **não** será disparado, pois `workflow_run` só reage a completions do workflow referenciado.

---

## 6. Referência de Documentação Oficial (Context7)

Todas as decisões técnicas deste SPEC foram validadas contra documentação oficial via Context7:

### actions/setup-node (Source: High Reputation)

- **Cache:** `cache-dependency-path` exige que o lockfile exista no repositório após checkout
- **Monorepo:** Suporta paths específicos como `frontend/package-lock.json`
- **npm ci:** Requer `package-lock.json` versionado

### docker/build-push-action (Source: High Reputation)

- **build-args:** Aceita lista de argumentos `KEY=VALUE`, passados como `--build-arg` ao Docker
- **id:** Suporta `id` para referenciar outputs e outcomes em steps subsequentes

### docker/metadata-action (Source: High Reputation)

- **type=sha:** `format=short` gera SHA de 7 caracteres (padrão, configurável via `DOCKER_METADATA_SHORT_SHA_LENGTH`)
- **Outputs:** `steps.meta.outputs.version` contém a tag de maior prioridade; `steps.meta.outputs.json` contém JSON com `tags[]` completo
- **Referência segura:** `${{ fromJSON(steps.meta.outputs.json).tags[0] }}` garante uso exato da tag publicada

### aquasecurity/trivy (Source: High Reputation)

- **SARIF:** `--format sarif -o <file>` gera relatório SARIF 2.1.0 para upload ao GitHub Code Scanning
- **Image scan:** Requer que a imagem exista (localmente ou no registry) para executar o scan
- **exit-code 0:** Não falha o step mesmo com vulnerabilidades encontradas

### github/codeql-action (Source: High Reputation)

- **Permissions:** `security-events: write` para upload SARIF; `contents: read` para checkout; `actions: read` para telemetria de workflow runs
- **workflow_run:** Padrão recomendado para separar workflows privilegiados de não-privilegiados

---

## 7. Validação & Testes

### 7.1 Validação Local (Pré-Push)

```bash
# 1. Verificar que package-lock.json existe
test -f frontend/package-lock.json && echo "OK" || echo "FAIL"

# 2. Validar YAML dos workflows (requer yamllint ou actionlint)
actionlint .github/workflows/backend-ci.yml
actionlint .github/workflows/frontend-ci.yml
actionlint .github/workflows/deploy-demo.yml
actionlint .github/workflows/storybook-deploy.yml

# 3. Docker build local do backend (validar SemVer)
docker build --build-arg BUILD_VERSION=0.0.0-dev --build-arg BUILD_SHA=abc1234 -f backend/Dockerfile backend/
```

### 7.2 Validação no CI (Pós-Push)

| Workflow | Validação | Critério de Sucesso |
|----------|-----------|---------------------|
| `frontend-ci.yml` | Setup Node + cache | Step `Setup Node.js` completa sem erro |
| `frontend-ci.yml` | Docker build + Trivy | Steps executam em sequência, Trivy scan gera SARIF |
| `frontend-ci.yml` | CodeQL | Step `Perform CodeQL Analysis` completa e faz upload |
| `backend-ci.yml` | Docker build | Step `Build and push` completa com versão `0.0.0-dev` |
| `backend-ci.yml` | Trivy + SARIF | Trivy executa, SARIF é uploaded |
| `backend-ci.yml` | CodeQL | Completa sem `Resource not accessible` |
| `deploy-demo.yml` | Trigger sequencial | Executa **somente após** CI completar com sucesso |
| `storybook-deploy.yml` | Trigger sequencial | Executa **somente após** Frontend CI completar |

### 7.3 Cenários de Falha Esperados

| Cenário | Resultado Esperado |
|---------|-------------------|
| Docker build falha | Trivy step é **skipped** (`steps.docker-build.outcome != 'success'`) |
| Trivy não executa | Upload SARIF é **skipped** (`steps.trivy.outcome != 'success'`) |
| Backend CI falha | Deploy DEMO **não inicia** |
| Frontend CI falha | Storybook deploy **não inicia** |

---

## 8. Rollback

Em caso de regressão, as mudanças podem ser revertidas com:

```bash
git revert <commit-sha>
```

Riscos de rollback:

- **`.gitignore`:** Reverter adicionaria `package-lock.json` de volta ao ignore, quebrando CI novamente
- **Workflows:** Revert individual de cada arquivo é seguro
- **`deploy-demo.yml`:** Reverter voltaria à race condition (comportamento atual — não é pior)

---

## 9. Checklist de Aceite

### Pipelines de CI

- [ ] `frontend-ci.yml` — Job `build-and-test` executa com sucesso (Setup Node + npm ci + lint + type-check + tests + build)
- [ ] `frontend-ci.yml` — Job `docker` constrói e publica imagem com tags corretas (SHA curto 7 chars)
- [ ] `frontend-ci.yml` — Trivy scan executa e gera SARIF quando Docker build é bem-sucedido
- [ ] `frontend-ci.yml` — Trivy scan é **skipped** graciosamente quando Docker build falha
- [ ] `frontend-ci.yml` — CodeQL Analysis completa e faz upload de resultados
- [ ] `backend-ci.yml` — Job `build-and-test` passa
- [ ] `backend-ci.yml` — Job `docker` constrói com versão SemVer válida (`0.0.0-dev` para branches, SemVer real para tags `v*`)
- [ ] `backend-ci.yml` — Trivy scan executa e gera SARIF quando Docker build é bem-sucedido
- [ ] `backend-ci.yml` — Trivy scan é **skipped** graciosamente quando Docker build falha
- [ ] `backend-ci.yml` — CodeQL Analysis completa e faz upload de resultados

### Pipelines de Deploy

- [ ] `deploy-demo.yml` — Só executa **após** CI completar com sucesso (sem race condition)
- [ ] `deploy-demo.yml` — **Não** deploya quando CI falha
- [ ] `deploy-demo.yml` — `docker manifest inspect` valida existência da imagem (dupla validação)
- [ ] `deploy.yml` — Deploy manual PROD continua funcional (sem regressão)
- [ ] `storybook-deploy.yml` — Só deploya código que passou pelo Frontend CI

### Consistência de SHA

- [ ] SHA curto (7 chars) é usado **consistentemente** em todos os workflows
- [ ] Trivy referencia tags publicadas pelo metadata-action (via `fromJSON(steps.meta.outputs.json).tags[0]`)
- [ ] Nenhum workflow usa `github.sha` completo (40 chars) como tag Docker para scan

### Infraestrutura

- [ ] `package-lock.json` está versionado no repositório e **removido** do `.gitignore`
- [ ] Configuração de permissões do repositório permite `actions: read` (`Settings → Actions → General`)
- [ ] Nenhuma regressão nos pipelines existentes
- [ ] `ai-driven/changelog.md` atualizado com resumo das mudanças

---

> **Estimativa:** ~4-6 horas (inclui validação de todos os workflows)  
> **Próximo passo:** Delegar execução ao Agente CI/CD com este SPEC como contrato.
