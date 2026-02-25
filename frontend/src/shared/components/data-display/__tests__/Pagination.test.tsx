import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Pagination } from '../Pagination';

describe('Pagination', () => {
  it('não deve renderizar nada quando totalPages <= 1', () => {
    const { container } = render(
      <Pagination currentPage={1} totalPages={1} onPageChange={vi.fn()} />,
    );
    expect(container).toBeEmptyDOMElement();
  });

  it('não deve renderizar nada quando totalPages é 0', () => {
    const { container } = render(
      <Pagination currentPage={1} totalPages={0} onPageChange={vi.fn()} />,
    );
    expect(container).toBeEmptyDOMElement();
  });

  it('deve exibir informação de página sem totalItems', () => {
    render(<Pagination currentPage={2} totalPages={5} onPageChange={vi.fn()} />);
    expect(screen.getByText(/Página 2 de 5/)).toBeInTheDocument();
  });

  it('deve exibir totalItems no plural quando > 1', () => {
    render(
      <Pagination currentPage={1} totalPages={3} totalItems={10} onPageChange={vi.fn()} />,
    );
    expect(screen.getByText(/10 itens/)).toBeInTheDocument();
  });

  it('deve exibir totalItems no singular quando === 1', () => {
    render(
      <Pagination currentPage={1} totalPages={2} totalItems={1} onPageChange={vi.fn()} />,
    );
    expect(screen.getByText(/1 item/)).toBeInTheDocument();
  });

  it('deve desabilitar botões de voltar na primeira página', () => {
    render(<Pagination currentPage={1} totalPages={3} onPageChange={vi.fn()} />);
    expect(screen.getByTitle('Primeira página')).toBeDisabled();
    expect(screen.getByTitle('Página anterior')).toBeDisabled();
  });

  it('deve desabilitar botões de avançar na última página', () => {
    render(<Pagination currentPage={3} totalPages={3} onPageChange={vi.fn()} />);
    expect(screen.getByTitle('Próxima página')).toBeDisabled();
    expect(screen.getByTitle('Última página')).toBeDisabled();
  });

  it('deve habilitar todos os botões em página intermediária', () => {
    render(<Pagination currentPage={2} totalPages={3} onPageChange={vi.fn()} />);
    expect(screen.getByTitle('Primeira página')).not.toBeDisabled();
    expect(screen.getByTitle('Página anterior')).not.toBeDisabled();
    expect(screen.getByTitle('Próxima página')).not.toBeDisabled();
    expect(screen.getByTitle('Última página')).not.toBeDisabled();
  });

  it('deve chamar onPageChange com 1 ao clicar em primeira página', async () => {
    const onPageChange = vi.fn();
    render(<Pagination currentPage={3} totalPages={5} onPageChange={onPageChange} />);
    await userEvent.click(screen.getByTitle('Primeira página'));
    expect(onPageChange).toHaveBeenCalledWith(1);
  });

  it('deve chamar onPageChange com currentPage - 1 ao clicar em anterior', async () => {
    const onPageChange = vi.fn();
    render(<Pagination currentPage={3} totalPages={5} onPageChange={onPageChange} />);
    await userEvent.click(screen.getByTitle('Página anterior'));
    expect(onPageChange).toHaveBeenCalledWith(2);
  });

  it('deve chamar onPageChange com currentPage + 1 ao clicar em próxima', async () => {
    const onPageChange = vi.fn();
    render(<Pagination currentPage={3} totalPages={5} onPageChange={onPageChange} />);
    await userEvent.click(screen.getByTitle('Próxima página'));
    expect(onPageChange).toHaveBeenCalledWith(4);
  });

  it('deve chamar onPageChange com totalPages ao clicar em última página', async () => {
    const onPageChange = vi.fn();
    render(<Pagination currentPage={2} totalPages={5} onPageChange={onPageChange} />);
    await userEvent.click(screen.getByTitle('Última página'));
    expect(onPageChange).toHaveBeenCalledWith(5);
  });
});
