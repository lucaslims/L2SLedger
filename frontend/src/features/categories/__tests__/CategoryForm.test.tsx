import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CategoryForm } from '../components/CategoryForm';

describe('CategoryForm', () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar formulário de criação com valores padrão', () => {
    render(<CategoryForm onSubmit={mockOnSubmit} />);

    expect(screen.getByLabelText('Nome da Categoria')).toBeInTheDocument();
    expect(screen.getByLabelText('Tipo')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Criar' })).toBeInTheDocument();
  });

  it('deve renderizar botão "Atualizar" quando há initialValues', () => {
    render(
      <CategoryForm
        initialValues={{ name: 'Alimentação', type: 'Expense' }}
        onSubmit={mockOnSubmit}
      />
    );

    expect(screen.getByRole('button', { name: 'Atualizar' })).toBeInTheDocument();
  });

  it('deve exibir erro quando nome está vazio', async () => {
    const user = userEvent.setup();
    render(<CategoryForm onSubmit={mockOnSubmit} />);

    // Clear the name field and submit
    const nameInput = screen.getByLabelText('Nome da Categoria');
    await user.clear(nameInput);
    await user.click(screen.getByRole('button', { name: 'Criar' }));

    expect(await screen.findByText('Nome é obrigatório')).toBeInTheDocument();
    expect(mockOnSubmit).not.toHaveBeenCalled();
  });

  it('deve chamar onSubmit com dados válidos', async () => {
    const user = userEvent.setup();
    render(<CategoryForm onSubmit={mockOnSubmit} />);

    const nameInput = screen.getByLabelText('Nome da Categoria');
    await user.type(nameInput, 'Nova Categoria');

    await user.click(screen.getByRole('button', { name: 'Criar' }));

    await vi.waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          name: 'Nova Categoria',
          type: 'Expense',
        }),
        expect.anything()
      );
    });
  });

  it('deve desabilitar botão quando isPending', () => {
    render(<CategoryForm onSubmit={mockOnSubmit} isPending />);

    const button = screen.getByRole('button', { name: 'Salvando...' });
    expect(button).toBeDisabled();
  });

  it('deve preencher formulário com initialValues', () => {
    render(
      <CategoryForm
        initialValues={{ name: 'Salário', type: 'Income' }}
        onSubmit={mockOnSubmit}
      />
    );

    expect(screen.getByLabelText('Nome da Categoria')).toHaveValue('Salário');
  });
});
