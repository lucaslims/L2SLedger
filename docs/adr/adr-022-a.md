# ADR-022-a — Estratégia de Versionamento de Contratos da API no L2SLedger

## Status

Aprovado

## Contexto

O **ADR-022** define que os contratos públicos da API do L2SLedger são **imutáveis**. No entanto, sistemas financeiros evoluem, exigindo uma **estratégia explícita de versionamento** para permitir mudanças controladas sem quebrar consumidores existentes.

---

## Decisão

Adotar uma estratégia clara e obrigatória de **versionamento de contratos de API**, alinhada à imutabilidade definida no ADR-022.

---

## Regras de Versionamento

* Alterações incompatíveis exigem **nova versão**
* Alterações compatíveis não exigem versionamento
* Versões coexistem quando necessário

---

## Estratégia Técnica

* Versionamento via URL:

  * `/api/v1/...`
  * `/api/v2/...`

* Nunca versionar via header ou query string

---

## Compatibilidade

Considera-se **quebra de contrato**:

* Remoção de campos
* Alteração de tipo
* Mudança de significado semântico
* Alteração de códigos de erro

---

## Depreciação

* Versões antigas devem ser mantidas por período definido
* Depreciação deve ser documentada
* Frontend deve ser migrado de forma coordenada

---

## Consequências

### Positivas

* Evolução segura da API
* Redução de regressões
* Governança clara

### Negativas

* Manutenção simultânea de versões

---

## Conclusão

A estratégia explícita de versionamento garante que o L2SLedger evolua de forma **controlada, previsível e auditável**, respeitando a imutabilidade dos contratos públicos.

---

## Referências

* ADR-022 — Contratos Públicos Imutáveis
* [Princípios de API Design (RESTful APIs)](https://restfulapi.net/resource-naming/)
* [Práticas recomendadas de versionamento de APIs RESTful](https://restfulapi.net/versioning/)