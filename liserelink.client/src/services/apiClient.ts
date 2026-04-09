import axios from 'axios';
import type { AxiosError } from 'axios';
import type { ProblemDetails } from '@/types';
import { useAuthStore } from '@/stores/authStore';

const apiClient = axios.create({
  baseURL: (import.meta.env.VITE_API_URL ?? '') + '/api',
});

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ProblemDetails>) => {
    const status = error.response?.status;

    if (status === 401) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
      return Promise.reject(error);
    }

    if (status !== undefined && status >= 400 && status < 500) {
      const data: unknown = error.response?.data;
      if (data !== null && typeof data === 'object' && 'title' in data) {
        return Promise.reject(data as ProblemDetails);
      }
      return Promise.reject(new Error('Une erreur est survenue. Veuillez réessayer.'));
    }

    if (status !== undefined && status >= 500) {
      return Promise.reject(new Error('Une erreur serveur est survenue. Veuillez réessayer.'));
    }

    return Promise.reject(error);
  }
);

export default apiClient;
