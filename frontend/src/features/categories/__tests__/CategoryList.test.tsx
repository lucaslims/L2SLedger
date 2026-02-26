import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import '@testing-library/jest-dom';
import { CategoryList } from '../components/CategoryList';
import { categoryService } from '../services/categoryService';
import type { CategoryDto } from '../types/category.types';

// Mock categoryService
vi.mock('../services/categoryService', () => ({
  categoryService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

// Mock sonner toast
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

const mockCategories: CategoryDto[] = [
  {
    id: '1',
    name: 'Alimentação',
    type: 'Expense',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
  },
  {
    id: '2',
    name: 'Salário',
    type: 'Income',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
  },
];

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return ({ children }: any) => (
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{children}</MemoryRouter>
    </QueryClientProvider>
  );
}

describe('CategoryList', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve exibir loading enquanto carrega', () => {
    vi.mocked(categoryService.getAll).mockReturnValue(new Promise(() => {}));

    render(<CategoryList />, { wrapper: createWrapper() });

    // Skeleton elements should render during loading
    const skeletons = document.querySelectorAll('[class*="animate-pulse"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('deve exibir categorias após carregamento', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue(mockCategories);

    render(<CategoryList />, { wrapper: createWrapper() });

    // Desktop + mobile views both render, so multiple elements exist
    expect(await screen.findAllByText('Alimentação')).not.toHaveLength(0);
    expect(screen.getAllByText('Salário')).not.toHaveLength(0);
  });

  it('deve exibir badges de tipo corretos', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue(mockCategories);

    render(<CategoryList />, { wrapper: createWrapper() });

    // Desktop + mobile views both render badges
    expect(await screen.findAllByText('Despesa')).not.toHaveLength(0);
    expect(screen.getAllByText('Receita')).not.toHaveLength(0);
  });

  it('deve exibir mensagem quando não há categorias', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue([]);

    render(<CategoryList />, { wrapper: createWrapper() });

    expect(await screen.findByText('Nenhuma categoria cadastrada')).toBeInTheDocument();
  });

  it('deve ter botões de editar e excluir para cada categoria', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue(mockCategories);

    render(<CategoryList />, { wrapper: createWrapper() });

    await screen.findAllByText('Alimentação');

    // Desktop + mobile both render action buttons
    expect(screen.getAllByLabelText('Editar Alimentação').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByLabelText('Excluir Alimentação').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByLabelText('Editar Salário').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByLabelText('Excluir Salário').length).toBeGreaterThanOrEqual(1);
  });

  it('deve abrir diálogo de exclusão ao clicar em excluir', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue(mockCategories);
    const user = userEvent.setup();

    render(<CategoryList />, { wrapper: createWrapper() });

    await screen.findAllByText('Alimentação');

    // Click the first delete button (desktop or mobile)
    await user.click(screen.getAllByLabelText('Excluir Alimentação')[0]);

    expect(await screen.findByText('Confirmar Exclusão')).toBeInTheDocument();
    expect(screen.getAllByText(/Alimentação/).length).toBeGreaterThanOrEqual(2);
  });
});
