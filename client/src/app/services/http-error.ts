export function parseHttpError(err: any, fallback: string): string {
  if (err?.error) {
    if (typeof err.error === 'string') {
      return err.error;
    }
    if (typeof err.error?.detail === 'string' && err.error.detail.trim()) {
      return err.error.detail;
    }
    if (err.error?.errors && typeof err.error.errors === 'object') {
      const firstKey = Object.keys(err.error.errors)[0];
      const firstMessages = firstKey ? err.error.errors[firstKey] : null;
      if (Array.isArray(firstMessages) && typeof firstMessages[0] === 'string') {
        return firstMessages[0];
      }
    }
    if (err.error?.title) {
      return err.error.title;
    }
  }
  return fallback;
}
