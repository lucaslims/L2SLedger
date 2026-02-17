import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useMediaQuery } from '../useMediaQuery';

describe('useMediaQuery', () => {
  let listeners: Array<(event: MediaQueryListEvent) => void> = [];
  let matchesValue = false;

  beforeEach(() => {
    listeners = [];
    matchesValue = false;

    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: vi.fn().mockImplementation((query: string) => ({
        matches: matchesValue,
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(
          (_: string, listener: (event: MediaQueryListEvent) => void) => {
            listeners.push(listener);
          }
        ),
        removeEventListener: vi.fn(
          (_: string, listener: (event: MediaQueryListEvent) => void) => {
            listeners = listeners.filter((l) => l !== listener);
          }
        ),
        dispatchEvent: vi.fn(),
      })),
    });
  });

  it('deve retornar false quando a query não corresponde', () => {
    matchesValue = false;

    const { result } = renderHook(() =>
      useMediaQuery('(max-width: 768px)')
    );

    expect(result.current).toBe(false);
  });

  it('deve retornar true quando a query corresponde', () => {
    matchesValue = true;

    const { result } = renderHook(() =>
      useMediaQuery('(max-width: 768px)')
    );

    expect(result.current).toBe(true);
  });

  it('deve atualizar quando o media query muda', () => {
    matchesValue = false;

    const { result } = renderHook(() =>
      useMediaQuery('(max-width: 768px)')
    );

    expect(result.current).toBe(false);

    // Simular mudança no media query
    act(() => {
      listeners.forEach((listener) =>
        listener({ matches: true } as MediaQueryListEvent)
      );
    });

    expect(result.current).toBe(true);
  });

  it('deve limpar listener ao desmontar', () => {
    const { unmount } = renderHook(() =>
      useMediaQuery('(max-width: 768px)')
    );

    expect(listeners.length).toBeGreaterThan(0);

    unmount();

    // removeEventListener should have been called
    expect(window.matchMedia).toHaveBeenCalled();
  });
});
