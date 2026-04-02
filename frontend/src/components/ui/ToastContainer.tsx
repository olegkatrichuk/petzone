import Snackbar from '@mui/material/Snackbar'
import Alert from '@mui/material/Alert'
import { useToastStore } from '../../store/toastStore'

export default function ToastContainer() {
  const { toasts, dismiss } = useToastStore()

  return (
    <>
      {toasts.map((t, i) => (
        <Snackbar
          key={t.id}
          open
          autoHideDuration={3000}
          onClose={() => dismiss(t.id)}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
          sx={{ bottom: { xs: 16 + i * 64, sm: 24 + i * 64 } }}
        >
          <Alert
            onClose={() => dismiss(t.id)}
            severity={t.severity}
            variant="filled"
            sx={{ minWidth: 260 }}
          >
            {t.message}
          </Alert>
        </Snackbar>
      ))}
    </>
  )
}
