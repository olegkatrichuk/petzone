import { create } from 'zustand'

type Severity = 'success' | 'error' | 'info' | 'warning'

interface Toast {
  id: number
  message: string
  severity: Severity
}

interface ToastState {
  toasts: Toast[]
  show: (message: string, severity?: Severity) => void
  dismiss: (id: number) => void
}

let nextId = 0

export const useToastStore = create<ToastState>((set) => ({
  toasts: [],
  show: (message, severity = 'success') =>
    set((s) => ({ toasts: [...s.toasts, { id: ++nextId, message, severity }] })),
  dismiss: (id) =>
    set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) })),
}))

export const toast = {
  success: (msg: string) => useToastStore.getState().show(msg, 'success'),
  error:   (msg: string) => useToastStore.getState().show(msg, 'error'),
  info:    (msg: string) => useToastStore.getState().show(msg, 'info'),
  warning: (msg: string) => useToastStore.getState().show(msg, 'warning'),
}
