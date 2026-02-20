import { Badge } from '@/shared/components/ui/badge';
import type { UserStatus } from '@/shared/types/common.types';
import { USER_STATUS_LABELS } from '@/shared/lib/utils/constants';

interface UserStatusBadgeProps {
  status: UserStatus;
}

const variants: Record<UserStatus, 'secondary' | 'default' | 'destructive' | 'outline'> = {
  Pending: 'secondary',
  Active: 'default',
  Suspended: 'destructive',
  Rejected: 'outline',
};

export function UserStatusBadge({ status }: UserStatusBadgeProps) {
  return <Badge variant={variants[status]}>{USER_STATUS_LABELS[status]}</Badge>;
}
