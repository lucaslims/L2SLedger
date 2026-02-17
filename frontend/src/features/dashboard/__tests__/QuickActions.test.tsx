import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { QuickActions } from '../components/QuickActions';

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('QuickActions', () => {
  it('deve renderizar botões de ação', () => {
    render(
      <BrowserRouter>
        <QuickActions />
      </BrowserRouter>
    );

    expect(screen.getByText('Ações Rápidas')).toBeInTheDocument();
    expect(screen.getByText('Nova Transação')).toBeInTheDocument();
    expect(screen.getByText('Exportar Dados')).toBeInTheDocument();
  });

  it('deve navegar ao clicar em Nova Transação', async () => {
    const user = userEvent.setup();

    render(
      <BrowserRouter>
        <QuickActions />
      </BrowserRouter>
    );

    await user.click(screen.getByText('Nova Transação'));
    expect(mockNavigate).toHaveBeenCalledWith('/transactions/new');
  });

  it('deve manter Exportar Dados desabilitado', () => {
    render(
      <BrowserRouter>
        <QuickActions />
      </BrowserRouter>
    );

    const exportButton = screen.getByText('Exportar Dados').closest('button');
    expect(exportButton).toBeDisabled();
  });
});
