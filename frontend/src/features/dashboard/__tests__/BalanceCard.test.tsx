import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BalanceCard } from '../components/BalanceCard';

describe('BalanceCard', () => {
  it('deve renderizar card de receita com valor formatado', () => {
    render(<BalanceCard type="income" value={5000} label="Total de Receitas" />);

    expect(screen.getByText('Total de Receitas')).toBeInTheDocument();
    expect(screen.getByTestId('balance-value-income')).toHaveTextContent(/R\$\s*5\.000,00/);
  });

  it('deve renderizar card de despesa com valor formatado', () => {
    render(<BalanceCard type="expense" value={3200.5} label="Total de Despesas" />);

    expect(screen.getByText('Total de Despesas')).toBeInTheDocument();
    expect(screen.getByTestId('balance-value-expense')).toHaveTextContent(/R\$\s*3\.200,50/);
  });

  it('deve renderizar card de saldo com valor formatado', () => {
    render(<BalanceCard type="balance" value={1800} label="Saldo Atual" />);

    expect(screen.getByText('Saldo Atual')).toBeInTheDocument();
    expect(screen.getByTestId('balance-value-balance')).toHaveTextContent(/R\$\s*1\.800,00/);
  });

  it('deve renderizar valor zero corretamente', () => {
    render(<BalanceCard type="balance" value={0} label="Saldo Atual" />);

    expect(screen.getByTestId('balance-value-balance')).toHaveTextContent(/R\$\s*0,00/);
  });

  it('deve renderizar valor negativo corretamente', () => {
    render(<BalanceCard type="balance" value={-500} label="Saldo Atual" />);

    expect(screen.getByTestId('balance-value-balance')).toBeInTheDocument();
  });

  it('deve aplicar cor correta para receita', () => {
    render(<BalanceCard type="income" value={1000} label="Receitas" />);

    const valueElement = screen.getByTestId('balance-value-income');
    expect(valueElement.className).toContain('text-income');
  });

  it('deve aplicar cor correta para despesa', () => {
    render(<BalanceCard type="expense" value={500} label="Despesas" />);

    const valueElement = screen.getByTestId('balance-value-expense');
    expect(valueElement.className).toContain('text-expense');
  });

  it('deve aplicar cor correta para saldo', () => {
    render(<BalanceCard type="balance" value={500} label="Saldo" />);

    const valueElement = screen.getByTestId('balance-value-balance');
    expect(valueElement.className).toContain('text-primary');
  });
});
