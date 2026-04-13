import { Temporal } from '@js-temporal/polyfill';
if (typeof (globalThis as Record<string, unknown>).Temporal === 'undefined') {
  Object.defineProperty(globalThis, 'Temporal', {
    value: Temporal,
    writable: false,
    configurable: false,
  });
}
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App.tsx';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>
);
