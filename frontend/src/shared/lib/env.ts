/**
 * Lê uma variável de ambiente com suporte a runtime injection (Docker)
 * e fallback para Vite build-time (desenvolvimento local).
 *
 * Em produção (Docker): lê de window.__ENV__ (gerado por env.sh)
 * Em dev (vite dev):    lê de import.meta.env
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
