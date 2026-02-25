import { describe, it, expect } from 'vitest';
import {
  formatDate,
  formatDateTime,
  formatRelativeDate,
  formatCurrency,
  formatNumber,
  formatPercent,
} from '../formatters';

describe('formatDate', () => {
  it('deve formatar string ISO no padrão brasileiro', () => {
    expect(formatDate('2026-02-25')).toBe('25/02/2026');
  });

  it('deve formatar objeto Date no padrão brasileiro', () => {
    expect(formatDate(new Date(2026, 1, 25))).toBe('25/02/2026');
  });
});

describe('formatDateTime', () => {
  it('deve formatar string ISO com data e hora', () => {
    const result = formatDateTime('2026-02-25T14:30:00');
    expect(result).toMatch(/25\/02\/2026 às \d{2}:\d{2}/);
  });

  it('deve formatar objeto Date com data e hora', () => {
    const date = new Date(2026, 1, 25, 14, 30);
    const result = formatDateTime(date);
    expect(result).toMatch(/25\/02\/2026 às 14:30/);
  });
});

describe('formatRelativeDate', () => {
  it('deve formatar string ISO como data relativa', () => {
    const recent = new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString();
    const result = formatRelativeDate(recent);
    expect(result).toMatch(/hora/i);
  });

  it('deve formatar objeto Date como data relativa', () => {
    const recent = new Date(Date.now() - 5 * 60 * 1000);
    const result = formatRelativeDate(recent);
    expect(result).toMatch(/minuto/i);
  });
});

describe('formatCurrency', () => {
  it('deve formatar valor monetário em BRL', () => {
    const result = formatCurrency(1500.5);
    expect(result).toMatch(/R\$\s*1\.500,50/);
  });

  it('deve formatar zero', () => {
    const result = formatCurrency(0);
    expect(result).toMatch(/R\$\s*0,00/);
  });

  it('deve formatar valor negativo', () => {
    const result = formatCurrency(-200);
    expect(result).toMatch(/-/);
  });
});

describe('formatNumber', () => {
  it('deve formatar número com 2 casas decimais por padrão', () => {
    expect(formatNumber(1234.5)).toBe('1.234,50');
  });

  it('deve formatar número com casas decimais customizadas', () => {
    expect(formatNumber(1234.5678, 3)).toBe('1.234,568');
  });

  it('deve formatar número com 0 casas decimais', () => {
    expect(formatNumber(1234, 0)).toBe('1.234');
  });
});

describe('formatPercent', () => {
  it('deve formatar percentual com 1 casa decimal por padrão', () => {
    expect(formatPercent(75)).toMatch(/75,0\s*%/);
  });

  it('deve formatar percentual com casas decimais customizadas', () => {
    expect(formatPercent(33.33, 2)).toMatch(/33,33\s*%/);
  });

  it('deve formatar zero porcento', () => {
    expect(formatPercent(0)).toMatch(/0,0\s*%/);
  });
});
