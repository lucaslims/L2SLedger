import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { DateDisplay } from '../DateDisplay';

const ISO_DATE = '2026-02-25';
const ISO_DATETIME = '2026-02-25T14:30:00';

describe('DateDisplay', () => {
  describe('formato padrão (date)', () => {
    it('deve exibir data formatada a partir de string ISO', () => {
      render(<DateDisplay date={ISO_DATE} />);
      expect(screen.getByRole('time')).toHaveTextContent('25/02/2026');
    });

    it('deve usar a string ISO como atributo dateTime', () => {
      render(<DateDisplay date={ISO_DATE} />);
      expect(screen.getByRole('time')).toHaveAttribute('dateTime', ISO_DATE);
    });
  });

  describe('formato datetime', () => {
    it('deve exibir data e hora formatadas', () => {
      render(<DateDisplay date={ISO_DATETIME} format="datetime" />);
      expect(screen.getByRole('time')).toHaveTextContent(/25\/02\/2026 às/);
    });
  });

  describe('formato relative', () => {
    it('deve exibir data relativa', () => {
      const recent = new Date(Date.now() - 60 * 60 * 1000).toISOString();
      render(<DateDisplay date={recent} format="relative" />);
      expect(screen.getByRole('time')).toHaveTextContent(/hora/i);
    });
  });

  describe('com objeto Date', () => {
    it('deve aceitar objeto Date e usar toISOString como atributo dateTime', () => {
      const date = new Date(2026, 1, 25);
      render(<DateDisplay date={date} />);
      const timeEl = screen.getByRole('time');
      expect(timeEl).toHaveTextContent('25/02/2026');
      expect(timeEl).toHaveAttribute('dateTime', date.toISOString());
    });
  });

  describe('className', () => {
    it('deve aplicar classe customizada', () => {
      render(<DateDisplay date={ISO_DATE} className="text-red-500" />);
      expect(screen.getByRole('time')).toHaveClass('text-red-500');
    });
  });
});
