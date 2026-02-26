/**
 * Lê uma variável de ambiente com suporte a runtime injection (Docker)
 * e fallback para Vite build-time (desenvolvimento local).
 *
 * Em produção (Docker): lê de window.__ENV__ (gerado por env.sh)
 * Em dev (vite dev):    lê de import.meta.env
 *
 * ## Segurança
 *
 * O arquivo `docker/env.sh` usa uma **whitelist explícita** para prevenir
 * exposição acidental de variáveis sensíveis. Apenas as seguintes variáveis
 * são exportadas para `window.__ENV__`:
 *
 * - VITE_API_BASE_URL (necessária para requisições)
 * - VITE_FIREBASE_* (6 vars - públicas por design, protegidas por Firebase rules)
 * - VITE_EMAIL_VERIFICATION_RESEND_COOLDOWN (config de UX segura)
 *
 * ⚠️ **VITE_ENABLE_DEVTOOLS** é explicitamente excluída de produção.
 *
 * Para adicionar uma nova variável pública, ela deve ser:
 * 1. Adicionada à whitelist em `docker/env.sh`
 * 2. Documentada aqui
 * 3. Revisada quanto a riscos de segurança
 */

interface RuntimeEnv {
  [key: string]: string | undefined;
}

declare global {
  interface Window {
    __ENV__?: RuntimeEnv;
  }
}

export function getEnv(key: string): string | undefined {
  // 1. Runtime (Docker container)
  if (typeof window !== 'undefined' && window.__ENV__?.[key]) {
    return window.__ENV__[key];
  }

  // 2. Build-time (Vite dev / build com .env)
  return import.meta.env[key] as string | undefined;
}
