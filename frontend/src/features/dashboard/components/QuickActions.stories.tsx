import type { Meta, StoryObj } from '@storybook/react';
import { MemoryRouter } from 'react-router-dom';
import { QuickActions } from './QuickActions';

const meta: Meta<typeof QuickActions> = {
  title: 'Dashboard/QuickActions',
  component: QuickActions,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Card com atalhos para ações frequentes. ' +
          'Inclui botão de nova transação e exportação (desabilitado). ' +
          'Apenas navegação — sem lógica de negócio.',
      },
    },
  },
  decorators: [
    (Story) => (
      <MemoryRouter initialEntries={['/dashboard']}>
        <div style={{ width: 320 }}>
          <Story />
        </div>
      </MemoryRouter>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Estado padrão com botão de nova transação ativo
 * e botão de exportação desabilitado.
 */
export const Default: Story = {};
