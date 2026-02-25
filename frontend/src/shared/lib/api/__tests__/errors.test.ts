import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { handleAuthError, getErrorMessage, ERROR_MESSAGES } from '../errors';
import { ApiError } from '@/shared/types/errors.types';

describe('handleAuthError', () => {
  beforeEach(() => {
    // jsdom não permite atribuição direta a window.location — substituímos o objeto
    vi.stubGlobal('location', { href: '' });
    vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it.each([
    ['AUTH_EMAIL_NOT_VERIFIED', '/verify-email'],
    ['AUTH_USER_PENDING', '/pending-approval'],
    ['AUTH_USER_SUSPENDED', '/suspended'],
    ['AUTH_USER_REJECTED', '/rejected'],
    ['AUTH_INVALID_TOKEN', '/login'],
    ['AUTH_SESSION_EXPIRED', '/login'],
    ['AUTH_UNAUTHORIZED', '/login'],
    ['AUTH_USER_INACTIVE', '/login'],
    ['AUTH_USER_NOT_FOUND', '/login'],
    ['AUTH_FIREBASE_ERROR', '/login'],
  ] as [string, string][])(
    'deve redirecionar %s para %s',
    (code: string, expectedPath: string) => {
      handleAuthError(new ApiError(code, 'Error'));
      expect(window.location.href).toContain(expectedPath);
    },
  );

  it('deve logar e não redirecionar para código desconhecido', () => {
    const consoleSpy = vi.spyOn(console, 'error');
    const prevHref = window.location.href;

    handleAuthError(new ApiError('UNKNOWN_CODE', 'Error'));

    expect(consoleSpy).toHaveBeenCalledWith('Unhandled error:', expect.any(ApiError));
    expect(window.location.href).toBe(prevHref);
  });

  it('deve logar erro nos casos de auth user inactive / not found / firebase', () => {
    const consoleSpy = vi.spyOn(console, 'error');

    handleAuthError(new ApiError('AUTH_USER_INACTIVE', 'Error'));

    expect(consoleSpy).toHaveBeenCalledWith('Auth error:', expect.any(ApiError));
  });
});

describe('getErrorMessage', () => {
  it('deve retornar mensagem para código conhecido', () => {
    expect(getErrorMessage('AUTH_INVALID_TOKEN')).toBe(ERROR_MESSAGES.AUTH_INVALID_TOKEN);
  });

  it('deve retornar mensagem para código de validação', () => {
    expect(getErrorMessage('VAL_REQUIRED_FIELD')).toBe(ERROR_MESSAGES.VAL_REQUIRED_FIELD);
  });

  it('deve retornar mensagem genérica para código desconhecido', () => {
    expect(getErrorMessage('CODIGO_INEXISTENTE')).toBe(ERROR_MESSAGES.GENERIC_ERROR);
  });

  it('deve retornar mensagem para todos os grupos de erro', () => {
    const sampledCodes = [
      'FIN_CATEGORY_NOT_FOUND',
      'PERM_ACCESS_DENIED',
      'USER_NOT_FOUND',
      'SYS_INTERNAL_ERROR',
      'INT_FIREBASE_UNAVAILABLE',
      'EXPORT_INVALID_FORMAT',
    ];

    sampledCodes.forEach((code) => {
      expect(getErrorMessage(code)).toBe(ERROR_MESSAGES[code]);
    });
  });
});
