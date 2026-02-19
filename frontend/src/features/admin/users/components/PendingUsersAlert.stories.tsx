import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { Alert, AlertDescription } from '@/shared/components/ui/alert';
import { Button } from '@/shared/components/ui/button';
import { Bell } from 'lucide-react';
import { PendingUsersAlert } from './PendingUsersAlert';

/**
 * O PendingUsersAlert depende do hook usePendingUsers (React Query).
 * No Storybook, o hook fará uma requisição real que falhará,
 * resultando no componente não renderizando (pendingCount = 0 → null).
 *
 * Para demonstrar os estados visuais, usamos stories que reproduzem
 * o layout do componente diretamente.
 */

function createMockQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: Infinity,
      },
    },
  });
}

const meta: Meta<typeof PendingUsersAlert> = {
  title: 'Admin/PendingUsersAlert',
  component: PendingUsersAlert,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Alerta exibido no topo da tela de gestão de usuários quando há cadastros pendentes de aprovação. ' +
          'Mostra a contagem de pendentes e botão para filtrar a lista. ' +
          'Usa usePendingUsers (React Query) com auto-refresh a cada 60s. ' +
          'Retorna null quando não há pendentes.',
      },
    },
  },
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient()}>
        <MemoryRouter>
          <div style={{ width: 700 }}>
            <Story />
          </div>
        </MemoryRouter>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Estado padrão — sem backend, o componente retorna null (sem pendentes).
 * Para ver o alerta visualmente, consulte a story "MockedAlert".
 */
export const Default: Story = {};

/**
 * Simulação visual do alerta com pendentes (layout estático).
 * Reproduz a aparência do componente quando há 3 usuários pendentes.
 */
export const MockedAlert: Story = {
  render: () => (
    <Alert className="mb-4">
      <Bell className="h-4 w-4" />
      <AlertDescription className="flex items-center justify-between">
        <span>
          Você tem <strong>3</strong> usuário(s) aguardando aprovação
        </span>
        <Button size="sm" onClick={() => alert('Navegar para pendentes')}>
          Ver Pendentes
        </Button>
      </AlertDescription>
    </Alert>
  ),
};
