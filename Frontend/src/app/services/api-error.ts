import { HttpErrorResponse } from '@angular/common/http';

export function apiError(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) return fallback;
  if (error.status === 0) return 'API tidak dapat dihubungi. Periksa koneksi dan coba lagi.';
  const body = error.error;
  const messages = body?.errors && Object.values(body.errors).flat();
  if (Array.isArray(messages) && messages.length) return messages.join(' ');
  if (typeof body?.message === 'string') return body.message;
  if (typeof body?.detail === 'string') return body.detail;
  if (typeof body === 'string' && body.trim() && !body.trim().startsWith('<')) return body;
  return fallback;
}
