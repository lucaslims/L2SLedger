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
      <CategoryForm initialValues={{ name: 'Salário', type: 'Income' }} onSubmit={mockOnSubmit} />
    );

    expect(screen.getByLabelText('Nome da Categoria')).toHaveValue('Salário');
  });

  // ─── Combobox de Categoria Pai ─────────────────────────────────────────────

  it('deve exibir combobox de categoria pai quando parentCategories é fornecido', () => {
    const parentCategories = [
      { id: '1', name: 'Moradia' },
      { id: '2', name: 'Alimentação' },
    ];

    render(<CategoryForm onSubmit={mockOnSubmit} parentCategories={parentCategories} />);

    // Deve exibir botão do combobox (não input de ID)
    expect(screen.getByRole('combobox', { name: /selecionar categoria pai/i })).toBeInTheDocument();
    expect(screen.queryByPlaceholderText('ID da categoria pai')).not.toBeInTheDocument();
  });

  it('deve exibir input de ID quando não há parentCategories', () => {
    render(<CategoryForm onSubmit={mockOnSubmit} />);

    // Sem parentCategories: exibe input de ID (fallback)
    expect(screen.getByPlaceholderText('ID da categoria pai')).toBeInTheDocument();
  });

  it('deve abrir popover de categorias ao clicar no combobox', async () => {
    const user = userEvent.setup();
    const parentCategories = [
      { id: '1', name: 'Moradia' },
      { id: '2', name: 'Alimentação' },
    ];

    render(<CategoryForm onSubmit={mockOnSubmit} parentCategories={parentCategories} />);

    await user.click(screen.getByRole('combobox', { name: /selecionar categoria pai/i }));

    // Popover aberto deve mostrar as categorias
    expect(await screen.findByText('Moradia')).toBeInTheDocument();
    expect(screen.getByText('Alimentação')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Buscar categoria...')).toBeInTheDocument();
  });

  it('deve filtrar categorias pela busca', async () => {
    const user = userEvent.setup();
    const parentCategories = [
      { id: '1', name: 'Moradia' },
      { id: '2', name: 'Alimentação' },
      { id: '3', name: 'Transporte' },
    ];

    render(<CategoryForm onSubmit={mockOnSubmit} parentCategories={parentCategories} />);

    await user.click(screen.getByRole('combobox', { name: /selecionar categoria pai/i }));
    await user.type(screen.getByPlaceholderText('Buscar categoria...'), 'mora');

    expect(screen.getByText('Moradia')).toBeInTheDocument();
    expect(screen.queryByText('Alimentação')).not.toBeInTheDocument();
    expect(screen.queryByText('Transporte')).not.toBeInTheDocument();
  });

  it('deve selecionar categoria pai ao clicar', async () => {
    const user = userEvent.setup();
    const parentCategories = [
      { id: 'cat-1', name: 'Moradia' },
      { id: 'cat-2', name: 'Alimentação' },
    ];

    render(<CategoryForm onSubmit={mockOnSubmit} parentCategories={parentCategories} />);

    // Abrir popover e selecionar
    await user.click(screen.getByRole('combobox', { name: /selecionar categoria pai/i }));
    await user.click(await screen.findByText('Moradia'));

    // Botão deve mostrar o nome selecionado
    expect(screen.getByRole('combobox', { name: /selecionar categoria pai/i })).toHaveTextContent(
      'Moradia'
    );
  });
});
